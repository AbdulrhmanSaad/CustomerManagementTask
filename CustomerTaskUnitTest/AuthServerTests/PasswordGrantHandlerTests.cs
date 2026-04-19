using AuthServer.Domain;
using AuthServer.Handlers.PasswordGrand;
using AuthServer.Services;
using AuthServer.Setting;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using OpenIddict.Abstractions;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CustomerTaskUnitTest.AuthServerTests
{
    public class PasswordGrantHandlerTests
    {
        private readonly UserManager<User> _userManagerMock;
        private readonly ITenantService _tenantServiceMock;
        private readonly ILocalizationService _localizationMock;
        private readonly IPrincipalFactory _principalFactoryMock;
        private readonly PasswordGrantHandler _handler;

        public PasswordGrantHandlerTests()
        {
            var storeMock = Substitute.For<IUserStore<User>>();
            _userManagerMock = Substitute.For<UserManager<User>>(
                storeMock, null, null, null, null, null, null, null, null);

            _tenantServiceMock = Substitute.For<ITenantService>();
            _localizationMock = Substitute.For<ILocalizationService>();
            _principalFactoryMock = Substitute.For<IPrincipalFactory>();

            _handler = new PasswordGrantHandler(
                _userManagerMock,
                _tenantServiceMock,
                _localizationMock,
                _principalFactoryMock);
        }

        // ── helpers ────────────────────────────────────────────────────────────────

        private static User MakeUser(string tenantId) =>
            new() { UserName = "abdo", TenantId = tenantId };

        private static ClaimsPrincipal MakePrincipal() =>
            new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "abdo") }));

        private static PasswordGrantCommand MakeCommand() =>
            new PasswordGrantCommand { UserName = "abdo", Password = "Test@123", Scopes = new List<string>() { "openid", "offline_access" } };

        // ── user not found ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            _userManagerMock
                .FindByNameAsync("abdo")
                .Returns((User?)null);

            var result = await _handler.Handle(MakeCommand());

            Assert.False(result.IsSuccess);
            Assert.Equal(_localizationMock.Localize("Invalid User Name OR Password"), result.ErrorMessage);
        }

        // ── wrong password ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_WrongPassword_ReturnsFailure()
        {
            var user = MakeUser("SharedTenant");

            _userManagerMock.FindByNameAsync("abdo").Returns(user);
            _userManagerMock.CheckPasswordAsync(user, "P@ssw0rd").Returns(false);

            var result = await _handler.Handle(MakeCommand());

            Assert.False(result.IsSuccess);
            Assert.Equal(_localizationMock.Localize("Invalid User Name OR Password"), result.ErrorMessage);
        }

        // ── tenant mismatch ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_TenantMismatch_ReturnsFailure()
        {
            var user = MakeUser("SharedTenant");

            _userManagerMock.FindByNameAsync("abdo").Returns(user);
            _userManagerMock.CheckPasswordAsync(user, "Test@123").Returns(true);
            _tenantServiceMock.GetCurrentTenant().Returns(new Tenant { TenantId = "Tenant1" });

            var result = await _handler.Handle(MakeCommand());

            Assert.False(result.IsSuccess);
            Assert.Contains(_localizationMock.Localize("Invalid tenant"), result.ErrorMessage);
        }

        // ── happy path ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsSuccessWithPrincipal()
        {
            var user = MakeUser("SharedTenant");
            var principal = MakePrincipal();

            _userManagerMock.FindByNameAsync("abdo").Returns(user);
            _userManagerMock.CheckPasswordAsync(user, "Test@123").Returns(true);
            _tenantServiceMock.GetCurrentTenant().Returns(new Tenant { TenantId = "SharedTenant" });
            _principalFactoryMock.CreatePrincipal(user, "SharedTenant").Returns(principal);

            var result = await _handler.Handle(MakeCommand());

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Principal);
            Assert.Same(principal, result.Principal);
        }

    }
}
