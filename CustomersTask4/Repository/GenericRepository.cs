using CustomersTask4.Data;
using CustomersTask4.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CustomersTask4.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext db;

        public GenericRepository(ApplicationDbContext db) {
            this.db = db;
        }
        public async Task Add(T entity)
        {
            db.Set<T>().Add(entity);
           await db.SaveChangesAsync();
        }

        public async Task Delete(T entity)
        {
            if (entity is ISoftDelete softDeleteEntity)
            {
                softDeleteEntity.IsDeleted = true;
                db.Set<T>().Update(entity);
            }
            else
            {
                db.Set<T>().Remove(entity);
            }
            await db.SaveChangesAsync();
        }

        public List<T> GetAll(Expression<Func<T, bool>> filter=null,params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = db.Set<T>();

            if(filter!=null)
                query = query.Where(filter);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query.ToList();
        }

        public async Task<T?> GetByIdAsync(string id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = db.Set<T>();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id);
        }
        public bool PhoneExistsAsync(string phoneNumber)
        {
            return db.Set<T>().IgnoreQueryFilters().Any(c => EF.Property<string>(c,"Phone") == phoneNumber&&
             EF.Property<bool>(c, "IsDeleted") == false);
        }

       
        public async Task Update(T entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
    }
}
