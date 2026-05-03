using AuthServer;
using CustomersTask4;
using CustomerTaskUnitTest.Client;
using CustomerTaskUnitTest.IntegrationTest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomerTaskUnitTest.Minimal_Api
{
    public class CustomerMinimalApiIntegrationTests : TestBase
    {
        private readonly string DefaultTenantId = "SharedTenant";
        public CustomerMinimalApiIntegrationTests(
            WebApplicationFactory<IAssmblyMarker> factory,
            WebApplicationFactory<IAuthAssmblyMarker> auth)
            : base(factory, auth) { }

        #region Get All Customers

        [Fact]
        public async Task GetAll_ShouldReturnAllCustomers()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            // Act
            var result= await client.CustomerAllAsync(DefaultTenantId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAll_WithoutAuthentication_ShouldThrowApiException()
        {
            // Arrange
            var client = CreateApiClient(); 

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerAllAsync(DefaultTenantId));
        }

        [Fact]
        public async Task GetAll_WithoutTenant_ShouldThrowApiException()
        {
            // Arrange
            var token = await GenerateToken();
            var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var client = new NswagClient(httpClient)
            {
                BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7120/"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerAllAsync(""));
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
            await client.CustomerPOSTAsync(DefaultTenantId, createCommand);

            var allCustomers = await client.CustomerAllAsync(DefaultTenantId);
            var createdCustomer = allCustomers.FirstOrDefault(c => c.Phone == createCommand.Phone);

            Assert.NotNull(createdCustomer);

            // Act
            var result = await client.CustomerGETAsync(createdCustomer.Id, DefaultTenantId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createCommand.Name, result.Name);
            Assert.Equal(createCommand.Phone, result.Phone);
        }

        [Fact]
        public async Task GetCustomerById_WithInvalidId_ShouldThrowApiException()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerGETAsync("nonexistent-id-12345", DefaultTenantId));
        }

        [Fact]
        public async Task GetCustomerById_WithoutAuthentication_ShouldThrowApiException()
        {
            // Arrange
            var client = CreateApiClient(); // No token

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerGETAsync("any-id", DefaultTenantId));
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
            await client.CustomerPOSTAsync(DefaultTenantId, createCommand);

            // Assert
            var allCustomers = await client.CustomerAllAsync(DefaultTenantId);
            var createdCustomer = allCustomers.FirstOrDefault(c => c.Phone == createCommand.Phone);

            Assert.NotNull(createdCustomer);
            Assert.Equal(createCommand.Name, createdCustomer.Name);
            Assert.Equal(createCommand.Phone, createdCustomer.Phone);
        }

        [Fact]
        public async Task CreateCustomer_WithoutAuthentication_ShouldThrowApiException()
        {
            // Arrange
            var client = CreateApiClient(); // No token

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerPOSTAsync(DefaultTenantId, CreateCustomer()));
        }

        [Fact]
        public async Task CreateCustomer_WithoutTenant_ShouldThrowApiException()
        {
            // Arrange
            var token = await GenerateToken();
            var httpClient = _factory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var client = new NswagClient(httpClient)
            {
                BaseUrl = httpClient.BaseAddress?.ToString() ?? "https://localhost:7120/"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerPOSTAsync("", CreateCustomer()));
        }

        [Fact]
        public async Task CreateCustomer_WithDuplicatePhone_ShouldThrowApiException()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var createCommand = CreateCustomer();
            await client.CustomerPOSTAsync(DefaultTenantId, createCommand);

            // Act & Assert - same phone again
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerPOSTAsync(DefaultTenantId, createCommand));
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
            await client.CustomerPOSTAsync(DefaultTenantId, createCommand);

            var allCustomers = await client.CustomerAllAsync(DefaultTenantId);
            var customer = allCustomers.FirstOrDefault(c => c.Phone == createCommand.Phone);

            Assert.NotNull(customer);

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
            await client.CustomerPUTAsync(customer.Id, DefaultTenantId, updateCommand);

            // Assert
            var updatedCustomer = await client.CustomerGETAsync(customer.Id, DefaultTenantId);
            Assert.Equal(updateCommand.Name, updatedCustomer.Name);
            Assert.Equal(updatedPhone, updatedCustomer.Phone);
        }

        [Fact]
        public async Task UpdateCustomer_WithInvalidId_ShouldThrowApiException()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var updateCommand = new UpdateCustomerCommand
            {
                Id = "invalid-id",
                Name = "Updated Name",
                Phone = GenerateUniquePhone()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerPUTAsync("invalid-id", DefaultTenantId, updateCommand));
        }

        [Fact]
        public async Task UpdateCustomer_WithoutAuthentication_ShouldThrowApiException()
        {
            // Arrange
            var client = CreateApiClient();

            var updateCommand = new UpdateCustomerCommand
            {
                Id = "any-id",
                Name = "Updated Name",
                Phone = GenerateUniquePhone()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerPUTAsync("any-id", DefaultTenantId, updateCommand));
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
            await client.CustomerPOSTAsync(DefaultTenantId, createCommand);

            var allCustomers = await client.CustomerAllAsync(DefaultTenantId);
            var createdCustomer = allCustomers.FirstOrDefault(c => c.Phone == createCommand.Phone);

            Assert.NotNull(createdCustomer);

            // Act
            await client.CustomerDELETEAsync(createdCustomer.Id, DefaultTenantId);

            // Assert
            var allCustomersAfter = await client.CustomerAllAsync(DefaultTenantId);
            var deletedCustomer = allCustomersAfter.FirstOrDefault(c => c.Id == createdCustomer.Id);

            Assert.Null(deletedCustomer);
        }

        [Fact]
        public async Task DeleteCustomer_WithInvalidId_ShouldThrowApiException()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerDELETEAsync("invalid-id", DefaultTenantId));
        }

        [Fact]
        public async Task DeleteCustomer_WithoutAuthentication_ShouldThrowApiException()
        {
            // Arrange
            var client = CreateApiClient();

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.CustomerDELETEAsync("any-id", DefaultTenantId));
        }

        #endregion

        #region Customer History

        [Fact]
        public async Task GetCustomerHistory_WithValidId_ShouldReturnHistory()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var createCommand = CreateCustomer();
            await client.CustomerPOSTAsync(DefaultTenantId, createCommand);

            var allCustomers = await client.CustomerAllAsync(DefaultTenantId);
            var customer = allCustomers.FirstOrDefault(c => c.Phone == createCommand.Phone);

            Assert.NotNull(customer);

            // Act
            var history = await client.History2Async(customer.Id, DefaultTenantId);

            // Assert
            Assert.NotNull(history);
        }

        [Fact]
        public async Task GetCustomerHistory_WithInvalidId_ShouldThrowApiException()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.History2Async("invalid-id",DefaultTenantId));
        }

        #endregion

        #region Address History

        [Fact]
        public async Task GetCustomerAddressHistory_WithValidId_ShouldReturnAddressHistory()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var createCommand = CreateCustomer();
            await client.CustomerPOSTAsync(DefaultTenantId, createCommand);

            var allCustomers = await client.CustomerAllAsync(DefaultTenantId);
            var customer = allCustomers.FirstOrDefault(c => c.Phone == createCommand.Phone);

            Assert.NotNull(customer);

            // Act
            var addressHistory = await client.AddressHistoryAsync(customer.Id, DefaultTenantId);

            // Assert
            Assert.NotNull(addressHistory);
        }

        [Fact]
        public async Task GetCustomerAddressHistory_WithInvalidId_ShouldThrowApiException()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.AddressHistoryAsync("invalid-id", DefaultTenantId));
        }

        #endregion

        #region Generate Report

        [Fact]
        public async Task GenerateReport_WithValidDateRange_ShouldSucceed()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var query = new GenerateCustomerPDFQuery
            {
                From = DateTimeOffset.UtcNow.AddMonths(-1),
                To = DateTimeOffset.UtcNow
            };

            // Act & Assert
            await client.ReportAsync(DefaultTenantId, query);
        }

        [Fact]
        public async Task GenerateReport_IsAllowedAnonymous_ShouldSucceed()
        {
            // Arrange
            var client = CreateApiClient();

            var query = new GenerateCustomerPDFQuery
            {
                From = DateTimeOffset.UtcNow.AddMonths(-1),
                To = DateTimeOffset.UtcNow
            };

            // Act & Assert
            await client.ReportAsync(DefaultTenantId, query);
        }

        #endregion

        #region Migrate

        [Fact]
        public async Task Migrate_WithValidTenants_ShouldReturnAccepted()
        {
            // Arrange
            var token = await GenerateToken();
            var client = CreateApiClient(token);

            var command = new MigrationCommand
            {
                From = "Sql",
                To = "Mongo"
            };

            // Act & Assert
           await client.Migrate2Async(DefaultTenantId, command);
        }

        [Fact]
        public async Task Migrate_WithoutAuthentication_ShouldThrowApiException()
        {
            // Arrange
            var client = CreateApiClient();

            var command = new MigrationCommand
            {
                From = "Sql",
                To = "Mongo"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                () => client.Migrate2Async(DefaultTenantId, command));
        }

        #endregion
    }
}
