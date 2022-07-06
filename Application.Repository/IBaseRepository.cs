
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface IBaseRepository<T> where T : class
    {      
        IQueryable<T> GetAll();
        IQueryable<T> GetAllAsNoTracking();
        Task<T> FindAsync(string id, CancellationToken cancellationToken = default);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);      
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task DeleteAsync(Expression<Func<T, bool>> predicate);
        Task DeleteAsync(IList<T> entities);
        Task<T> GetDetailAsync(Expression<Func<T, bool>> predicate, string includeProperties, CancellationToken cancellationToken = default);
    }
}