using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using CustomersTask4.UserHandler.Command.RefreshToken;
using Mediator;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CustomersTaskUnitTest.UnitTesting;

public class RefreshTokenCommandHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly IUserTokenMangerService _tokenService;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        var userStore = Substitute.For<Microsoft.AspNetCore.Identity.IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(
            userStore, null, null, null, null, null, null, null, null);

        _tokenService = Substitute.For<IUserTokenMangerService>();

        _handler = new RefreshTokenCommandHandler(_userManager, _tokenService);
    }

    [Fact]
    public async Task Handle_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            AccessToken = "expired-access-token",
            RefreshToken = "valid-refresh-token"
        };

        var claims = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@gmail.com")
            }));

        var user = new User
        {
            Email = "test@gmail.com",
            RefreshToken = "valid-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1)
        };

        _tokenService.GetPrincipalFromExpiredToken(command.AccessToken).Returns(claims);
        _userManager.FindByEmailAsync("test@gmail.com").Returns(user);
        _tokenService.GenerateJwtToken(user).Returns("new-access-token");
        _tokenService.GenerateRefreshToken().Returns("new-refresh-token");
        _userManager.UpdateAsync(user).Returns(new IdentityResult());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.Equal("new-refresh-token", user.RefreshToken);
        Assert.True(user.RefreshTokenExpiryTime > DateTime.UtcNow);

        await _userManager.Received(1).UpdateAsync(user);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenAccessTokenInvalid()
    {
        var command = new RefreshTokenCommand { AccessToken = "invalid", RefreshToken = "rt" };
        _tokenService.GetPrincipalFromExpiredToken(command.AccessToken).Returns((ClaimsPrincipal)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRefreshTokenExpired()
    {
        var command = new RefreshTokenCommand
        {
            AccessToken = "expired-access-token",
            RefreshToken = "old-refresh-token"
        };

        var claims = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@gmail.com")
            }));

        var user = new User
        {
            Email = "test@gmail.com",
            RefreshToken = "old-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(-1)
        };

        _tokenService.GetPrincipalFromExpiredToken(command.AccessToken).Returns(claims);
        _userManager.FindByEmailAsync("test@gmail.com").Returns(user);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenRefreshTokenDoesNotMatch()
    {
        var command = new RefreshTokenCommand
        {
            AccessToken = "expired-access-token",
            RefreshToken = "invalid-refresh-token"
        };

        var claims = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "test@gmail.com")
            }));

        var user = new User
        {
            Email = "test@gmail.com",
            RefreshToken = "some-other-refresh-token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1)
        };

        _tokenService.GetPrincipalFromExpiredToken(command.AccessToken).Returns(claims);
        _userManager.FindByEmailAsync("test@gmail.com").Returns(user);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None).AsTask());
    }
}