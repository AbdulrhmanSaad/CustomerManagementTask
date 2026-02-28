using CustomersTask4;
using CustomersTask4.Services;
using CustomersTask4.UserHandler.Command.LoginUser;
using CustomerTaskUnitTest.UnitTesting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CustomerTaskUnitTest.UnitTesting
{
    public class TestBase(WebApplicationFactory<IAssmblyMarker> factory, IUserTokenMangerService userTokenManger) : IClassFixture<WebApplicationFactory<IAssmblyMarker>>
    {
        protected readonly WebApplicationFactory<IAssmblyMarker> _factory = factory;


        protected Client CreateApiClient()
        {
            var httpClient = _factory.CreateClient();
            var client = new Client(httpClient);
            client.BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7120/";
            return client;
        }

        protected Client CreateApiClient(string accessToken)
        {
            var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
            var client = new Client(httpClient);
            client.BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7120/";
            return client;
        }


        //protected async Task<string> GetAdminTokenAsync()
        //{
        //    var client = CreateApiClient();
        //   var res= await client.Me2Async(new LoginUserCommand
        //    {
        //        Email = "abdo@gmail.com",
        //        Password = "Test@12"
        //    }, CancellationToken.None);
        //    return res.AccessToken;
        //}

    }
}