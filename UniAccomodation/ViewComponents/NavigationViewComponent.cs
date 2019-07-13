using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniAccomodation.Models;

namespace UniAccomodation.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        private Dictionary<string, NavLink> navLinks;
        private SignInManager<ApplicationUser> signInManager;
        private UserManager<ApplicationUser> userManager;

        public NavigationViewComponent(SignInManager<ApplicationUser> signInMgr, 
            UserManager<ApplicationUser> userMgr)
        {
            signInManager = signInMgr;
            userManager = userMgr;
            navLinks = new Dictionary<string, NavLink>();
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (signInManager.IsSignedIn(HttpContext.User))
            {
                var user = await userManager.GetUserAsync(HttpContext.User);
                if(await userManager.IsInRoleAsync(user, "Admin"))
                {
                    navLinks.Add("Review Adverts", new NavLink()
                    {
                        Controller = "Officer",
                        Action = "Index"
                    });
                    navLinks.Add("Approved by me", new NavLink()
                    {
                        Controller = "Officer",
                        Action = "Adverts",
                        RouteValues = new Dictionary<string, string>() { ["status"] = "Approved" }
                    });
                    navLinks.Add("Rejected by me", new NavLink()
                    {
                        Controller = "Officer",
                        Action = "Adverts",
                        RouteValues = new Dictionary<string, string>() { ["status"] = "Rejected" }
                    });
                }
                if (await userManager.IsInRoleAsync(user, "Landlord"))
                {
                    navLinks.Add("All adverts", new NavLink()
                    {
                        Controller = "Landlord",
                        Action = "Index"
                    });
                    navLinks.Add("Approved", new NavLink()
                    {
                        Controller = "Landlord",
                        Action = "Adverts"
                    });
                    navLinks.Add("Rejected", new NavLink()
                    {
                        Controller = "Landlord",
                        Action = "Adverts",
                        RouteValues = new Dictionary<string, string>() { ["status"] = "Rejected" }
                    });
                    navLinks.Add("Pending", new NavLink()
                    {
                        Controller = "Landlord",
                        Action = "Adverts",
                        RouteValues = new Dictionary<string, string>() { ["status"] = "Pending" }
                    });
                }
            }
            else
            {
                navLinks.Add("Home", new NavLink()
                {
                    Controller = "Home",
                    Action = "Index"
                });
                navLinks.Add("About", new NavLink()
                {
                    Controller = "Home",
                    Action = "About"
                });
                navLinks.Add("Contact", new NavLink()
                {
                    Controller = "Home",
                    Action = "Contact"
                });
            }
            return View(navLinks);
        }
    }

    public class NavLink
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public Dictionary<string, string> RouteValues { get; set; }
    }
}
