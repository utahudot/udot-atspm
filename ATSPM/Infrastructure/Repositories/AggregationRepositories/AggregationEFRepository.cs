#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Repositories.AggregationRepositories/AggregationEFRepository.cs
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
using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Specifications;
using ATSPM.Data;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Domain.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.AggregationRepositories
{
    ///<inheritdoc cref="IAggregationRepository"/>
    public class AggregationEFRepository : ATSPMRepositoryEFBase<CompressedAggregationBase>, IAggregationRepository
    {
        ///<inheritdoc/>
        public AggregationEFRepository(AggregationContext db, ILogger<AggregationEFRepository> log) : base(db, log) { }

        #region IAggregationRepository

        ///<inheritdoc/>
        public IReadOnlyList<CompressedAggregationBase> GetArchivedAggregations(string locationIdentifier, DateOnly start, DateOnly end, Type dataType)
        {
            return GetList()
                .FromSpecification(new AggregationDateRangeSpecification(locationIdentifier, start, end))
                .Where(w => w.DataType == dataType)
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedAggregations<T>> GetArchivedAggregations<T>(string locationIdentifier, DateOnly start, DateOnly end) where T : AggregationModelBase
        {
            var type = typeof(T);

            return GetList()
                .FromSpecification(new AggregationDateRangeSpecification(locationIdentifier, start, end))
                .Where(w => w.DataType == type)
                .Cast<CompressedAggregations<T>>()
                .ToList();
        }

        ///<inheritdoc/>
        public IReadOnlyList<CompressedAggregationBase> GetArchivedAggregations(string locationIdentifier, DateOnly start, DateOnly end)
        {
            return GetList()
                .FromSpecification(new AggregationDateRangeSpecification(locationIdentifier, start, end))
                .ToList();
        }

        #endregion
    }
}
