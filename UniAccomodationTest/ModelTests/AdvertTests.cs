using System;
using Xunit;
using UniAccomodation.Models;

namespace UniAccomodationTest
{
    public class AdvertTests
    {
        [Fact]
        public void CanReviewAdvert()
        {
            //Arrange
            var advert = new Advert()
            {
                Id = 1000,
                Title = "Advert 1",
                Description = "Advert 1 description",
                MonthlyPrice = 300M,
                PhotoUrl = "",
                Status = AdvertStatus.Pending,
                LandlordId = 1                
            };

            //Act
            var reviewed = advert.Review(AdvertStatus.Approved, "This advert is ok", 2);

            //Assert
            Assert.True(reviewed);
            Assert.Equal(AdvertStatus.Approved, advert.Status);
            Assert.Equal("This advert is ok", advert.Comments);
            Assert.Equal(2, advert.OfficerId);
        }

        [Fact]
        public void CannotReviewAdvert()
        {
            //Arrange
            var advert = new Advert()
            {
                Id = 1000,
                Title = "Advert 1",
                Description = "Advert 1 description",
                MonthlyPrice = 300M,
                PhotoUrl = "",
                Status = AdvertStatus.Rejected,
                LandlordId = 1,
                OfficerId = 2,
                Comments = "Picture is missing and description too poor."
            };

            //Act
            var reviewed = advert.Review(AdvertStatus.Approved, "This advert is ok", 3);

            //Assert
            Assert.False(reviewed);
            Assert.Equal(AdvertStatus.Rejected, advert.Status);
            Assert.Equal("Picture is missing and description too poor.", advert.Comments);
            Assert.Equal(2, advert.OfficerId);
        }
    }
}
