using CustomersTask4.NswagClient;
using Microsoft.AspNetCore.DataProtection;
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
            var credsFromFile = JsonSerializer.Deserialize<Credentials>(json);

            return new Credentials
            {
                Email = protector.Unprotect(credsFromFile!.Email),
                Password = protector.Unprotect(credsFromFile!.Password)
            };

        }

        public static async Task<string?> Authenticate(string email, string pass, Client apiClient)
        {
            string token = null;
            var loginRequest = new LoginUserCommand
            {
                Email = email,
                Password = pass
            };
            try
            {
                var loginResponse = await apiClient.LoginUserAsync(loginRequest);

                token = loginResponse.AccessToken;

                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Login successful.");
                    return token;
                }
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == 404)
                {
                    Console.WriteLine("Invalid Email Or Password");
                }
                else
                {
                    Console.WriteLine($"API Error: {ex.StatusCode}");
                    Console.WriteLine(ex.Response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            return null;
        }

    }
}
