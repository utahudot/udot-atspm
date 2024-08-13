﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Repositories.EventLogRepositories/EventLogEFRepository.cs
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

using Microsoft.Extensions.Logging;
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

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateOnly start, DateOnly end, Type dataType, int deviceId)
        {
            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end, deviceId))
                .Where(w => w.DataType == dataType)
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogs<T>> GetArchivedEvents<T>(string locationIdentifier, DateOnly start, DateOnly end, int deviceId) where T : EventLogModelBase
        {
            var type = typeof(T);

            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end, deviceId))
                .Where(w => w.DataType == type)
                .Cast<CompressedEventLogs<T>>()
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateOnly start, DateOnly end)
        {
            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end))
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateOnly start, DateOnly end, Type dataType)
        {
            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end))
                .Where(w => w.DataType == dataType)
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogBase> GetArchivedEvents(string locationIdentifier, DateOnly start, DateOnly end, int deviceId)
        {
            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end, deviceId))
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedEventLogs<T>> GetArchivedEvents<T>(string locationIdentifier, DateOnly start, DateOnly end) where T : EventLogModelBase
        {
            var type = typeof(T);

            return GetList()
                .FromSpecification(new EventLogDateRangeSpecification(locationIdentifier, start, end))
                .Where(w => w.DataType == type)
                .Cast<CompressedEventLogs<T>>()
                .ToList();
        }

        #endregion
    }
}
