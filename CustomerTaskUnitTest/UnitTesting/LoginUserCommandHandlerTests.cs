using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using CustomersTask4.UserHandler.Command.LoginUser;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
namespace CustomersTaskUnitTest.UnitTesting;
public class LoginUserCommandHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUserTokenMangerService _tokenService;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        var userStore = Substitute.For<IUserStore<User>>();

        _userManager = Substitute.For<UserManager<User>>(
            userStore, null, null, null, null, null, null, null, null);

        _signInManager = Substitute.For<SignInManager<User>>(
            _userManager,
            Substitute.For<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<User>>(),
            null, null, null, null);

        _tokenService = Substitute.For<IUserTokenMangerService>();

        _handler = new LoginUserCommandHandler(
            _userManager,
            _signInManager,
            _tokenService
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnLoginDto_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "test@test.com",
            Password = "123456"
        };

        var user = new User { Email = command.Email };

        _userManager.FindByEmailAsync(command.Email).Returns(user);

        _signInManager.CheckPasswordSignInAsync(user, command.Password, false)
            .Returns(SignInResult.Success);

        _tokenService.GenerateJwtToken(user).Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns("refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);

        await _userManager.Received(1).UpdateAsync(user);
        await _signInManager.Received(1).SignInAsync(user, false);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "notfound@test.com",
            Password = "123"
        };

        _userManager.FindByEmailAsync(command.Email).Returns((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenPasswordInvalid()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "test@test.com",
            Password = "wrong"
        };

        var user = new User { Email = command.Email };

        _userManager.FindByEmailAsync(command.Email).Returns(user);

        _signInManager.CheckPasswordSignInAsync(user, command.Password, false)
            .Returns(SignInResult.Failed);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None).AsTask());
    }
}