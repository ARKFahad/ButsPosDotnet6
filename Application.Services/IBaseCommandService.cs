
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PbApp.Service
{
    public interface IBaseCommandService<T> 
    {
       
        Task<T> FindAsync(string id);  
        Task AddAsync(T model);
        Task UpdateAsync(T model);        
        Task DeleteAsync(string id);
        Task DeleteAsync(Expression<Func<T, bool>> predicate);
        Task DeleteAsync(IList<T> entities);
        string GetReferanceId(string character);
        Task<List<T>> GetAllByPredicate(Expression<Func<T, bool>> predicate);

    }
}