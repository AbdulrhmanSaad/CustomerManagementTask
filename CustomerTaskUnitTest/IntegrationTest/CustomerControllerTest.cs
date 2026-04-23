using CustomersTask4.Abstraction;
using CustomersTask4.Controllers;
using CustomersTask4.CQRS.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.CQRS.CustomerHandler.Command.DeleteCustomer;
using CustomersTask4.CQRS.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.CQRS.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.CQRS.CustomerHandler.Query.GetCustomerAddressesHistory;
using CustomersTask4.CQRS.CustomerHandler.Query.GetCustomerById;
using CustomersTask4.CQRS.CustomerHandler.Query.GetCustomerHistory;
using CustomersTask4.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Services;

namespace CustomerTaskUnitTest.IntegrationTest
{
    public class CustomerControllerTest
    {
        private readonly IAppMeditor _mediator;
        private readonly CustomerController _controller;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<CustomerController> logger;
        private readonly ILocalizationService localization;

        public CustomerControllerTest()
        {
            _mediator = Substitute.For<IAppMeditor>();
            scopeFactory = Substitute.For<IServiceScopeFactory>();
            logger = Substitute.For<ILogger<CustomerController>>();
            localization = Substitute.For<ILocalizationService>();
            _controller = new CustomerController(_mediator,scopeFactory,logger,localization);
        }

        #region GetAll

        [Fact]
        public async Task GetAll_ReturnsOkWithCustomers()
        {
            // Arrange
            var customers = new List<CustomerDto>
            {
                new CustomerDto { Id = "1", Name = "Alice", Phone = "01013513652", CreatedAt = DateTime.UtcNow, CreatedBy = "abdo@gmail.com" },
                new CustomerDto { Id = "2", Name = "Bob", Phone = "01013513656", CreatedAt = DateTime.UtcNow, CreatedBy = "abdo@gmail.com" }
            };

            _mediator.Send<IEnumerable<CustomerDto>>(Arg.Any<GetAllCustomerQuery>())
                .Returns(Task.FromResult((IEnumerable<CustomerDto>)customers));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCustomers = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(okResult.Value);
            Assert.Equal(customers.Count, returnedCustomers.Count());
            Assert.Equal(customers, returnedCustomers);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoCustomers()
        {
            // Arrange
            var emptyList = new List<CustomerDto>();

            _mediator.Send<IEnumerable<CustomerDto>>(Arg.Any<GetAllCustomerQuery>())
                .Returns(Task.FromResult((IEnumerable<CustomerDto>)emptyList));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(okResult.Value);
            Assert.Empty(value);
        }

        #endregion

        #region GetCustomerById

        [Fact]
        public async Task GetCustomerById_ReturnsOkWithCustomer()
        {
            // Arrange
            var customer = new CustomerDto { Id = "1", Name = "Alice", Phone = "01013513652", CreatedAt = DateTime.UtcNow, CreatedBy = "abdo@gmail.com" };

            _mediator.Send<CustomerDto>(Arg.Any<GetCustomerByIdQuery>())
                .Returns(Task.FromResult(customer));

            // Act
            var result = await _controller.GetCustomerById("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(customer, okResult.Value);
        }

        [Fact]
        public async Task GetCustomerById_SendsQueryWithCorrectId()
        {
            // Arrange
            var customer = new CustomerDto { Id = "32", Name = "Charlie" };

            _mediator.Send<CustomerDto>(Arg.Any<GetCustomerByIdQuery>())
                .Returns(Task.FromResult(customer));

            // Act
            await _controller.GetCustomerById("32");

            // Assert
            await _mediator.Received(1).Send<CustomerDto>(Arg.Is<GetCustomerByIdQuery>(q => q.id == "32"));
        }

        #endregion

        #region DeleteCustomer

        [Fact]
        public async Task DeleteCustomer_ReturnsOkWithMessage()
        {
            // Arrange
            var expectedMessage = "Customer Deleted Successfully";
            localization.Localize("Customer Deleted Successfully").Returns(expectedMessage);

            _mediator.Send(Arg.Any<DeleteCustomerCommand>())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteCustomer("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedMessage, okResult.Value);
        }

        [Fact]
        public async Task DeleteCustomer_SendsCommandWithCorrectId()
        {
            // Arrange
            _mediator.Send(Arg.Any<DeleteCustomerCommand>())
                .Returns(Task.CompletedTask);

            // Act
            await _controller.DeleteCustomer("42");

            // Assert
            await _mediator.Received(1).Send(Arg.Is<DeleteCustomerCommand>(c => c.Id == "42"));
        }

        #endregion

        #region AddCustomer

        [Fact]
        public async Task AddCustomer_ReturnsOkWithMessage()
        {
            // Arrange
            var command = new CreateCustomerCommand { Name = "Dave" };
            var expectedMessage = "Customer Added from version 1";
            localization.Localize("Customer Added from version 1").Returns(expectedMessage);

            _mediator.Send(Arg.Any<CreateCustomerCommand>())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddCustomer(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedMessage, okResult.Value);
        }

        [Fact]
        public async Task AddCustomer_SendsCorrectCommand()
        {
            // Arrange
            var command = new CreateCustomerCommand { Name = "Eve" };

            _mediator.Send(Arg.Any<CreateCustomerCommand>())
                .Returns(Task.CompletedTask);

            // Act
            await _controller.AddCustomer(command);

            // Assert
            await _mediator.Received(1).Send(command);
        }

        #endregion

        #region UpdateCustomer

        [Fact]
        public async Task UpdateCustomer_ReturnsOkWithMessage()
        {
            // Arrange
            var command = new UpdateCustomerCommand { Name = "Frank" };
            var expectedMessage = "Customer Updated";
            localization.Localize("Customer Updated").Returns(expectedMessage);

            _mediator.Send(Arg.Any<UpdateCustomerCommand>())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateCustomer(command, "10");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedMessage, okResult.Value);
        }

        [Fact]
        public async Task UpdateCustomer_SetsIdFromRoute()
        {
            // Arrange
            var command = new UpdateCustomerCommand { Name = "Grace" };

            _mediator.Send(Arg.Any<UpdateCustomerCommand>())
                .Returns(Task.CompletedTask);

            // Act
            await _controller.UpdateCustomer(command, "99");

            // Assert
            Assert.Equal("99", command.Id);
            await _mediator.Received(1).Send(Arg.Is<UpdateCustomerCommand>(c => c.Id == "99"));
        }

        #endregion

        #region GetCustomerHistory

        [Fact]
        public async Task GetCustomerHistory_ReturnsOkWithHistory()
        {
            // Arrange
            var history = new CustomerHistoryResponse
            {
                Name = "Abdo Saad"
            };

            _mediator.Send<CustomerHistoryResponse>(Arg.Any<GetCustomerHistoryQuery>())
                .Returns(Task.FromResult((CustomerHistoryResponse)(object)history));

            // Act
            var result = await _controller.GetCustomerHistory("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetCustomerHistory_SendsQueryWithCorrectId()
        {
            // Arrange
            _mediator.Send<CustomerHistoryResponse>(Arg.Any<GetCustomerHistoryQuery>())
                .Returns(Task.FromResult((CustomerHistoryResponse)null));

            // Act
            await _controller.GetCustomerHistory("69a5ab9bfe4b58bfdfcf9836");

            // Assert
            await _mediator.Received(1).Send<CustomerHistoryResponse>(Arg.Is<GetCustomerHistoryQuery>(q => q.CustomerId == "69a5ab9bfe4b58bfdfcf9836"));
        }

        #endregion

        #region GetCustomerAddressHistory

        [Fact]
        public async Task GetCustomerAddressHistory_ReturnsOkWithHistory()
        {
            // Arrange
            var addresses=new List<AddressDto>
            {
                new AddressDto
                {
                AddressName="Cairo",
                AddressType="Home"
                }
            };

            _mediator.Send<List<AddressDto>>(Arg.Any<GetCustomerAddressesHistoryQuery>())
                           .Returns(Task.FromResult(addresses));


            // Act
            var result = await _controller.GetCustomerAddressHistory("69cd6317233843a4f3531510");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
            //Assert.Equal(addresses, okResult.Value);

        }
        #endregion
    }
}