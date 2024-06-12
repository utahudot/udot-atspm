#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Repositories/ATSPMRepositoryEFBase.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
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
    /// <summary>
    /// Base repository for working with entity framework contexts
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ATSPMRepositoryEFBase<T> : IAsyncRepository<T> where T : class
    {
        protected readonly ILogger _log;
        protected readonly DbContext _db;
        protected readonly DbSet<T> table;

        /// <inheritdoc/>
        public ATSPMRepositoryEFBase(DbContext db, ILogger<ATSPMRepositoryEFBase<T>> log)
        {
            _log = log;
            _db = db;
            table = _db.Set<T>();
        }

        #region IAsyncRepository

        /// <inheritdoc/>
        public void Add(T item)
        {
            table.Add(item);
            _db.SaveChanges();
        }

        /// <inheritdoc/>
        public async Task AddAsync(T item)
        {
            await table.AddAsync(item).ConfigureAwait(false);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void AddRange(IEnumerable<T> items)
        {
            table.AddRange(items);
            _db.SaveChanges();
        }

        /// <inheritdoc/>
        public async Task AddRangeAsync(IEnumerable<T> items)
        {
            await table.AddRangeAsync(items).ConfigureAwait(false);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public IReadOnlyList<T> GetList(Expression<Func<T, bool>> criteria)
        {
            return GetList().Where(criteria).ToList();
        }

        /// <inheritdoc/>
        public IReadOnlyList<T> GetList(ISpecification<T> criteria)
        {
            return GetList().Where(criteria.Criteria).ToList();
        }

        /// <inheritdoc/>
        public virtual IQueryable<T> GetList()
        {
            return table;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria)
        {
            return await GetList().Where(criteria).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<T>> GetListAsync(ISpecification<T> criteria)
        {
            return await GetList().Where(criteria.Criteria.Compile()).AsQueryable().ToListAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public T Lookup(T item)
        {
            var result = table.Find(_db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(item, null)).ToArray());

            foreach (var n in _db.Entry(result).Navigations)
            {
                //if (!n.IsLoaded)
                n.Load();
            }

            return result;
        }

        //TODO: replace with this for multiple key values (params object?[]? keyValues)
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Remove(T item)
        {
            table.Remove(item);
            _db.SaveChanges();
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(T item)
        {
            table.Remove(item);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void RemoveRange(IEnumerable<T> items)
        {
            table.RemoveRange(items);
            _db.SaveChanges();
        }

        /// <inheritdoc/>
        public async Task RemoveRangeAsync(IEnumerable<T> items)
        {
            table.RemoveRange(items);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
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
                        break;
                    }
            }

            _db.SaveChanges();
        }

        /// <inheritdoc/>
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
                        break;
                    }
            }

            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        //TODO: Check item for changes attach/unattach
        /// <inheritdoc/>
        public void UpdateRange(IEnumerable<T> items)
        {
            table.UpdateRange(items);
            _db.SaveChanges();
        }

        //TODO: Check item for changes attach/unattach
        /// <inheritdoc/>
        public async Task UpdateRangeAsync(IEnumerable<T> items)
        {
            table.UpdateRange(items);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        #endregion

        /// <summary>
        /// Determines what to do with related one-to-many and many-to-many entity collections
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="oldCollection"></param>
        /// <param name="newItem"></param>
        /// <param name="newCollection"></param>
        protected virtual void UpdateCollections(T oldItem, CollectionEntry oldCollection, T newItem, CollectionEntry newCollection)
        {
        }

        /// <summary>
        /// Determines what to do with related one-to-one entities
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="oldReference"></param>
        /// <param name="newItem"></param>
        /// <param name="newReference"></param>
        protected virtual void UpdateReferences(T oldItem, ReferenceEntry oldReference, T newItem, ReferenceEntry newReference)
        {
        }
    }
}