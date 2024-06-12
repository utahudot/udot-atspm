#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Repositories.ConfigurationRepositories/IDetectionTypeRepository.cs
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
using System.Collections.Generic;

namespace ATSPM.Application.Repositories.ConfigurationRepositories
{
    /// <summary>
    /// Detection type repository
    /// </summary>
    public interface IDetectionTypeRepository : IAsyncRepository<DetectionType>
    {
        /// <summary>
        /// don't know what this does or if it's needed
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<DetectionType> GetAllDetectionTypesNoBasic();

        #region Obsolete

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<DetectionType> GetAllDetectionTypes();

        //[Obsolete("Use Lookup in the BaseClass")]
        //DetectionType GetDetectionTypeByDetectionTypeID(int detectionTypeID);

        //[Obsolete("Exists in base class")]
        //void Update(DetectionType detectionType);

        //[Obsolete("Exists in base class")]
        //void Add(DetectionType detectionType);

        //[Obsolete("Exists in base class")]
        //void Remove(DetectionType detectionType);

        #endregion
    }
}
