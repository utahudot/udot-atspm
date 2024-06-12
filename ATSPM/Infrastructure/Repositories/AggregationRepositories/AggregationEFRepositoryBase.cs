#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Repositories.AggregationRepositories/AggregationEFRepositoryBase.cs
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
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
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
                .FromSpecification(new AggregationDateRangeSpecification(locationId, DateOnly.FromDateTime(startTime), DateOnly.FromDateTime(endTime)))
                .AsNoTracking()
                .AsEnumerable()
                .SelectMany(m => m.Data)
                .Where(c => c.Start >= startTime && c.End <= endTime)
                .FromSpecification(new AggregationDateTimeRangeSpecification(locationId, startTime, endTime))
                .Cast<T>()
                .ToList();

            return result;
        }

        #endregion
    }
}
