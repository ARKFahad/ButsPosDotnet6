using Application.Core.Entities;
using Application.RequestModel;
using Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface ISaleQueryService
    {
        Task<SaleViewModel> GetDetailAsync(string id);
        Task<SaleViewModel> GetDetailAsync(Expression<Func<Sale, bool>> predicate, string includeProperties);
        Task<Tuple<List<SaleViewModel>, int>> SearchAsync(SaleRequestModel request);

    }
}
