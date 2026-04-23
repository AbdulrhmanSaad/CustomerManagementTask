using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.GraphQL.Query;
using CustomersTask4.Repository;
using HotChocolate;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Services;
using CustomersTask4.DTO;
using System.Linq.Expressions;

namespace CustomerTaskUnitTest.GraphQLUnitTest
{
    public class CustomerManagemantQueryTests
    {
        private readonly IGenericRepository<Customer> _customerRepo;
        private readonly ICustomerHistoryRepository _historyRepo;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localization;
        private readonly ILogger<CustomerManagemantQuery> _logger;
        private readonly CustomerManagemantQuery _sut;

        public CustomerManagemantQueryTests()
        {
            _customerRepo = Substitute.For<IGenericRepository<Customer>>();
            _historyRepo = Substitute.For<ICustomerHistoryRepository>();
            _mapper = Substitute.For<IMapper>();
            _localization = Substitute.For<ILocalizationService>();
            _logger = Substitute.For<ILogger<CustomerManagemantQuery>>();
            _sut = new CustomerManagemantQuery();
        }

        //GetCustomers

        [Fact]
        public void GetCustomers_ReturnsAllMappedCustomers()
        {
            // Arrange
            var customers = new List<Customer> { new Customer { Id = "1" }, new Customer { Id = "2" } };
            var customerDtos = new List<CustomerDto> { new CustomerDto { Id = "1" }, new CustomerDto { Id = "2" } };

            _customerRepo.GetAll(includes: Arg.Any<Expression<Func<Customer, object>>>())
                         .Returns(customers);
            _mapper.Map<IEnumerable<CustomerDto>>(customers).Returns(customerDtos);

            // Act
            var result = _sut.GetCustomers(_customerRepo, _mapper);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetCustomers_WhenNoCustomers_ReturnsEmptyCollection()
        {
            // Arrange
            _customerRepo.GetAll(includes: Arg.Any<Expression<Func<Customer, object>>>())
                         .Returns(new List<Customer>());
            _mapper.Map<IEnumerable<CustomerDto>>(Arg.Any<IEnumerable<Customer>>())
                   .Returns(new List<CustomerDto>());

            // Act
            var result = _sut.GetCustomers(_customerRepo, _mapper);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        //GetCustomerById

        [Fact]
        public async Task GetCustomerById_ExistingCustomer_ReturnsMappedDto()
        {
            // Arrange
            var customerId = "customer-1";
            var customer = new Customer { Id = customerId };
            var customerDto = new CustomerDto { Id = customerId };

            _customerRepo.GetByIdAsync(customerId, Arg.Any<Expression<Func<Customer, object>>>())
                         .Returns(customer);
            _mapper.Map<CustomerDto>(customer).Returns(customerDto);

            // Act
            var result = await _sut.GetCustomerById(_customerRepo, customerId, _localization, _mapper);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.Id);
        }

        [Fact]
        public async Task GetCustomerById_CustomerNotFound_ThrowsGraphQLException()
        {
            // Arrange
            var customerId = "non-existent";
            var errorMessage = $"Customer with id {customerId} not found.";

            _customerRepo.GetByIdAsync(customerId, Arg.Any<Expression<Func<Customer, object>>>())
                         .Returns((Customer?)null);
            _localization.Localize(Arg.Any<string>()).Returns(errorMessage);

           
            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _sut.GetCustomerById(_customerRepo, customerId, _localization, _mapper));
        }

        //GetCustomerAddressesHistory

        [Fact]
        public async Task GetCustomerAddressesHistory_ExistingCustomer_ReturnsAddresses()
        {
            // Arrange
            var customerId = "customer-1";
            var customer = new Customer { Id = customerId };
            var addresses = new List<AddressDto> { new AddressDto(), new AddressDto() };

            _historyRepo.GetByIdAsync(customerId).Returns(customer);
            _historyRepo.GetAllCustomerAddressHistory(customerId).Returns(addresses);

            // Act
            var result = await _sut.GetCustomerAddressesHistory(
                _historyRepo, customerId, _mapper, _logger, _localization);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetCustomerAddressesHistory_CustomerNotFound_ThrowsGraphQLException()
        {
            // Arrange
            var customerId = "non-existent";
            var errorMessage = $"Customer with id {customerId} not found.";

            _historyRepo.GetByIdAsync(customerId).Returns((Customer?)null);
            _localization.Localize(Arg.Any<string>()).Returns(errorMessage);

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _sut.GetCustomerAddressesHistory(
                    _historyRepo, customerId, _mapper, _logger, _localization));
        }

        [Fact]
        public async Task GetCustomerAddressesHistory_LogsInformation()
        {
            // Arrange
            var customerId = "customer-1";
            var customer = new Customer { Id = customerId };

            _historyRepo.GetByIdAsync(customerId).Returns(customer);
            _historyRepo.GetAllCustomerAddressHistory(customerId).Returns(new List<AddressDto>());

            // Act
            await _sut.GetCustomerAddressesHistory(
                _historyRepo, customerId, _mapper, _logger, _localization);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString()!.Contains("Fetch customer Addresses History")),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }

        //GetCustomerHistory

        [Fact]
        public async Task GetCustomerHistory_ExistingCustomer_ReturnsMappedHistory()
        {
            // Arrange
            var customerId = "customer-1";
            var customer = new Customer { Id = customerId };
            var historyEntities = new List<Customer>();
            var historyDtos = new List<CustomerHistoryResponse>();

            _historyRepo.GetByIdAsync(customerId).Returns(customer);
            _historyRepo.GetAllCustomerHistory(customerId).Returns(historyEntities);
            _mapper.Map<IEnumerable<CustomerHistoryResponse>>(historyEntities).Returns(historyDtos);

            // Act
            var result = await _sut.GetCustomerHistory(
                _historyRepo, customerId, _mapper, _logger, _localization);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetCustomerHistory_CustomerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var customerId = "non-existent";
            var errorMessage = $"Customer with id {customerId} not found.";

            _historyRepo.GetByIdAsync(customerId).Returns((Customer?)null);
            _localization.Localize(Arg.Any<string>()).Returns(errorMessage);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.GetCustomerHistory(
                    _historyRepo, customerId, _mapper, _logger, _localization));
        }
    }
}
