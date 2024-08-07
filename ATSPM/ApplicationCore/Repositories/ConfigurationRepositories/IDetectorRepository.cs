#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Repositories.ConfigurationRepositories/IDetectorRepository.cs
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

namespace ATSPM.Application.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Dectector Repository
    /// </summary>
    public interface IDetectorRepository : IAsyncRepository<Detector>
    {
        #region ExtensionMethods

        #endregion

        #region Obsolete

        //[Obsolete("This method is not used", true)]
        //IReadOnlyList<Detector> GetDetectorsBylocationIdAndMetricType(string locationId, int metricId);

        //[Obsolete("User CheckReportAvialbility(Detector detector, int metricId) instead", true)]
        //bool CheckReportAvialbility(string detectorID, int metricId);

        //[Obsolete("User GetList() instead", true)]
        //Detector GetDetectorByDetectorID(string DetectorID);

        //[Obsolete("User GetList() instead", true)]
        //IReadOnlyList<Detector> GetDetectorsBylocationId(string locationId);

        //[Obsolete("Use Lookup instead", true)]
        //Detector GetDetectorByID(int ID);

        //[Obsolete("Use Add in the BaseClass", true)]
        //Detector Add(Detector Detector);

        //[Obsolete("Use Update in the BaseClass", true)]
        //void Update(Detector Detector);

        //[Obsolete("Use Remove in the BaseClass", true)]
        //void Remove(Detector Detector);

        //[Obsolete("Use Remove in the BaseClass", true)]
        //void Remove(int ID);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Detector> GetDetectorsByIds(List<int> excludedDetectorIds);

        #endregion
    }
}
