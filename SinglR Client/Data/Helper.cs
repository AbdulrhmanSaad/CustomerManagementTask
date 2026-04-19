using AuthServer.DTO;
using Azure.Core;
using CustomersTask4.NswagClient;
using JasperFx.MultiTenancy;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SinglR_Client.Data
{
    internal static class Helper
    {
        public static Credentials GetLoginDataFromDataProtection()
        {
            var keyFolder = Path.Combine(Directory.GetCurrentDirectory(), "Keys");

            if (!Directory.Exists(keyFolder))
                Directory.CreateDirectory(keyFolder);

            var provider = DataProtectionProvider.Create(new DirectoryInfo(keyFolder));

            var protector = provider.CreateProtector("credentials");


            var json = File.ReadAllText("credentials.json");
            var credsFromFile = System.Text.Json.JsonSerializer.Deserialize<Credentials>(json);

            return new Credentials
            {
                UserName = protector.Unprotect(credsFromFile!.UserName),
                Password = protector.Unprotect(credsFromFile!.Password)
            };

        }

        public static async Task<string?> Authenticate(string username, string password,string tenantId)
        {
            var client = new HttpClient { BaseAddress = new Uri("https://localhost:7032/") };
            client.DefaultRequestHeaders.Add("tenant", tenantId);
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = username,
                ["password"] = password,
                ["scope"] = "openid offline_access"
            });

            var response = await client.PostAsync("api/Account/token", form);
            var jsonContent = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(jsonContent);

            if (tokenResponse == null)
                return null;

            return tokenResponse?.access_token;
        }
    }
}
