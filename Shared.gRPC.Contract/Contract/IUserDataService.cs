using ProtoBuf;
using ProtoBuf.Grpc;
using System.ServiceModel;
using Google.Protobuf.WellKnownTypes;

namespace Shared.gRPC.Contract.Contract
{
    [ProtoContract]
    public class UserDataReply
    {
        [ProtoMember(1)]
        public string UserId { get; set; } = string.Empty;

        [ProtoMember(2)]
        public string UserName { get; set; } = string.Empty;

        [ProtoMember(3)]
        public List<string> Roles { get; set; } = new();
    }

    [ServiceContract]
    public interface IUserDataService
    {
        [OperationContract]
        UserDataReply GetUserDataAsync(
            Empty request,
            CallContext context = default);
    }
}