using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using UniAccomodation.Controllers;
using Moq;
using UniAccomodation.Data;
using UniAccomodation.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using UniAccomodation.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UniAccomodation.Models.ViewModels;

namespace UniAccomodationTest.ControllerTests
{
    public class HomeControllerFixture : IDisposable
    {
        public HomeController Controller { get; private set; }
        public HomeControllerFixture()
        {
            //Mock adverts
            var adverts = new List<Advert>()
            {
                new Advert
                {
                    Id = 1,
                    Title = "Advert 1",
                    Description = "Advert description 1",
                    Status = AdvertStatus.Approved
                },new Advert
                {
                    Id = 2,
                    Title = "Advert 2",
                    Description = "Advert description 2",
                    Status = AdvertStatus.Approved
                },
                new Advert
                {
                    Id = 3,
                    Title = "Advert 3",
                    Description = "Advert description 3",
                    Status = AdvertStatus.Approved
                },
                new Advert
                {
                    Id = 4,
                    Title = "Advert 4",
                    Description = "Advert description 4",
                    Status = AdvertStatus.Approved
                },
                new Advert
                {
                    Id = 5,
                    Title = "Advert 5",
                    Description = "Advert description 5",
                    Status = AdvertStatus.Approved
                },
                new Advert
                {
                    Id = 6,
                    Title = "Advert 6",
                    Description = "Advert description 6",
                    Status = AdvertStatus.Rejected
                },
                new Advert
                {
                    Id = 7,
                    Title = "Advert 7",
                    Description = "Advert description 7",
                    Status = AdvertStatus.Pending
                }
            }.AsQueryable();

            Mock<DbSet<Advert>> mockAdverts = new Mock<DbSet<Advert>>();
            mockAdverts.As<IQueryable<Advert>>().Setup(m => m.Provider).Returns(adverts.Provider);
            mockAdverts.As<IQueryable<Advert>>().Setup(m => m.Expression).Returns(adverts.Expression);
            mockAdverts.As<IQueryable<Advert>>().Setup(m => m.ElementType).Returns(adverts.ElementType);
            mockAdverts.As<IQueryable<Advert>>().Setup(m => m.GetEnumerator()).Returns(adverts.GetEnumerator());

            //Mock advert repository
            Mock<IAdvertRepository> mockRepo = new Mock<IAdvertRepository>();
            mockRepo.Setup(m => m.Adverts).Returns(mockAdverts.Object);

            //Mock pagingoptions
            Mock<MyPagingOptions> mockPagingOpts = new Mock<MyPagingOptions>();
            mockPagingOpts.SetupGet(po => po.PageSize).Returns(3);

            Mock<IOptions<MyPagingOptions>> mockIOptions = new Mock<IOptions<MyPagingOptions>>();
            mockIOptions.Setup(m => m.Value).Returns(mockPagingOpts.Object);

            //Create controller
            Controller = new HomeController(mockRepo.Object, mockIOptions.Object);
        }

        public void Dispose()
        {
            Controller.Dispose();
        }
    }

    public class HomeControllerTests : IClassFixture<HomeControllerFixture>
    {
        public HomeController Controller { get; private set; }

        public HomeControllerTests(HomeControllerFixture controllerFixture)
        {
            Controller = controllerFixture.Controller;
        }

        [Fact]
        public void CanPaginateAndOrderResults()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            var result = Controller.Adverts(2);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<AdvertListViewModel>(
                viewResult.ViewData.Model);
            Assert.Equal(2, model.Adverts.Count());
            Assert.Equal("Advert 2", model.Adverts.First().Title);
            Assert.Equal("Advert 1", model.Adverts.Last().Title);
        }

        [Fact]
        public void CanSendPaginationViewModel()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            var result = Controller.Adverts(2);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<AdvertListViewModel>(
                viewResult.ViewData.Model);
            PagingInfo pageInfo = model.PagingInfo;
            Assert.Equal(2, pageInfo.CurrentPage);
            Assert.Equal(3, pageInfo.ItemsPerPage);
            Assert.Equal(5, pageInfo.TotalItems);
            Assert.Equal(2, pageInfo.TotalPages);
        }

        [Fact]
        public void ShowsOnlyApprovedAdverts()
        {
            //Arrange
            // controller arranged from the controller fixture

            //Act
            var result = Controller.Adverts(1);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<AdvertListViewModel>(
                viewResult.ViewData.Model);
            Assert.Equal(3, model.Adverts.Count());
            Assert.Equal("Advert 5", model.Adverts.First().Title);
            Assert.Equal("Advert 3", model.Adverts.Last().Title);
        }

        [Fact]
        public void GeneratesCorrectAdvertCount()
        {
            //Arrange
            // controller arranged from the controller fixture
            // function to get the model from the viewresult
            Func<IActionResult, AdvertListViewModel> GetModel = result =>
                (result as ViewResult)?.ViewData?.Model as AdvertListViewModel;

            //Act
            var count = GetModel(Controller.Adverts())?.PagingInfo.TotalItems;

            //Assert
            Assert.Equal(5, count);
        }
    }
}
