#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Repositories.ConfigurationRepositories/IMeasureCommentRepository.cs
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
    /// Metric Comment Repository
    /// </summary>
    public interface IMeasureCommentRepository : IAsyncRepository<MeasureComment>
    {
        #region ExtensionMethods

        //MetricComment GetLatestCommentForReport(string locationId, int metricId);

        #endregion

        #region Obsolete

        //IReadOnlyList<MetricType> GetMetricTypesByMetricComment(MetricComment metricComment);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<MetricComment> GetAllMetricComments();

        //[Obsolete("Use Lookup instead")]
        //MetricComment GetMetricCommentByMetricCommentID(int metricCommentID);

        //[Obsolete("Use Add in the BaseClass")]
        //void AddOrUpdate(MetricComment metricComment);

        //[Obsolete("Use Add in the BaseClass")]
        //void Add(MetricComment metricComment);

        //[Obsolete("Use Remove in the BaseClass")]
        //void Remove(MetricComment metricComment);

        #endregion
    }
}
