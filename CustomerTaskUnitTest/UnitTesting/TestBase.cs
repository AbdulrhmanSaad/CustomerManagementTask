using CustomersTask4;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;
using System.Net.Http.Headers;

namespace CustomerTaskUnitTest.UnitTesting
{
    public class TestBase(WebApplicationFactory<IAssmblyMarker> factory) : IClassFixture<WebApplicationFactory<IAssmblyMarker>>
    {
        protected readonly WebApplicationFactory<IAssmblyMarker> _factory = factory;
        private readonly string DefaultTenantId = "Tenant1";
        private readonly string UserEmail = "abdo@gmail.com";
        private readonly string UserPassword = "Test@12";
        private readonly string DefaultApiVersion = "1";



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

        protected async Task<string> GenerateToken()
        {
            var http = _factory.CreateClient();
            var client = new NswagClient(http);
            client.BaseUrl = http.BaseAddress?.ToString() ?? "https://localhost:7120/";

            var response = await client.LoginUserAsync(DefaultApiVersion, DefaultTenantId,new LoginUserCommand
            {
                Email = UserEmail,
                Password = UserPassword
            });
            return response.AccessToken;
        }

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