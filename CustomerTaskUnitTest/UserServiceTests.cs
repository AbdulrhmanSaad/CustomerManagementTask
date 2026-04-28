using CustomersTask4.Users;
using CustomersTask4.GRPC.Services;
using Grpc.Core;
using NSubstitute;
using CustomersTask4;


namespace CustomerTaskUnitTest
{
        public class UserServiceTests
        {
            private readonly IUserContext _userContext;
            private readonly UserService _userService;
            private readonly ServerCallContext _serverCallContext;

            public UserServiceTests()
            {
                _userContext = Substitute.For<IUserContext>();
                _userService = new UserService(_userContext);
                _serverCallContext = Substitute.For<ServerCallContext>();
            }

            //Test 1: Returns correct user data when user is authenticated
            [Fact]
            public async Task GetUserData_AuthenticatedUser_ReturnsCorrectUserResponse()
            {
                // Arrange
                var mockUser = new CurrentUser(
                    Id: "user-123",
                    Name: "Abdo Saad",
                    Roles: ["1", "2"]
                );
                _userContext.GetCurrentUser().Returns(mockUser);

                var request = new GetUserByIdRequest();

                // Act
                var result = await _userService.GettUserData(request, _serverCallContext);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("user-123", result.UserId);
                Assert.Equal("Abdo Saad", result.UserName);
                Assert.Equal(2, result.Roles.Count);
                Assert.Contains("1", result.Roles);
                Assert.Contains("2", result.Roles);
            }

            //Test 2: Throws RpcException when user is not authenticated (null)
            [Fact]
            public async Task GetserData_UnauthenticatedUser_ThrowsRpcException()
            {
                // Arrange
                _userContext.GetCurrentUser().Returns((CurrentUser?)null);

                var request = new GetUserByIdRequest();

                // Act & Assert
                var exception = await Assert.ThrowsAsync<RpcException>(
                    () => _userService.GettUserData(request, _serverCallContext)
                );

                Assert.Equal(StatusCode.Unauthenticated, exception.Status.StatusCode);
                Assert.Equal("User is not authenticated", exception.Status.Detail);
            }

            //Test 3: Returns empty roles list when user has no roles
            [Fact]
            public async Task GetUserData_AuthenticatedUserWithNoRoles_ReturnsEmptyRoles()
            {
                // Arrange
                var mockUser = new CurrentUser(
                    Id: "user-456",
                    Name: "Abdo Saad",
                    Roles: []
                );
                _userContext.GetCurrentUser().Returns(mockUser);

                var request = new GetUserByIdRequest();

                // Act
                var result = await _userService.GettUserData(request, _serverCallContext);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("user-456", result.UserId);
                Assert.Equal("Abdo Saad", result.UserName);
                Assert.Empty(result.Roles);
            }

            //Test 4: Verifies GetCurrentUser() is called exactly once
            [Fact]
            public async Task GetUserData_Always_CallsGetCurrentUserOnce()
            {
                // Arrange
                var mockUser = new CurrentUser(
                    Id: "user-789",
                    Name: "Abdo Saad",
                    Roles: ["1", "2"]
                );
                _userContext.GetCurrentUser().Returns(mockUser);

                var request = new GetUserByIdRequest();

                // Act
                await _userService.GettUserData(request, _serverCallContext);

                // Assert
                _userContext.Received(1).GetCurrentUser();
            }

            //Test 5: Returns correct user data with a single role
            [Fact]
            public async Task GetUserData_AuthenticatedUserWithSingleRole_ReturnsSingleRole()
            {
                // Arrange
                var mockUser = new CurrentUser(
                    Id: "user-101",
                    Name: "Single Role User",
                    Roles: ["1"]
                );
                _userContext.GetCurrentUser().Returns(mockUser);

                var request = new GetUserByIdRequest();

                // Act
                var result = await _userService.GettUserData(request, _serverCallContext);

                // Assert
                Assert.Single(result.Roles);
                Assert.Equal("1", result.Roles[0]);
            }
        }
}
