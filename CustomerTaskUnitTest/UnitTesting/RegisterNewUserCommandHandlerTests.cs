using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Users;
using CustomersTask4.UserHandler.Command;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CustomersTask4.Services;

namespace CustomersTaskUnitTest.UnitTesting;
public class RegisterNewUserCommandHandlerTests
{
    private readonly IAppUserManager _userManager;
    private readonly RegisterNewUserCommandHandler _handler;

    public RegisterNewUserCommandHandlerTests()
    {
        var userStore = Substitute.For<IUserStore<User>>();

        _userManager = Substitute.For<IAppUserManager>();

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

        _userManager.CreateAsync(Arg.Any<IAppUser>(), command.Password)
            .Returns(true);

        _userManager.AddToRoleAsync(Arg.Any<IAppUser>(), UserRoles.User)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(Mediator.Unit.Value, result);
        await _userManager.Received(1).CreateAsync(Arg.Any<IAppUser>(), command.Password);
        await _userManager.Received(1).AddToRoleAsync(Arg.Any<IAppUser>(), UserRoles.User);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserCreationFails()
    {
        // Arrange
        var command = new RegisterNewUserCommand
        {
            Email = "test@gmail.com",
            Password = "Test@123"
        };

        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None).AsTask());

        Assert.Equal("Username Already Exists", ex.Message);
        await _userManager.DidNotReceive().CreateAsync(Arg.Any<User>(), command.Password);
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>());
    }
}