namespace DailyDeBugle.Models
{
    public class TextStyle
    {
        public string PrimaryFont { get; set; } = "Times New Roman";
        public string HeadingFont { get; set; } = "Times New Roman";
        
        // Размеры текста - для комфортного чтения
        public int H1Size { get; set; } = 24;      // Крупный заголовок
        public int H2Size { get; set; } = 20;      // Подзаголовок  
        public int BodySize { get; set; } = 14;    // Увеличенный для читаемости
        
        // Интервалы - от плотного до воздушного
        public double BodyLineSpacing { get; set; } = 1.4;      // Комфортное чтение
        public double HeadingLineSpacing { get; set; } = 1.2;   // Плотнее для заголовков
        
        // Колонки - стандартные газетные варианты
        public int ColumnCount { get; set; } = 2;      // Классическая газетная верстка
        public double ColumnGap { get; set; } = 1.0;   // Достаточное расстояние
    }
}