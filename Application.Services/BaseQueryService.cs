
using Application.Core.Exceptions;
using Application.Repositories;
using Application.RequestModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PbApp.Service
{
    public class BaseQueryService<T, Tr, Tv> : IBaseQueryService<T, Tr, Tv> where T : class where Tr : BaseRequestModel<T>
    {

        public IBaseRepository<T> _repository;
        public BaseQueryService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }
     
        public virtual async Task<Tv> GetDetailAsync(string id)
        {
            T x = await _repository.FindAsync(id);
            if (x == null)
                throw new NotFoundResultException("No Result Found with this id:" + id);

            var vm = CreateVmInstance(x);
            return vm;
        }

        public virtual async Task<Tv> GetDetailAsync(Expression<Func<T, bool>> predicate,string includeProperties)
        {
            T x = await _repository.GetDetailAsync(predicate,includeProperties);
            if (x == null)
                throw new NotFoundResultException("Not Found ");

            var vm = CreateVmInstance(x);
            return vm;
        }
        public virtual async Task<IEnumerable<Tv>> GetAllAsync()
        {
            var list = await _repository.GetAll().ToListAsync();
            if (!list.Any<T>())
            {
                throw new NotFoundResultException("Not found.");
            }
            return list.ConvertAll(CreateVmInstance);
        }

        public virtual async Task<Tuple<List<Tv>, int>> SearchAsync(Tr request)
        {
            var queryable = request.GetOrderedData(_repository.GetAllAsNoTracking());
            if (!queryable.Any<T>())
            {
                throw new NotFoundResultException("Ooo! No Search Result Found.");
            }
            int count = queryable.Count();
            queryable = request.SkipAndTake(queryable);
            if (request.IsIncludeParents)
            {
                queryable = request.IncludeParents(queryable);
            }

            var list = await queryable.ToListAsync();
            List<Tv> vms = list.ConvertAll(CreateVmInstance);
            return new Tuple<List<Tv>, int>(vms, count);
        }

      
        private static Tv CreateVmInstance(T x)
        {
            return (Tv)Activator.CreateInstance(typeof(Tv), x);
        }

       
    }
}