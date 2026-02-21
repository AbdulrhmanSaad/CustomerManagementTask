using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.Users;
using System.Text.Json;

namespace CustomersTask4.Services
{
    public interface IAuditService
    {
        Task LogCustomerChangeAsync(Customer oldValues, Customer newValues, string action);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserContext _userContext;

        public AuditService(ApplicationDbContext dbContext, IUserContext userContext)
        {
            _dbContext = dbContext;
            _userContext = userContext;
        }

       
        public async Task LogCustomerChangeAsync(Customer oldValues, Customer newValues, string action)
        {
            var currentUser = _userContext.GetCurrentUser();

            var audit = new CustomerHistory
            {
                CustomerId = oldValues.Id,
                Action = action,
                ChangedAt = DateTime.UtcNow,
                UserId = currentUser.Id,
                UserEmail = currentUser.Name,
                UserRole = currentUser.Roles.FirstOrDefault() ?? "No Role",
                OldValues = oldValues != null ? JsonSerializer.Serialize(new
                {
                    oldValues.Id,
                    oldValues.Name,
                    oldValues.Phone,
                    oldValues.CreatedAt,
                    oldValues.CreatedBy,
                    oldValues.Addresses
                }) : "{}",
                NewValues = newValues != null ? JsonSerializer.Serialize(new
                {
                    newValues.Id,
                    newValues.Name,
                    newValues.Phone,
                    newValues.CreatedAt,
                    newValues.CreatedBy,
                    newValues.Addresses
                }) : "{}"
            };

            _dbContext.CustomerHistories.Add(audit);
            await _dbContext.SaveChangesAsync();
        }
    }
}