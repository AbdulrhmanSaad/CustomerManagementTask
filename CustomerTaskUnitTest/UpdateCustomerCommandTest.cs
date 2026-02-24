using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Users;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Linq.Expressions;
using Xunit;

namespace CustomerTaskUnitTest
{
    public class UpdateCustomerCommandTest
    {
        private readonly UpdateCustomerCommandHandler _handler;
        private readonly IGenericRepository<Customer> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateCustomerCommandHandler> _logger;
        private readonly IUserContext _userContext;

        private static readonly UpdateCustomerCommand _validCommand = new()
        {
            Id = 32,
            Name = "Ahmed Updated",
            Phone = "01013513652",
            AddressType = AddressType.Home,
            HomeAddressLocation = "Updated Alex Address",
            AddressType2 = AddressType.Work,
            WorkAddressLocation = "Updated Cairo Address"
        };

        public UpdateCustomerCommandTest()
        {
            _repository = Substitute.For<IGenericRepository<Customer>>();
            _mapper = Substitute.For<IMapper>();
            _logger = Substitute.For<ILogger<UpdateCustomerCommandHandler>>();
            _userContext = Substitute.For<IUserContext>();

            _handler = new UpdateCustomerCommandHandler(_repository, _logger, _mapper,_userContext);
        }

        #region Success Cases

        [Fact]
        public async Task Handle_WithValidCustomer_ShouldUpdateCustomerSuccessfully()
        {
            // Arrange
            var command = _validCommand;
            var existingCustomer = new Customer
            {
                Id = 32,
                Name = "updated now ",
                Phone = "01213213665",
                CreatedAt = DateTime.Now,
                CreatedBy = "abdo@gmail.com",
                Addresses = new List<Address>()
            };

            var oldCustomer = new Customer
            {
                Id = 32,
                Name = "Ahmed",
                Phone = "01013513652",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "user"
            };

            _repository.GetByIdAsync(command.Id, Arg.Any<Expression<Func<Customer, object>>>())
                .Returns(existingCustomer);


            _repository.PhoneExistsAsync(command.Phone).Returns(false);

            _mapper.Map<Customer>(existingCustomer).Returns(oldCustomer);

            _repository.Update(Arg.Any<Customer>()).Returns(Task.CompletedTask);


            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            await _repository.Received(1).Update(Arg.Any<Customer>());
        }

        [Fact]
        public async Task Handle_WithNewPhoneNumber_ShouldUpdatePhoneSuccessfully()
        {
            // Arrange
            var command = new UpdateCustomerCommand
            {
                Id = 1,
                Name = "Ahmed",
                Phone = "01550830821",
            };

            var existingCustomer = new Customer
            {
                Id = 1,
                Name = "Ahmed",
                Phone = "01013513652",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "user",
                Addresses = new List<Address>()
            };

            _repository.GetByIdAsync(command.Id, Arg.Any<System.Linq.Expressions.Expression<System.Func<Customer, object>>>())
                .Returns(existingCustomer);

            _repository.PhoneExistsAsync(command.Phone).Returns(false);

            _mapper.Map<Customer>(Arg.Any<Customer>()).Returns(existingCustomer);
            _mapper.Map(command, existingCustomer);

            _repository.Update(Arg.Any<Customer>()).Returns(Task.CompletedTask);


            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _repository.Received(1).PhoneExistsAsync(command.Phone);
            await _repository.Received(1).Update(Arg.Any<Customer>());
        }

        [Fact]
        public async Task Handle_WithAddresses_ShouldIncludeAddressesInUpdate()
        {
            // Arrange
            var command = _validCommand;
            var addresses = new List<Address>
            {
                new Address { Id = 1, CustomerId = 1, AddressName = "Home", AddressType = AddressType.Home },
                new Address { Id = 2, CustomerId = 1, AddressName = "Work", AddressType = AddressType.Work }
            };

            var existingCustomer = new Customer
            {
                Id = 1,
                Name = "Ahmed",
                Phone = "01013513652",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "user",
                Addresses = addresses
            };

            _repository.GetByIdAsync(command.Id, Arg.Any<System.Linq.Expressions.Expression<System.Func<Customer, object>>>())
                .Returns(existingCustomer);

            _repository.PhoneExistsAsync(command.Phone).Returns(false);

            _mapper.Map<Customer>(Arg.Any<Customer>()).Returns(existingCustomer);
            _mapper.Map(command, existingCustomer);

            _repository.Update(Arg.Any<Customer>()).Returns(Task.CompletedTask);


            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(2, existingCustomer.Addresses.Count);
            await _repository.Received(1).Update(Arg.Any<Customer>());
        }


        #endregion

        #region Exception Cases

        [Fact]
        public async Task Handle_WithNonExistentCustomer_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new UpdateCustomerCommand { Id = 999, Name = "Test", Phone = "01013513652" };

            _repository.GetByIdAsync(command.Id, Arg.Any<System.Linq.Expressions.Expression<System.Func<Customer, object>>>())
                .Returns((Customer)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            Assert.Equal($"Customer with id {command.Id} not found.", exception.Message);
            await _repository.DidNotReceive().Update(Arg.Any<Customer>());
        }

        [Fact]
        public async Task Handle_WithDuplicatePhone_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new UpdateCustomerCommand
            {
                Id = 1,
                Name = "Ahmed",
                Phone = "01550830820",
            };

            var existingCustomer = new Customer
            {
                Id = 1,
                Name = "Ahmed",
                Phone = "01013513652",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "user",
                Addresses = new List<Address>()
            };

            _repository.GetByIdAsync(command.Id, Arg.Any<System.Linq.Expressions.Expression<System.Func<Customer, object>>>())
                .Returns(existingCustomer);

            _repository.PhoneExistsAsync(command.Phone).Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, CancellationToken.None).AsTask()
            );

            Assert.Equal($"Phone Number: {command.Phone} aleardy exists.", exception.Message);
            await _repository.DidNotReceive().Update(Arg.Any<Customer>());
        }

        #endregion

    }
}