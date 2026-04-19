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

//string userNamePlain = "abdo";
//string passwordPlain = "Test@123";

//string UserNameEncrypted = protector.Protect(userNamePlain);
//string passwordEncrypted = protector.Protect(passwordPlain);

//var credentials = new Credentials
//{
//    UserName = UserNameEncrypted,
//    Password = passwordEncrypted
//};

//File.WriteAllText("credentials.json", JsonSerializer.Serialize(credentials));

var credentials = Helper.GetLoginDataFromDataProtection();


if (string.IsNullOrEmpty(credentials.UserName) || string.IsNullOrEmpty(credentials.Password))
{
    Console.WriteLine("Error in Reading Email and Password.");
    return;
}

var tenantId= "SharedTenant";
var token = await Helper.Authenticate(credentials.UserName, credentials.Password,tenantId);

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






