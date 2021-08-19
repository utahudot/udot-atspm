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
        protected readonly ILogger log;
        protected readonly DbContext db;
        protected readonly DbSet<T> table;

        public ATSPMRepositoryEFBase(DbContext db, ILogger<ATSPMRepositoryEFBase<T>> log)
        {
            log = log;
            db = db;
            table = db.Set<T>();
        }

        public void Add(T item)
        {
            table.Add(item);
            db.SaveChanges();
        }

        public async Task AddAsync(T item)
        {
            await table.AddAsync(item).ConfigureAwait(false);
            await db.SaveChangesAsync().ConfigureAwait(false);
        }

        public void AddRange(IReadOnlyList<T> items)
        {
            table.AddRange(items);
            db.SaveChanges();
        }

        public async Task AddRangeAsync(IReadOnlyList<T> items)
        {
            await table.AddRangeAsync(items).ConfigureAwait(false);
            await db.SaveChangesAsync().ConfigureAwait(false);
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
            return table.Find(db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(item, null)).ToArray());
        }

        public async Task<T> LookupAsync(T item)
        {
            return await table.FindAsync(db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(item, null)).ToArray()).ConfigureAwait(false);
        }

        public void Remove(T item)
        {
            table.Remove(item);
            db.SaveChanges();
        }

        public async Task RemoveAsync(T item)
        {
            table.Remove(item);
            await db.SaveChangesAsync().ConfigureAwait(false);
        }

        public void RemoveRange(IReadOnlyList<T> items)
        {
            table.RemoveRange(items);
            db.SaveChanges();
        }

        public async Task RemoveRangeAsync(IReadOnlyList<T> items)
        {
            table.RemoveRange(items);
            await db.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
