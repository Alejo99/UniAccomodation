using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UniAccomodation.Models
{
    public class ApplicationUser : IdentityUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicationId { get; set; }
    }

    public class Landlord : ApplicationUser
    {
        public ICollection<Advert> Adverts { get; set; }
    }

    public class Student : ApplicationUser
    {
        public string Program { get; set; }

        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "Please provide a valid 8 digit student ID number")]
        public string StudentId { get; set; }
    }

    public class AccomodationOfficer : ApplicationUser
    {
        public string OfficeLocation { get; set; }

        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^([0-9]{9,11})$", ErrorMessage = "Please provide a valid phone number (9 to 11 digits)")]
        public string OfficePhone { get; set; }
    }
}
