using CustomersTask4.CQRS.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.CQRS.CustomerHandler.Command.DeleteCustomer;
using CustomersTask4.CQRS.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.Domain;
using CustomersTask4.GraphQL.Mutaion;
using CustomersTask4.Repository;
using CustomersTask4.Users;
using FluentValidation;
using FluentValidation.Results;
using HotChocolate;
using MapsterMapper;
using NSubstitute;
using Renci.SshNet.Messages.Authentication;
using Shared.Services;
using System.Linq.Expressions;

namespace CustomerTaskUnitTest.GraphQLUnitTest
{
    public class CustomerManagementMutaionTests
    {
        private readonly IGenericRepository<Customer> _repo;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly ILocalizationService _localization;
        private readonly IValidator<CreateCustomerCommand> _createValidator;
        private readonly IValidator<UpdateCustomerCommand> _updateValidator;
        private readonly CustomerManagementMutaion _sut;

        public CustomerManagementMutaionTests()
        {
            _repo = Substitute.For<IGenericRepository<Customer>>();
            _mapper = Substitute.For<IMapper>();
            _userContext = Substitute.For<IUserContext>();
            _localization = Substitute.For<ILocalizationService>();
            _createValidator = Substitute.For<IValidator<CreateCustomerCommand>>();
            _updateValidator = Substitute.For<IValidator<UpdateCustomerCommand>>();
            _sut = new CustomerManagementMutaion();
        }

        //Helpers

        private static ValidationResult ValidResult() => new ValidationResult();

        private static ValidationResult InvalidResult(string field, string message) =>
            new ValidationResult(new[] { new ValidationFailure(field, message) });


        private void SetupLocalizationContains(string value) =>
            _localization.Localize(Arg.Any<string>()).Returns(value);

        //CreateCustomer

        [Fact]
        public async Task CreateCustomer_ValidRequest_CreatesAndReturnsSuccessMessage()
        {
            // Arrange
            var request = new CreateCustomerCommand { Phone = "01012345678" };
            var customer = new Customer();
            var currentUser = new CurrentUser("123","abdoTest",new List<string> { "User"});
            var successMessage = "Customer Created Successfully";

            _createValidator.ValidateAsync(request).Returns(ValidResult());
            _repo.PhoneExistsAsync(request.Phone).Returns(false);
            _mapper.Map<Customer>(request).Returns(customer);
            _userContext.GetCurrentUser().Returns(currentUser);
            _localization.Localize("Customer Created Successfully").Returns(successMessage);

            // Act
            var result = await _sut.CreateCustomer(
                request, _createValidator, _repo, _mapper, _userContext, _localization);

            // Assert
            Assert.Equal(successMessage, result);
            await _repo.Received(1).Add(customer);
        }

        [Fact]
        public async Task CreateCustomer_ValidRequest_SetsCreatedByFromCurrentUser()
        {
            // Arrange
            var request = new CreateCustomerCommand { Phone = "01012345678" };
            var customer = new Customer();
            var currentUser = new CurrentUser("123", "abdoTest", new List<string> { "User" });

            _createValidator.ValidateAsync(request).Returns(ValidResult());
            _repo.PhoneExistsAsync(request.Phone).Returns(false);
            _mapper.Map<Customer>(request).Returns(customer);
            _userContext.GetCurrentUser().Returns(currentUser);
            SetupLocalizationContains("Customer Created Successfully");

            // Act
            await _sut.CreateCustomer(
                request, _createValidator, _repo, _mapper, _userContext, _localization);

            // Assert
            Assert.Equal("abdoTest", customer.CreatedBy);
        }

        [Fact]
        public async Task CreateCustomer_NullUser_DoesNotSetCreatedBy()
        {
            // Arrange
            var request = new CreateCustomerCommand { Phone = "01012345678" };
            var customer = new Customer();

            _createValidator.ValidateAsync(request).Returns(ValidResult());
            _repo.PhoneExistsAsync(request.Phone).Returns(false);
            _mapper.Map<Customer>(request).Returns(customer);
            _userContext.GetCurrentUser().Returns((CurrentUser?)null);
            SetupLocalizationContains("Customer Created Successfully");

            // Act
            await _sut.CreateCustomer(
                request, _createValidator, _repo, _mapper, _userContext, _localization);

            // Assert
            Assert.Null(customer.CreatedBy);
        }

        [Fact]
        public async Task CreateCustomer_ValidationFails_ThrowsGraphQLException()
        {
            // Arrange
            var request = new CreateCustomerCommand { Name="abdo" };
            _createValidator.ValidateAsync(request)
                            .Returns(InvalidResult("Phone", "Phone is required"));

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _sut.CreateCustomer(
                    request, _createValidator, _repo, _mapper, _userContext, _localization));

            await _repo.DidNotReceive().Add(Arg.Any<Customer>());
        }

        [Fact]
        public async Task CreateCustomer_PhoneAlreadyExists_ThrowsGraphQLException()
        {
            // Arrange
            var request = new CreateCustomerCommand { Phone = "01012345678" };
            _createValidator.ValidateAsync(request).Returns(ValidResult());
            _repo.PhoneExistsAsync(request.Phone).Returns(true);
            SetupLocalizationContains("Phone number already exists");

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _sut.CreateCustomer(
                    request, _createValidator, _repo, _mapper, _userContext, _localization));

            await _repo.DidNotReceive().Add(Arg.Any<Customer>());
        }

        //UpdateCustomer

        [Fact]
        public async Task UpdateCustomer_ValidRequest_UpdatesAndReturnsSuccessMessage()
        {
            // Arrange
            var request = new UpdateCustomerCommand { Id = "1", Phone = "01099999999" };
            var existingCustomer = new Customer { Id = "1", Phone = "01011111111" };
            var currentUser = new CurrentUser("123", "abdoTest", new List<string> { "User" });
            var successMessage = "Customer Updated";

            _updateValidator.ValidateAsync(request).Returns(ValidResult());
            _repo.GetByIdAsync(request.Id, Arg.Any<System.Linq.Expressions.Expression<Func<Customer, object>>>())
                 .Returns(existingCustomer);
            _repo.PhoneExistsAsync(request.Phone).Returns(false);
            _userContext.GetCurrentUser().Returns(currentUser);
            _localization.Localize("Customer Updated").Returns(successMessage);

            // Act
            var result = await _sut.UpdateCustomer(
                request, _repo, _mapper, _userContext, _localization, _updateValidator);

            // Assert
            Assert.Equal(successMessage, result);
            await _repo.Received(1).Update(existingCustomer);
        }

        [Fact]
        public async Task UpdateCustomer_ValidRequest_SetsChangedByAndChangedAt()
        {
            // Arrange
            var request = new UpdateCustomerCommand { Id = "1", Phone = "01099999999" };
            var existingCustomer = new Customer { Id = "1", Phone = "01011111111" };
            var currentUser = new CurrentUser("123", "abdoTest", new List<string> { "User" });

            _updateValidator.ValidateAsync(request).Returns(ValidResult());
            _repo.GetByIdAsync(request.Id, Arg.Any<System.Linq.Expressions.Expression<Func<Customer, object>>>())
                 .Returns(existingCustomer);
            _repo.PhoneExistsAsync(request.Phone).Returns(false);
            _userContext.GetCurrentUser().Returns(currentUser);
            _localization.Localize("Update Customer");

            var before = DateTime.UtcNow;

            // Act
            await _sut.UpdateCustomer(
                request, _repo, _mapper, _userContext, _localization, _updateValidator);

            // Assert
            Assert.Equal("abdoTest", existingCustomer.ChangedBy);
            Assert.True(existingCustomer.ChangedAt >= before);
        }

        [Fact]
        public async Task UpdateCustomer_ValidationFails_ThrowsGraphQLException()
        {
            // Arrange
            var request = new UpdateCustomerCommand { Id = "1", Phone = "01099999999" };
            _updateValidator.ValidateAsync(request)
                            .Returns(InvalidResult("Phone", "Invalid phone"));

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _sut.UpdateCustomer(
                    request, _repo, _mapper, _userContext, _localization, _updateValidator));

            await _repo.DidNotReceive().Update(Arg.Any<Customer>());
        }

        [Fact]
        public async Task UpdateCustomer_CustomerNotFound_ThrowsGraphQLException()
        {
            // Arrange
            var request = new UpdateCustomerCommand { Id = "999", Phone = "01099999999" };
            _updateValidator.ValidateAsync(request).Returns(ValidResult());
            _repo.GetByIdAsync(request.Id, Arg.Any<Expression<Func<Customer, object>>>())
                 .Returns((Customer?)null);
            SetupLocalizationContains($"Customer with id {request.Id} not found.");

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _sut.UpdateCustomer(
                    request, _repo, _mapper, _userContext, _localization, _updateValidator));

            await _repo.DidNotReceive().Update(Arg.Any<Customer>());
        }

        [Fact]
        public async Task UpdateCustomer_PhoneExistsForDifferentCustomer_ThrowsGraphQLException()
        {
            // Arrange
            var request = new UpdateCustomerCommand { Id = "1", Phone = "01013513652" };
            var existingCustomer = new Customer { Id = "1", Phone = "01011111111" };

            _updateValidator.ValidateAsync(request).Returns(ValidResult());
            _repo.GetByIdAsync(request.Id, Arg.Any<Expression<Func<Customer, object>>>())
                 .Returns(existingCustomer);
            _repo.PhoneExistsAsync(request.Phone).Returns(true); 
            _localization.Localize("Customer Phone Exist", request.Phone)
                .Returns("Customer Phone Exist");

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _sut.UpdateCustomer(
                    request, _repo, _mapper, _userContext, _localization, _updateValidator));

            await _repo.DidNotReceive().Update(Arg.Any<Customer>());
        }

        [Fact]
        public async Task UpdateCustomer_SamePhoneAsCurrentCustomer_DoesNotThrow()
        {
            // Arrange 
            var request = new UpdateCustomerCommand { Id = "1", Phone = "01011111111" };
            var existingCustomer = new Customer { Id = "1", Phone = "01011111111" };

            _updateValidator.ValidateAsync(request).Returns(ValidResult());
            _repo.GetByIdAsync(request.Id, Arg.Any<Expression<Func<Customer, object>>>())
                 .Returns(existingCustomer);
            _repo.PhoneExistsAsync(request.Phone).Returns(true);
            _userContext.GetCurrentUser().Returns((CurrentUser?)null);
            SetupLocalizationContains("Customer Updated");

            // Act
            var result = await _sut.UpdateCustomer(
                request, _repo, _mapper, _userContext, _localization, _updateValidator);

            // Assert
            await _repo.Received(1).Update(existingCustomer);
        }

        //DeleteCustomer

        [Fact]
        public async Task DeleteCustomer_ExistingCustomer_DeletesAndReturnsSuccessMessage()
        {
            // Arrange
            var request = new DeleteCustomerCommand("1");
            var customer = new Customer { Id = "1" };
            var successMessage = "Customer Deleted Successfully";

            _repo.GetByIdAsync(request.Id).Returns(customer);
            _localization.Localize("Customer Deleted Successfully").Returns(successMessage);

            // Act
            var result = await _sut.DeleteCustomer(
                request, _repo, _mapper, _userContext, _localization);

            // Assert
            Assert.Equal(successMessage, result);
            await _repo.Received(1).Delete(customer);
        }

        [Fact]
        public async Task DeleteCustomer_CustomerNotFound_ThrowsGraphQLException()
        {
            // Arrange
            var request = new DeleteCustomerCommand("999");
            _repo.GetByIdAsync(request.Id).Returns((Customer?)null);

            _localization.Localize("CustomerNotFound", request.Id).Returns("CustomerNotFound");

            // Act & Assert
            await Assert.ThrowsAsync<GraphQLException>(() =>
                _sut.DeleteCustomer(
                    request, _repo, _mapper, _userContext, _localization));

            await _repo.DidNotReceive().Delete(Arg.Any<Customer>());
        }
    }
}
