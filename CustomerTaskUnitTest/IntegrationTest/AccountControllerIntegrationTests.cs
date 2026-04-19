using AuthServer;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace CustomerTaskUnitTest.IntegrationTest
{
    /// <summary>
    /// Integration tests for AuthServer AccountController
    /// </summary>
    public class AccountControllerIntegrationTests : AuthServerTestBase
    {
        private WebApplicationFactory<IAuthAssmblyMarker> _factory = null!;


        #region Register Tests

        [Fact]
        public async Task Register_WithValidCredentials_ShouldReturnOk()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var registerCommand = new RegisterUserCommand
            {
                Username = $"user_{Guid.NewGuid():N}",
                Email = email,
                Password = "Test@123456"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(
                "api/Account/register",
                registerCommand);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("user created successfully", content.ToLower());
        }

        [Fact]
        public async Task Register_WithDuplicateUserName_ShouldReturnBadRequest()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var username = $"user_{Guid.NewGuid():N}";
            var registerCommand = new RegisterUserCommand
            {
                Username = username,
                Email = email,
                Password = "Test@123456"
            };

            // Register the first user
            await _httpClient.PostAsJsonAsync("api/Account/register", registerCommand);

            // Try to register with the same email
            var duplicateCommand = new RegisterUserCommand
            {
                Username = username,
                Email = email,
                Password = "Test@123456"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(
                "api/Account/register",
                duplicateCommand);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var registerCommand = new RegisterUserCommand
            {
                Username = $"user_{Guid.NewGuid():N}",
                Email = "invalid-email",
                Password = "Test@123456"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(
                "api/Account/register",
                registerCommand);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var registerCommand = new RegisterUserCommand
            {
                Username = $"user_{Guid.NewGuid():N}",
                Email = GenerateUniqueEmail(),
                Password = "weak"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(
                "api/Account/register",
                registerCommand);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithMissingFields_ShouldReturnBadRequest()
        {
            // Arrange
            var registerCommand = new
            {
                Username = $"user_{Guid.NewGuid():N}"
                // Missing Email and Password
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(
                "api/Account/register",
                registerCommand);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Token/Login Tests

        [Fact]
        public async Task Exchange_WithValidCredentials_ShouldReturnAccessToken()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var password = "Test@123456";
            var username = $"user_{Guid.NewGuid():N}";

            // First register a user
            var registerCommand = new RegisterUserCommand
            {
                Username = username,
                Email = email,
                Password = password
            };
            var client = CreateClientWithTenant("Tenant1");
            await client.PostAsJsonAsync("api/Account/register", registerCommand);
            // Now try to get token
            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", username },
                { "password", password },
                {"scope", "openid offline_access"}
            };

            // Act
            var response = await client.PostAsync(
                "api/Account/token",
                new FormUrlEncodedContent(tokenRequest));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("access_token", content.ToLower());
        }

        [Fact]
        public async Task Exchange_WithInvalidPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var password = "Test@123456";
            
            // First register a user
            var registerCommand = new RegisterUserCommand
            {
                Username = $"user_{Guid.NewGuid():N}",
                Email = email,
                Password = password
            };
            await _httpClient.PostAsJsonAsync("api/Account/register", registerCommand);

            // Try with wrong password
            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", email },
                { "password", "WrongPassword" }
            };

            // Act
            var response = await _httpClient.PostAsync(
                "api/Account/token",
                new FormUrlEncodedContent(tokenRequest));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Exchange_WithNonexistentUser_ShouldReturnBadRequest()
        {
            // Arrange
            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", "nonexistent@example.com" },
                { "password", "Test@123456" }
            };

            // Act
            var response = await _httpClient.PostAsync(
                "api/Account/token",
                new FormUrlEncodedContent(tokenRequest));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Exchange_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            // Arrange
            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "password" }
                // Missing username and password
            };

            // Act
            var response = await _httpClient.PostAsync(
                "api/Account/token",
                new FormUrlEncodedContent(tokenRequest));
            

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Tenant Tests

        [Fact]
        public async Task Register_WithDifferentTenant_ShouldCreateUserForTenant()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var username = $"dupuser_{Guid.NewGuid():N}";
            var registerCommand = new RegisterUserCommand
            {
                Username = username,
                Email = email,
                Password = "Test@123456"
            };

            var clientWithDifferentTenant = CreateClientWithTenant("Tenant1");

            // Act
            var response = await clientWithDifferentTenant.PostAsJsonAsync(
                "api/Account/register",
                registerCommand);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_UserFromOneTenant_CannotLoginFromAnotherTenant()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var password = "Test@123456";

            // Register user in Tenant1
            var registerCommand = new RegisterUserCommand
            {
                Username = $"user_{Guid.NewGuid():N}",
                Email = email,
                Password = password
            };
            
            var tenant1Client = CreateClientWithTenant("Tenant1");
            await tenant1Client.PostAsJsonAsync("api/Account/register", registerCommand);

            // Try to login from SharedTenant
            var tokenRequest = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", email },
                { "password", password }
            };

            // Act
            var response = await _httpClient.PostAsync(
                "api/Account/token",
                new FormUrlEncodedContent(tokenRequest));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Missing Tenant Tests

        [Fact]
        public async Task Register_WithoutTenantHeader_ShouldReturnBadRequest()
        {
            // Arrange
            var client = CreateClientWithoutTenant();

            var registerCommand = new RegisterUserCommand
            {
                Username = $"dupuser_{Guid.NewGuid():N}",
                Email = GenerateUniqueEmail(),
                Password = "Test@123456"
            };

            // Act
            var response = await client.PostAsJsonAsync(
                "api/Account/register",
                registerCommand);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        #endregion
    }

    /// <summary>
    /// DTO for register command
    /// </summary>
    public class RegisterUserCommand
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}