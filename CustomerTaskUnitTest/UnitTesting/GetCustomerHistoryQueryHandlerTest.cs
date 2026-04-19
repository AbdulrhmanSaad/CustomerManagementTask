using CustomersTask4.CustomerHandler.Query.GetCustomerHistory;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using MapsterMapper;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Services;

namespace CustomerTaskUnitTest.UnitTesting
{
        public class GetCustomerHistoryQueryHandlerTest
        {
            private readonly ILogger<GetCustomerHistoryQueryHandler> _logger;
            private readonly ICustomerHistoryRepository _repository;
            private readonly GetCustomerHistoryQueryHandler _handler;
            private readonly IMapper _mapper;
            private readonly ILocalizationService localization;
            private readonly HybridCache cachingService;

         
        public GetCustomerHistoryQueryHandlerTest()
            {
                _logger = Substitute.For<ILogger<GetCustomerHistoryQueryHandler>>();
                _repository = Substitute.For<ICustomerHistoryRepository>();
                _mapper = Substitute.For<IMapper>();
                localization = Substitute.For<ILocalizationService>();
                cachingService = Substitute.For<HybridCache>();

            _handler = new GetCustomerHistoryQueryHandler(_logger, _repository,_mapper,localization,cachingService);
            }

        private void MockHybridCaching()
        {
            cachingService
            .GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<IEnumerable<CustomerHistoryResponse>>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var factory = callInfo
                    .ArgAt<Func<CancellationToken, ValueTask<IEnumerable<CustomerHistoryResponse>>>>(1);
                return factory(CancellationToken.None);
            });

        }

        #region Success Cases

        [Fact]
            public async Task Handle_WithValidCustomerId_ShouldReturnCustomerHistory()
            {
                // Arrange
                var customerId = "25";
                var query = new GetCustomerHistoryQuery(customerId);
            var existtingcustomer = new Customer
            {
                Id = "32",
                Name = "Ahmed Updated",
                Phone = "01013513653",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                CreatedBy = "admin",
                Addresses = new List<Address>()
            };
            var historyRecords = new List<Customer>
            {
                new Customer
                {
                    Id = customerId,
                    Name = "Ahmed",
                    Phone = "01013513652",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    Addresses = new List<Address>()
                }
            };
            _repository.GetByIdAsync(customerId)
             .Returns(existtingcustomer);

            _repository.GetAllCustomerHistory(customerId)
                    .Returns(historyRecords);


            _mapper.Map<IEnumerable<CustomerHistoryResponse>>(historyRecords)
                    .Returns(historyRecords.Select(c => new CustomerHistoryResponse
                    {
                        Name = c.Name,
                        Phone = c.Phone,
                        CreatedAt = c.CreatedAt,
                        CreatedBy = c.CreatedBy
                    }));
            MockHybridCaching();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                var firstRecord = result.First();
                Assert.Equal("Ahmed", firstRecord.Name);
            }

         
            [Fact]
            public async Task Handle_WithMultipleHistoryRecords_ShouldReturnAllRecords()
            {
                // Arrange
                var customerId = "32";
                var query = new GetCustomerHistoryQuery(customerId);
            var existtingcustomer = new Customer
            {
                Id = "32",
                Name = "Ahmed Updated",
                Phone = "01013513653",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                CreatedBy = "admin",
                Addresses = new List<Address>()
            };

                var historyRecords = new List<Customer>
            {
                new Customer
                {
                    Id = customerId,
                    Name = "Ahmed",
                    Phone = "01013513652",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    CreatedBy = "admin",
                    Addresses = new List<Address>()
                },
                new Customer
                {
                    Id = customerId,
                    Name = "Ahmed Updated",
                    Phone = "01013513653",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    CreatedBy = "admin",
                    Addresses = new List<Address>()
                },
                new Customer
                {
                    Id = customerId,
                    Name = "Ahmed Final",
                    Phone = "01013513654",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "admin",
                    Addresses = new List<Address>()
                }
            };

            _repository.GetByIdAsync(customerId)
              .Returns(existtingcustomer);

            _repository.GetAllCustomerHistory(customerId)
                    .Returns(historyRecords);

              _mapper.Map<IEnumerable<CustomerHistoryResponse>>(historyRecords)
                    .Returns(historyRecords.Select(c => new CustomerHistoryResponse
                    {
                        Name = c.Name,
                        Phone = c.Phone,
                        CreatedAt = c.CreatedAt,
                        CreatedBy = c.CreatedBy
                    }));
             MockHybridCaching();

               // Act
               var result = await _handler.Handle(query, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(3, result.Count());
                Assert.Equal("Ahmed", result.First().Name);
                Assert.Equal("Ahmed Final", result.Last().Name);
            }        

            

            [Fact]
            public async Task Handle_WithEmptyHistoryRecords_ShouldReturnNotFoundException()
            {
                // Arrange
                var customerId = "999";
                var query = new GetCustomerHistoryQuery(customerId);

                var emptyHistoryRecords = new List<Customer>();

                _repository.GetAllCustomerHistory(customerId)
                    .Returns(emptyHistoryRecords);


             _repository.GetByIdAsync(customerId)
                .Returns((Customer)null);

            MockHybridCaching();

           // Act & Assert
           var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(query, CancellationToken.None));

            Assert.Equal(localization.Localize("Customer with id {0} not found.",customerId), exception.Message);
            await _repository.DidNotReceive().GetAllCustomerHistory(Arg.Any<string>());

            }

           

            #endregion

        }
}

