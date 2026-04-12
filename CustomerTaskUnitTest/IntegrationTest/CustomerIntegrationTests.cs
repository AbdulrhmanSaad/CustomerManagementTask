using CustomersTask4;
using CustomerTaskUnitTest.Client;
using CustomerTaskUnitTest.UnitTesting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomerTaskUnitTest.IntegrationTest
{
    public class CustomerIntegrationTests : TestBase
    {
        private const string ApiVersion = "1";
        private const string DefaultTenant = "Tenant1";

        public CustomerIntegrationTests(WebApplicationFactory<IAssmblyMarker> factory) 
            : base(factory)
        {
        }

        #region Get All Customers

        [Fact]
        public async Task GetAll_ShouldReturnAllCustomers()
        {
            // Act
            var token =await GenerateToken();
            var result = await CreateApiClient(token).CustomerAllAsync(ApiVersion, DefaultTenant);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetAll_WithoutTenant_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GenerateToken();
            var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var client = new NswagClient(httpClient);
            client.BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7120/";

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerAllAsync(ApiVersion, ""));
        }

        #endregion

        #region Get Customer By Id

        [Fact]
        public async Task GetCustomerById_WithValidId_ShouldReturnCustomer()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var createCommand = CreateCustomer();

            // Act - Create the customer
            await client.CustomerPOSTAsync(ApiVersion, DefaultTenant, createCommand);

            // Get all customers to find the created one
            var allCustomers = await client.CustomerAllAsync(ApiVersion, DefaultTenant);
            var createdCustomer = allCustomers.FirstOrDefault(c => c.Name == createCommand.Name);

            Assert.NotNull(createdCustomer);

            // Get the specific customer
            var result = await client.CustomerGETAsync(createdCustomer.Id, ApiVersion, DefaultTenant);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createCommand.Name, result.Name);
            Assert.Equal(createCommand.Phone, result.Phone);
        }

        [Fact]
        public async Task GetCustomerById_WithInvalidId_ShouldThrowNotFound()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);
            var invalidId = "nonexistent-id-12345";

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerGETAsync(invalidId, ApiVersion, DefaultTenant));
        }

        #endregion

        #region Create Customer

        [Fact]
        public async Task CreateCustomer_WithValidData_ShouldCreateSuccessfully()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var createCommand = CreateCustomer();

            // Act
            await client.CustomerPOSTAsync(ApiVersion, DefaultTenant, createCommand);

            // Assert
            var allCustomers = await client.CustomerAllAsync(ApiVersion, DefaultTenant);
            var createdCustomer = allCustomers.FirstOrDefault(c => c.Name == createCommand.Name);

            Assert.NotNull(createdCustomer);
            Assert.Equal(createCommand.Phone, createdCustomer.Phone);
        }

        [Fact]
        public async Task CreateCustomer_WithoutAuthentication_ShouldFail()
        {
            // Arrange
            var client = CreateApiClient(); // No token

            var createCustomer = CreateCustomer();

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerPOSTAsync(ApiVersion, DefaultTenant, createCustomer));
        }

        [Fact]
        public async Task CreateCustomer_WithMissingRequiredFields_ShouldFail()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            // Missing Phone field
            var invalidCommand = new CreateCustomerCommand
            {
                Name = "Invalid Customer"
            };
            // Act & Assert
            await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerPOSTAsync(ApiVersion, DefaultTenant, invalidCommand));
        }

        #endregion

        #region Update Customer

        [Fact]
        public async Task UpdateCustomer_WithValidData_ShouldUpdateSuccessfully()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var createCommand = CreateCustomer();
            await client.CustomerPOSTAsync(ApiVersion, DefaultTenant, createCommand);

            // Get the customer
            var allCustomers = await client.CustomerAllAsync(ApiVersion, DefaultTenant);
            var customer = allCustomers.FirstOrDefault(c => c.Name == createCommand.Name);

            Assert.NotNull(customer);

            // Update the customer
            var updatedPhone = GenerateUniquePhone();
            var updateCommand = new UpdateCustomerCommand
            {
                Id = customer.Id,
                Name = "Updated Name" + updatedPhone,
                Phone = updatedPhone,
                Addresses = new List<AddressDtoEnum>
                {
                    new AddressDtoEnum
                    {
                        AddressName = "Cairo",
                        AddressType = 0
                    }
                }
            };

            // Act
            await client.CustomerPUTAsync(customer.Id, ApiVersion, DefaultTenant, updateCommand);

            // Assert
            var updatedCustomer = await client.CustomerGETAsync(customer.Id, ApiVersion, DefaultTenant);
            Assert.Equal(updateCommand.Name, updatedCustomer.Name);
            Assert.Equal(updatedPhone, updatedCustomer.Phone);
        }

        [Fact]
        public async Task UpdateCustomer_WithInvalidId_ShouldFail()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var updateCommand = new UpdateCustomerCommand
            {
                Id = "invalid-id",
                Name = "Updated Name",
                Phone = "01013513652"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerPUTAsync("invalid-id", ApiVersion, DefaultTenant, updateCommand));
        }
        #endregion

        #region Delete Customer
        [Fact]
        public async Task DeleteCustomer_WithValidId_ShouldDeleteSuccessfully()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var createCommand = CreateCustomer();
            await client.CustomerPOSTAsync(ApiVersion, DefaultTenant, createCommand);

            // Get the created customer
            var allCustomers = await client.CustomerAllAsync(ApiVersion, DefaultTenant);
            var createdCustomer = allCustomers.FirstOrDefault(c => c.Name == createCommand.Name);

            Assert.NotNull(createdCustomer);

            // Act - Delete the customer
            await client.CustomerDELETEAsync(createdCustomer.Id, ApiVersion, DefaultTenant);

            // Assert
            var allCustomersAfter = await client.CustomerAllAsync(ApiVersion, DefaultTenant);
            var deletedCustomer = allCustomersAfter.FirstOrDefault(c => c.Id == createdCustomer.Id);

            Assert.Null(deletedCustomer);
        }

        [Fact]
        public async Task DeleteCustomer_WithInvalidId_ShouldFail()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerDELETEAsync("invalid-id", ApiVersion, DefaultTenant));
        }

        #endregion

        #region Customer History

        [Fact]
        public async Task GetCustomerHistory_WithValidId_ShouldReturnHistory()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);
            var phone = GenerateUniquePhone();

            // Create a customer
            var createCommand = CreateCustomer();

            await client.CustomerPOSTAsync(ApiVersion, DefaultTenant, createCommand);

            // Get the customer
            var allCustomers = await client.CustomerAllAsync(ApiVersion, DefaultTenant);
            var customer = allCustomers.FirstOrDefault(c => c.Name == createCommand.Name);

            // Act
            var history = await client.HistoryAsync(customer.Id, ApiVersion, DefaultTenant);
            // Assert
            Assert.NotNull(history);
        }

        [Fact]
        public async Task GetCustomerAddressHistory_WithValidId_ShouldReturnAddressHistory()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var createCommand = CreateCustomer();
            await client.CustomerPOSTAsync(ApiVersion, DefaultTenant, createCommand);

            // Get the customer
            var allCustomers = await client.CustomerAllAsync(ApiVersion, DefaultTenant);
            var customer = allCustomers.FirstOrDefault(c => c.Name == createCommand.Name);

            Assert.NotNull(customer);

            // Act
            var addressHistory = await client.AddressHistoryAsync(customer.Id, ApiVersion, DefaultTenant);

            // Assert
            Assert.NotNull(addressHistory);
        }

        #endregion

        #region Authorization Tests

        [Fact]
        public async Task AllCustomerEndpoints_WithoutTenant_ShouldFail()
        {
            // Arrange
            var token = await GenerateToken();
            var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // Note: Not adding tenant header

            var client = new NswagClient(httpClient);
            client.BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7120/";

            var createCommand = CreateCustomer();
            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerPOSTAsync(ApiVersion, "", createCommand));
        }

        [Fact]
        public async Task AllCustomerEndpoints_WithWrongTenant_ShouldFail()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var createCommand = CreateCustomer();

            // Act & Assert - Using wrong tenant
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerPOSTAsync(ApiVersion, "WrongTenant", createCommand));
        }

        #endregion
    }
}