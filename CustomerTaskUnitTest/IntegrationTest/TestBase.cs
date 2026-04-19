using AuthServer;
using AuthServer.DTO;
using CustomersTask4;
using CustomerTaskUnitTest.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using System.Net.Http.Headers;

namespace CustomerTaskUnitTest.IntegrationTest
{
    public class TestBase: IClassFixture<WebApplicationFactory<IAssmblyMarker>>,IClassFixture<WebApplicationFactory<IAuthAssmblyMarker>>
    {
        protected readonly WebApplicationFactory<IAssmblyMarker> _factory ;
        protected readonly WebApplicationFactory<IAuthAssmblyMarker> _authFactory;
        private readonly string DefaultTenantId = "SharedTenant";
        private readonly string UserEmail = "abdo@gmail.com";
        private readonly string UserName = "abdo";
        private readonly string UserPassword = "Test@123";
        private readonly string DefaultApiVersion = "1";


        public TestBase(WebApplicationFactory<IAssmblyMarker> factory,
                    WebApplicationFactory<IAuthAssmblyMarker> auth)
        {
            _authFactory = auth.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.PostConfigure<OpenIddictServerAspNetCoreOptions>(options =>
                    {
                        options.DisableTransportSecurityRequirement = true;
                    });
                });
            });
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    services.PostConfigure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme, options =>
                    {
                        var signingKey = _authFactory.Services
                        .GetRequiredService<IOptions<OpenIddictServerOptions>>()
                        .Value.SigningCredentials.First().Key;

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = signingKey
                        };
                    });
                });
            });

        }
        protected string GenerateUniquePhone()
        {
            return $"010{DateTime.Now.Ticks % 100000000:D8}";

        }

        protected NswagClient CreateApiClient()
        {
            var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("tenant", DefaultTenantId);
            var client = new NswagClient(httpClient);
            client.BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7120/";
            return client;
        }

        protected NswagClient CreateApiClient(string accessToken)
        {
            var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("tenant", DefaultTenantId);
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
            var client = new NswagClient(httpClient);
            client.BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7120/";
            return client;
        }

        public async Task<string> GenerateToken()
        {
            var httpClient = _authFactory.Server.CreateClient();
            httpClient.DefaultRequestHeaders.Add("tenant", DefaultTenantId);

            var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", UserName },
                { "password", UserPassword },
                { "scope", "openid offline_access" }
            });

            var response = await httpClient.PostAsync("api/Account/token", requestContent);
            
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Failed to generate token. Status: {response.StatusCode}, Response: {content}");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonContent);

            if (tokenResponse == null)
                throw new InvalidOperationException("Failed to deserialize token response");

            return tokenResponse.access_token;
        }

        //protected async Task<string> GenerateToken()
        //{
        //    var http = _factory.CreateClient();
        //    var client = new NswagClient(http);
        //    client.BaseUrl = http.BaseAddress?.ToString() ?? "https://localhost:7120/";

        //    var response = await client.LoginUserAsync(DefaultApiVersion, DefaultTenantId,new LoginUserCommand
        //    {
        //        Email = UserEmail,
        //        Password = UserPassword
        //    });
        //    return response.AccessToken;
        //}

        protected CreateCustomerCommand CreateCustomer()
        {
            var phone = GenerateUniquePhone();
            var createCommand = new CreateCustomerCommand
            {
                Name = "Integration Test Customer" + phone,
                Phone = phone,
                Addresses = new List<AddressDtoEnum>
                {
                    new AddressDtoEnum
                    {
                        AddressName = "Cairo",
                        AddressType = 0
                    }
                }
            };
            return createCommand;
        }

    
    }
}