using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UniAccomodation.Models.ViewModels
{
    public class AdvertViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "The title must be at least 10 characters long")]
        [MaxLength(100, ErrorMessage = "The title must be at most 100 characters long")]
        public string Title { get; set; }

        [Required]
        [MinLength(30, ErrorMessage = "The description must be at least 30 characters long")]
        [MaxLength(100, ErrorMessage = "The description must be at most 100 characters long")]
        public string Description { get; set; }

        public string PhotoUrl { get; set; }

        [Required]
        [Display(Name = "Price per month")]
        [DataType(DataType.Currency)]
        [Range(1, 4000, ErrorMessage = "The price per month must be between 1 and 4000")]
        public decimal MonthlyPrice { get; set; }

        public AdvertStatus Status { get; set; } = AdvertStatus.Pending;

        [Display(Name = "Officer comments")]
        public string Comments { get; set; }

        public IFormFile Photo { get; set; }
    }
}
