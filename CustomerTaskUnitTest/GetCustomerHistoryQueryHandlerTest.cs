using AutoMapper;
using CustomersTask4.CustomerHandler.Query.GetCustomerHistory;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CustomerTaskUnitTest
{
        public class GetCustomerHistoryQueryHandlerTest
        {
            private readonly ILogger<GetCustomerHistoryQueryHandler> _logger;
            private readonly ICustomerHistoryRepository _repository;
            private readonly GetCustomerHistoryQueryHandler _handler;
            private readonly IMapper _mapper;

            public GetCustomerHistoryQueryHandlerTest()
            {
                _logger = Substitute.For<ILogger<GetCustomerHistoryQueryHandler>>();
                _repository = Substitute.For<ICustomerHistoryRepository>();
                _mapper = Substitute.For<IMapper>();

            _handler = new GetCustomerHistoryQueryHandler(_logger, _repository,_mapper);
            }

            #region Success Cases

            [Fact]
            public async Task Handle_WithValidCustomerId_ShouldReturnCustomerHistory()
            {
                // Arrange
                var customerId = 25;
                var query = new GetCustomerHistoryQuery(customerId);
            var existtingcustomer = new Customer
            {
                Id = 32,
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
                var customerId = 32;
                var query = new GetCustomerHistoryQuery(customerId);
            var existtingcustomer = new Customer
            {
                Id = 32,
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
                var customerId = 999;
                var query = new GetCustomerHistoryQuery(customerId);

                var emptyHistoryRecords = new List<Customer>();

                _repository.GetAllCustomerHistory(customerId)
                    .Returns(emptyHistoryRecords);


             _repository.GetByIdAsync(customerId)
                .Returns((Customer)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(query, CancellationToken.None)
            );

            Assert.Equal($"Customer with id {customerId} not found.", exception.Message);
            await _repository.DidNotReceive().GetAllCustomerHistory(Arg.Any<int>());

            }

           

            #endregion

        }
}

