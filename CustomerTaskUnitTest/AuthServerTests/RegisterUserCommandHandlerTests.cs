using AuthServer.Domain;
using AuthServer.DTO;
using AuthServer.Handlers.RegisterHandler;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerTaskUnitTest.AuthServerTests
{
    public class RegisterUserCommandHandlerTests
    {
            private readonly UserManager<User> _userManagerMock;
            private readonly ILogger<RegisterUserCommandHandler> _loggerMock;
            private readonly ILocalizationService _localizationMock;
            private readonly RegisterUserCommandHandler _handler;

            public RegisterUserCommandHandlerTests()
            {
                var storeMock = Substitute.For<IUserStore<User>>();
                _userManagerMock = Substitute.For<UserManager<User>>(
                    storeMock, null, null, null, null, null, null, null, null);

                _loggerMock = Substitute.For<ILogger<RegisterUserCommandHandler>>();
                _localizationMock = Substitute.For<ILocalizationService>();

                _handler = new RegisterUserCommandHandler(
                    _userManagerMock,
                    _loggerMock,
                    _localizationMock);
            }

            // ── Helpers ──────────────────────────────────────────────

            private static RegisterUserCommand ValidCommand() => new()
            {
                Username = "ahmedSaad",
                Email = "ahmed@gmail.com",
                Password = "Test@123"
            };

            private static IdentityResult FailureResult(params string[] errors) =>
                IdentityResult.Failed(errors.Select(e => new IdentityError { Description = e }).ToArray());

            // ── Happy-path tests ──────────────────────────────────────

            [Fact]
            public async Task Handle_ValidRequest_ReturnsSuccess()
            {
                // Arrange
                _userManagerMock
                    .CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                    .Returns(IdentityResult.Success);

                _userManagerMock
                    .AddToRoleAsync(Arg.Any<User>(), UserRoles.User)
                    .Returns(IdentityResult.Success);

                // Act
                var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

                // Assert
                Assert.True(result.Succeeded);
            }


            [Fact]
            public async Task Handle_UserManagerFailure_ReturnsFailure()
            {
                // Arrange
                _userManagerMock
                    .CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                    .Returns(FailureResult("Email already taken."));

                // Act
                var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

                // Assert
                Assert.False(result.Succeeded);
            }

            [Fact]
            public async Task Handle_UserManagerFailure_ReturnsAllErrors()
            {
                // Arrange
                var errors = new[] { "Email already taken.", "Username unavailable." };

                _userManagerMock
                    .CreateAsync(Arg.Any<User>(), Arg.Any<string>())
                    .Returns(FailureResult(errors));

                // Act
                var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

                // Assert
                Assert.All(errors, e => Assert.Contains(e, result.Errors));
            }

    }

}
