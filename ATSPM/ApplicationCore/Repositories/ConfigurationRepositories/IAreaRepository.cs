#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Repositories.ConfigurationRepositories/IAreaRepository.cs
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
    /// Area Repository
    /// </summary>
    public interface IAreaRepository : IAsyncRepository<Area>
    {
        #region ExtensionMethods

        //Area GetAreaByName(string name);

        #endregion

        #region Obsolete

        //[Obsolete("Related collection to Location model")]
        //IReadOnlyList<Area> GetListOfAreasForLocation(string locationId);

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<Area> GetAllAreas();

        //[Obsolete("Use Lookup instead")]
        //Area GetAreaByID(int areaId);

        //[Obsolete("Use Remove in the BaseClass")]
        //void DeleteByID(int areaId);

        //[Obsolete("Use Remove in the BaseClass")]
        //void Remove(Area Area);

        //[Obsolete("Use Update in the BaseClass")]
        //void Update(Area newArea);

        //[Obsolete("Use Add in the BaseClass")]
        //void Add(Area newArea);

        #endregion
    }
}
