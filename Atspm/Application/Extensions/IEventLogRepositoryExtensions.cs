﻿#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/IEventLogRepositoryExtensions.cs
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

using System.Collections;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IEventLogRepository"/>
    /// </summary>
    public static class IEventLogRepositoryExtensions
    {
        /// <summary>
        /// Used to update/insert an entry to the <see cref="IEventLogRepository"/> repository
        /// using a <see cref="HashSet{T}"/> to ensure there are no duplicate data events.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repo"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<T> Upsert<T>(this IEventLogRepository repo, T input) where T : CompressedEventLogBase
        {
            var searchLog = await repo.LookupAsync(input);

            if (searchLog != null)
            {
                dynamic list = Activator.CreateInstance(typeof(List<>).MakeGenericType(input.DataType));

                foreach (var i in Enumerable.Union(searchLog.Data, input.Data).ToHashSet())
                {
                    if (list is IList l)
                    {
                        l.Add(i);
                    }
                }

                searchLog.Data = list;

                await repo.UpdateAsync(searchLog);

                return (T)searchLog;
            }
            else
            {
                await repo.AddAsync(input);

                return input;
            }
        }

        /// <summary>
        /// Returns a list of unique days that have event logs for the given location.
        /// </summary>
        /// <param name="locationIdentifier">The location identifier</param>
        /// <param name="repo"></param>
        /// <param name="dataType">Type of event log data to filter by.</param>
        /// <param name="start">Start date</param>
        /// <param name="end">End date</param>
        /// <returns>A read-only list of days with event log data</returns>
        public static IReadOnlyList<DateOnly> GetDaysWithEventLogs(this IEventLogRepository repo, string locationIdentifier, Type dataType, DateTime start, DateTime end)
        {

            return repo.GetList()
                .FromSpecification(new CompressedEventLogSpecification(locationIdentifier, start, end))
                .Where(x => x.DataType == dataType)
                .Select(x => DateOnly.FromDateTime(x.Start))
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }
    }
}
