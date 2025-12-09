#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Repositories.EventLogRepositories/EventLogEFRepositoryBase.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Repositories.EventLogRepositories
{
    /// <summary>
    /// Generic base for accessing device event logs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EventLogEFRepositoryBase<T> : ATSPMRepositoryEFBase<CompressedEventLogs<T>>, IEventLogRepository<T> where T : EventLogModelBase
    {
        ///<inheritdoc/>
        public EventLogEFRepositoryBase(EventLogContext db, ILogger<EventLogEFRepositoryBase<T>> log) : base(db, log) { }

        #region IEventLogRepository

        ///<inheritdoc/>
        public virtual IReadOnlyList<T> GetEventsBetweenDates(string locationIdentifier, DateTime start, DateTime end)
        {
            var result = table
                .FromSpecification(new CompressedDataSpecification<CompressedEventLogBase>(locationIdentifier, start, end))
                .AsNoTracking()
                .AsEnumerable()
                .SelectMany(m => m.Data)
                .FromSpecification(new EventLogSpecification(locationIdentifier, start, end))
                .Cast<T>()
                .ToList();

            return result;
        }

        #endregion
    }
}
