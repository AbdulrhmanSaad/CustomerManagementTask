using AutoMapper;
using CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using NSubstitute;
using System.Linq.Expressions;
using Xunit;

namespace CustomerTaskUnitTest
{
    public class GetCustomerAddressesHistoryQueryHandlerTest
    {
        private readonly ICustomerHistoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly GetCustomerAddressesHistoryQueryHandler _handler;

        public GetCustomerAddressesHistoryQueryHandlerTest()
        {
            _repository = Substitute.For<ICustomerHistoryRepository>();
            _mapper = Substitute.For<IMapper>();

            _handler = new GetCustomerAddressesHistoryQueryHandler(_repository, _mapper);
        }

        #region Success Cases

        [Fact]
        public async Task Handle_WithValidCustomerId_ShouldReturnAddressHistory()
        {
            // Arrange
            var customerId = 25;
            var query = new GetCustomerAddressesHistoryQuery(customerId);

            var existingCustomer = new Customer
            {
                Id = customerId,
                Name = "Ahmed",
                Phone = "01013513652",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "admin",
                Addresses = new List<Address>()
            };

            var addressHistoryRecords = new List<AddressDto>
            {
                new AddressDto
                {
                    AddressType = "Home",
                    AddressName = "Cairo"
                }
            };

            _repository.GetByIdAsync(customerId)
                .Returns(existingCustomer);

            _repository.GetAllCustomerAddressHistory(customerId)
                .Returns(addressHistoryRecords);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            var firstRecord = result.First();
            Assert.Equal("Home", firstRecord.AddressType);
            Assert.Equal("Cairo", firstRecord.AddressName);

            await _repository.Received(1).GetByIdAsync(customerId);
            await _repository.Received(1).GetAllCustomerAddressHistory(customerId);
        }

        [Fact]
        public async Task Handle_WithMultipleAddressHistoryRecords_ShouldReturnAllRecords()
        {
            // Arrange
            var customerId = 32;
            var query = new GetCustomerAddressesHistoryQuery(customerId);

            var existingCustomer = new Customer
            {
                Id = customerId,
                Name = "Ahmed Updated",
                Phone = "01013513653",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "admin",
                Addresses = new List<Address>()
            };

            var addressHistoryRecords = new List<AddressDto>
            {
                new AddressDto
                {
                    AddressType = "Home",
                    AddressName = "Cairo"
                },
                new AddressDto
                {
                    AddressType = "Home",
                    AddressName = "New Cairo"
                },
                new AddressDto
                {
                    AddressType = "Work",
                    AddressName = "Alexandria"
                }
            };

            _repository.GetByIdAsync(customerId)
                .Returns(existingCustomer);

            _repository.GetAllCustomerAddressHistory(customerId)
                .Returns(addressHistoryRecords);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            
            var resultList = result.ToList();
            Assert.Equal("Home", resultList[0].AddressType);
            Assert.Equal("Cairo", resultList[0].AddressName);
            
            Assert.Equal("Work", resultList[2].AddressType);
            Assert.Equal("Alexandria", resultList[2].AddressName);
        }


        [Fact]
        public async Task Handle_WithAddressNameChanges_ShouldReturnAllVersions()
        {
            // Arrange
            var customerId = 32;
            var query = new GetCustomerAddressesHistoryQuery(customerId);

            var existingCustomer = new Customer
            {
                Id = customerId,
                Name = "Customer Name",
                Phone = "01234567890",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "admin",
                Addresses = new List<Address>()
            };

            // Shows how address name evolved over time
            var addressHistoryRecords = new List<AddressDto>
            {
                new AddressDto
                {
                    AddressType = "Home",
                    AddressName = "Cairo"
                },
                new AddressDto
                {
                    AddressType = "Home",
                    AddressName = "New Cairo"
                },
                new AddressDto
                {
                    AddressType = "Home",
                    AddressName = "New Cairo - Apartment 5"
                }
            };

            _repository.GetByIdAsync(customerId)
                .Returns(existingCustomer);

            _repository.GetAllCustomerAddressHistory(customerId)
                .Returns(addressHistoryRecords);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            
            var resultList = result.ToList();
            Assert.All(resultList, a => Assert.Equal("Home", a.AddressType));
            
            // Verify evolution of address names
            Assert.Equal("Cairo", resultList[0].AddressName);
            Assert.Equal("New Cairo", resultList[1].AddressName);
            Assert.Equal("New Cairo - Apartment 5", resultList[2].AddressName);
        }

        #endregion

        #region Exception Cases

        [Fact]
        public async Task Handle_WithNonExistentCustomer_ShouldThrowNotFoundException()
        {
            // Arrange
            var customerId = 999;
            var query = new GetCustomerAddressesHistoryQuery(customerId);

            _repository.GetByIdAsync(customerId)
                .Returns((Customer)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(query, CancellationToken.None)
            );

            Assert.Equal($"Customer with id {customerId} not found.", exception.Message);

            await _repository.DidNotReceive().GetAllCustomerAddressHistory(Arg.Any<int>());
        }

        #endregion

       
    }
}