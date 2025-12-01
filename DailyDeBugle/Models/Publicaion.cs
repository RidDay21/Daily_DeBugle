using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

        [Required(ErrorMessage = "Publication name is required.")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        ///Сделал типизацию строгую
        /// </summary>
        public PublicationFrequency Frequency { get; set; } 
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [ValidateNever]
        public List<Issue> Issues { get; set; } = new();

        [ValidateNever]
        public List<User> Editors { get; set; } = new();
    }
}