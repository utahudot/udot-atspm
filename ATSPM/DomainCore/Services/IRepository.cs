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

        void AddRange(IReadOnlyList<T> items);

        void Remove(T item);

        void RemoveRange(IReadOnlyList<T> items);
    }

    public interface IAsyncRepository<T> : IRepository<T>
    {
        Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria);

        Task<IReadOnlyList<T>> GetListAsync(ISpecification<T> criteria);

        Task<T> LookupAsync(T item);

        Task AddAsync(T item);

        Task AddRangeAsync(IReadOnlyList<T> items);

        Task RemoveAsync(T item);

        Task RemoveRangeAsync(IReadOnlyList<T> items);
    }
}
