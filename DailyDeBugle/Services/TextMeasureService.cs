// DailyDeBugle/Services/TextMeasureService.cs
using DailyDeBugle.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace DailyDeBugle.Services
{
    public class TextMeasureService : ITextMeasureService, IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private IJSObjectReference? _measurementModule;
        private bool _isInitialized = false;

        // Константы для преобразования единиц
        private const double CM_TO_PX = 37.7952755906; // 1 см = 37.795 px
        private const double PT_TO_PX = 1.3333333333;  // 1 pt = 1.333 px

        public TextMeasureService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_isInitialized)
            {
                // Загружаем JS модуль для измерений
                _measurementModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", 
                    "./js/text-measurement.js"
                );
                _isInitialized = true;
            }
        }

        public async Task<TextMeasurement> MeasureTextAsync(TextMeasurementOptions options)
        {
            await EnsureInitializedAsync();
            
            try
            {
                // Конвертируем параметры для JS
                var jsOptions = new
                {
                    text = options.Text,
                    fontFamily = options.FontFamily,
                    fontSize = options.FontSize * PT_TO_PX, // pt -> px
                    lineHeight = options.LineHeight,
                    maxWidth = options.MaxWidth * CM_TO_PX, // см -> px
                    maxHeight = options.MaxHeight * CM_TO_PX, // см -> px
                    fontWeight = options.FontWeight,
                    fontStyle = options.FontStyle,
                    padding = options.Padding
                };

                var result = await _measurementModule!.InvokeAsync<TextMeasurement>(
                    "measureText",
                    jsOptions
                );
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка измерения текста: {ex.Message}");
                
                // Fallback: приблизительный расчет
                return CalculateFallbackMeasurement(options);
            }
        }

        public async Task<int> FindBreakPointAsync(string text, double availableHeightCm, double columnWidthCm, bool isFirstPage = true)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            await EnsureInitializedAsync();

            try
            {
                // Создаем заголовок для первой страницы (если нужно)
                string textToMeasure = text;
                double headerHeight = 0;
                
                if (isFirstPage)
                {
                    // Добавляем заголовок статьи (примерно 3 строки)
                    headerHeight = 3.0 * 12 * PT_TO_PX * 1.2 / CM_TO_PX; // 3 строки * размер шрифта * межстрочный
                }

                double availableHeightPx = (availableHeightCm * CM_TO_PX) - (headerHeight * CM_TO_PX);
                
                var jsOptions = new
                {
                    text = textToMeasure,
                    fontFamily = "'Times New Roman', serif",
                    fontSize = 12 * PT_TO_PX,
                    lineHeight = 1.2,
                    maxWidth = columnWidthCm * CM_TO_PX,
                    maxHeight = availableHeightPx,
                    findBreakPoint = true
                };

                var result = await _measurementModule!.InvokeAsync<dynamic>(
                    "findTextBreakPoint",
                    jsOptions
                );

                return result.breakIndex;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка поиска точки разрыва: {ex.Message}");
                return FindBreakPointFallback(text, availableHeightCm, columnWidthCm);
            }
        }

        public async Task<double> CalculateTextHeightAsync(string text, double columnWidthCm)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            var options = new TextMeasurementOptions
            {
                Text = text,
                MaxWidth = columnWidthCm,
                MaxHeight = 0 // Без ограничения по высоте
            };

            var measurement = await MeasureTextAsync(options);
            return measurement.Height / CM_TO_PX; // Возвращаем в см
        }

        private TextMeasurement CalculateFallbackMeasurement(TextMeasurementOptions options)
        {
            // Приблизительный расчет (используется при ошибках JS)
            double charWidth = 7.0; // примерная ширина символа в px при 12pt
            double lineHeight = 12 * options.LineHeight * PT_TO_PX;
            
            double maxWidthPx = options.MaxWidth * CM_TO_PX;
            int charsPerLine = (int)(maxWidthPx / charWidth);
            
            if (charsPerLine <= 0) charsPerLine = 1;
            
            int lineCount = (int)Math.Ceiling((double)options.Text.Length / charsPerLine);
            double heightPx = lineCount * lineHeight;
            
            return new TextMeasurement
            {
                Width = maxWidthPx,
                Height = heightPx,
                LineCount = lineCount,
                CharactersThatFit = options.Text.Length
            };
        }

        private int FindBreakPointFallback(string text, double availableHeightCm, double columnWidthCm)
        {
            // Fallback алгоритм (улучшенная версия вашего FindGoodBreakPoint)
            double cmPerChar = 0.05; // Эмпирический коэффициент: 0.05 см на символ
            double lineHeightCm = 0.45;
            
            int charsPerLine = (int)(columnWidthCm / cmPerChar);
            int linesPerPage = (int)(availableHeightCm / lineHeightCm);
            int maxCharsPerPage = charsPerLine * linesPerPage;
            
            if (text.Length <= maxCharsPerPage)
                return text.Length;
            
            // Ищем хорошую точку разрыва
            return FindGoodBreakPoint(text, maxCharsPerPage);
        }

        private int FindGoodBreakPoint(string text, int maxChars)
        {
            if (text.Length <= maxChars) 
                return text.Length;
            
            // Ищем конец абзаца
            int paragraphBreak = text.LastIndexOf("\n\n", Math.Min(maxChars, text.Length - 2));
            if (paragraphBreak > maxChars * 0.7)
                return paragraphBreak + 2;
            
            // Ищем конец предложения
            int sentenceBreak = Math.Max(
                text.LastIndexOf(". ", Math.Min(maxChars, text.Length - 2)),
                Math.Max(
                    text.LastIndexOf("! ", Math.Min(maxChars, text.Length - 2)),
                    text.LastIndexOf("? ", Math.Min(maxChars, text.Length - 2))
                )
            );
            
            if (sentenceBreak > maxChars * 0.7)
                return sentenceBreak + 2;
            
            // Ищем ближайший пробел
            int spaceBreak = text.LastIndexOf(' ', Math.Min(maxChars, text.Length - 1));
            if (spaceBreak > maxChars * 0.7)
                return spaceBreak + 1;
            
            return maxChars;
        }

        public async ValueTask DisposeAsync()
        {
            if (_measurementModule != null)
            {
                await _measurementModule.DisposeAsync();
            }
        }
    }
}