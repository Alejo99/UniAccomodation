using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UniAccomodation.Models
{
    public enum AdvertStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public class Advert
    {
        public int Id { get; set; }

        [Required]
        [MinLength(10)]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MinLength(30)]
        [MaxLength(100)]
        public string Description { get; set; }

        public string PhotoUrl { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Range(0, 4000)]
        public decimal MonthlyPrice { get; set; }

        [Required]
        public AdvertStatus Status { get; set; } = AdvertStatus.Pending;

        [MinLength(30)]
        [MaxLength(100)]
        public string Comments { get; set; }

        #region Relationships with other entities

        public int LandlordId { get; set; }
        public Landlord Landlord { get; set; }

        public int? OfficerId { get; set; }
        public AccomodationOfficer Officer { get; set; }

        #endregion

        public Advert() { }

        public bool Review(AdvertStatus status, string comments, int officerId)
        {
            if(Status == AdvertStatus.Pending)
            {
                Status = status;
                Comments = comments;
                OfficerId = officerId;
                return true;
            }
            return false;
        }
    }
}
