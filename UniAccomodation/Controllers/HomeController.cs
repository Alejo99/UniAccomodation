using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UniAccomodation.Models;
using UniAccomodation.Data;
using UniAccomodation.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using UniAccomodation.Models.ViewModels;

namespace UniAccomodation.Controllers
{
    public class HomeController : Controller
    {
        private IAdvertRepository advertRepository;
        private readonly MyPagingOptions pagingOptions;

        public HomeController(IAdvertRepository advertRepo, 
            IOptions<MyPagingOptions> pagOptions)
        {
            advertRepository = advertRepo;
            pagingOptions = pagOptions.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "More about our service";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Adverts(int pag = 1)
        {
            var advertListVM = new AdvertListViewModel();
            advertListVM.Adverts = advertRepository.Adverts
                .Where(adv => adv.Status == AdvertStatus.Approved)
                .Include(adv => adv.Landlord)
                .OrderByDescending(adv => adv.Id)
                .Skip((pag - 1) * pagingOptions.PageSize)
                .Take(pagingOptions.PageSize);
            // Pagination
            advertListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = pag,
                ItemsPerPage = pagingOptions.PageSize,
                TotalItems = advertRepository.Adverts
                    .Where(adv => adv.Status == AdvertStatus.Approved)
                    .Count()
            };
            advertListVM.Status = AdvertStatus.Approved;
            return View(advertListVM);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
