#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Repositories.ConfigurationRepositories/IApproachRepository.cs
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
    /// Approach Repository
    /// </summary>
    public interface IApproachRepository : IAsyncRepository<Approach>
    {
        #region ExtensionMethods

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Approach> GetApproachesByIds(List<int> excludedApproachIds);

        #endregion

        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Approach> GetAllApproaches();

        //[Obsolete("Use Lookup instead")]
        //Approach GetApproachByApproachID(int approachID);

        //[Obsolete("Use Add instead")]
        //void AddOrUpdate(Approach approach);

        //[Obsolete("Use Add in the BaseClass")]
        //Approach FindAppoachByVersionIdPhaseOverlapAndDirection(int versionId, int phaseNumber, bool isOverlap, int directionTypeId);

        //[Obsolete("Use Remove in the BaseClass")]
        //void Remove(Approach approach);

        //[Obsolete("Use Remove in the BaseClass")]
        //void Remove(int approachID);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Approach> GetApproachesByIds(List<int> excludedApproachIds);

        #endregion
    }
}
