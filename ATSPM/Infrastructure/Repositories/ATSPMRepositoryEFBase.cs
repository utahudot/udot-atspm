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
        protected readonly DbSet<T> table;

        public ATSPMRepositoryEFBase(DbContext db, ILogger<ATSPMRepositoryEFBase<T>> log)
        {
            _log = log;
            _db = db;
            table = _db.Set<T>();
        }

        public void Add(T item)
        {
            table.Add(item);
            _db.SaveChanges();
        }

        public async Task AddAsync(T item)
        {
            await table.AddAsync(item).ConfigureAwait(false);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public void AddRange(IEnumerable<T> items)
        {
            table.AddRange(items);
            _db.SaveChanges();
        }

        public async Task AddRangeAsync(IEnumerable<T> items)
        {
            await table.AddRangeAsync(items).ConfigureAwait(false);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public IReadOnlyList<T> GetList(Expression<Func<T, bool>> criteria)
        {
            return table.Where(criteria).ToList();
        }

        public IReadOnlyList<T> GetList(ISpecification<T> criteria)
        {
            return table.Where(criteria.Criteria).ToList();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria)
        {
            return await table.Where(criteria).ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(ISpecification<T> criteria)
        {
            return await table.Where(criteria.Criteria.Compile()).AsQueryable().ToListAsync().ConfigureAwait(false);
        }

        public T Lookup(T item)
        {
            return table.Find(_db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(item, null)).ToArray());
        }

        public async Task<T> LookupAsync(T item)
        {
            return await table.FindAsync(_db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(item, null)).ToArray()).ConfigureAwait(false);
        }

        public void Remove(T item)
        {
            table.Remove(item);
            _db.SaveChanges();
        }

        public async Task RemoveAsync(T item)
        {
            table.Remove(item);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            table.RemoveRange(items);
            _db.SaveChanges();
        }

        public async Task RemoveRangeAsync(IEnumerable<T> items)
        {
            table.RemoveRange(items);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        //TODO: Check item for changes attach/unattach
        public void Update(T item)
        {
            table.Update(item);
            _db.SaveChanges();
        }

        //TODO: Check item for changes attach/unattach
        public async Task UpdateAsync(T item)
        {
            table.Update(item);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        //TODO: Check item for changes attach/unattach
        public void UpdateRange(IEnumerable<T> items)
        {
            table.UpdateRange(items);
            _db.SaveChanges();
        }

        //TODO: Check item for changes attach/unattach
        public async Task UpdateRangeAsync(IEnumerable<T> items)
        {
            table.UpdateRange(items);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
