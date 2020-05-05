﻿using CordEstates.Areas.Staff.Controllers;
using CordEstates.Areas.Staff.Models.DTOs;
using CordEstates.Entities;
using CordEstates.Models;
using CordEstates.Tests.Setup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using static CordEstates.Helpers.ImageUpload;

namespace CordEstates.Tests.UnitTests.StaffAreaTests.ListingControllerTests
{
    public class IndexShould
    {
        private readonly SetupFixture fixture;
        private readonly ListingController sut;
        private readonly Mock<IHostEnvironment> env;
        private readonly Mock<IImageUploadWrapper> imageUploadWrapper;
        public IndexShould()
        {

            fixture = new SetupFixture();

            env = new Mock<IHostEnvironment>();
            imageUploadWrapper = new Mock<IImageUploadWrapper>();
            env.Setup(m => m.EnvironmentName).Returns("Hosting:UnitTestEnvironment");
            sut = new ListingController(fixture.Logger.Object,
                                      fixture.repositoryWrapper.Object,
                                      fixture.mapper.Object,
                                      env.Object,
                                      imageUploadWrapper.Object);
            fixture.repositoryWrapper.Setup(x => x.Listing.GetAllListingsAsync()).ReturnsAsync(It.IsAny<List<Listing>>());
            fixture.mapper.Setup(x => x.Map<List<ListingManagementDTO>>(It.IsAny<Listing>())).Returns(new List<ListingManagementDTO>());
            imageUploadWrapper.Setup(x => x.Upload(It.IsAny<IFormFile>(), It.IsAny<IHostEnvironment>()))
                .Returns("imageUrl");
        }


        [Fact]
        public async void ReturnCorrectView()
        {
            var result = await sut.Index("", 5);
            var vr = Assert.IsType<ViewResult>(result);
            Assert.Equal("Index", vr.ViewName);
        }


        [Fact]
        public async Task ReturnListOfAllListing()
        {

            var result = await sut.Index("", 5);
            var vr = Assert.IsType<ViewResult>(result);
            Assert.IsType<PaginatedList<ListingManagementDTO>>(vr.Model);

        }



    }
}
