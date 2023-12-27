using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Repositories;
using Application.RequestModel;
using Application.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SaleQueryService: ISaleQueryService
    {
        public IBaseRepository<Sale> _repository;
        public SaleQueryService(IBaseRepository<Sale> repository)
        {
            _repository = repository;
        }

        public virtual async Task<SaleViewModel> GetDetailAsync(string id)
        {
            Sale x = await _repository.FindAsync(id);
            if (x == null)
                throw new NotFoundResultException("No Result Found with this id:" + id);

            return new SaleViewModel(x);
        }

        public virtual async Task<SaleViewModel> GetDetailAsync(Expression<Func<Sale, bool>> predicate, string includeProperties)
        {
            Sale x = await _repository.GetDetailAsync(predicate, includeProperties);
            if (x == null)
                throw new NotFoundResultException("Not Found ");
            return new SaleViewModel(x);
        }

       
        public virtual async Task<Tuple<List<SaleViewModel>, int>> SearchAsync(SaleRequestModel request)
        {
            var queryable = request.GetOrderedData(_repository.GetAll());
            if (!string.IsNullOrWhiteSpace(request.BranchId))
                queryable = queryable.Where(x => x.BranchId == request.BranchId);
                  
            if (!queryable.Any<Sale>())
                throw new NotFoundResultException("Ooo! No Search Result Found.");
            int count = queryable.Count();
            queryable = request.SkipAndTake(queryable);
            if (request.IsIncludeParents)
                queryable = request.IncludeParents(queryable);

            var vms = await queryable.Select(x => new SaleViewModel(x)).ToListAsync();
            
            //List<SaleViewModel> vms = new List<SaleViewModel>();
            //foreach (var item in list)
            //{
            //   vms.Add(new SaleViewModel(item));
            //}
            return new Tuple<List<SaleViewModel>, int>(vms, count);
        }

    }
}
