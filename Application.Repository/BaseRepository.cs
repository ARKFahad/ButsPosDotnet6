
using Application.Core.Common;
using Application.Core.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly GHP_POS_DBContext _context;
        public BaseRepository(GHP_POS_DBContext context)
        {
            _context = context;
        }

        public IQueryable<T> GetAll()
        {
            return _context.Set<T>().AsQueryable();
        }
        public IQueryable<T> GetAllAsNoTracking()
        {
            return _context.Set<T>().AsNoTracking();
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                _context.Set<T>().Add(entity);
                _context.SaveChanges();
            });
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var persistent = await _context.Set<T>().FindAsync(id);
            await Task.Run(() =>
            {
               _context.Remove(persistent);
               _context.SaveChanges();
            });
        }
        public async Task DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            await Task.Run(() =>
            {
                _context.RemoveRange(this.GetAll().Where(predicate));
                _context.SaveChanges();
            });
        }

        public async Task DeleteAsync(IList<T> entities)
        {
                await Task.Run(() =>
            {
                _context.RemoveRange(entities);
                _context.SaveChanges();
            });
        }
        public async Task<T> FindAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FindAsync(id);
        }     

        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                _context.Set<T>().Update(entity);
                //_context.Entry(entity).State = EntityState.Modified;
                _context.SaveChanges();
               
            });
        }

        public async Task<T> GetDetailAsync(Expression<Func<T, bool>> predicate, string includeProperties, CancellationToken cancellationToken = default)
        {
            return await _context.QueryWithDynamicInclude<T>(includeProperties).FirstOrDefaultAsync(predicate);
        }
    }
}