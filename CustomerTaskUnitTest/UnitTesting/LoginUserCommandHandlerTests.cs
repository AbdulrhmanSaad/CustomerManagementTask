using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using CustomersTask4.Setting;
using CustomersTask4.UserHandler.Command.LoginUser;
using NSubstitute;
using Shared.Services;

namespace CustomersTaskUnitTest.UnitTesting;

public class LoginUserCommandHandlerTests
{
    private readonly IAppUserManager _userManager;
    private readonly IUserTokenMangerService _tokenService;
    private readonly ILocalizationService localization;
    private readonly ITenantService tenantService;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _userManager = Substitute.For<IAppUserManager>();
        _tokenService = Substitute.For<IUserTokenMangerService>();
        localization = Substitute.For<ILocalizationService>();
        tenantService = Substitute.For<ITenantService>();

        _handler = new LoginUserCommandHandler(
            _userManager,
            _tokenService,
            localization,
            tenantService
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnLoginDto_WhenCredentialsAreValid()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "abdo@gmail.com",
            Password = "Test@12"
        };

        var user = new User { Email = command.Email };

        // Same reference used in both GetRolesAsync stub and GenerateJwtToken stub
        var roles = new List<string> { "User" };

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password).Returns(true);

        _userManager.GetRolesAsync(user).Returns(roles);
        var tenant = new Tenant { TenantId = "Tenant1" };
        tenantService.GetCurrentTenant().Returns(tenant);
        _tokenService.GenerateJwtToken(user, roles,"Tenant1").Returns("access-token");
        _tokenService.GenerateRefreshToken().Returns("refresh-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);

        await _userManager.Received(1).UpdateAsync(user);
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
            _handler.Handle(command, CancellationToken.None));
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
        _userManager.CheckPasswordAsync(user, command.Password).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}