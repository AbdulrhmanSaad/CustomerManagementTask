using CustomersTask4.Abstraction;
using CustomersTask4.Controllers;
using CustomersTask4.DTO;
using CustomersTask4.UserHandler.Command;
using CustomersTask4.UserHandler.Command.AssignUserRole;
using CustomersTask4.UserHandler.Command.LoginUser;
using CustomersTask4.UserHandler.Command.RefreshToken;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shared.Services;

namespace CustomerTaskUnitTest.IntegrationTest
{
    public class IdentityControllerTests
    {
        private readonly IAppMeditor _mediator;
        private readonly ILocalizationService _localization;
        private readonly IdentityController _controller;

        public IdentityControllerTests()
        {
            _mediator = Substitute.For<IAppMeditor>();
            _localization = Substitute.For<ILocalizationService>();

            _controller = new IdentityController(_mediator, _localization);
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenRequestIsValid()
        {
            var command = new CustomersTask4.UserHandler.Command.RegisterNewUserCommand();
            _localization.Localize("User registered successfully").Returns("User registered successfully");

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
            // Arrange
            var command = new LoginUserCommand
            {
                Email = "abdo@gmail.com",
                Password = "Test@12"
            };

            var loginDto = new LoginDto
            {
                AccessToken = "fake-jwt-token",
                ExpiresIn = 3600,
                RefreshToken = "fake-jwt-token",
                tokenType = "Bearer"
            };

            _mediator.Send<LoginDto>(command).Returns(Task.FromResult(loginDto));

            // Act
            var result = await _controller.Login(command);

            // Assert
            Assert.NotNull(result);

            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(loginDto, okResult.Value);

            await _mediator.Received(1).Send<LoginDto>(command);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnOk_WhenValid()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = "valid-refresh-token"
            };

            var loginDto = new LoginDto
            {
                AccessToken = "fake-jwt-token",
                ExpiresIn = 3600,
                RefreshToken = "fake-jwt-token",
                tokenType = "Bearer"
            };

            _mediator.Send<LoginDto>(command).Returns(Task.FromResult(loginDto));

            // Act
            var result = await _controller.RefreshToken(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(loginDto, okResult.Value);

            await _mediator.Received(1).Send<LoginDto>(command);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnUnauthorized_WhenTokenMissing()
        {
            // Arrange
            var command = new RefreshTokenCommand
            {
                RefreshToken = ""
            };

            _localization.Localize("Refresh token is required").Returns("Refresh token is required");

            // Act
            var result = await _controller.RefreshToken(command);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task AddRoleToUser_ShouldReturnNoContent()
        {
            // Arrange
            var command = new AssignUserRoleCommand();

            // Act
            var result = await _controller.AddRoleToUser(command);

            // Assert
            Assert.IsType<NoContentResult>(result);
            await _mediator.Received(1).Send(command);
        }
    }
}