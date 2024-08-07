﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - Utah.Udot.NetStandardToolkit.Extensions/QueryExtensions.cs
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

using Utah.Udot.NetStandardToolkit.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Utah.Udot.NetStandardToolkit.Extensions
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
        /// Creates a query based on an <c>string</c> ISpecification&lt;<typeparamref name="T"/>&gt;
        /// <see href="ControllerLogArchive"/>
        /// </summary>
        /// <typeparam name="T">POCO that the query pertains to</typeparam>
        /// <param name="list">Input list</param>
        /// <param name="specification"></param>
        /// <returns>Returns the query created from the ISpecification<typeparamref name="T"/></returns>
        public static IEnumerable<T> FromSpecification<T>(this IEnumerable<T> list, ISpecification<T> specification) where T : class
        {
            var newQuery = list.AsQueryable();

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

            return newQuery.ToList();
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
        public static IQueryable<T> IfCondition<T>(
            this IQueryable<T> query,
            Func<bool> condition,
            Expression<Func<IQueryable<T>, IQueryable<T>>> pass,
            Expression<Func<IQueryable<T>, IQueryable<T>>> fail = null)
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

        /// <summary>
        /// Use to create an if statement on an linline linq query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="condition">Condition that will pass or fail</param>
        /// <param name="pass">query to return if <paramref name="condition"/> passes</param>
        /// <param name="fail">query to return if <paramref name="condition"/> should fail</param>
        /// <returns></returns>
        public static IEnumerable<T> IfCondition<T>(
            this IEnumerable<T> query,
            Func<bool> condition,
            Expression<Func<IEnumerable<T>, IEnumerable<T>>> pass,
            Expression<Func<IEnumerable<T>, IEnumerable<T>>> fail = null)
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
