using BoardBloom.Data;
using BoardBloom.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BoardBloom.Tests
{
    public class UserTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceProvider _serviceProvider;

        public UserTests()
        {
            // Set up a service collection
            var services = new ServiceCollection();

            // Set up an in-memory database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDb"));

            // Configure Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            // Add logging service (to resolve ILogger dependencies)
            services.AddLogging();

            // Build the service provider
            _serviceProvider = services.BuildServiceProvider();

            // Get the database context and user manager from the service provider
            _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            _userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateUser_SuccessfullyCreatesUser()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser", Email = "testuser@example.com" };
            var password = "Test@123";

            // Act
            var result = await _userManager.CreateAsync(user, password);

            // Assert
            Assert.True(result.Succeeded);
            var createdUser = await _userManager.FindByNameAsync(user.UserName);
            Assert.NotNull(createdUser);
            Assert.Equal(user.Email, createdUser.Email);
        }

        [Fact]
        public async Task CreateUser_FailsWithInvalidPassword()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser", Email = "testuser@example.com" };
            var password = "123"; // Too short and doesn't meet default password requirements

            // Act
            var result = await _userManager.CreateAsync(user, password);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, error => error.Description.Contains("Passwords must be at least 6 characters."));
            var createdUser = await _userManager.FindByNameAsync(user.UserName);
            Assert.Null(createdUser);
        }
    }
}
