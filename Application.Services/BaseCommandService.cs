
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
    public class BaseCommandService<T> : IBaseCommandService<T> where T : class
    {

        public IBaseRepository<T> _repository;
        public BaseCommandService(IBaseRepository<T> repository)
        {
            _repository = repository;
        }
        public virtual async Task<T> FindAsync(string id)
        {
            T x = await _repository.FindAsync(id);
            if (x == null)
                throw new NotFoundResultException("No Result Found with this id:" + id);
            return x;
        }
       
        public virtual async  Task AddAsync(T model)
        {
            await _repository.AddAsync(model);
        }

        public virtual async Task DeleteAsync(string id)
        {
            await _repository.DeleteAsync(id);
        }

        public virtual async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            await _repository.DeleteAsync(predicate);
        }

        public virtual async Task DeleteAsync(IList<T> entities)
        {
            await _repository.DeleteAsync(entities);
        }


        public virtual async Task UpdateAsync(T model)
        {
            await _repository.UpdateAsync(model);
        }

        public virtual  string GetReferanceId(string character)
        {
            int count =  _repository.GetAll().Count();
            while (true)
            {
                count++;
                return character + count.ToString().PadLeft(7, '0');
            }

        }

        public virtual async Task<List<T>> GetAllByPredicate(Expression<Func<T, bool>> predicate)
        {
            return await _repository.GetAll().Where(predicate).ToListAsync();
        }


    }
}