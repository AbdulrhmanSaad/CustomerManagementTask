using CustomersTask4.Abstraction;
using CustomersTask4.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Hubs;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using CustomersTask4.Services.Caching;
using CustomersTask4.Users;
using MapsterMapper;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CustomerTaskUnitTest
{
    public class CreateCustomerCommandTest
    {
        private readonly CreateCustomerCommandHandler _handler;
        private static readonly CreateCustomerCommand command=new()
        {
            Name = "Ahmed",
            Phone = "01013513652",
            Addresses =new List<AddressDtoEnum>()
            {
                new AddressDtoEnum
                {
                    AddressType=AddressType.Home,
                    AddressName="Cairo"
                },
                new AddressDtoEnum
                {
                    AddressType=AddressType.Work,
                    AddressName="Alex"
                }
            }
        };
        private readonly IGenericRepository<Customer> repository;
        private readonly IMapper mapper;
        private readonly ILogger<CreateCustomerCommandHandler> logger;
        private readonly IUserContext userContext;
        private readonly IConfiguration configuration;
        private readonly IHubContext<MessageHub> hubContext;
        private readonly IAppMeditor bus;
        private readonly ILocalizationService localization;
        private readonly IRedisCachingService cachingService;


        public CreateCustomerCommandTest()
        {
            repository=Substitute.For<IGenericRepository<Customer>>();
            mapper=Substitute.For<IMapper>();
            logger = Substitute.For<ILogger<CreateCustomerCommandHandler>>();
            userContext = Substitute.For<CustomersTask4.Users.IUserContext>();
            configuration = Substitute.For<IConfiguration>();
            hubContext = Substitute.For<IHubContext<MessageHub>>();
            bus = Substitute.For<IAppMeditor>();
            localization = Substitute.For<ILocalizationService>();
            cachingService = Substitute.For<IRedisCachingService>();

            _handler =new CreateCustomerCommandHandler(repository,
                logger,mapper, userContext,
                configuration,hubContext,bus,localization,cachingService);
        }
        [Fact]
        public async Task Handle_WithDuplicatePhone_ShouldThrowNotFoundException()
        {
            //Arrange
            
            repository.PhoneExistsAsync(command.Phone).Returns(true);
            //Act &&
            //assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));

            Assert.Equal(localization.Localize("this phone number already exists"),ex.Message);
            await repository.DidNotReceive().Add(Arg.Any<Customer>());
        }

        [Fact]
        public async Task Handle_WithValidCustomer_ShouldCreateNewCustomer()
        {
            //Arrange
            var NewCustomer = new CreateCustomerCommand
            {
                Name = "updated now ",
                Phone = "01213213665",
                Addresses = new List<AddressDtoEnum>()
            {
                new AddressDtoEnum
                {
                    AddressType=AddressType.Home,
                    AddressName="Cairo"
                },
                new AddressDtoEnum
                {
                    AddressType=AddressType.Work,
                    AddressName="Alex"
                }
            }
            };
            repository.PhoneExistsAsync(command.Phone).Returns(true);
            mapper.Map<Customer>(NewCustomer).Returns(new Customer());
            repository.Add(Arg.Any<Customer>()).Returns(Task.CompletedTask);

            //Act 
              await _handler.Handle(NewCustomer, default);
            //assert
            await repository.Received(1).Add(Arg.Any<Customer>());
        }
    }
}
