using ATSPM.Domain.Services;
using ATSPM.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Repositories
{
    public abstract class ATSPMRepositoryEFBase<T> : IAsyncRepository<T> where T : class
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
            return GetList().Where(criteria).ToList();
        }

        public IReadOnlyList<T> GetList(ISpecification<T> criteria)
        {
            return GetList().Where(criteria.Criteria).ToList();
        }

        public virtual IQueryable<T> GetList()
        {
            return table;
        }

        public async Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria)
        {
            return await GetList().Where(criteria).ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(ISpecification<T> criteria)
        {
            return await GetList().Where(criteria.Criteria.Compile()).AsQueryable().ToListAsync().ConfigureAwait(false);
        }

        public T Lookup(T item)
        {
            var result =  table.Find(_db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(item, null)).ToArray());

            foreach (var n in _db.Entry(result).Navigations)
            {
                //if (!n.IsLoaded)
                n.Load();
            }

            return result;
        }

        //TODO: replace with this for multiple key values (params object?[]? keyValues)
        public T Lookup(object key)
        {
            var result = table.Find(key);

            foreach (var n in _db.Entry(result).Navigations)
            {
                //if (!n.IsLoaded)
                n.Load();
            }

            return result;
        }

        public async Task<T> LookupAsync(T item)
        {
            var result = await table.FindAsync(_db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(item, null)).ToArray()).ConfigureAwait(false);

            foreach (var n in _db.Entry(result).Navigations)
            {
                //if (!n.IsLoaded)
                await n.LoadAsync().ConfigureAwait(false);
            }

            return result;
        }

        //TODO: replace with this for multiple key values (params object?[]? keyValues)
        public async Task<T> LookupAsync(object key)
        {
            var result = await table.FindAsync(key);

            foreach (var n in _db.Entry(result).Navigations)
            {
                //if (!n.IsLoaded)
                await n.LoadAsync().ConfigureAwait(false);
            }

            return result;
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

        public void Update(T item)
        {
            switch (_db.Entry(item).State)
            {
                case EntityState.Detached:
                    {
                        var old = Lookup(item);

                        if (old != null)
                        {
                            _db.Entry(old).CurrentValues.SetValues(item);

                            foreach (var i in _db.Entry(old).Collections)
                            {
                                if (!i.IsLoaded)
                                    i.Load();

                                UpdateCollections(old, i, item, _db.Entry(item).Collections.First(w => w.Metadata.Name == i.Metadata.Name));
                            }

                            foreach (var i in _db.Entry(old).References)
                            {
                                if (!i.IsLoaded)
                                    i.Load();

                                UpdateReferences(old, i, item, _db.Entry(item).References.First(w => w.Metadata.Name == i.Metadata.Name));
                            }
                        }
                        else
                        {
                            table.Update(item);
                        }

                        break;
                    }
                case EntityState.Modified:
                    {
                        break;
                    }
                case EntityState.Unchanged:
                    {
                        //foreach (var n in _db.Entry(item).References)
                        //{
                        //    n.IsModified = true;
                        //}

                        break;
                    }
                default:
                    {
                        return;
                    }
            }

            _db.SaveChanges();
        }

        public async Task UpdateAsync(T item)
        {
            switch (_db.Entry(item).State)
            {
                case EntityState.Detached:
                    {
                        var old = await LookupAsync(item);

                        if (old != null)
                        {
                            _db.Entry(old).CurrentValues.SetValues(item);

                            foreach (var i in _db.Entry(old).Collections)
                            {
                                if (!i.IsLoaded)
                                    await i.LoadAsync();

                                UpdateCollections(old, i, item, _db.Entry(item).Collections.First(w => w.Metadata.Name == i.Metadata.Name));
                            }

                            foreach (var i in _db.Entry(old).References)
                            {
                                if (!i.IsLoaded)
                                    await i.LoadAsync();

                                UpdateReferences(old, i, item, _db.Entry(item).References.First(w => w.Metadata.Name == i.Metadata.Name));
                            }
                        }
                        else
                        {
                            table.Update(item);
                        }

                        break;
                    }
                case EntityState.Modified:
                    {
                        break;
                    }
                case EntityState.Unchanged:
                    {
                        //foreach (var n in _db.Entry(item).References)
                        //{
                        //    n.IsModified = true;
                        //}

                        break;
                    }
                default:
                    {
                        return;
                    }
            }

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        protected virtual void UpdateCollections(T oldItem, CollectionEntry oldCollection, T newItem, CollectionEntry newCollection)
        {
            //oldCollection.CurrentValue = newCollection.CurrentValue;
        }

        protected virtual void UpdateReferences(T oldItem, ReferenceEntry oldReference, T newItem, ReferenceEntry newReference)
        {
            //oldReference.CurrentValue = newReference.CurrentValue;
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