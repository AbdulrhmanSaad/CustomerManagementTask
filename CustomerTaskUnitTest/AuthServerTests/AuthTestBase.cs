using AuthServer;
using AuthServer.NswagClient;
using CustomersTask4;
using CustomerTaskUnitTest.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CustomerTaskUnitTest.AuthServerTests
{
    public class AuthTestBase(WebApplicationFactory<IAuthAssmblyMarker> factory) : IClassFixture<WebApplicationFactory<IAuthAssmblyMarker>>
    {
        protected readonly WebApplicationFactory<IAuthAssmblyMarker> _factory = factory;
        private readonly string DefaultTenantId = "Tenant1";

        protected AuthClient CreateApiClient()
        {
            var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("tenant", DefaultTenantId);
            var client = new AuthClient(httpClient);
            client.BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7032/";
            return client;
        }

        

     


    }
}
