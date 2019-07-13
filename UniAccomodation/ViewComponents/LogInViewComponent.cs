using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UniAccomodation.Models;

namespace UniAccomodation.ViewComponents
{
    public class LogInViewComponent : ViewComponent
    {
        private SignInManager<ApplicationUser> signInManager;
        private UserManager<ApplicationUser> userManager;

        public LogInViewComponent(SignInManager<ApplicationUser> signInMgr, UserManager<ApplicationUser> userMgr)
        {
            signInManager = signInMgr;
            userManager = userMgr;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (signInManager.IsSignedIn(HttpContext.User))
            {
                var user = await userManager.GetUserAsync(HttpContext.User);
                return View("LoggedIn", user);
            }
            else
            {
                return View();
            }
        }
    }
}
