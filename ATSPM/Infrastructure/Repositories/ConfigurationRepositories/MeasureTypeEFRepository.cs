#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Repositories.ConfigurationRepositories/MeasureTypeEFRepository.cs
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
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IMeasureTypeRepository"/>
    public class MeasureTypeEFRepository : ATSPMRepositoryEFBase<MeasureType>, IMeasureTypeRepository
    {
        /// <inheritdoc/>
        public MeasureTypeEFRepository(ConfigContext db, ILogger<MeasureTypeEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<MeasureType> GetList()
        {
            return base.GetList().OrderBy(o => o.DisplayOrder);
        }

        #endregion

        #region IMeasureTypeRepository

        //[Obsolete("Not required in v5.0")]
        //IReadOnlyList<MeasureType> GetAllToDisplayMetrics();

        //[Obsolete("Not required in v5.0")]
        //IReadOnlyList<MeasureType> GetAllToAggregateMetrics();

        //[Obsolete("Not required in v5.0")]
        //IReadOnlyList<MeasureType> GetBasicMetrics();

        //[Obsolete("Not required in v5.0")]
        //IReadOnlyList<MeasureType> GetMetricsByIDs(List<int> metricIDs);

        //[Obsolete("Not required in v5.0")]
        //IReadOnlyList<MeasureType> GetMetricTypesByMetricComment(MeasureComment metricComment);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<MetricType> GetAllMetrics();

        //[Obsolete("Use Lookup instead")]
        //MetricType GetMetricsByID(int metricID);

        #endregion
    }
}
