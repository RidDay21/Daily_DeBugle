namespace DailyDeBugle.Models
{
    /// <summary>
    /// Класс енум для строгой типизации нашей периодичности
    /// </summary>
    public enum PublicationFrequency
    {
        Daily,
        Weekly,
        Monthly
    }

    public class Publication
    {
        public int PublicationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        /// <summary>
        ///Сделал типизацию строгую
        /// </summary>
        public PublicationFrequency Frequency { get; set; } 
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Навигационные свойства
        public List<Issue> Issues { get; set; } = new();
        public List<User> Editors { get; set; } = new();
    }
}