using ATSPM.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ATSPM.Domain.Extensions
{
    /// <summary>
    /// Extensions specific to IQueryable
    /// </summary>
    public static class QueryExtensions
    {
        /// <summary>
        /// Creates a query based on an <c>string</c> ISpecification&lt;<typeparamref name="T"/>&gt;
        /// <see href="ControllerLogArchive"/>
        /// </summary>
        /// <typeparam name="T">POCO that the query pertains to</typeparam>
        /// <param name="query">Input query</param>
        /// <param name="specification"></param>
        /// <returns>Returns the query created from the ISpecification<typeparamref name="T"/></returns>
        public static IQueryable<T> FromSpecification<T>(this IQueryable<T> query, ISpecification<T> specification) where T : class
        {
            var newQuery = query;

            if (specification.Criteria != null) { newQuery = newQuery.Where(specification.Criteria); }

            //newQuery = specification.Includes.Aggregate(newQuery, (current, include) => current.Include(include));

            if (specification.OrderBy != null)
            {
                newQuery = newQuery.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                newQuery = newQuery.OrderByDescending(specification.OrderByDescending);
            }

            if (specification.GroupBy != null) { newQuery = newQuery.GroupBy(specification.GroupBy).SelectMany(x => x); }

            if (specification.IsPagingEnabled) { newQuery = newQuery.Skip(specification.Skip).Take(specification.Take); }

            return newQuery;
        }

        /// <summary>
        /// Use to create an if statement on an linline linq query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition">Condition that will pass or fail</param>
        /// <param name="pass">query to return if <paramref name="condition"/> passes</param>
        /// <param name="fail">query to return if <paramref name="condition"/> should fail</param>
        /// <returns></returns>
        public static IQueryable<T> IfCondition<T>(this IQueryable<T> query, Func<bool> condition, Expression<Func<IQueryable<T>, IQueryable<T>>> pass, Expression<Func<IQueryable<T>, IQueryable<T>>> fail = null)
        {
            if (condition.Invoke())
            {
                return pass.Compile().Invoke(query);
            }
            else if (fail != null)
            {
                return fail.Compile().Invoke(query);
            }

            return query;
        }
    }
}
