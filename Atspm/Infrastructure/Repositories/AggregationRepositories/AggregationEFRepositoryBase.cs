#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Repositories.AggregationRepositories/AggregationEFRepositoryBase.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Repositories.AggregationRepositories
{
    /// <summary>
    /// Generic base for accessing aggregations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AggregationEFRepositoryBase<T> : ATSPMRepositoryEFBase<CompressedAggregations<T>>, IAggregationRepository<T> where T : AggregationModelBase
    {
        ///<inheritdoc/>
        public AggregationEFRepositoryBase(AggregationContext db, ILogger<AggregationEFRepositoryBase<T>> log) : base(db, log) { }

        #region IAggregationRepository

        ///<inheritdoc/>
        public virtual IReadOnlyList<T> GetAggregationsBetweenDates(string locationId, DateTime startTime, DateTime endTime)
        {
            var result = table
                .FromSpecification(new CompressedDataSpecification<CompressedAggregationBase>(locationId, startTime, endTime))
                .AsNoTracking()
                .AsEnumerable()
                .SelectMany(m => m.Data)
                .Where(c => c.Start >= startTime && c.End <= endTime)
                .FromSpecification(new AggregationSpecification(locationId, startTime, endTime))
            .Cast<T>()
            .ToList();
            return result;
        }

        #endregion
    }
}