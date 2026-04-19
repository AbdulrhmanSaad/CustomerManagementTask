using AuthServer.Domain;
using AuthServer.Handlers.PasswordGrand;
using AuthServer.Handlers.RefreshTokenGrand;
using AuthServer.Services;
using AuthServer.Setting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public class RefreshTokenGrantHandlerTests
    {
        private readonly UserManager<User> _userManagerMock;
        private readonly ITenantService _tenantServiceMock;
        private readonly ILocalizationService _localizationMock;
        private readonly IPrincipalFactory _principalFactoryMock;
        private readonly RefreshTokenGrantCommandHandler _handler;

        public RefreshTokenGrantHandlerTests()
        {
            var storeMock = Substitute.For<IUserStore<User>>();
            
            // Create UserManager with non-null substitutes for required parameters
            _userManagerMock = Substitute.For<UserManager<User>>(
                storeMock, null, null, null, null, null, null, null, null);

            

            _tenantServiceMock = Substitute.For<ITenantService>();
            _localizationMock = Substitute.For<ILocalizationService>();
            _principalFactoryMock = Substitute.For<IPrincipalFactory>();

            _handler = new RefreshTokenGrantCommandHandler(
                _userManagerMock,
                _tenantServiceMock,
                _localizationMock,
                _principalFactoryMock);
        }

        // ── helpers ─────────────────────────────────────────────────────────────────

        private static ClaimsPrincipal MakeIncomingPrincipal(string userId = "user-42") =>
            new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));

        private static User MakeUser(string tenantId, string userName = "abdo") =>
            new() { UserName = userName, TenantId = tenantId };

        private static ClaimsPrincipal MakeOutgoingPrincipal() =>
            new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "abdo") }));

        private static RefreshTokenGrantCommand MakeCommand() =>
            new RefreshTokenGrantCommand
            {
                Principal = MakeIncomingPrincipal(),
                Scopes = new List<string>() { "openid", "offline_access" }
            };

        // ── user not found ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            _userManagerMock
                .GetUserAsync(Arg.Any<ClaimsPrincipal>())
                .Returns((User?)null);

            var result = await _handler.Handle(MakeCommand());

            Assert.False(result.IsSuccess);
            Assert.Equal(_localizationMock.Localize("Invalid User Name OR Password"), result.ErrorMessage);
        }
        // ── tenant mismatch ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_TenantMismatch_ReturnsFailure()
        {
            var user = MakeUser("SharedTenant");

            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
            _tenantServiceMock.GetCurrentTenant().Returns(new Tenant { TenantId = "Tenant1" });

            var result = await _handler.Handle(MakeCommand());

            Assert.False(result.IsSuccess);
            Assert.Contains(_localizationMock.Localize("Invalid tenant"), result.ErrorMessage);
        }

        // ── happy path ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidRefreshToken_ReturnsSuccessWithPrincipal()
        {
            var user = MakeUser("SharedTenant");
            var outgoing = MakeOutgoingPrincipal();

            _userManagerMock.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
            _tenantServiceMock.GetCurrentTenant().Returns(new Tenant { TenantId = "SharedTenant" });
            _principalFactoryMock.CreatePrincipal(user, "SharedTenant").Returns(outgoing);

            var result = await _handler.Handle(MakeCommand());

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Principal);
            Assert.Same(outgoing, result.Principal);
        }
    }
}
