#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Repositories.ConfigurationRepositories/IMeasureTypeRepository.cs
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
using ATSPM.Data.Models;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Measure type repository
    /// </summary>
    public interface IMeasureTypeRepository : IAsyncRepository<MeasureType>
    {
        #region Obsolete

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
