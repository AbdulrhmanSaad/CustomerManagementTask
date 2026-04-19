using AuthServer;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;

namespace CustomerTaskUnitTest.IntegrationTest
{
    public class AuthServerTestBase : IAsyncLifetime
    {
        private WebApplicationFactory<IAuthAssmblyMarker> _factory = null!;
        protected HttpClient _httpClient = null!;
        
        private readonly string _defaultTenant = "SharedTenant";

        public async Task InitializeAsync()
        {
            _factory = new WebApplicationFactory<IAuthAssmblyMarker>();
            _httpClient = _factory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7032/");
            _httpClient.DefaultRequestHeaders.Add("tenant", _defaultTenant);
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _httpClient?.Dispose();
            _factory?.Dispose();
            await Task.CompletedTask;
        }
        protected HttpClient CreateClientWithTenant(string tenant = "SharedTenant")
        {
            var client = _factory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7032/");
            client.DefaultRequestHeaders.Add("tenant", tenant);
            return client;
        }
        protected HttpClient CreateClientWithoutTenant()
        {
            var client = _factory.CreateClient();
            client.BaseAddress = new Uri("https://localhost:7032/");
            return client;
        }
        protected string GenerateUniqueEmail()
        {
            return $"testuser_{DateTime.Now.Ticks}@gmail.com";
        }
    }
}