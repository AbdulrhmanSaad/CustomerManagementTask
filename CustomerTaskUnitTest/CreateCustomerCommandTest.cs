using AutoMapper;
using CustomersTask4.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using CustomersTask4.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace CustomerTaskUnitTest
{
    public class CreateCustomerCommandTest
    {
        private readonly CreateCustomerCommandHandler _handler;
        private static readonly CreateCustomerCommand _command=new()
        {
            Name = "Ahmed",
            Phone = "01013513652",
            AddressType = CustomersTask4.Domain.AddressType.Home,
            HomeAddressLocation = "123 Alex",
            AddressType2 = CustomersTask4.Domain.AddressType.Work,
            WorkAddressLocation = "456 Cairo"
        };
        private readonly IGenericRepository<Customer> repository;
        private readonly IAuditService auditService;
        private readonly IMapper mapper;
        private readonly ILogger<CreateCustomerCommandHandler> logger;
        private readonly IUserContext userContext;

        public CreateCustomerCommandTest()
        {
            repository=Substitute.For<IGenericRepository<Customer>>();
            auditService=Substitute.For<IAuditService>();
            mapper=Substitute.For<IMapper>();
            logger = Substitute.For<ILogger<CreateCustomerCommandHandler>>();
            userContext = Substitute.For<CustomersTask4.Users.IUserContext>();

            _handler =new CreateCustomerCommandHandler(repository,logger,mapper,auditService, userContext);
        }
        [Fact]
        public async Task HandlerShouldReturnPhoneExistsWhenPhoneExistInDb()
        {
            //Arrange
            var command = new CreateCustomerCommand { Phone= "011111111111" };
            repository.PhoneExistsAsync(command.Phone).Returns(true);
            //Act &&
            //assert
            var ex=await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, default));

            Assert.Equal("this phone number already exists", ex.Message);
        }
    }
}
