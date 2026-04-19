using AuthServer;
using AuthServer.NswagClient;
using CustomersTask4;
using CustomerTaskUnitTest.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CustomerTaskUnitTest.AuthServerTests
{
    public class AuthTestBase : IClassFixture<WebApplicationFactory<IAuthAssmblyMarker>>
    {
        protected readonly WebApplicationFactory<IAuthAssmblyMarker> _factory;
        private readonly string DefaultTenantId = "Tenant1";
        public AuthTestBase(WebApplicationFactory<IAuthAssmblyMarker> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
            });
        }

        protected AuthClient CreateAuthClient()
        {
            var httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            httpClient.DefaultRequestHeaders.Add("tenant", DefaultTenantId);
            return new AuthClient(httpClient)
            {
                BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7032/"
            };
        }


        #region Helper Methods
        protected async Task<TokenResponse> GetTokenAsync(string username, string password)
        {
            var httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
            httpClient.DefaultRequestHeaders.Add("tenant", DefaultTenantId);

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
