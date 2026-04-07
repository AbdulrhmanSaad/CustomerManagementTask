using CustomersTask4.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Repository;
using CustomersTask4.Services.Caching;
using MapsterMapper;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CustomerTaskUnitTest
{
    public class GetAllCustomerCommandHandlerTest
    {
        private readonly IGenericRepository<Customer> _repository;
        private readonly ILogger<GetAllCustomerQueryHandler> _logger;
        private readonly IMapper _mapper;
        private readonly HybridCache _cachingService;
        private readonly GetAllCustomerQueryHandler _handler;

        public GetAllCustomerCommandHandlerTest()
        {
            _repository = Substitute.For<IGenericRepository<Customer>>();
            _logger = Substitute.For<ILogger<GetAllCustomerQueryHandler>>();
            _mapper = Substitute.For<IMapper>();
            _cachingService = Substitute.For<HybridCache>();

            _handler = new GetAllCustomerQueryHandler(_repository, _logger, _mapper, _cachingService);
        }

        private void MockHybridCaching()
        {
            _cachingService
            .GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<IEnumerable<CustomerDto>>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var factory = callInfo
                    .ArgAt<Func<CancellationToken, ValueTask<IEnumerable<CustomerDto>>>>(1);
                return factory(CancellationToken.None);
            });

        }
        private List<Customer> GenerateCustomersList()
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    Id = "1",
                    Name = "Ahmed",
                    Phone = "01013513652",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    Addresses = new List<Address>
                    {
                        new Address { CustomerId = "1", AddressName = "Home", AddressType = AddressType.Home }
                    }
                },
                new Customer
                {
                    Id = "2",
                    Name = "Fatima",
                    Phone = "01550830820",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    Addresses = new List<Address>
                    {
                        new Address { CustomerId = "2", AddressName = "Work", AddressType = AddressType.Work }
                    }
                }
            };
            return customers;
        }
        private List<CustomerDto> GenerateCustomersResponseList()
        {
            var customersResponse = new List<CustomerDto>
            {
                new CustomerDto
                {
                    Id = "1",
                    Name = "Ahmed",
                    Phone = "01013513652",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    Addresses = new List<AddressDto>
                    {
                        new AddressDto { AddressName = "Cairo", AddressType = AddressType.Home.ToString() },
                        new AddressDto { AddressName = "Alex", AddressType = AddressType.Work.ToString() }
                    }
                },
                new CustomerDto
                {
                    Id = "2",
                    Name = "Fatima",
                    Phone = "01550830820",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    Addresses = new List<AddressDto>
                    {
                        new AddressDto { AddressName = "Cairo", AddressType = AddressType.Home.ToString() },
                        new AddressDto { AddressName = "Alex", AddressType = AddressType.Work.ToString() }
                    }
                }
            };
         return customersResponse;
        }

        #region Success Cases

        [Fact]
        public async Task Handle_WithCustomersInDatabase_ShouldReturnAllCustomers()
        {
            // Arrange
            var customers=GenerateCustomersList();
            var expectedDtos = GenerateCustomersResponseList();

            // Mock repository to return customers with addresses included
            _repository.GetAll(Arg.Any<Expression<Func<Customer, bool>>>())
                .Returns(customers.AsEnumerable());
            MockHybridCaching();
            // Mock mapper to convert customers to DTOs
            _mapper.Map<IEnumerable<CustomerDto>>(Arg.Any<IEnumerable<Customer>>())
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(new GetAllCustomerQuery(), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal(expectedDtos, result);
            
            }

        [Fact]
        public async Task Handle_WithEmptyDatabase_ShouldReturnEmptyEnumerable()
        {
            // Arrange
            var emptyCustomers = new List<Customer>();
            var emptyDtos = new List<CustomerDto>();

            // Mock repository to return empty list
            _repository.GetAll(Arg.Any<Expression<Func<Customer, bool>>>())
                .Returns(emptyCustomers.AsEnumerable());

            MockHybridCaching();
            // Mock mapper to convert to empty DTOs
            _mapper.Map<IEnumerable<CustomerDto>>(Arg.Any<IEnumerable<Customer>>())
                .Returns(emptyDtos);

            // Act
            var result = await _handler.Handle(new GetAllCustomerQuery(), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ShouldMapCustomersToDto()
        {
            // Arrange
            var customers = GenerateCustomersList();
            var expectedDtos = GenerateCustomersResponseList();



            // Mock repository to return customers
            _repository.GetAll(Arg.Any<Expression<Func<Customer, bool>>>())
                .Returns(customers.AsEnumerable());


            MockHybridCaching();

            // Mock mapper to convert customers to DTOs
            _mapper.Map<IEnumerable<CustomerDto>>(Arg.Any<IEnumerable<Customer>>())
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(new GetAllCustomerQuery(), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mapper.Received(1).Map<IEnumerable<CustomerDto>>(Arg.Any<IEnumerable<Customer>>());
        }

        #endregion
    }
}
