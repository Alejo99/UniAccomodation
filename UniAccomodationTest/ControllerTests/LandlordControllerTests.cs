using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UniAccomodation.Configuration;
using UniAccomodation.Controllers;
using UniAccomodation.Data;
using UniAccomodation.Models;
using UniAccomodation.Models.ViewModels;
using Xunit;

namespace UniAccomodationTest.ControllerTests
{
    public class LandlordControllerFixture : IDisposable
    {
        public LandlordController Controller { get; private set; }
        public Mock<IAdvertRepository> MockRepo { get; private set; }
        public Mock<IAuthorizationService> MockAuthSrv { get; private set; }

        public LandlordControllerFixture()
        {
            //Mock DbSet<adverts>
            var adverts = new List<Advert>()
            {
                new Advert
                {
                    Id = 1,
                    Title = "Advert 1",
                    Description = "Advert description 1",
                    Status = AdvertStatus.Approved,
                    LandlordId = 1,
                    OfficerId = 888
                },
                new Advert
                {
                    Id = 2,
                    Title = "Advert 2",
                    Description = "Advert description 2",
                    Status = AdvertStatus.Approved,
                    LandlordId = 2,
                    OfficerId = 999
                },
                new Advert
                {
                    Id = 3,
                    Title = "Advert 3",
                    Description = "Advert description 3",
                    Status = AdvertStatus.Approved,
                    LandlordId = 1,
                    OfficerId = 999
                },
                new Advert
                {
                    Id = 4,
                    Title = "Advert 4",
                    Description = "Advert description 4",
                    Status = AdvertStatus.Approved,
                    LandlordId = 1,
                    OfficerId = 999
                },
                new Advert
                {
                    Id = 5,
                    Title = "Advert 5",
                    Description = "Advert description 5",
                    Status = AdvertStatus.Rejected,
                    LandlordId = 1,
                    OfficerId = 999
                },
                new Advert
                {
                    Id = 6,
                    Title = "Advert 6",
                    Description = "Advert description 6",
                    Status = AdvertStatus.Rejected,
                    LandlordId = 1,
                    OfficerId = 999
                },
                new Advert
                {
                    Id = 7,
                    Title = "Advert 7",
                    Description = "Advert description 7",
                    Status = AdvertStatus.Rejected,
                    LandlordId = 1,
                    OfficerId = 999
                },
                new Advert
                {
                    Id = 8,
                    Title = "Advert 8",
                    Description = "Advert description 8",
                    Status = AdvertStatus.Pending,
                    LandlordId = 1
                },
                new Advert
                {
                    Id = 9,
                    Title = "Advert 9",
                    Description = "Advert description 9",
                    Status = AdvertStatus.Pending,
                    LandlordId = 1
                },
                new Advert
                {
                    Id = 10,
                    Title = "Advert 10",
                    Description = "Advert description 10",
                    Status = AdvertStatus.Pending,
                    LandlordId = 2
                }
            }.AsQueryable();

            Mock<DbSet<Advert>> mockAdverts = new Mock<DbSet<Advert>>();
            mockAdverts.As<IQueryable<Advert>>().Setup(m => m.Provider).Returns(adverts.Provider);
            mockAdverts.As<IQueryable<Advert>>().Setup(m => m.Expression).Returns(adverts.Expression);
            mockAdverts.As<IQueryable<Advert>>().Setup(m => m.ElementType).Returns(adverts.ElementType);
            mockAdverts.As<IQueryable<Advert>>().Setup(m => m.GetEnumerator()).Returns(adverts.GetEnumerator());

            //Mock advert repository
            MockRepo = new Mock<IAdvertRepository>();
            MockRepo.Setup(m => m.Adverts).Returns(mockAdverts.Object);

            //Mock ihostingenvironment
            Mock<IHostingEnvironment> mockIHostingEnv = new Mock<IHostingEnvironment>();
            mockIHostingEnv.Setup(env => env.WebRootPath).Returns(ApplicationEnvironment.ApplicationBasePath);

            //Mock iauthorizatoinservice
            MockAuthSrv = new Mock<IAuthorizationService>();
            MockAuthSrv.Setup(auth => auth.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), 
                It.Is<Advert>(adv => adv.LandlordId == 1),
                "CanAccessAdvert"))
                .ReturnsAsync(AuthorizationResult.Success());

            MockAuthSrv.Setup(auth => auth.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.Is<Advert>(adv => adv.LandlordId != 1),
                "CanAccessAdvert"))
                .ReturnsAsync(AuthorizationResult.Failed());

            //Mock pagingoptions
            Mock<MyPagingOptions> mockPagingOpts = new Mock<MyPagingOptions>();
            mockPagingOpts.SetupGet(po => po.PageSize).Returns(3);

            Mock<IOptions<MyPagingOptions>> mockIOptions = new Mock<IOptions<MyPagingOptions>>();
            mockIOptions.Setup(m => m.Value).Returns(mockPagingOpts.Object);

            //Mock current user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.Name, "ALandlord"),
                 new Claim("UniAccomodationId", "1")
            }));

            //Mock tempdata
            Mock<ITempDataDictionary> mockTempData = new Mock<ITempDataDictionary>();

            //Create controller
            Controller = new LandlordController(MockRepo.Object, 
                mockIHostingEnv.Object, 
                MockAuthSrv.Object, 
                mockIOptions.Object)
            {
                TempData = mockTempData.Object
            };

            Controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        public void Dispose()
        {
            Controller.Dispose();
            //TODO: remove img created by upload test case
        }

    }

    public class LandlordControllerTests : IClassFixture<LandlordControllerFixture>
    {
        public LandlordController Controller { get; private set; }
        public Mock<IAdvertRepository> Repository { get; set; }
        public Mock<IAuthorizationService> IAuthService { get; set; }

        public LandlordControllerTests(LandlordControllerFixture fixture)
        {
            Controller = fixture.Controller;
            Repository = fixture.MockRepo;
            IAuthService = fixture.MockAuthSrv;
        }

        private T GetViewModel<T>(IActionResult result) where T : class
        {
            return (result as ViewResult)?.ViewData?.Model as T;
        }

        [Fact]
        public void CanListAllCurrentUserAdverts()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertListViewModel result = GetViewModel<AdvertListViewModel>(Controller.Index());

            //Assert
            Assert.Equal(8, result.PagingInfo.TotalItems);
            Assert.Equal("Advert 9", result.Adverts.First()?.Title);
            Assert.Equal("Advert 7", result.Adverts.Last()?.Title);
        }

        [Fact]
        public void CanPaginateAndOrderResults()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertListViewModel result = GetViewModel<AdvertListViewModel>(Controller.Index(2));

            //Assert
            Assert.Equal(3, result.Adverts.Count());
            var advList = result.Adverts.ToList();
            Assert.Equal("Advert 6", advList.First()?.Title);
            Assert.Equal("Advert 4", advList.Last()?.Title);
            Assert.Equal("Advert 5", advList.ElementAt(1)?.Title);
        }

        [Fact]
        public void CanSendPaginationViewModel()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertListViewModel result = GetViewModel<AdvertListViewModel>(Controller.Index(2));

            //Assert
            PagingInfo pageInfo = result.PagingInfo;
            Assert.Equal(2, pageInfo.CurrentPage);
            Assert.Equal(3, pageInfo.ItemsPerPage);
            Assert.Equal(8, pageInfo.TotalItems);
            Assert.Equal(3, pageInfo.TotalPages);
        }

        [Fact]
        public void CanFilterAdvertsByStatusAndCurrentUser()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertListViewModel res1 = GetViewModel<AdvertListViewModel>(Controller.Adverts(1, AdvertStatus.Approved));
            AdvertListViewModel res2 = GetViewModel<AdvertListViewModel>(Controller.Adverts(1, AdvertStatus.Rejected));

            //Assert
            Assert.Equal(3, res1.Adverts.Count());
            Assert.Equal(3, res2.Adverts.Count());

            var advList1 = res1.Adverts.ToList();
            var advList2 = res2.Adverts.ToList();

            Assert.Equal("Advert 4", advList1.First()?.Title);
            Assert.Equal("Advert 1", advList1.Last()?.Title);
            Assert.Equal("Advert 3", advList1.ElementAt(1)?.Title);

            Assert.Equal("Advert 7", advList2.First()?.Title);
            Assert.Equal("Advert 5", advList2.Last()?.Title);
            Assert.Equal("Advert 6", advList2.ElementAt(1)?.Title);
        }

        [Fact]
        public async void CanRetrieveOwnedAdvert()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertViewModel result = GetViewModel<AdvertViewModel>(await Controller.Advert(7));

            //Assert
            Assert.Equal("Advert 7", result.Title);
            Assert.Equal(7, result.Id);
            IAuthService.Verify(auth => auth.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.Is<Advert>(adv => adv.Id == 7),
                "CanAccessAdvert"), Times.Once);
        }

        [Fact]
        public async void CannotRetrieveNotOwnedAdvert()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            IActionResult result = await Controller.Advert(2);

            //Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDenied", (result as RedirectToActionResult).ActionName);
            Assert.Equal("Account", (result as RedirectToActionResult).ControllerName);
            IAuthService.Verify(auth => auth.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.Is<Advert>(adv => adv.Id == 2),
                "CanAccessAdvert"), Times.Once);
        }

        [Fact]
        public async void CannotRetrieveNonExistingAdvert()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            IActionResult result = await Controller.Advert(77);

            //Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
            IAuthService.Verify(auth => auth.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.Is<Advert>(adv => adv.Id == 77),
                "CanAccessAdvert"), Times.Never);
        }

        [Fact]
        public void CanSeeNewAdvertForm()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertViewModel result = GetViewModel<AdvertViewModel>(Controller.NewAdvert());

            //Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
        }

        [Fact]
        public async void CanCreateNewAdvert()
        {
            //Arrange
            // controller arranged from the controller fixture
            AdvertViewModel advert = new AdvertViewModel()
            {
                Id = 0,
                Title = "New advert for landlord 1",
                Description = "This is a new advert for the landlord 1, description should be 30 chars long.",
                MonthlyPrice = 550M
            };

            //Act
            IActionResult result = await Controller.Advert(advert);

            //Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
            Repository.Verify(r => r.SaveAdvert(It.Is<Advert>(adv => adv.Title == "New advert for landlord 1")), Times.AtLeastOnce);
            IAuthService.Verify(auth => auth.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.Is<Advert>(adv => adv.Title == "New advert for landlord 1"),
                "CanAccessAdvert"), Times.Never);
        }

        [Fact]
        public async void CanEditOwnedAdvert()
        {
            //Arrange
            // controller arranged from the controller fixture
            AdvertViewModel advert = new AdvertViewModel
            {
                Id = 8,
                Title = "Advert 8",
                Description = "Advert description 8, description is updated by landlord.",
                MonthlyPrice = 456M
            };

            //Act
            IActionResult result = await Controller.Advert(advert);

            //Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
            IAuthService.Verify(auth => auth.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.Is<Advert>(adv => adv.Id == 8),
                "CanAccessAdvert"), Times.Once);
            Repository.Verify(r => r.SaveAdvert(It.Is<Advert>(adv => adv.Id == 8)), Times.AtLeastOnce);
        }

        [Fact]
        public async void CannotEditNotOwnedAdvert()
        {
            //Arrange
            // controller arranged from the controller fixture
            AdvertViewModel advert = new AdvertViewModel
            {
                Id = 10,
                Title = "Advert 10",
                Description = "Advert description 10, trying to modify from another landlord",
                MonthlyPrice = 111M
            };
            //Mock current user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.Name, "ALandlord"),
                 new Claim("UniAccomodationId", "1")
            }));

            //Act
            IActionResult result = await Controller.Advert(advert);

            //Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDenied", (result as RedirectToActionResult).ActionName);
            Assert.Equal("Account", (result as RedirectToActionResult).ControllerName);
            IAuthService.Verify(auth => auth.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.Is<Advert>(adv => adv.Id == 10),
                "CanAccessAdvert"), Times.Once);
            Repository.Verify(r => r.SaveAdvert(It.Is<Advert>(adv => adv.Id == 10)), Times.Never);
        }

        [Fact]
        public async void CanUploadPhoto()
        {
            //Arrange
            // controller arranged from the controller fixture
            // mock iformfile
            Mock<IFormFile> file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns("aphoto.jpg");
            file.Setup(f => f.Length).Returns(1024);
            // advertview model
            AdvertViewModel advert = new AdvertViewModel
            {
                Id = 9,
                Title = "Advert 9",
                Description = "Advert description 9, photo upload",
                MonthlyPrice = 456M,
                Photo = file.Object
            };

            //Act
            IActionResult result = await Controller.Advert(advert);

            //Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
            IAuthService.Verify(auth => auth.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(),
                It.Is<Advert>(adv => adv.Id == 9),
                "CanAccessAdvert"), Times.Between(1, 2, Range.Inclusive));
            Repository.Verify(r => r.SaveAdvert(It.Is<Advert>(adv => adv.Id == 9 && !string.IsNullOrEmpty(adv.PhotoUrl))), Times.Between(1, 2, Range.Inclusive));
        }
        
    }
}
