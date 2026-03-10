using CustomersTask4.NswagClient;
using Microsoft.AspNetCore.SignalR.Client;

var httpClient = new HttpClient();
var apiClient = new Client(httpClient);

Console.WriteLine("Enter Email:");
var email=Console.ReadLine();
Console.WriteLine("Enter Password:");
var pass=Console.ReadLine();

var loginRequest = new LoginUserCommand
{
    Email = email,
    Password = pass
};

var loginResponse = await apiClient.LoginUserAsync(loginRequest);

string token = loginResponse.AccessToken;

if(string.IsNullOrEmpty(token))
{
    Console.WriteLine("Login failed.");
    return;
}

var Url = "https://localhost:7120/messagehub";
var connection = new HubConnectionBuilder()
    .WithUrl(Url, options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(token);
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