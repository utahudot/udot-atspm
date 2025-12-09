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
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Repositories.AggregationRepositories;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.EventLogRepositories
{
    ///<inheritdoc cref="IEventLogRepository"/>
    public class EventLogEFRepository(EventLogContext db, ILogger<EventLogEFRepository> log) : CompressedDataEFRepositoryBase<CompressedEventLogBase>(db, log), IEventLogRepository
    {
        #region IEventLogRepository

        ///<inheritdoc/>
        public IAsyncEnumerable<CompressedEventLogs<T>> GetData<T>(string locationIdentifier, DateTime start, DateTime end) where T : EventLogModelBase
        {
            var type = typeof(T);

            return GetList()
                .FromSpecification(new CompressedDataSpecification<CompressedEventLogBase>(locationIdentifier, start, end, type))
                .Cast<CompressedEventLogs<T>>()
                .AsAsyncEnumerable();
        }

        ///<inheritdoc/>
        public IAsyncEnumerable<CompressedEventLogBase> GetData(string locationIdentifier, DateTime start, DateTime end, int deviceId)
        {
            return GetList()
                .FromSpecification(new CompressedDataSpecification<CompressedEventLogBase>(locationIdentifier, start, end))
                .Where(w => w.DeviceId == deviceId)
                .AsAsyncEnumerable();
        }

        ///<inheritdoc/>
        public IAsyncEnumerable<CompressedEventLogBase> GetData(string locationIdentifier, DateTime start, DateTime end, Type dataType, int deviceId)
        {
            return GetList()
                .FromSpecification(new CompressedDataSpecification<CompressedEventLogBase>(locationIdentifier, start, end, dataType))
                .Where(w => w.DeviceId == deviceId)
                .AsAsyncEnumerable();
        }

        ///<inheritdoc/>
        public IAsyncEnumerable<CompressedEventLogs<T>> GetData<T>(string locationIdentifier, DateTime start, DateTime end, int deviceId) where T : EventLogModelBase
        {
            var type = typeof(T);

            return GetList()
                .FromSpecification(new CompressedDataSpecification<CompressedEventLogBase>(locationIdentifier, start, end, type))
                .Where(w => w.DeviceId == deviceId)
                .Cast<CompressedEventLogs<T>>()
                .AsAsyncEnumerable();
        }

        #endregion
    }
}