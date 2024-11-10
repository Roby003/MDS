using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoardBloom.Controllers;
using BoardBloom.Data;
using BoardBloom.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BoardBloom.Tests
{
    public class BloomsControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly BloomsController _controller;

        public BloomsControllerTests()
        {
            // Set up UserManager mock
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Set up ApplicationDbContext mock
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _mockContext = new Mock<ApplicationDbContext>(options);

            // Initialize the controller with the mocks
            _controller = new BloomsController(_mockContext.Object, _mockUserManager.Object, null, null, null);
        }

        [Fact]
        public void Index_ReturnsViewResult_WithExpectedData()
        {
            // Arrange
            var fakeBlooms = new List<Bloom>
        {
            new Bloom { Id = 1, Title = "Bloom 1", Content = "Content 1", TotalLikes = 10, UserId = "user1" },
            new Bloom { Id = 2, Title = "Bloom 2", Content = "Content 2", TotalLikes = 20, UserId = "user2" }
        }.AsQueryable();

            var fakeLikes = new List<Like>
        {
            new Like { Id = 1, BloomId = 1, UserId = "user1" },
            new Like { Id = 2, BloomId = 2, UserId = "user1" }
        }.AsQueryable();

            var mockBloomDbSet = CreateMockDbSet(fakeBlooms);

            var mockLikeDbSet =CreateMockDbSet(fakeLikes);

            _mockContext.Setup(c => c.Blooms).Returns(mockBloomDbSet.Object);
            _mockContext.Setup(c => c.Likes).Returns(mockLikeDbSet.Object);

            // Mock the HttpContext
            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "search", "Bloom" },
            { "page", "1" }
        });

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Request.Query).Returns(queryCollection);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Mock UserManager to return a user ID
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user1");

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            // Verify that the result is not null and is of type ViewResult
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var viewDataBloomsEnumerable = result?.ViewData["blooms"] as IEnumerable<Bloom>;
            var viewDataBlooms = viewDataBloomsEnumerable?.AsQueryable();
            // Verify the data in ViewData
            Assert.NotNull(viewDataBlooms);
            Assert.Equal(fakeBlooms.Count(), viewDataBlooms?.Count());

        }
        [Fact]
        public void Show_ReturnsViewResult_WithBloom()
        {
            // Arrange
            var fakeLikes = new List<Like>
    {
        new Like { Id = 1, BloomId = 1, UserId = "user1" }
    }.AsQueryable();
            var bloom = new Bloom
            {
                Id = 1,
                Title = "Bloom 1",
                Content = "Content 1",
                UserId = "user1", // Note: User ID should be set directly on the Bloom object
                Likes=fakeLikes.ToList()
            };

            

            var mockBloomDbSet = CreateMockDbSet(new List<Bloom> { bloom }.AsQueryable());

            _mockContext.Setup(c => c.Blooms).Returns(mockBloomDbSet.Object);

            // Mock the Likes DbSet
            var mockLikesDbSet = CreateMockDbSet(fakeLikes.AsQueryable());
            _mockContext.Setup(c => c.Likes).Returns(mockLikesDbSet.Object);

            // Mock UserManager to return a user ID
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "user1"), // Set the user ID as needed
                                                       // Add any other claims your application expects
            }, "mock"));

            _mockUserManager.Setup(um => um.GetUserId(user)).Returns("user1");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = _controller.Show(1) as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Bloom>(viewResult.Model);
            Assert.Equal("Bloom 1", model.Title);
            
           
        }
        [Fact]
        public void New_ReturnsViewResult_WithEmptyBloom()
        {
            // Act
            var result = _controller.New(null) as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Bloom>(viewResult.Model);
            Assert.Null(model.Title);
            Assert.Null(model.Content);
        }
        [Fact]
        public void Edit_ReturnsViewResult_WithBloomForAuthorizedUser()
        {
            // Arrange
            var bloom = new Bloom { Id = 1, Title = "Bloom 1", Content = "Content 1", UserId = "user1" };
            _mockContext.Setup(db => db.Blooms.Find(1)).Returns(bloom);

            // Mocking the User property
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user1"), // Set the user ID as needed
                                                   // Add any other claims your application expects
            }, "mock"));

            _mockUserManager.Setup(um => um.GetUserId(user)).Returns("user1");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = _controller.Edit(1) as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Bloom>(viewResult.Model);
            Assert.Equal("Bloom 1", model.Title);

        }
        [Fact]
        public void Edit_ReturnsRedirectToHomeForUnauthorizedUser()
        {
            // Arrange
            var bloom = new Bloom { Id = 1, Title = "Bloom 1", Content = "Content 1", UserId = "user2" };
            _mockContext.Setup(db => db.Blooms.Find(1)).Returns(bloom);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
           {
                new Claim(ClaimTypes.NameIdentifier, "user1"), // Set the user ID as needed
                                                               // Add any other claims your application expects
           }, "mock"));

            _mockUserManager.Setup(um => um.GetUserId(user)).Returns("user1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            // Act
            var result = _controller.Edit(1) as RedirectToActionResult;

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
   }
        [Fact]
        public void Delete_ReturnsRedirectToHomeForUnauthorizedUser()
        {
            // Arrange
            var bloom = new Bloom { Id = 1, Title = "Bloom 1", Content = "Content 1", UserId = "user2" };
            _mockContext.Setup(db => db.Blooms.Find(1)).Returns(bloom);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
          {
                new Claim(ClaimTypes.NameIdentifier, "user1"), // Set the user ID as needed
                                                               // Add any other claims your application expects
          }, "mock"));

            _mockUserManager.Setup(um => um.GetUserId(user)).Returns("user1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            // Act
            var result = _controller.Delete(1) as RedirectToActionResult;

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Home", result.ControllerName);
        }
        [Fact]
        public void Delete_RemovesBloomAndReturnsRedirectForAuthorizedUser()
        {
            // Arrange
            var bloom = new Bloom { Id = 1, Title = "Bloom 1", Content = "Content 1", UserId = "user1" };
            _mockContext.Setup(db => db.Blooms.Find(1)).Returns(bloom);
            var likes = new List<Like>();
            var comments = new List<Comment>();
            var mockLikeDbSet = CreateMockDbSet(likes.AsQueryable());
            var mockCommentDbSet = CreateMockDbSet(comments.AsQueryable());
            _mockContext.Setup(db => db.Likes).Returns(mockLikeDbSet.Object);
            _mockContext.Setup(db => db.Comments).Returns(mockCommentDbSet.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
                new Claim(ClaimTypes.NameIdentifier, "user1"), // Set the user ID as needed
                                                               // Add any other claims your application expects
        }, "mock"));

            _mockUserManager.Setup(um => um.GetUserId(user)).Returns("user1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = _controller.Delete(1) as RedirectToActionResult;

            // Assert
            _mockContext.Verify(db => db.Blooms.Remove(bloom), Times.Once);
            _mockContext.Verify(db => db.SaveChanges(), Times.Once);
            Assert.Equal("Home", result.ControllerName);
            Assert.Equal("Index", result.ActionName);
        }
     /*
        [Fact]
        public void Like_TogglesLikeAndReturnsJson()
        {
            // Arrange
            var bloom = new Bloom
            {
                Id = 1,
                Title = "Bloom 1",
                Content = "Content 1",
                TotalLikes = 0
            };

            var mockDbSet = CreateMockDbSet(new List<Bloom> { bloom }.AsQueryable());
            _mockContext.Setup(db => db.Blooms.Find(1)).Returns(bloom);

            var likes = new List<Like>();
            var mockLikeDbSet = CreateMockDbSet(likes.AsQueryable());
            _mockContext.Setup(db => db.Likes).Returns(mockLikeDbSet.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
       {
                new Claim(ClaimTypes.NameIdentifier, "user1"), // Set the user ID as needed
                                                               // Add any other claims your application expects
       }, "mock"));

            _mockUserManager.Setup(um => um.GetUserId(user)).Returns("user1");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = _controller.Like(1) as JsonResult;

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic responseData = jsonResult.Value;

            Assert.NotNull(responseData);
            Assert.Equal(1, (int)responseData.LikeCount);
            Assert.True((bool)responseData.userLikedPost);

        }*/

        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            return mockSet;
        }


    }
}