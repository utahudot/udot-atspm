#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Extensions/AreaRepositoryExtensions.cs
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
using ATSPM.Application.Specifications;
using ATSPM.Application.ValueObjects;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class AreaRepositoryExtensions
    {
        public static Area GetAreaByName(this IAreaRepository repo, string name)
        {
            return repo.GetList().FirstOrDefault(f => f.Name == name);
        }

        #region Obsolete

        [Obsolete("Related collection to Location model")]
        public static IReadOnlyList<Area> GetListOfAreasForLocation(this IAreaRepository repo, string locationId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetList instead")]
        public static IReadOnlyList<Area> GetAllAreas(this IAreaRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Lookup instead")]
        public static Area GetAreaByID(this IAreaRepository repo, int areaId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Remove in the BaseClass")]
        public static void DeleteByID(this IAreaRepository repo, int areaId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Remove in the BaseClass")]
        public static void Remove(this IAreaRepository repo, Area area)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Update in the BaseClass")]
        public static void Update(this IAreaRepository repo, Area newArea)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Add in the BaseClass")]
        public static void Add(this IAreaRepository repo, Area newArea)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
