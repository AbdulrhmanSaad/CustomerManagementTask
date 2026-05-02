using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using Shared.gRPC.Contract.Contract;

using var channel = GrpcChannel.ForAddress("https://localhost:7120");

var client = channel.CreateGrpcService<IUserDataService>();

Console.WriteLine("Enter a valid token");
var token=Console.ReadLine();

var headers = new Metadata
{
    { "tenant", "SharedTenant" },
    {"Authorization", "Bearer "+token }
};
var callOptions = new CallOptions(headers: headers);

var reply = client.GetUserDataAsync(
    new Empty(),
    new CallContext(callOptions)
    );
if(reply == null)
{
    Console.WriteLine("No user data received.");
    return;
}
Console.WriteLine();
Console.WriteLine("--------------------------Getting the user Data From Token ---------------------------------");
Console.WriteLine();
Console.WriteLine("UserId: " + reply.UserId);
Console.WriteLine("UserEmail: " + reply.UserName);
Console.WriteLine("UserRoles: " + string.Join(", ", reply.Roles));
