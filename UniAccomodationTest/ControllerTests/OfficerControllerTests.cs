using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using UniAccomodation.Configuration;
using UniAccomodation.Controllers;
using UniAccomodation.Data;
using UniAccomodation.Models;
using UniAccomodation.Models.ViewModels;
using Xunit;

namespace UniAccomodationTest.ControllerTests
{
    public class OfficerControllerFixture : IDisposable
    {
        public OfficerController Controller { get; private set; }
        public Mock<IAdvertRepository> MockRepo { get; private set; }
        public OfficerControllerFixture()
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
                    OfficerId = 888
                },
                new Advert
                {
                    Id = 2,
                    Title = "Advert 2",
                    Description = "Advert description 2",
                    Status = AdvertStatus.Approved,
                    OfficerId = 999
                },
                new Advert
                {
                    Id = 3,
                    Title = "Advert 3",
                    Description = "Advert description 3",
                    Status = AdvertStatus.Approved,
                    OfficerId = 999
                },
                new Advert
                {
                    Id = 4,
                    Title = "Advert 4",
                    Description = "Advert description 4",
                    Status = AdvertStatus.Approved,
                    OfficerId = 999
                },
                new Advert
                {
                    Id = 5,
                    Title = "Advert 5",
                    Description = "Advert description 5",
                    Status = AdvertStatus.Rejected,
                    OfficerId = 999
                },
                new Advert
                {
                    Id = 6,
                    Title = "Advert 6",
                    Description = "Advert description 6",
                    Status = AdvertStatus.Rejected,
                    OfficerId = 999
                },
                new Advert
                {
                    Id = 7,
                    Title = "Advert 7",
                    Description = "Advert description 7",
                    Status = AdvertStatus.Pending
                },
                new Advert
                {
                    Id = 8,
                    Title = "Advert 8",
                    Description = "Advert description 8",
                    Status = AdvertStatus.Pending
                },
                new Advert
                {
                    Id = 9,
                    Title = "Advert 9",
                    Description = "Advert description 9",
                    Status = AdvertStatus.Pending
                },
                new Advert
                {
                    Id = 10,
                    Title = "Advert 10",
                    Description = "Advert description 10",
                    Status = AdvertStatus.Pending
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

            //Mock pagingoptions
            Mock<MyPagingOptions> mockPagingOpts = new Mock<MyPagingOptions>();
            mockPagingOpts.SetupGet(po => po.PageSize).Returns(3);

            Mock<IOptions<MyPagingOptions>> mockIOptions = new Mock<IOptions<MyPagingOptions>>();
            mockIOptions.Setup(m => m.Value).Returns(mockPagingOpts.Object);

            //Mock current user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.Name, "AnOfficer"),
                 new Claim("UniAccomodationId", "999")
            }));

            //Mock tempdata
            Mock<ITempDataDictionary> mockTempData = new Mock<ITempDataDictionary>();

            //Create controller
            Controller = new OfficerController(MockRepo.Object, mockIOptions.Object)
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
        }

    }

    public class OfficerControllerTests : IClassFixture<OfficerControllerFixture>
    {
        public OfficerController Controller { get; private set; }
        public Mock<IAdvertRepository> MockRepo { get; private set; }

        public OfficerControllerTests(OfficerControllerFixture controllerFixture)
        {
            Controller = controllerFixture.Controller;
            MockRepo = controllerFixture.MockRepo;
        }

        private T GetViewModel<T>(IActionResult result) where T : class
        {
            return (result as ViewResult)?.ViewData?.Model as T;
        }

        [Fact]
        public void CanListPendingAdverts()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertListViewModel result = GetViewModel<AdvertListViewModel>(Controller.Index(2));

            //Assert
            Assert.Equal(4, result.PagingInfo.TotalItems);
            Assert.Equal("Advert 7", result.Adverts.First()?.Title);
            Assert.Equal("Advert 7", result.Adverts.Last()?.Title);
        }

        [Fact]
        public void CanPaginateAndOrderResults()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertListViewModel result = GetViewModel<AdvertListViewModel>(Controller.Index());

            //Assert
            Assert.Equal(3, result.Adverts.Count());
            var advList = result.Adverts.ToList();
            Assert.Equal("Advert 10", advList.First()?.Title);
            Assert.Equal("Advert 8", advList.Last()?.Title);
            Assert.Equal("Advert 9", advList.ElementAt(1)?.Title);
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
            Assert.Equal(4, pageInfo.TotalItems);
            Assert.Equal(2, pageInfo.TotalPages);
        }

        [Fact]
        public void CanListApprovedAdvertsFilteredByCurrentUser()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertListViewModel result = GetViewModel<AdvertListViewModel>(Controller.Adverts(status: AdvertStatus.Approved));

            //Assert
            Assert.Equal(3, result.PagingInfo.TotalItems);
            Assert.Equal("Advert 4", result.Adverts.First()?.Title);
            Assert.Equal("Advert 2", result.Adverts.Last()?.Title);
        }

        [Fact]
        public void CanListRejectedAdvertsFilteredByCurrentUser()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertListViewModel result = GetViewModel<AdvertListViewModel>(Controller.Adverts(status: AdvertStatus.Rejected));

            //Assert
            Assert.Equal(2, result.PagingInfo.TotalItems);
            Assert.Equal("Advert 6", result.Adverts.First()?.Title);
            Assert.Equal("Advert 5", result.Adverts.Last()?.Title);
        }

        [Fact]
        public void CanViewPendingAdverts()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertReviewViewModel a1 = GetViewModel<AdvertReviewViewModel>(Controller.Advert(7));
            AdvertReviewViewModel a2 = GetViewModel<AdvertReviewViewModel>(Controller.Advert(8));
            AdvertReviewViewModel a3 = GetViewModel<AdvertReviewViewModel>(Controller.Advert(9));

            //Assert
            Assert.Equal(7, a1.Id);
            Assert.Equal(8, a2.Id);
            Assert.Equal(9, a3.Id);
        }

        [Fact]
        public void CannotReviewAprovedOrRejectedAdvert()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertReviewViewModel ad1 = new AdvertReviewViewModel()
            {
                Id = 1,
                Comments = "All ok in this advert. The comments should be at least 30 chars long.",
                Confirm = true,
                Status = AdvertStatus.Approved
            };
            AdvertReviewViewModel ad2 = new AdvertReviewViewModel()
            {
                Id = 5,
                Comments = "All ok in this advert. The comments should be at least 30 chars long.",
                Confirm = true,
                Status = AdvertStatus.Approved
            };
            var res1 = Controller.Advert(ad1);
            var res2 = Controller.Advert(ad2);

            //Assert
            // Edit was not called
            MockRepo.Verify(m => m.SaveAdvert(It.IsAny<Advert>()), Times.Never());
            // Result is ViewResult and no RedirectToAction
            Assert.IsType<ViewResult>(res1);
            Assert.IsType<ViewResult>(res2);
            Assert.IsNotType<RedirectToActionResult>(res1);
            Assert.IsNotType<RedirectToActionResult>(res2);
        }

        [Fact]
        public void CannotReviewNonExistingAdvert()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            var result = Controller.Advert(99);

            //Assert
            // Result type is a redirection to index page
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
        }

        [Fact]
        public void CannotReviewAdvertWithoutConfirming()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            AdvertReviewViewModel advr = new AdvertReviewViewModel()
            {
                Id = 7,
                Comments = "All ok in this advert. The comments should be at least 30 chars long.",
                Confirm = false,
                Status = AdvertStatus.Approved
            };
            var res = Controller.Advert(advr);

            //Assert
            // Edit was not called
            MockRepo.Verify(m => m.SaveAdvert(It.IsAny<Advert>()), Times.Never());
            // Result is ViewResult and no RedirectToAction
            Assert.IsType<ViewResult>(res);
            Assert.IsNotType<RedirectToActionResult>(res);
        }

        public class OfficerControllerReviewTests : IDisposable
        {
            public OfficerController Controller { get; private set; }
            public Mock<IAdvertRepository> MockRepo { get; private set; }

            public OfficerControllerReviewTests()
            {
                //Mock DbSet<adverts>
                var adverts = new List<Advert>()
                {
                    new Advert
                    {
                        Id = 1,
                        Title = "Advert 1",
                        Description = "Advert description 1",
                        Status = AdvertStatus.Pending
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

                //Mock pagingoptions
                Mock<MyPagingOptions> mockPagingOpts = new Mock<MyPagingOptions>();
                mockPagingOpts.SetupGet(po => po.PageSize).Returns(3);

                Mock<IOptions<MyPagingOptions>> mockIOptions = new Mock<IOptions<MyPagingOptions>>();
                mockIOptions.Setup(m => m.Value).Returns(mockPagingOpts.Object);

                //Mock current user
                var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                 new Claim(ClaimTypes.Name, "AnOfficer"),
                 new Claim("UniAccomodationId", "888")
                }));

                //Mock tempdata
                Mock<ITempDataDictionary> mockTempData = new Mock<ITempDataDictionary>();

                //Create controller
                Controller = new OfficerController(MockRepo.Object, mockIOptions.Object)
                {
                    TempData = mockTempData.Object
                };

                Controller.ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = user }
                };
            }


            [Fact]
            public void CanReviewAdvert()
            {
                //Arrange
                // controller arranged from the controller fixture
                // repository arranged from the controller fixture

                //Act
                AdvertReviewViewModel advert = new AdvertReviewViewModel()
                {
                    Id = 1,
                    Comments = "All ok in this advert. The comments should be at least 30 chars long.",
                    Confirm = true,
                    Status = AdvertStatus.Pending
                };
                IActionResult result = Controller.Advert(advert);

                //Assert
                // Edit was called once, saved an advert with id 7
                MockRepo.Verify(m => m.SaveAdvert(It.Is<Advert>(a => a.Id == 1)), Times.Once());
                // Result type is a redirection to index page
                Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", (result as RedirectToActionResult).ActionName);
            }

            public void Dispose()
            {
                Controller.Dispose();
            }
        }
    }
}
