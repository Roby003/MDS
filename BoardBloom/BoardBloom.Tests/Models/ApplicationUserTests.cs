using Xunit;
using BoardBloom;
using BoardBloom.Models;

namespace BoardBloom.Test
{
    public class ApplicationUserTest
    {
        [Fact]
        public void TestApplicationUserProperties()
        {
            // Arrange
            var user = new ApplicationUser()
            {
                FirstName = "John",
                LastName = "Doe",
                ProfilePicture = new byte[] { 0x20, 0x20, 0x20, 0x20 }
            };

            // Act
            var firstName = user.FirstName;
            var lastName = user.LastName;
            var profilePicture = user.ProfilePicture;

            // Assert
            Assert.Equal("John", firstName);
            Assert.Equal("Doe", lastName);
            Assert.Equal(new byte[] { 0x20, 0x20, 0x20, 0x20 }, profilePicture);
        }
    }
}