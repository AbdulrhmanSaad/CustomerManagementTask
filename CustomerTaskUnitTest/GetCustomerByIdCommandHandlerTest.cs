using Mapster;
using CustomersTask4.CustomerHandler.Query;
using CustomersTask4.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.CustomerHandler.Query.GetCustomerById;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;
using MapsterMapper;


namespace CustomerTaskUnitTest
{
    public class GetCustomerByIdCommandHandlerTest
    {
            private readonly IGenericRepository<Customer> _repository;
            private readonly ILogger<GetAllCustomerQueryHandler> _logger;
            private readonly IMapper _mapper;
            private readonly GetCustomerByIdQueryHandler _handler;

            public GetCustomerByIdCommandHandlerTest()
            {
                _repository = Substitute.For<IGenericRepository<Customer>>();
                _logger = Substitute.For<ILogger<GetAllCustomerQueryHandler>>();
                _mapper = Substitute.For<IMapper>();

                _handler = new GetCustomerByIdQueryHandler(_repository, _logger, _mapper);
            }

            [Fact]
            public async Task Handle_WithValidCustomerId_ShouldReturnCustomerDto()
            {
                // Arrange
                var customerId = 1;
                var query = new GetCustomerByIdQuery(customerId);

                var customer = new Customer
                {
                    Id = customerId,
                    Name = "Ahmed",
                    Phone = "01013513652",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    Addresses = new List<Address>
                {
                    new Address { Id = 1, CustomerId = customerId, AddressName = "Cairo", AddressType = AddressType.Home },
                    new Address { Id = 2, CustomerId = customerId, AddressName = "Alex", AddressType = AddressType.Work }

                }
                };

            var expectedDto = new CustomerDto
            {
                Id = customerId,
                Name = "Ahmed",
                Phone = "01013513652",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "admin",
                Addresses = new List<AddressDto>
                {
                    new AddressDto { AddressName = "Cairo", AddressType = AddressType.Home.ToString() },
                    new AddressDto {AddressName = "Alex", AddressType = AddressType.Work.ToString() }
                }
            }; 

                _repository.GetByIdAsync(customerId, Arg.Any<Expression<Func<Customer, object>>>())
                    .Returns(customer);

                _mapper.Map<CustomerDto>(customer)
                    .Returns(expectedDto);

                // Act
                var result = await _handler.Handle(query, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(expectedDto.Id, result.Id);
                Assert.Equal(expectedDto.Name, result.Name);
                Assert.Equal(expectedDto.Phone, result.Phone);
            }
            [Fact]
            public async Task Handle_WithNonExistentCustomerId_ShouldThrowNotFoundException()
            {
                // Arrange
                var customerId = 999;
                var query = new GetCustomerByIdQuery(customerId);

                _repository.GetByIdAsync(customerId, Arg.Any<Expression<Func<Customer, object>>>())
                    .Returns((Customer)null);

                _mapper.Map<CustomerDto>(Arg.Any<Customer>())
                    .Returns((CustomerDto)null);

                // Act & Assert
                var exception = await Assert.ThrowsAsync<NotFoundException>(
                    () => _handler.Handle(query, CancellationToken.None)
                );

                Assert.Equal($"Customer with id {customerId} not found.", exception.Message);
            }

           


        }
}


