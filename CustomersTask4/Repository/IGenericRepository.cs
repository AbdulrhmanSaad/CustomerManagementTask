using System.Linq.Expressions;

namespace CustomersTask4.Repository
{
    public interface IGenericRepository<T>
    {
        List<T> GetAll(params Expression<Func<T, object>>[] includes);
        Task<T?> GetByIdAsync(string id, params Expression<Func<T, object>>[] includes);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(T entity);
        bool PhoneExistsAsync(string phoneNumber);

    }
}
