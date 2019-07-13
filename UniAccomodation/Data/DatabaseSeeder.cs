using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniAccomodation.Configuration;
using UniAccomodation.Models;

namespace UniAccomodation.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider, 
            MyIdentityOptions options, 
            UniAccomodationDbContext context)
        {
            // If there are no users, seed the database
            if (!context.Users.Any())
            {
                UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                // 1. Create roles
                //Users
                if (!await roleManager.RoleExistsAsync("User"))
                {
                    await roleManager.CreateAsync(new IdentityRole("User"));
                }

                //Landlords
                if (!await roleManager.RoleExistsAsync("Landlord"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Landlord"));
                }

                //Admins
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // 2. Create Admin account
                string username = options.AdminUser.Name;
                string email = options.AdminUser.Email;
                string password = options.AdminUser.Password;
                string role = options.AdminUser.Role;

                if (await userManager.FindByNameAsync(username) == null)
                {
                    if (await roleManager.FindByNameAsync(role) == null)
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                    AccomodationOfficer admin = new AccomodationOfficer
                    {
                        UserName = username,
                        Email = email
                    };
                    IdentityResult result = await userManager.CreateAsync(admin, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(admin, role);
                    }
                }

                // 3. Create some Landlords
                foreach (var lnd in options.Landlords)
                {
                    var landlord = new Landlord()
                    {
                        UserName = lnd.Name,
                        Email = lnd.Email
                    };
                    IdentityResult result = await userManager.CreateAsync(landlord, lnd.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(landlord, "Landlord");
                        // 4. Create some adverts for each landlord
                        foreach (var adv in lnd.Adverts)
                        {
                            var advert = new Advert()
                            {
                                Title = adv.Title,
                                Description = adv.Description,
                                MonthlyPrice = adv.MonthlyPrice,
                                Landlord = landlord
                            };
                            context.Adverts.Add(advert);
                        }
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}
