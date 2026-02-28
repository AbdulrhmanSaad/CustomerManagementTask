using CustomersTask4.Abstraction;
using CustomersTask4.Controllers;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.UserHandler.Command;
using CustomersTask4.UserHandler.Command.LoginUser;
using CustomersTask4.UserHandler.Command.RefreshToken;
using CustomersTask4.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;

namespace CustomersTask4.Tests.Controllers
{
    public class IdentityControllerTests
    {
        private readonly IAppMeditor _mediator;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IdentityController _controller;

        public IdentityControllerTests()
        {
            _mediator = Substitute.For<IAppMeditor>();

            var userStore = Substitute.For<IUserStore<User>>();
            _userManager = Substitute.For<UserManager<User>>(
                userStore, null, null, null, null, null, null, null, null);

            _signInManager = Substitute.For<SignInManager<User>>(
                _userManager,
                Substitute.For<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Substitute.For<IUserClaimsPrincipalFactory<User>>(),
                null, null, null, null);

            _controller = new IdentityController(_mediator, _userManager, _signInManager);
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenRequestIsValid()
        {
            var command = new RegisterNewUserCommand();

            var result = await _controller.Register(command);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            await _mediator.Received(1).Send(command);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenModelStateInvalid()
        {
            var command = new RegisterNewUserCommand();
            _controller.ModelState.AddModelError("key", "error");

            var result = await _controller.Register(command);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ShouldReturnToken()
        {
            var command = new LoginUserCommand() { Email="abdo@gmail.com",Password="Test@12"};
            var res = new LoginDto() { AccessToken = "fake-jwt-token", ExpiresIn = 3600, RefreshToken = "fake-jwt-token", tokenType = "Bearer" };
            _mediator.Send(command).Returns(res);

            var result = await _controller.Login(command);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnOk_WhenValid()
        {
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid-refresh-token"
            };

            var res = new LoginDto() { AccessToken = "fake-jwt-token", ExpiresIn = 3600, RefreshToken = "fake-jwt-token", tokenType = "Bearer" };
            _mediator.Send(command).Returns(res);

            var result = await _controller.RefreshToken(command);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenMissing()
        {
            var command = new RefreshTokenCommand
            {
                RefreshToken = ""
            };

            var result = await _controller.RefreshToken(command);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task AddRoleToUser_ShouldReturnNoContent()
        {
            var command = new AssignUserRoleCommand();

            var result = await _controller.AddRoleToUser(command);

            Assert.IsType<NoContentResult>(result);
            await _mediator.Received(1).Send(command);
        }
    }
}