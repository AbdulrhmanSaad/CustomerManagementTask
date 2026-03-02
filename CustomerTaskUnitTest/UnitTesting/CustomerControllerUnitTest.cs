using CustomersTask4.Abstraction;
using CustomersTask4.Controllers;
using CustomersTask4.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand;
using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory;
using CustomersTask4.CustomerHandler.Query.GetCustomerById;
using CustomersTask4.CustomerHandler.Query.GetCustomerHistory;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace CustomersTaskUnitTest.UnitTesting
{
    public class CustomerControllerUnitTest
    {
        private readonly IAppMeditor _mediator;
        private readonly CustomerController _controller;

        public CustomerControllerUnitTest()
        {
            _mediator = Substitute.For<IAppMeditor>();
            _controller = new CustomerController(_mediator);
        }

        #region GetAll

        [Fact]
        public async Task GetAll_ReturnsOkWithCustomers()
        {
            // Arrange
            var customers = new List<CustomerDto>
            {
                new CustomerDto { Id = "1", Name = "Alice",Phone="01013513652",CreatedAt=DateTime.UtcNow,CreatedBy="abdo@gmail.com" },
                new CustomerDto { Id = "2", Name = "Bob",Phone="01013513656",CreatedAt=DateTime.UtcNow,CreatedBy="abdo@gmail.com"  }
            };
            _mediator.Send(Arg.Any<GetAllCustomerQuery>()).Returns(customers);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(customers, ok.Value);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoCustomers()
        {
            // Arrange
            _mediator.Send(Arg.Any<GetAllCustomerQuery>()).Returns(new List<CustomerDto>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<CustomerDto>>(ok.Value);
            Assert.Empty(value);
        }

        #endregion

        #region GetCustomerById

        [Fact]
        public async Task GetCustomerById_ReturnsOkWithCustomer()
        {
            // Arrange
            var customer = new CustomerDto { Id = "1", Name = "Alice", Phone = "01013513652", CreatedAt = DateTime.UtcNow, CreatedBy = "abdo@gmail.com" };
            _mediator.Send(Arg.Any<GetCustomerByIdQuery>()).Returns(customer);

            // Act
            var result = await _controller.GetCustomerById("1");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(customer, ok.Value);
        }

        [Fact]
        public async Task GetCustomerById_SendsQueryWithCorrectId()
        {
            // Arrange
            var customer = new CustomerDto { Id = "32", Name = "Charlie" };
            _mediator.Send(Arg.Any<GetCustomerByIdQuery>()).Returns(customer);

            // Act
            await _controller.GetCustomerById("32");

            // Assert
            await _mediator.Received(1).Send(Arg.Is<GetCustomerByIdQuery>(q => q.id == "32"));
        }

        #endregion

        #region DeleteCustomer

        [Fact]
        public async Task DeleteCustomer_ReturnsOkWithMessage()
        {
            // Arrange
            _mediator.Send(Arg.Any<DeleteCustomerCommand>()).Returns(Task.FromResult(Unit.Value));

            // Act
            var result = await _controller.DeleteCustomer("1");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Customer Deleted Successfully", ok.Value);
        }

        [Fact]
        public async Task DeleteCustomer_SendsCommandWithCorrectId()
        {
            // Arrange
            _mediator.Send(Arg.Any<DeleteCustomerCommand>()).Returns(Task.FromResult(Unit.Value));

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
            _mediator.Send(Arg.Any<CreateCustomerCommand>()).Returns(Task.FromResult(Unit.Value));

            // Act
            var result = await _controller.AddCustomer(command);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Customer Added", ok.Value);
        }

        [Fact]
        public async Task AddCustomer_SendsCorrectCommand()
        {
            // Arrange
            var command = new CreateCustomerCommand { Name = "Eve" };
            _mediator.Send(Arg.Any<CreateCustomerCommand>()).Returns(Task.FromResult(Unit.Value));

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
            _mediator.Send(Arg.Any<UpdateCustomerCommand>()).Returns(Task.FromResult(Unit.Value));

            // Act
            var result = await _controller.UpdateCustomer(command, "10");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Customer Updated", ok.Value);
        }

        [Fact]
        public async Task UpdateCustomer_SetsIdFromRoute()
        {
            // Arrange
            var command = new UpdateCustomerCommand { Name = "Grace" };
            _mediator.Send(Arg.Any<UpdateCustomerCommand>()).Returns(Task.FromResult(Unit.Value));

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
            var history = new List<CustomerHistoryResponse>(){
                new CustomerHistoryResponse { Name = "Abdo Saad" },
                new CustomerHistoryResponse { Name = "Abdo" },
            };
            _mediator.Send(Arg.Any<GetCustomerHistoryQuery>()).Returns(history);

            // Act
            var result = await _controller.GetCustomerHistory("1");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(history, ok.Value);
        }

        [Fact]
        public async Task GetCustomerHistory_SendsQueryWithCorrectId()
        {
            // Arrange
            _mediator.Send(Arg.Any<GetCustomerHistoryQuery>()).Returns(new List<CustomerHistoryResponse>());

            // Act
            await _controller.GetCustomerHistory("7");

            // Assert
            await _mediator.Received(1).Send(Arg.Is<GetCustomerHistoryQuery>(q => q.CustomerId == ""));
        }

        #endregion

        #region GetCustomerAddressHistory

        [Fact]
        public async Task GetCustomerAddressHistory_ReturnsOkWithHistory()
        {
            // Arrange
            var history = new List<AddressDto>() { 
                new AddressDto { AddressType =AddressType.Work.ToString() ,AddressName="Cairo" },
                new AddressDto { AddressType =AddressType.Home.ToString() ,AddressName="Cairo" },
            };
            _mediator.Send(Arg.Any<GetCustomerAddressesHistoryQuery>()).Returns(history);

            // Act
            var result = await _controller.GetCustomerAddressHistory("3");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(history, ok.Value);
        }

        [Fact]
        public async Task GetCustomerAddressHistory_SendsQueryWithCorrectId()
        {
            // Arrange
            _mediator.Send(Arg.Any<GetCustomerAddressesHistoryQuery>()).Returns(new List<AddressDto>());

            // Act
            await _controller.GetCustomerAddressHistory("15");

            // Assert
            await _mediator.Received(1).Send(Arg.Is<GetCustomerAddressesHistoryQuery>(q => q.CustomerId == "15"));
        }

        #endregion
    }
}