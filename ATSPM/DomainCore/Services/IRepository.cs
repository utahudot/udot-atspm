using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ATSPM.Domain.Services
{
    public interface IRepository<T>
    {
        IReadOnlyList<T> GetList(Expression<Func<T, bool>> criteria);

        IReadOnlyList<T> GetList(ISpecification<T> criteria);

        T Lookup(T item);

        void Add(T item);

        void AddRange(IEnumerable<T> items);

        void Remove(T item);

        void RemoveRange(IEnumerable<T> items);

        void Update(T item);

        void UpdateRange(IEnumerable<T> items);
    }

    public interface IAsyncRepository<T> : IRepository<T>
    {
        Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria);

        Task<IReadOnlyList<T>> GetListAsync(ISpecification<T> criteria);

        Task<T> LookupAsync(T item);

        Task AddAsync(T item);

        Task AddRangeAsync(IEnumerable<T> items);

        Task RemoveAsync(T item);

        Task RemoveRangeAsync(IEnumerable<T> items);

        Task UpdateAsync(T item);

        Task UpdateRangeAsync(IEnumerable<T> items);
    }
}
