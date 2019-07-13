using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniAccomodation.Configuration;
using UniAccomodation.Data;
using UniAccomodation.Models;
using UniAccomodation.Models.ViewModels;

namespace UniAccomodation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OfficerController : Controller
    {
        private IAdvertRepository advertRepository;
        private readonly MyPagingOptions pagingOptions;

        public OfficerController (IAdvertRepository advRepository,
            IOptions<MyPagingOptions> pagOptions)
        {
            advertRepository = advRepository;
            pagingOptions = pagOptions.Value;
        }

        // All Pending adverts
        public IActionResult Index(int pag = 1)
        {
            // List all pending adverts
            var advertListVM = new AdvertListViewModel();
            advertListVM.Adverts = advertRepository.Adverts
                .Where(adv =>adv.Status == AdvertStatus.Pending)
                .OrderByDescending(adv => adv.Id)
                .Skip((pag - 1) * pagingOptions.PageSize)
                .Take(pagingOptions.PageSize);
            // Pagination
            advertListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = pag,
                ItemsPerPage = pagingOptions.PageSize,
                TotalItems = advertRepository.Adverts
                    .Where(adv => adv.Status == AdvertStatus.Pending)
                    .Count()
            };
            advertListVM.Status = AdvertStatus.Pending;
            return View(advertListVM);
        }

        public IActionResult Adverts(int pag = 1, AdvertStatus status = AdvertStatus.Approved)
        {
            // Get application Id from current user
            var userId = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "UniAccomodationId")?.Value;
            // Get approved adverts by the current officer
            // Rely on claims transformation to ensure userId is not null
            var advertListVM = new AdvertListViewModel();
            advertListVM.Adverts = advertRepository.Adverts
                .Where(adv => adv.OfficerId == int.Parse(userId) && adv.Status == status)
                .OrderByDescending(adv => adv.Id)
                .Skip((pag - 1) * pagingOptions.PageSize)
                .Take(pagingOptions.PageSize);
            // Pagination
            advertListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = pag,
                ItemsPerPage = pagingOptions.PageSize,
                TotalItems = advertRepository.Adverts
                    .Where(adv => adv.OfficerId == int.Parse(userId) && adv.Status == status)
                    .Count()
            };
            advertListVM.Status = status;
            return View("Adverts", advertListVM);
        }

        public IActionResult Advert(int id)
        {
            var advert = advertRepository.Adverts
                .FirstOrDefault(adv => adv.Id == id);
            if (advert != null)
            {
                return View(new AdvertReviewViewModel()
                {
                    Id = advert.Id,
                    Title = advert.Title,
                    Description = advert.Description,
                    MonthlyPrice = advert.MonthlyPrice,
                    PhotoUrl = advert.PhotoUrl,
                    Status = advert.Status,
                    Comments = advert.Comments,
                    Confirm = false
                });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Advert(AdvertReviewViewModel advReview)
        {
            // check if confirm = true, otherwise modelstate is not valid
            if (!advReview.Confirm)
            {
                ModelState.AddModelError("Confirm", "You must confirm the review before submitting.");
            }
            var advert = advertRepository.Adverts.
                    FirstOrDefault(adv => adv.Id == advReview.Id);
            if(advert != null && (advert.Status == AdvertStatus.Approved || advert.Status == AdvertStatus.Rejected))
            {
                ModelState.AddModelError("Status", "This advert has already been reviewed");
            }
            if (ModelState.IsValid && advert != null)
            {
                // Get application Id from current user
                var userId = HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == "UniAccomodationId")?.Value;
                // update advert status and comments
                advert.Review(advReview.Status, advReview.Comments, Int32.Parse(userId));
                advertRepository.SaveAdvert(advert);
                // add message as advert was reviewed
                TempData["Message"] = $"Advert {advert.Id} has been reviewed. Status: {advert.Status}";
                // return to pending list (index action)
                return RedirectToAction("Index");
            }
            else
            {
                return View("Advert", new AdvertReviewViewModel()
                {
                    Id = advert.Id,
                    Title = advert.Title,
                    Description = advert.Description,
                    MonthlyPrice = advert.MonthlyPrice,
                    PhotoUrl = advert.PhotoUrl,
                    Status = advert.Status,
                    Comments = advert.Comments
                });
            }
        }
    }
}
