using CustomersTask4.CustomerHandler.Query.GetCustomerHistory;
using CustomersTask4.Domain;
using CustomersTask4.Repository;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerTaskUnitTest
{
        public class GetCustomerHistoryQueryHandlerTest
        {
            private readonly ILogger<GetCustomerHistoryQueryHandler> _logger;
            private readonly ICustomerHistoryRepository _repository;
            private readonly GetCustomerHistoryQueryHandler _handler;

            public GetCustomerHistoryQueryHandlerTest()
            {
                _logger = Substitute.For<ILogger<GetCustomerHistoryQueryHandler>>();
                _repository = Substitute.For<ICustomerHistoryRepository>();

                _handler = new GetCustomerHistoryQueryHandler(_logger, _repository);
            }

            #region Success Cases

            [Fact]
            public async Task Handle_WithValidCustomerId_ShouldReturnCustomerHistory()
            {
                // Arrange
                var customerId = 1;
                var query = new GetCustomerHistoryQuery(customerId);

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

                _repository.GetAllCustomerHistory(customerId)
                    .Returns(historyRecords);

                // Act
                var result = await _handler.Handle(query, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                var firstRecord = result.First();
                Assert.Equal(customerId, firstRecord.Id);
                Assert.Equal("Ahmed", firstRecord.Name);
            }

         
            [Fact]
            public async Task Handle_WithMultipleHistoryRecords_ShouldReturnAllRecords()
            {
                // Arrange
                var customerId = 1;
                var query = new GetCustomerHistoryQuery(customerId);

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

                _repository.GetAllCustomerHistory(customerId)
                    .Returns(historyRecords);

                // Act
                var result = await _handler.Handle(query, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(3, result.Count());
                Assert.Equal("Ahmed", result.First().Name);
                Assert.Equal("Ahmed Final", result.Last().Name);
            }        

            

            [Fact]
            public async Task Handle_WithEmptyHistoryRecords_ShouldReturnEmptyEnumerable()
            {
                // Arrange
                var customerId = 999;
                var query = new GetCustomerHistoryQuery(customerId);

                var emptyHistoryRecords = new List<Customer>();

                _repository.GetAllCustomerHistory(customerId)
                    .Returns(emptyHistoryRecords);

                // Act
                var result = await _handler.Handle(query, CancellationToken.None);

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
            }

           

            #endregion

        }
}

