#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.Services/IRepository.cs
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
using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ATSPM.Domain.Services
{
    /// <summary>
    /// For interfacing with repositories
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T>
    {
        /// <summary>
        /// Gets list of items from repository as an <see cref="IQueryable"/>
        /// </summary>
        /// <returns></returns>
        IQueryable<T> GetList();

        /// <summary>
        /// Gets list of items from repository
        /// </summary>
        /// <param name="criteria">Linq query criteria</param>
        /// <returns></returns>
        IReadOnlyList<T> GetList(Expression<Func<T, bool>> criteria);

        /// <summary>
        /// Gets list of items from repository
        /// </summary>
        /// <param name="criteria"><see cref="ISpecification{T}"/> criteria</param>
        /// <returns></returns>
        IReadOnlyList<T> GetList(ISpecification<T> criteria);

        /// <summary>
        /// Lookup item from repository by key
        /// </summary>
        /// <param name="key">Key of item to lookup</param>
        /// <returns></returns>
        T Lookup(object key);

        /// <summary>
        /// Lookup item from repository
        /// </summary>
        /// <param name="item">Item to lookup</param>
        /// <returns></returns>
        T Lookup(T item);

        /// <summary>
        /// Add an item to the repository
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns></returns>
        void Add(T item);

        /// <summary>
        /// Add a range of items to repository
        /// </summary>
        /// <param name="items">Item range to add</param>
        void AddRange(IEnumerable<T> items);

        /// <summary>
        /// Remove an item from the repository
        /// </summary>
        /// <param name="item">Item to remove</param>
        void Remove(T item);

        /// <summary>
        /// Remove range of items from the repository
        /// </summary>
        /// <param name="items">Item range to remove</param>
        void RemoveRange(IEnumerable<T> items);

        /// <summary>
        /// Update an item to the repository
        /// </summary>
        /// <param name="item">Item to update</param>
        void Update(T item);

        /// <summary>
        /// Update a range of items to the repository
        /// </summary>
        /// <param name="items">Item range to update</param>
        void UpdateRange(IEnumerable<T> items);
    }

    /// <summary>
    /// For interfacing with asynchronous repositories
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsyncRepository<T> : IRepository<T>
    {
        /// <summary>
        /// Gets list of items from repository
        /// </summary>
        /// <param name="criteria">Linq query criteria</param>
        /// <returns></returns>
        Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria);

        /// <summary>
        /// Gets list of items from repository
        /// </summary>
        /// <param name="criteria"><see cref="ISpecification{T}"/> criteria</param>
        /// <returns></returns>
        Task<IReadOnlyList<T>> GetListAsync(ISpecification<T> criteria);

        /// <summary>
        /// Lookup item from repository by key
        /// </summary>
        /// <param name="key">Key of item to lookup</param>
        /// <returns></returns>
        Task<T> LookupAsync(object key);

        /// <summary>
        /// Lookup item from repository
        /// </summary>
        /// <param name="item">Item to lookup</param>
        /// <returns></returns>
        Task<T> LookupAsync(T item);

        /// <summary>
        /// Add an item to the repository
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns></returns>
        Task AddAsync(T item);

        /// <summary>
        /// Add a range of items to repository
        /// </summary>
        /// <param name="items">Item range to add</param>
        Task AddRangeAsync(IEnumerable<T> items);

        /// <summary>
        /// Remove an item from the repository
        /// </summary>
        /// <param name="item">Item to remove</param>
        Task RemoveAsync(T item);

        /// <summary>
        /// Remove range of items from the repository
        /// </summary>
        /// <param name="items">Item range to remove</param>
        Task RemoveRangeAsync(IEnumerable<T> items);

        /// <summary>
        /// Update an item to the repository
        /// </summary>
        /// <param name="item">Item to update</param>
        Task UpdateAsync(T item);

        /// <summary>
        /// Update a range of items to the repository
        /// </summary>
        /// <param name="items">Item range to update</param>
        Task UpdateRangeAsync(IEnumerable<T> items);
    }
}
