
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PbApp.Service
{
    public interface IBaseQueryService<T, Tr, Tv> 
    {
        Task<Tv> GetDetailAsync(string id);
        Task<IEnumerable<Tv>> GetAllAsync();
        Task<Tuple<List<Tv>, int>> SearchAsync(Tr request);
        Task<Tv> GetDetailAsync(Expression<Func<T, bool>> predicate, string includeProperties);

    }
}