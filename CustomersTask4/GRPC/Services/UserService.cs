using CustomersTask4.Users;
using FluentValidation.Validators;
using Grpc.Core;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace CustomersTask4.GRPC.Services
{
    public class UserService(IUserContext userContext): GetUserGRPC.GetUserGRPCBase
    {
        public override async Task<UserResponse> GettUserData(
       GetUserByIdRequest request,
       ServerCallContext context)
        {
            var current=userContext.GetCurrentUser();
            if(current ==null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "User is not authenticated"));
            }
            var user = new UserResponse
            {
                UserId = current?.Id,
                UserName = current?.Name
            };
            foreach(var role in current!.Roles)
            user.Roles.Add(role);

            return await Task.FromResult(user);
        }
    }
}
