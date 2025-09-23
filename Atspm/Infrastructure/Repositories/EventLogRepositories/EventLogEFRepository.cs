#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Repositories.EventLogRepositories/EventLogEFRepository.cs
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.EventLogRepositories
{
    ///<inheritdoc cref="IEventLogRepository"/>
    public class EventLogEFRepository : ATSPMRepositoryEFBase<CompressedEventLogBase>, IEventLogRepository
    {
        ///<inheritdoc/>
        public EventLogEFRepository(EventLogContext db, ILogger<EventLogEFRepository> log) : base(db, log) { }

        #region IEventLogRepository



        /// <summary>
        /// Used to update/insert an entry to the <see cref="IEventLogRepository"/> repository
        /// using a <see cref="HashSet{T}"/> to ensure there are no duplicate data events.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repo"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// 
        public async Task<T> UpsertAsync<T>( T input) where T : CompressedEventLogBase
        {
            var searchLog = await LookupAsync(input);

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

                await UpdateAsync(searchLog);

                return (T)searchLog;
            }
            else
            {
                await AddAsync(input);

                return input;
            }
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateTime start, DateTime end, Type dataType, int deviceId)
        {
            return GetList()
                .FromSpecification(new CompressedEventLogSpecification(locationIdentifier, start, end, deviceId))
                .Where(w => w.DataType == dataType)
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogs<T>> GetArchivedEvents<T>(string locationIdentifier, DateTime start, DateTime end, int deviceId) where T : EventLogModelBase
        {
            var type = typeof(T);

            return [.. GetList()
                .FromSpecification(new CompressedEventLogSpecification(locationIdentifier, start, end, deviceId))
                .Where(w => w.DataType == type)
                .Cast<CompressedEventLogs<T>>()];
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateTime start, DateTime end)
        {
            return GetList()
                .FromSpecification(new CompressedEventLogSpecification(locationIdentifier, start, end))
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateTime start, DateTime end, Type dataType)
        {
            return GetList()
                .FromSpecification(new CompressedEventLogSpecification(locationIdentifier, start, end))
                .Where(w => w.DataType == dataType)
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateTime start, DateTime end, int deviceId)
        {
            return GetList()
                .FromSpecification(new CompressedEventLogSpecification(locationIdentifier, start, end, deviceId))
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogs<T>> GetArchivedEvents<T>(string locationIdentifier, DateTime start, DateTime end) where T : EventLogModelBase
        {
            var type = typeof(T);

            return GetList()
                .FromSpecification(new CompressedEventLogSpecification(locationIdentifier, start, end))
                .Where(w => w.DataType == type)
                .Cast<CompressedEventLogs<T>>()
                .ToList();
        }

        #endregion
    }
}