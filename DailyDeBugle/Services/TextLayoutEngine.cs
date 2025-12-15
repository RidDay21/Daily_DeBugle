// DailyDeBugle/Services/TextLayoutEngine.cs
using System.Text;
using System.Text.RegularExpressions;
using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    /// <summary>
    /// Движок для серверного расчета разметки текста. Работает без JavaScript.
    /// Основан на математических моделях типографики.
    /// </summary>
    public class TextLayoutEngine
    {
        // Типографские константы для расчётов (основаны на стандартах печати)
        private const double PT_TO_CM = 0.0352778; // 1 пункт = 0.0352778 см
        private const double INCH_TO_CM = 2.54;

        // Средние значения для Times New Roman (стандартный газетный шрифт)
        private readonly TypographySettings _defaultSettings = new()
        {
            FontSizePt = 10, // Стандартный для газет
            LineHeight = 1.2, // Интерлиньяж 120%
            CharactersPerPica = 2.2, // Среднее количество символов в 1 пике (12pt)
            WordLengthAvg = 5.5, // Средняя длина слова в символах
            ParagraphSpacingCm = 0.3, // Отступ между абзацами
            IndentFirstLineCm = 0.5 // Абзацный отступ
        };

        /// <summary>
        /// Рассчитывает, сколько текста поместится в заданную высоту
        /// </summary>
        public TextFitResult CalculateTextFit(string text, double availableHeightCm, double columnWidthCm,
            TypographySettings? settings = null)
        {
            settings ??= _defaultSettings;

            // 1. Рассчитываем базовые метрики
            double lineHeightCm = settings.FontSizePt * PT_TO_CM * settings.LineHeight;
            double charsPerLine = CalculateCharsPerLine(columnWidthCm, settings);
            double linesAvailable = Math.Floor(availableHeightCm / lineHeightCm);

            if (linesAvailable <= 0)
                return new TextFitResult { FittedText = "", RemainingText = text, FittedHeight = 0 };

            // 2. Анализируем структуру текста
            var paragraphs = SplitIntoSemanticBlocks(text);
            var result = new StringBuilder();
            double currentHeight = 0;
            int totalCharsFitted = 0;

            // 3. Заполняем доступное пространство
            foreach (var paragraph in paragraphs)
            {
                // Высота параграфа = высота строк + отступ
                double paraHeight = CalculateParagraphHeight(paragraph, charsPerLine, lineHeightCm, settings);

                if (currentHeight + paraHeight <= availableHeightCm)
                {
                    // Весь параграф помещается
                    result.AppendLine(paragraph);
                    result.AppendLine(); // Пустая строка между абзацами
                    currentHeight += paraHeight + settings.ParagraphSpacingCm;
                    totalCharsFitted += paragraph.Length + 2;
                }
                else
                {
                    // Параграф не помещается целиком - разбиваем на строки
                    var lines = SplitParagraphIntoLines(paragraph, charsPerLine);
                    double lineAccumulatedHeight = currentHeight;

                    foreach (var line in lines)
                    {
                        lineAccumulatedHeight += lineHeightCm;

                        if (lineAccumulatedHeight <= availableHeightCm)
                        {
                            result.AppendLine(line);
                            totalCharsFitted += line.Length + 1;
                        }
                        else
                        {
                            // Нашли строку, которая не помещается
                            break;
                        }
                    }

                    // Нашли точку разрыва
                    break;
                }
            }

            // 4. Уточняем точку разрыва (не обрезать слово)
            string fittedText = result.ToString().TrimEnd();
            if (totalCharsFitted < text.Length)
            {
                fittedText = AdjustToNearestWordBoundary(fittedText, text, totalCharsFitted);
                totalCharsFitted = fittedText.Length;
            }

            return new TextFitResult
            {
                FittedText = fittedText,
                RemainingText = totalCharsFitted < text.Length ? text.Substring(totalCharsFitted) : "",
                FittedHeight = currentHeight,
                LinesFitted = (int)(currentHeight / lineHeightCm),
                CharactersFitted = totalCharsFitted
            };
        }

        /// <summary>
        /// Находит оптимальную точку разрыва статьи между страницами
        /// </summary>
        public ArticleBreakResult FindOptimalBreakPoint(Article article, double availableHeightCm,
            double columnWidthCm, bool isFirstPage = true)
        {
            if (article == null || string.IsNullOrEmpty(article.Content))
                return new ArticleBreakResult { BreakIndex = 0 };

            // Вызываем перегруженную версию, которая работает только с текстом
            return FindOptimalBreakPoint(article.Content, article.Headline, availableHeightCm,
                columnWidthCm, isFirstPage);
        }

        /// <summary>
        /// Перегруженная версия, работающая только с текстом и заголовком
        /// </summary>
        public ArticleBreakResult FindOptimalBreakPoint(string content, string? headline,
            double availableHeightCm, double columnWidthCm,
            bool isFirstPage = true)
        {
            if (string.IsNullOrEmpty(content))
                return new ArticleBreakResult { BreakIndex = 0 };

            var settings = _defaultSettings;

            // Для первой страницы учитываем заголовок
            double headerHeight = 0;
            if (isFirstPage && !string.IsNullOrEmpty(headline))
            {
                // Заголовок обычно крупнее: 14pt вместо 10pt
                var headerSettings = settings with { FontSizePt = 14, LineHeight = 1.1 };
                headerHeight = CalculateTextHeight(headline, columnWidthCm, headerSettings) + 0.5;
            }

            double actualAvailableHeight = availableHeightCm - headerHeight;
            if (actualAvailableHeight <= 0)
                return new ArticleBreakResult { BreakIndex = 0 };

            // Вычисляем, сколько текста помещается
            var fitResult = CalculateTextFit(content, actualAvailableHeight, columnWidthCm, settings);

            return new ArticleBreakResult
            {
                BreakIndex = fitResult.CharactersFitted,
                FittedText = fitResult.FittedText,
                RemainingText = fitResult.RemainingText,
                HeaderHeight = headerHeight,
                ContentHeight = fitResult.FittedHeight
            };
        }

        /// <summary>
        /// Рассчитывает высоту текста без ограничений
        /// </summary>
        public double CalculateTextHeight(string text, double columnWidthCm, TypographySettings? settings = null)
        {
            settings ??= _defaultSettings;

            if (string.IsNullOrEmpty(text))
                return 0;

            double lineHeightCm = settings.FontSizePt * PT_TO_CM * settings.LineHeight;
            double charsPerLine = CalculateCharsPerLine(columnWidthCm, settings);

            var paragraphs = SplitIntoSemanticBlocks(text);
            double totalHeight = 0;

            foreach (var paragraph in paragraphs)
            {
                totalHeight += CalculateParagraphHeight(paragraph, charsPerLine, lineHeightCm, settings);
            }

            return totalHeight;
        }

        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====

        private double CalculateCharsPerLine(double columnWidthCm, TypographySettings settings)
        {
            // Формула: ширина_колонки / (размер_шрифта * ширина_символа)
            // Ширина символа в см = (размер_шрифта * PT_TO_CM) / charactersPerPica
            double charWidthCm = (settings.FontSizePt * PT_TO_CM) / settings.CharactersPerPica;
            return Math.Floor(columnWidthCm / charWidthCm);
        }

        private List<string> SplitIntoSemanticBlocks(string text)
        {
            // Делим на абзацы, но сохраняем смысловые блоки
            var blocks = new List<string>();
            var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var para in paragraphs)
            {
                var trimmed = para.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    blocks.Add(trimmed);
            }

            return blocks;
        }

        private double CalculateParagraphHeight(string paragraph, double charsPerLine, double lineHeightCm,
            TypographySettings settings)
        {
            if (string.IsNullOrEmpty(paragraph))
                return 0;

            // Учитываем абзацный отступ для первого абзаца
            double effectiveLength = paragraph.Length;
            double lines = Math.Ceiling(effectiveLength / charsPerLine);

            return lines * lineHeightCm + settings.ParagraphSpacingCm;
        }

        private List<string> SplitParagraphIntoLines(string paragraph, double charsPerLine)
        {
            var lines = new List<string>();
            var words = paragraph.Split(' ');
            var currentLine = new StringBuilder();
            double currentLength = 0;

            foreach (var word in words)
            {
                if (currentLength + word.Length + 1 > charsPerLine) // +1 для пробела
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();
                        currentLength = 0;
                    }
                }

                if (currentLine.Length > 0)
                {
                    currentLine.Append(' ');
                    currentLength++;
                }

                currentLine.Append(word);
                currentLength += word.Length;
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            return lines;
        }

        private string AdjustToNearestWordBoundary(string fittedText, string originalText, int charIndex)
        {
            if (charIndex >= originalText.Length || charIndex <= 0)
                return fittedText;

            // Ищем ближайший пробел слева от точки разрыва
            int adjustedIndex = charIndex;

            // Сначала попробуем найти конец предложения
            for (int i = Math.Min(charIndex, originalText.Length - 1); i >= 0; i--)
            {
                if (i < originalText.Length - 1)
                {
                    char current = originalText[i];
                    char next = originalText[i + 1];

                    // Конец предложения: точка/воскл/вопрос + пробел
                    if ((current == '.' || current == '!' || current == '?') &&
                        (next == ' ' || next == '\n' || next == '\r'))
                    {
                        adjustedIndex = i + 1;
                        break;
                    }
                }
            }

            // Если не нашли конец предложения, ищем пробел
            if (adjustedIndex == charIndex)
            {
                for (int i = Math.Min(charIndex, originalText.Length - 1); i >= 0; i--)
                {
                    if (originalText[i] == ' ')
                    {
                        adjustedIndex = i;
                        break;
                    }
                }
            }

            // Обрезаем по скорректированному индексу
            return originalText.Substring(0, adjustedIndex).TrimEnd();
        }

        public List<ArticlePartLayout> SplitArticleIntoPages(Article article, double pageHeightCm,
            double columnWidthCm, int maxPages = 10)
        {
            var parts = new List<ArticlePartLayout>();
            string remainingText = article.Content ?? "";
            int pageNumber = 1;
            bool isFirstPage = true;

            while (!string.IsNullOrEmpty(remainingText) && pageNumber <= maxPages)
            {
                // Используем перегруженную версию
                var breakResult = FindOptimalBreakPoint(
                    remainingText,
                    isFirstPage ? article.Headline : null,
                    pageHeightCm,
                    columnWidthCm,
                    isFirstPage
                );

                if (breakResult.BreakIndex <= 0)
                    break;

                parts.Add(new ArticlePartLayout
                {
                    PartNumber = pageNumber,
                    Content = breakResult.FittedText,
                    HeightCm = breakResult.TotalHeight,
                    IsFirstPage = isFirstPage,
                    IsLastPage = string.IsNullOrEmpty(breakResult.RemainingText),
                    HeaderHeight = breakResult.HeaderHeight,
                    ContentHeight = breakResult.ContentHeight
                });

                remainingText = breakResult.RemainingText;
                pageNumber++;
                isFirstPage = false;
            }

            // Обновляем TotalParts для всех частей
            foreach (var part in parts)
            {
                part.TotalParts = parts.Count;
            }

            return parts;
        }
        // ===== МОДЕЛИ ДАННЫХ =====

        public record TypographySettings
        {
            public double FontSizePt { get; init; } = 10;
            public double LineHeight { get; init; } = 1.2;
            public double CharactersPerPica { get; init; } = 2.2;
            public double WordLengthAvg { get; init; } = 5.5;
            public double ParagraphSpacingCm { get; init; } = 0.3;
            public double IndentFirstLineCm { get; init; } = 0.5;
        }

        public class TextFitResult
        {
            public string FittedText { get; set; } = "";
            public string RemainingText { get; set; } = "";
            public double FittedHeight { get; set; }
            public int LinesFitted { get; set; }
            public int CharactersFitted { get; set; }
        }

        public class ArticleBreakResult
        {
            public int BreakIndex { get; set; }
            public string FittedText { get; set; } = "";
            public string RemainingText { get; set; } = "";
            public double HeaderHeight { get; set; }
            public double ContentHeight { get; set; }
            public double TotalHeight => HeaderHeight + ContentHeight;
        }

        public class ArticlePartLayout
        {
            public int PartNumber { get; set; }
            public int TotalParts { get; set; }
            public string Content { get; set; } = "";
            public double HeightCm { get; set; }
            public bool IsFirstPage { get; set; }
            public bool IsLastPage { get; set; }
            public double HeaderHeight { get; set; }
            public double ContentHeight { get; set; }
        }

        public interface ITextLayoutEngine
        {
            TextFitResult CalculateTextFit(string text, double availableHeightCm, double columnWidthCm,
                TypographySettings? settings = null);

            ArticleBreakResult FindOptimalBreakPoint(Article article, double availableHeightCm,
                double columnWidthCm, bool isFirstPage = true);

            double CalculateTextHeight(string text, double columnWidthCm, TypographySettings? settings = null);

            List<ArticlePartLayout> SplitArticleIntoPages(Article article, double pageHeightCm,
                double columnWidthCm, int maxPages = 10);
        }
    }
}