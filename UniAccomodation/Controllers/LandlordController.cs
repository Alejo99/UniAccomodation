using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UniAccomodation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UniAccomodation.Models;
using UniAccomodation.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;
using UniAccomodation.Configuration;
using Microsoft.Extensions.Options;

namespace UniAccomodation.Controllers
{
    [Authorize(Roles = "Landlord")]
    public class LandlordController : Controller
    {
        private IAdvertRepository advertRepository;
        private IHostingEnvironment hostingEnvironment;
        private readonly IAuthorizationService authorizationService;
        private readonly MyPagingOptions pagingOptions;

        public LandlordController(IAdvertRepository advertRepo, 
            IHostingEnvironment environment,
            IAuthorizationService authService,
            IOptions<MyPagingOptions> pagOptions)
        {
            advertRepository = advertRepo;
            hostingEnvironment = environment;
            authorizationService = authService;
            pagingOptions = pagOptions.Value;
        }

        // List of all the adverts
        public IActionResult Index(int pag = 1)
        {
            // Get application Id from current user
            var userId = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "UniAccomodationId")?.Value;
            // Get adverts of the current landlord
            // Rely on claims transformation to ensure userId is not null
            var advertListVM = new AdvertListViewModel();
            advertListVM.Adverts = advertRepository.Adverts
                .Where(adv => adv.LandlordId == Int32.Parse(userId))
                .OrderByDescending(adv => adv.Id)
                .Skip((pag - 1) * pagingOptions.PageSize)
                .Take(pagingOptions.PageSize);
            // Pagination
            advertListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = pag,
                ItemsPerPage = pagingOptions.PageSize,
                TotalItems = advertRepository.Adverts
                    .Where(adv => adv.LandlordId == Int32.Parse(userId))
                    .Count()
            };
            advertListVM.Status = null;
            return View(advertListVM);
        }

        // List of adverts by status
        public IActionResult Adverts(int pag = 1, AdvertStatus status = AdvertStatus.Approved)
        {
            // Get application Id from current user
            var userId = HttpContext.User.Claims
                .FirstOrDefault(c => c.Type == "UniAccomodationId")?.Value;
            // Get adverts from current landlord by status
            var advertListVM = new AdvertListViewModel();
            advertListVM.Adverts = advertRepository.Adverts
                .Where(adv => 
                    adv.LandlordId == Int32.Parse(userId) && 
                    adv.Status == status)
                .OrderByDescending(adv => adv.Id)
                .Skip((pag - 1) * pagingOptions.PageSize)
                .Take(pagingOptions.PageSize);
            // Pagination
            advertListVM.PagingInfo = new PagingInfo
            {
                CurrentPage = pag,
                ItemsPerPage = pagingOptions.PageSize,
                TotalItems = advertRepository.Adverts
                    .Where(adv => adv.LandlordId == Int32.Parse(userId) && adv.Status == status)
                    .Count()
            };
            advertListVM.Status = status;
            return View(advertListVM);
        }

        // Advert details 
        public async Task<IActionResult> Advert(int id)
        {
            var advert = advertRepository.Adverts
                .FirstOrDefault(adv => adv.Id == id);
            if(advert != null)
            {
                //Authorize landlord
                var isAuthorized = await authorizationService.AuthorizeAsync(User, advert, "CanAccessAdvert");
                if (isAuthorized.Succeeded)
                {
                    return View(new AdvertViewModel()
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
                else
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
            }
            return RedirectToAction("Index");
        }

        // Create/save advert details
        [HttpPost]
        public async Task<IActionResult> Advert(AdvertViewModel advert)
        {
            #region Existing advert
            // Check if advert exists
            var adv = advertRepository.Adverts.FirstOrDefault(a => a.Id == advert.Id);
            if (adv != null)
            {
                //Authorize landlord
                var isAuthorized = await authorizationService.AuthorizeAsync(User, adv, "CanAccessAdvert");
                if (!isAuthorized.Succeeded)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }
                //Check advert status
                if (adv.Status == AdvertStatus.Approved || adv.Status == AdvertStatus.Rejected)
                {
                    ModelState.AddModelError("Status", "This advert has already been reviewed and can not be changed.");
                }
            }
            #endregion
            var photoIsValid = PhotoIsValid(advert.Photo);
            if (ModelState.IsValid)
            {
                // Get application Id from current user
                var userId = HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == "UniAccomodationId")?.Value;
                // Create/overwrite advert
                var newAdvert = new Advert()
                {
                    Id = advert.Id,
                    Title = advert.Title,
                    Description = advert.Description,
                    MonthlyPrice = advert.MonthlyPrice,
                    LandlordId = Int32.Parse(userId),
                    PhotoUrl = (adv != null) 
                        ? adv.PhotoUrl 
                        : string.Empty,
                    Status = (adv != null) 
                        ? adv.Status 
                        : AdvertStatus.Pending
                };
                // Save advert
                advertRepository.SaveAdvert(newAdvert);
                // Upload photo and update advert photo url if valid
                if(photoIsValid)
                {
                    newAdvert.PhotoUrl = await SavePhoto(advert.Photo, userId, newAdvert.Id.ToString());
                    advertRepository.SaveAdvert(newAdvert);
                }
                TempData["Message"] = $"Advert \"{advert.Title}\" was saved";
                return RedirectToAction("Index");
            }
            return View(advert);
        }

        /// <summary>
        /// Checks if a IFormFile is a valid photograph: extension is jpg and file is not empty.
        /// If the photo is not valid, adds a model error to the current ModelState
        /// </summary>
        /// <param name="photo">The IFormFile to check</param>
        /// <returns>true if the file is valid, false otherwise</returns>
        private bool PhotoIsValid(IFormFile photo)
        {
            var valid = false;
            if (photo != null && photo.Length > 0)
            {
                var ext = Path.GetExtension(photo.FileName);
                if (ext != ".jpg")
                {
                    ModelState.AddModelError("Photo", "Incorrect file extension. The uploaded file must a jpg file");
                }
                else
                {
                    valid = true;
                }
            }
            return valid;
        }

        /// <summary>
        /// Saves a landlord photo and returns the URL where it can be found.
        /// </summary>
        /// <param name="photo">The IFormFile to upload</param>
        /// <param name="userId">The landlord id that owns the photo</param>
        /// <returns>A relative URL path where the photo can be found</returns>
        private async Task<string> SavePhoto(IFormFile photo, string userId, string advertId)
        {
            var advertPath = Path.Combine("images", "AdvertPhotos", userId, advertId);
            var directory = Path.Combine(hostingEnvironment.WebRootPath, advertPath);
            Directory.CreateDirectory(directory);
            var upload = Path.Combine(directory, "adv.jpg");
            using (var fileStream = new FileStream(upload, FileMode.Create))
            {
                await photo.CopyToAsync(fileStream);
            }
            return "/images/AdvertPhotos/" + userId + "/" + advertId  + "/adv.jpg";
        }

        // Add an advert
        public IActionResult NewAdvert()
        {
            return View("Advert", new AdvertViewModel());
        }
    }
}
