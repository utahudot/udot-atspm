using ATSPM.Application.Models;
using ATSPM.Domain.Services;
using ATSPM.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public abstract class ATSPMRepositoryEFBase<T> : IAsyncRepository<T> where T : ATSPMModelBase
    {
        protected readonly ILogger _log;
        protected readonly DbContext _db;
        protected readonly DbSet<T> _table;

        public ATSPMRepositoryEFBase(DbContext db, ILogger<ATSPMRepositoryEFBase<T>> log)
        {
            _log = log;
            _db = db;
            _table = _db.Set<T>();
        }

        public void Add(T item)
        {
            _table.Add(item);
            _db.SaveChanges();
        }

        public async Task AddAsync(T item)
        {
            await _table.AddAsync(item).ConfigureAwait(false);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public void AddRange(IReadOnlyList<T> items)
        {
            _table.AddRange(items);
            _db.SaveChanges();
        }

        public async Task AddRangeAsync(IReadOnlyList<T> items)
        {
            await _table.AddRangeAsync(items).ConfigureAwait(false);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public IReadOnlyList<T> GetList(Expression<Func<T, bool>> criteria)
        {
            return _table.Where(criteria).ToList();
        }

        public IReadOnlyList<T> GetList(ISpecification<T> criteria)
        {
            return _table.Where(criteria.Criteria).ToList();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria)
        {
            return await _table.Where(criteria).ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(ISpecification<T> criteria)
        {
            return await _table.Where(criteria.Criteria.Compile()).AsQueryable().ToListAsync().ConfigureAwait(false);
        }

        public T Lookup(T item)
        {
            return _table.Find(_db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(item, null)).ToArray());
        }

        public async Task<T> LookupAsync(T item)
        {
            return await _table.FindAsync(_db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(item, null)).ToArray()).ConfigureAwait(false);
        }

        public void Remove(T item)
        {
            _table.Remove(item);
            _db.SaveChanges();
        }

        public async Task RemoveAsync(T item)
        {
            _table.Remove(item);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public void RemoveRange(IReadOnlyList<T> items)
        {
            _table.RemoveRange(items);
            _db.SaveChanges();
        }

        public async Task RemoveRangeAsync(IReadOnlyList<T> items)
        {
            _table.RemoveRange(items);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
