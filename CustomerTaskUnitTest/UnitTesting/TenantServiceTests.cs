using CustomersTask4.Exceptions;
using CustomersTask4.Services;
using CustomersTask4.Setting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace CustomersTaskUnitTest.UnitTesting
{
    public class TenantServiceTests
    {
        private readonly IOptions<TenantSetting> _tenantSettingsOptions;
        private readonly TenantService _tenantService;
        private readonly TenantSetting _tenantSettings;

        public TenantServiceTests()
        {
            _tenantSettings = new TenantSetting
            {
                Defaults = new Configration
                {
                    DBProvider = "sql",
                    ConnectionString = "Server=.;Database=DefaultDb;Trusted_Connection=true;"
                },
                Tenants = new List<Tenant>
                {
                    new Tenant
                    {
                        TenantId = "Tenant1",
                        Name = "Tenant1",
                        ConnectionString = "Server=.;Database=Tenant1Db;Trusted_Connection=true;"
                    },
                    new Tenant
                    {
                        TenantId = "Tenant2",
                        Name = "Tenant2",
                        ConnectionString = "Server=.;Database=Tenant2Db;Trusted_Connection=true;"
                    },
                    new Tenant
                    {
                        TenantId = "Tenant3",
                        Name = "Tenant3",
                        ConnectionString = null
                    }
                }
            };

            _tenantSettingsOptions = Options.Create(_tenantSettings);
            _tenantService = new TenantService(_tenantSettingsOptions);
        }

        #region SetCurrentTenant Tests

        [Fact]
        public void SetCurrentTenant_ShouldSetTenant_WhenTenantIdExists()
        {
            // Arrange
            var tenantId = "Tenant1";

            // Act
            _tenantService.SetCurrentTenant(tenantId);

            // Assert
            var currentTenant = _tenantService.GetCurrentTenant();
            Assert.NotNull(currentTenant);
            Assert.Equal("Tenant1", currentTenant.TenantId);
            Assert.Equal("Tenant1", currentTenant.Name);
        }

        [Fact]
        public void SetCurrentTenant_ShouldThrowNotFoundException_WhenTenantIdDoesNotExist()
        {
            // Arrange
            var invalidTenantId = "non-existent-tenant";

            // Act & Assert
            var exception = Assert.Throws<NotFoundException>(
                () => _tenantService.SetCurrentTenant(invalidTenantId));
            
            Assert.Contains($"Tenant with id {invalidTenantId} not found", exception.Message);
        }

        [Fact]
        public void SetCurrentTenant_ShouldUseDefaultConnectionString_WhenTenantConnectionStringIsNull()
        {
            // Arrange
            var tenantIdWithNullConnection = "Tenant3";

            // Act
            _tenantService.SetCurrentTenant(tenantIdWithNullConnection);

            // Assert
            var currentTenant = _tenantService.GetCurrentTenant();
            Assert.NotNull(currentTenant);
            Assert.Equal(_tenantSettings.Defaults.ConnectionString, currentTenant.ConnectionString);
        }

        [Fact]
        public void SetCurrentTenant_ShouldSetTenantToNull_WhenCalledWithEmptyTenantId()
        {
            // Arrange
            var emptyTenantId = "";

            // Act & Assert
            Assert.Throws<NotFoundException>(
                () => _tenantService.SetCurrentTenant(emptyTenantId));
        }

        #endregion

        #region GetCurrentTenant Tests

        [Fact]
        public void GetCurrentTenant_ShouldReturnNull_WhenNoTenantIsSet()
        {
            // Act
            var currentTenant = _tenantService.GetCurrentTenant();

            // Assert
            Assert.Null(currentTenant);
        }

        [Fact]
        public void GetCurrentTenant_ShouldReturnSetTenant_WhenTenantIsSet()
        {
            // Arrange
            _tenantService.SetCurrentTenant("Tenant2");

            // Act
            var currentTenant = _tenantService.GetCurrentTenant();

            // Assert
            Assert.NotNull(currentTenant);
            Assert.Equal("Tenant2", currentTenant.TenantId);
            Assert.Equal("Tenant2", currentTenant.Name);
        }

        #endregion

        #region GetConnectionString Tests

        [Fact]
        public void GetConnectionString_ShouldReturnCurrentTenantConnectionString_WhenTenantIsSet()
        {
            // Arrange
            var expectedConnectionString = "Server=.;Database=Tenant1Db;Trusted_Connection=true;";
            _tenantService.SetCurrentTenant("Tenant1");

            // Act
            var connectionString = _tenantService.GetConnectionString();

            // Assert
            Assert.Equal(expectedConnectionString, connectionString);
        }

        [Fact]
        public void GetConnectionString_ShouldReturnDefaultConnectionString_WhenNoTenantIsSet()
        {
            // Act
            var connectionString = _tenantService.GetConnectionString();

            // Assert
            Assert.Equal(_tenantSettings.Defaults.ConnectionString, connectionString);
        }

        [Fact]
        public void GetConnectionString_ShouldReturnDefaultConnectionString_WhenTenantHasNoConnectionString()
        {
            // Arrange
            _tenantService.SetCurrentTenant("Tenant3");

            // Act
            var connectionString = _tenantService.GetConnectionString();

            // Assert
            Assert.Equal(_tenantSettings.Defaults.ConnectionString, connectionString);
        }

        #endregion

        #region GetDatabaseProvider Tests

        [Fact]
        public void GetDatabaseProvider_ShouldReturnDefaultDatabaseProvider()
        {
            // Act
            var dbProvider = _tenantService.GetDatabaseProvider();

            // Assert
            Assert.Equal("sql", dbProvider);
        }

        [Fact]
        public void GetDatabaseProvider_ShouldReturnSameProvider_RegardlessOfCurrentTenant()
        {
            // Arrange
            _tenantService.SetCurrentTenant("Tenant1");
            var providerForTenant1 = _tenantService.GetDatabaseProvider();

            _tenantService.SetCurrentTenant("Tenant2");
            var providerForTenant2 = _tenantService.GetDatabaseProvider();

            // Assert
            Assert.Equal(providerForTenant1, providerForTenant2);
            Assert.Equal("sql", providerForTenant1);
        }

        #endregion
    }
}