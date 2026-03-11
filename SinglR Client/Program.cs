using CustomersTask4.NswagClient;
using Microsoft.AspNetCore.SignalR.Client;

var httpClient = new HttpClient();
var apiClient = new Client(httpClient);

Console.WriteLine("Enter Email:");
var email=Console.ReadLine();
Console.WriteLine("Enter Password:");
var pass=Console.ReadLine();

if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
{
    Console.WriteLine("Email and Password cannot be empty.");
    return;
}
var token = await Authenticate(email, pass, apiClient);

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







static async Task<string?> Authenticate(string email,string pass,Client apiClient)
{
    string token=null;
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