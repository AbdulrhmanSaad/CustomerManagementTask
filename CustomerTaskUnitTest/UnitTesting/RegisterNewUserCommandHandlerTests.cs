using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Users;
using CustomersTask4.UserHandler.Command;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CustomersTaskUnitTest.UnitTesting;
public class RegisterNewUserCommandHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly RegisterNewUserCommandHandler _handler;

    public RegisterNewUserCommandHandlerTests()
    {
        var userStore = Substitute.For<IUserStore<User>>();

        _userManager = Substitute.For<UserManager<User>>(
            userStore, null, null, null, null, null, null, null, null);

        _handler = new RegisterNewUserCommandHandler(_userManager);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUserSuccessfully()
    {
        // Arrange
        var command = new RegisterNewUserCommand
        {
            Email = "test@gmail.com",
            Password = "Test@12"
        };

        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Success);

        _userManager.AddToRoleAsync(Arg.Any<User>(), UserRoles.User)
            .Returns(new IdentityResult());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Mediator.Unit.Value, result);
        await _userManager.Received(1).CreateAsync(Arg.Any<User>(), command.Password);
        await _userManager.Received(1).AddToRoleAsync(Arg.Any<User>(), UserRoles.User);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserCreationFails()
    {
        // Arrange
        var command = new RegisterNewUserCommand
        {
            Email = "test@gmail.com",
            Password = "Test@12"
        };

        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Username Already Exists" }));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None).AsTask());

        Assert.Equal("Username Already Exists", ex.Message);
        await _userManager.Received(1).CreateAsync(Arg.Any<User>(), command.Password);
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>());
    }
}