using CustomersTask4.Users;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ProtoBuf.Grpc;
using Shared.gRPC.Contract.Contract;

namespace CustomersTask4.GRPC.Services
{
    public class UserService(IUserContext userContext):IUserDataService
    {
        public UserDataReply GetUserDataAsync(Empty request, CallContext context = default)
        {
            var current = userContext.GetCurrentUser();
            if (current == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "User is not authenticated"));
            }
            var user = new UserDataReply
            {
                UserId = current!.Id,
                UserName = current!.Name
            };
            foreach (var role in current!.Roles)
                user.Roles.Add(role);

            return user;
        }
    }
}
