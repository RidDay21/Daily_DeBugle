using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DailyDeBugle.Models
{
    public class Publication
    {
        public int PublicationId { get; set; }

        [Required(ErrorMessage = "Publication name is required.")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [ValidateNever]
        public List<Issue> Issues { get; set; } = new();

        [ValidateNever]
        public List<User> Editors { get; set; } = new();
    }
}