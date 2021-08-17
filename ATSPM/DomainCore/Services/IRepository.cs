using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ATSPM.Domain.Services
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetList(Expression<Func<T, bool>> criteria);

        IEnumerable<T> GetList(ISpecification<T> criteria);

        T Lookup(T item);

        void Add(T item);

        void AddRange(IEnumerable<T> items);

        void Remove(T item);

        void RemoveRange(IEnumerable<T> items);
    }

    public interface IAsyncRepository<T> : IRepository<T>
    {
        Task<IEnumerable<T>> GetListAsync(Expression<Func<T, bool>> criteria);

        Task<IEnumerable<T>> GetListAsync(ISpecification<T> criteria);

        Task<T> LookupAsync(T item);

        Task AddAsync(T item);

        Task AddRangeAsync(IEnumerable<T> items);

        Task RemoveAsync(T item);

        Task RemoveRangeAsync(IEnumerable<T> items);
    }
}
