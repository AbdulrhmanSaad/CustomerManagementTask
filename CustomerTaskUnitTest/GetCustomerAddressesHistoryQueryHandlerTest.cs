using Amazon.Runtime.Internal.Util;
using Azure.Core;
using Castle.Core.Resource;
using CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using CustomersTask4.Services.Caching;
using MapsterMapper;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Linq.Expressions;
using Xunit;

namespace CustomerTaskUnitTest
{
    public class GetCustomerAddressesHistoryQueryHandlerTest
    {
        private readonly ICustomerHistoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILocalizationService localization;
        private readonly ILogger<GetCustomerAddressesHistoryQueryHandler> logger;
        private readonly GetCustomerAddressesHistoryQueryHandler _handler;
        private readonly HybridCache cachingService;


        public GetCustomerAddressesHistoryQueryHandlerTest()
        {
            _repository = Substitute.For<ICustomerHistoryRepository>();
            _mapper = Substitute.For<IMapper>();
            localization = Substitute.For<ILocalizationService>();
            logger = Substitute.For<ILogger<GetCustomerAddressesHistoryQueryHandler>>();
            cachingService = Substitute.For<HybridCache>();

            _handler = new GetCustomerAddressesHistoryQueryHandler(_repository, _mapper,logger,localization,cachingService);
        }
        private void MockHybridCaching()
        {
            cachingService
            .GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<IEnumerable<AddressDto>>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var factory = callInfo
                    .ArgAt<Func<CancellationToken, ValueTask<IEnumerable<AddressDto>>>>(1);
                return factory(CancellationToken.None);
            });

        }

        private Customer GenerateCustomer(string customerId)
        {
            var customer = new Customer
            {
                Id = customerId,
                Name = "Ahmed Updated",
                Phone = "01013513653",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "admin",
                Addresses = new List<Address>()
            };

            return customer;
        }
        private List<AddressDto> GenerateAddressesForCustomer()
        {
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
            return addressHistoryRecords;
        }
        #region Success Cases

        [Fact]
        public async Task Handle_WithValidCustomerId_ShouldReturnAddressHistory()
        {
            // Arrange
            var customerId = "25";
            var query = new GetCustomerAddressesHistoryQuery(customerId);

            var existingCustomer = GenerateCustomer(customerId);

            var addressHistoryRecords = GenerateAddressesForCustomer();
            _repository.GetByIdAsync(customerId)
                .Returns(existingCustomer);

            MockHybridCaching();

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
            var customerId = "32";
            var query = new GetCustomerAddressesHistoryQuery(customerId);

            var existingCustomer = GenerateCustomer(customerId);
            var addressHistoryRecords = GenerateAddressesForCustomer();
            _repository.GetByIdAsync(customerId)
                .Returns(existingCustomer);
            // Mock HybridCache.GetOrCreateAsync to execute the factory function
             
            
            _repository.GetAllCustomerAddressHistory(customerId)
                .Returns(addressHistoryRecords);

            MockHybridCaching();
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
            var customerId = "32";
            var query = new GetCustomerAddressesHistoryQuery(customerId);

            var existingCustomer = GenerateCustomer(customerId);


            // Shows how address name evolved over time
            var addressHistoryRecords = GenerateAddressesForCustomer();

            _repository.GetByIdAsync(customerId)
                .Returns(existingCustomer);

            _repository.GetAllCustomerAddressHistory(customerId)
                .Returns(addressHistoryRecords);

            MockHybridCaching();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            
            var resultList = result.ToList();
            Assert.Equal("Home",resultList[0].AddressType);
            
            // Verify evolution of address names
            Assert.Equal("Cairo", resultList[0].AddressName);
            Assert.Equal("New Cairo", resultList[1].AddressName);
            Assert.Equal("Alexandria", resultList[2].AddressName);
        }

        #endregion

        #region Exception Cases

        [Fact]
        public async Task Handle_WithNonExistentCustomer_ShouldThrowNotFoundException()
        {
            // Arrange
            var customerId = "999";
            var query = new GetCustomerAddressesHistoryQuery(customerId);

            _repository.GetByIdAsync(customerId)
                .Returns((Customer)null);

            MockHybridCaching();


            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(query, CancellationToken.None));

            Assert.Equal(localization.Localize("Customer with id {0} not found.",customerId), exception.Message);

            await _repository.DidNotReceive().GetAllCustomerAddressHistory(Arg.Any<string>());
        }

        #endregion

       
    }
}