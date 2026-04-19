using AuthServer;
using AuthServer.NswagClient;
using CustomersTask4.NswagClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CustomerTaskUnitTest.AuthServerTests
{
    public class AuthClientIntegrationTests : IClassFixture<WebApplicationFactory<IAuthAssmblyMarker>>
    {
        private readonly WebApplicationFactory<IAuthAssmblyMarker> _factory;
        private readonly string _defaultTenant = "Tenant1";

        public AuthClientIntegrationTests(WebApplicationFactory<IAuthAssmblyMarker> factory)
        {
            _factory = factory;
        }

        private AuthClient CreateAuthClient()
        {
            var httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            httpClient.DefaultRequestHeaders.Add("tenant", _defaultTenant);
            return new AuthClient(httpClient)
            {
                BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7032/"
            };
        }

        #region Registration Tests

        [Fact]
        public async Task Register_WithValidCredentials_ReturnsOk()
        {
            // Arrange
            var client = CreateAuthClient();
            var command = new RegisterUserCommand
            {
                Username = $"testuser_{Guid.NewGuid():N}",
                Email = $"test_{Guid.NewGuid():N}@example.com",
                Password = "P@ssw0rd!123"
            };

            // Act & Assert — no exception means HTTP 200
            await client.RegisterAsync(command);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var client = CreateAuthClient();
            var command = new RegisterUserCommand
            {
                Username = $"dupuser_{Guid.NewGuid():N}",
                Email = $"dup_{Guid.NewGuid():N}@example.com",
                Password = "P@ssw0rd!123"
            };
            await client.RegisterAsync(command);

            // Act — register again with the same e-mail
            var exception = await Assert.ThrowsAsync<AuthServer.NswagClient.ApiException>(
                () => client.RegisterAsync(command));

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, exception.StatusCode);
        }

        [Theory]
        [InlineData("", "valid@example.com", "P@ssw0rd!123")]  // empty username
        [InlineData("validuser", "", "P@ssw0rd!123")]  // empty email
        [InlineData("validuser", "not-an-email", "P@ssw0rd!123")]  // malformed email
        [InlineData("validuser", "valid@example.com", "")]  // empty password
        [InlineData("validuser", "valid@example.com", "short")]  // password too weak
        public async Task Register_WithInvalidPayload_ReturnsBadRequest(
            string username, string email, string password)
        {
            // Arrange
            var client = CreateAuthClient();
            var command = new RegisterUserCommand
            {
                Username = username,
                Email = email,
                Password = password
            };

            // Act
            var exception = await Assert.ThrowsAsync<AuthServer.NswagClient.ApiException<AuthServer.NswagClient.ProblemDetails>>(
                () => client.RegisterAsync(command));

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, exception.StatusCode);
        }

        #endregion

        #region Token Tests (Password Grant Flow)

        [Fact]
        public async Task Token_WithValidCredentials_ReturnsTokenResponse()
        {
            // Arrange
            var username= $"tokenuser_{Guid.NewGuid():N}";
            var email = $"tokentest_{Guid.NewGuid():N}@example.com";
            var password = "P@ssw0rd!123";

            // Register user first
            var client = CreateAuthClient();
            var registerCommand = new RegisterUserCommand
            {
                Username = username,
                Email = email,
                Password = password
            };
            await client.RegisterAsync(registerCommand);

            // Act - Request token using password grant
            var tokenResponse = await GetTokenAsync(username, password);

            // Assert
            Assert.NotNull(tokenResponse);
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.Access_token),
                "access_token must not be empty");
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.Token_type),
                "token_type must not be empty");
            Assert.True(tokenResponse.Expires_in > 0,
                "expires_in must be a positive value");
        }

        [Fact]
        public async Task Token_ResponseContainsExpectedTokenType()
        {
            // Arrange
            var email = $"tokentest_{Guid.NewGuid():N}@example.com";
            var password = "P@ssw0rd!123";
            var username = $"tokenuser_{Guid.NewGuid():N}";
            var client = CreateAuthClient();
            var registerCommand = new RegisterUserCommand
            {
                Username = username,
                Email = email,
                Password = password
            };
            await client.RegisterAsync(registerCommand);

            // Act
            var tokenResponse = await GetTokenAsync(username, password);

            // Assert — OAuth2 servers return "Bearer"
            Assert.Equal("Bearer", tokenResponse.Token_type, ignoreCase: true);
        }

        [Fact]
        public async Task Token_ResponseContainsRefreshToken()
        {
            // Arrange
            var username = $"tokenuser_{Guid.NewGuid():N}";
            var email = $"tokentest_{Guid.NewGuid():N}@example.com";
            var password = "P@ssw0rd!123";

            var client = CreateAuthClient();
            var registerCommand = new RegisterUserCommand
            {
                Username =username,
                Email = email,
                Password = password
            };
            await client.RegisterAsync(registerCommand);

            // Act
            var tokenResponse = await GetTokenAsync(username, password);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.Refresh_token),
                "refresh_token should be present");
        }

      
        #endregion

        #region Register → Token flow

        [Fact]
        public async Task RegisterThenToken_FullFlow_ReturnsValidToken()
        {
            // Arrange
            var email = $"flow_{Guid.NewGuid():N}@example.com";
            var username = $"flowuser_{Guid.NewGuid():N}";
            var password = "P@ssw0rd!123";

            var client = CreateAuthClient();
            var command = new RegisterUserCommand
            {
                Username = username,
                Email = email,
                Password = password
            };

            // Act — Step 1: register
            await client.RegisterAsync(command);

            // Act — Step 2: obtain token using password grant
            var tokenResponse = await GetTokenAsync(username, password);

            // Assert
            Assert.NotNull(tokenResponse);
            Assert.False(string.IsNullOrWhiteSpace(tokenResponse.Access_token));
            Assert.Equal("Bearer", tokenResponse.Token_type, ignoreCase: true);
            Assert.True(tokenResponse.Expires_in > 0);
        }


        #endregion

        #region Helper Methods
        private async Task<TokenResponse> GetTokenAsync(string username, string password)
        {
            var httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            httpClient.DefaultRequestHeaders.Add("tenant", _defaultTenant);

            var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                  { "grant_type", "password" },
                  { "username", username },
                  { "password", password },
                  { "scope", "openid offline_access" }
            });

            var response = await httpClient.PostAsync("api/Account/token", requestContent);
            var jsonContent = await response.Content.ReadAsStringAsync();

           

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonContent);

            if (tokenResponse == null)
                throw new InvalidOperationException("Failed to deserialize token response");

            return tokenResponse;
        }

        #endregion
    }
}
