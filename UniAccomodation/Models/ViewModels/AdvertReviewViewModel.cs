using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UniAccomodation.Models.ViewModels
{
    public class AdvertReviewViewModel
    {
        [Required]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string PhotoUrl { get; set; }

        public decimal MonthlyPrice { get; set; }

        public AdvertStatus Status { get; set; }

        [Required]
        [MinLength(30, ErrorMessage = "The comments field must be at least 30 characters long")]
        [MaxLength(100, ErrorMessage = "The comments field must be at most 100 characters long")]
        public string Comments { get; set; }

        [Required]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm the review")]
        public bool Confirm { get; set; } = false;
    }
}
