using CustomersTask4.NswagClient;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SignalR.Client;
using SinglR_Client.Data;
using System.Text.Json;



//var keyFolder = Path.Combine(Directory.GetCurrentDirectory(), "Keys");

//if (!Directory.Exists(keyFolder))
//    Directory.CreateDirectory(keyFolder);

//var provider = DataProtectionProvider.Create(new DirectoryInfo(keyFolder));
//var protector = provider.CreateProtector("credentials");

//string emailPlain = "abdo@gmail.com";
//string passwordPlain = "Test@12";

//string emailEncrypted = protector.Protect(emailPlain);
//string passwordEncrypted = protector.Protect(passwordPlain);

//var credentials = new Credentials
//{
//    Email = emailEncrypted,
//    Password = passwordEncrypted
//};

//File.WriteAllText("credentials.json", JsonSerializer.Serialize(credentials));

var credentials = Helper.GetLoginDataFromDataProtection();


if (string.IsNullOrEmpty(credentials.Email) || string.IsNullOrEmpty(credentials.Password))
{
    Console.WriteLine("Error in Reading Email and Password.");
    return;
}

var httpClient = new HttpClient();
var apiClient = new Client(httpClient);

var token = await Helper.Authenticate(credentials.Email, credentials.Password, apiClient);

if (string.IsNullOrEmpty(token))
{
    Console.WriteLine("Authentication failed. Exiting.");
    return;
}

    var Url = "https://localhost:7120/messagehub";
var connection = new HubConnectionBuilder()
    .WithUrl(Url, options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(token);
        options.Headers.Add("tenant", "SharedTenant");
        options.Headers.Add("api-version", "1");
    })
    .WithAutomaticReconnect()
    .Build();


connection.On<string,string>("ReceiveMessage", (message,action) =>
{
    Console.WriteLine($"Received: {action} with data : {message}\n\n");
});

await connection.StartAsync();


Console.WriteLine("Connected.");



await connection.InvokeAsync("SendMessage","Message","action");

Console.ReadLine();






