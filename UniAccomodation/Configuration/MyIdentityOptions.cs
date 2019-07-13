using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniAccomodation.Configuration
{
    public class MyIdentityOptions
    {
        public AdminEntry AdminUser { get; set; }
        public LandlordEntry[] Landlords { get; set; }

        public MyIdentityOptions()
        {
            AdminUser = new AdminEntry();
        }
    }

    public class AdminEntry
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public AdminEntry()
        {
            Name = "Admin";
            Email = "admin@admin.com";
            Password = "secret";
            Role = "Admin";
        }
    }

    public class LandlordEntry
    {
        public string Name { get; set; }
        public string Email{ get; set; }
        public string Password{ get; set; }
        public AdvertEntry[] Adverts { get; set; }

        public LandlordEntry()
        {
            Name = "Landlord";
            Email = "landlord@example.com";
            Password = "secret";
        }
}

    public class AdvertEntry
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal MonthlyPrice { get; set; }

        public AdvertEntry()
        {
            Title = "Advert title";
            Description = "Advert description";
            MonthlyPrice = 100;
        }
    }
}
