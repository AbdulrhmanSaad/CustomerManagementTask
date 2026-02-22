using AutoMapper;
using CustomersTask4.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Repository;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CustomerTaskUnitTest
{
    public class GetAllCustomerCommandHandlerTest
    {
            private readonly IGenericRepository<Customer> _repository;
            private readonly ILogger<GetAllCustomerQueryHandler> _logger;
            private readonly IMapper _mapper;
            private readonly GetAllCustomerQueryHandler _handler;

            public GetAllCustomerCommandHandlerTest()
            {
                _repository = Substitute.For<IGenericRepository<Customer>>();
                _logger = Substitute.For<ILogger<GetAllCustomerQueryHandler>>();
                _mapper = Substitute.For<IMapper>();

                _handler = new GetAllCustomerQueryHandler(_repository, _logger, _mapper);
            }

            #region Success Cases

            [Fact]
            public async Task Handle_WithCustomersInDatabase_ShouldReturnAllCustomers()
            {
                // Arrange
                var customers = new List<Customer>
            {
                new Customer
                {
                    Id = 1,
                    Name = "Ahmed",
                    Phone = "01013513652",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    Addresses = new List<Address>
                    {
                        new Address { Id = 1, CustomerId = 1, AddressName = "Home", AddressType = AddressType.Home }
                    }
                },
                new Customer
                {
                    Id = 2,
                    Name = "Fatima",
                    Phone = "01550830820",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    Addresses = new List<Address>
                    {
                        new Address { Id = 2, CustomerId = 2, AddressName = "Work", AddressType = AddressType.Work }
                    }
                }
            };

                var expectedDtos = new List<CustomerDto>
            {
                new CustomerDto
                {
                    Id = 1,
                    Name = "Ahmed",
                    Phone = "01013513652",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    HomeAddressLocation = "Home"
                },
                new CustomerDto
                {
                    Id = 2,
                    Name = "Fatima",
                    Phone = "01550830820",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    WorkAddressLocation = "Work"
                }
            };

                _repository.GetAll(Arg.Any<Expression<Func<Customer, object>>>())
                    .Returns(customers);

                _mapper.Map<IEnumerable<CustomerDto>>(customers)
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

                _repository.GetAll(Arg.Any<Expression<Func<Customer, object>>>())
                    .Returns(emptyCustomers);

                _mapper.Map<IEnumerable<CustomerDto>>(emptyCustomers)
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
            var customers = new List<Customer>
            {
                new Customer
                {
                    Id = 1,
                    Name = "Ahmed",
                    Phone = "01013513652",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    Addresses = new List<Address>()
                    {
                        new Address { Id = 1, CustomerId = 1, AddressName = "Cairo", AddressType = AddressType.Home },
                        new Address { Id = 2, CustomerId = 1, AddressName = "Alex", AddressType = AddressType.Work }
                    }
                }
            };

                var expectedDtos = new List<CustomerDto>
            {
                new CustomerDto
                {
                    Id = 1,
                    Name = "Ahmed",
                    Phone = "01013513652",
                    CreatedBy = "admin",
                    AddressType=AddressType.Home.ToString(),
                    HomeAddressLocation="Cairo",
                    AddressType2=AddressType.Work.ToString(),
                    WorkAddressLocation="Alex"
                }
            };

                _repository.GetAll(Arg.Any<Expression<Func<Customer, object>>>())
                    .Returns(customers);

                _mapper.Map<IEnumerable<CustomerDto>>(customers)
                    .Returns(expectedDtos);

                // Act
                var result = await _handler.Handle(new GetAllCustomerQuery(), CancellationToken.None);

                // Assert
                _mapper.Received(1).Map<IEnumerable<CustomerDto>>(customers);
            }

            #endregion

    }
}
