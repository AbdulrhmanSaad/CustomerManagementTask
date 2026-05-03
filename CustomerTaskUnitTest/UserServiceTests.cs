using CustomersTask4.GRPC.Services;
using CustomersTask4.Users;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NSubstitute;
using ProtoBuf.Grpc;
using Shared.gRPC.Contract.Contract;

namespace CustomerTaskUnitTest
{
    public class UserServiceTests
    {
        private readonly IUserContext _userContext;
        private readonly UserService _userService;
        private readonly CallContext _serverCallContext;

        public UserServiceTests()
        {
            _userContext = Substitute.For<IUserContext>();
            _userService = new UserService(_userContext);
            _serverCallContext = CallContext.Default;
        }

        // Test 1: Returns correct user data when user is authenticated
        [Fact]
        public void GetUserData_AuthenticatedUser_ReturnsCorrectUserResponse()
        {
            // Arrange
            var mockUser = new CurrentUser(
                Id: "user-123",
                Name: "Abdo Saad",
                Roles: new List<string> { "1", "2" }
            );

            _userContext.GetCurrentUser().Returns(mockUser);

            // Act
            var result = _userService.GetUserDataAsync(new Empty(), _serverCallContext);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user-123", result.UserId);
            Assert.Equal("Abdo Saad", result.UserName);
            Assert.Equal(2, result.Roles.Count);
            Assert.Contains("1", result.Roles);
            Assert.Contains("2", result.Roles);
        }

        // Test 2: Throws RpcException when user is not authenticated
        [Fact]
        public void GetUserData_UnauthenticatedUser_ThrowsRpcException()
        {
            // Arrange
            _userContext.GetCurrentUser().Returns((CurrentUser?)null);

            // Act & Assert
            var exception = Assert.Throws<RpcException>(() =>
                _userService.GetUserDataAsync(new Empty(), _serverCallContext)
            );

            Assert.Equal(StatusCode.Unauthenticated, exception.StatusCode);
            Assert.Equal("User is not authenticated", exception.Status.Detail);
        }

        // Test 3: Returns empty roles list when user has no roles
        [Fact]
        public void GetUserData_AuthenticatedUserWithNoRoles_ReturnsEmptyRoles()
        {
            // Arrange
            var mockUser = new CurrentUser(
                Id: "user-456",
                Name: "Abdo Saad",
                Roles: new List<string>()
            );

            _userContext.GetCurrentUser().Returns(mockUser);

            // Act
            var result = _userService.GetUserDataAsync(new Empty(), _serverCallContext);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user-456", result.UserId);
            Assert.Equal("Abdo Saad", result.UserName);
            Assert.Empty(result.Roles);
        }

        // Test 4: Verifies GetCurrentUser() is called exactly once
        [Fact]
        public void GetUserData_Always_CallsGetCurrentUserOnce()
        {
            // Arrange
            var mockUser = new CurrentUser(
                Id: "user-789",
                Name: "Abdo Saad",
                Roles: new List<string> { "1", "2" }
            );

            _userContext.GetCurrentUser().Returns(mockUser);

            // Act
            _userService.GetUserDataAsync(new Empty(), _serverCallContext);

            // Assert
            _userContext.Received(1).GetCurrentUser();
        }

        // Test 5: Returns correct user data with a single role
        [Fact]
        public void GetUserData_AuthenticatedUserWithSingleRole_ReturnsSingleRole()
        {
            // Arrange
            var mockUser = new CurrentUser(
                Id: "user-101",
                Name: "Single Role User",
                Roles: new List<string> { "1" }
            );

            _userContext.GetCurrentUser().Returns(mockUser);

            // Act
            var result = _userService.GetUserDataAsync(new Empty(), _serverCallContext);

            // Assert
            Assert.Single(result.Roles);
            Assert.Equal("1", result.Roles[0]);
        }
    }
}