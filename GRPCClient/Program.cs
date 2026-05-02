using CustomersTask4;
using Grpc.Core;
using Grpc.Net.Client;
using Google.Protobuf.WellKnownTypes;

using var channel = GrpcChannel.ForAddress("https://localhost:7120");

var client = new GetUserGRPC.GetUserGRPCClient(channel);

Console.WriteLine("Enter a valid token");
var token=Console.ReadLine();

var headers = new Metadata
{
    { "tenant", "SharedTenant" },
    {"Authorization","Bearer " + token}
};

var reply = await client.GettUserDataAsync(
    new Empty(),
    headers);
if(reply == null)
{
    Console.WriteLine("No user data received.");
    return;
}
Console.WriteLine();
Console.WriteLine("--------------------------Getting the user Data From token ---------------------------------");
Console.WriteLine();
Console.WriteLine("UserId: " + reply.UserId);
Console.WriteLine("UserEmail: " + reply.UserName);
Console.WriteLine("UserRoles: " + string.Join(", ", reply.Roles));
