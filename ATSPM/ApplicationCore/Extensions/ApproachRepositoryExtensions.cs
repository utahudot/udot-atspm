#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Extensions/ApproachRepositoryExtensions.cs
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
    public static class ApproachRepositoryExtensions
    {
        public static IReadOnlyList<Approach> GetApproachesByIds(this IApproachRepository repo, IEnumerable<int> excludedApproachIds)
        {
            return repo.GetList(w => excludedApproachIds.Contains(w.Id));
        }

        #region Obsolete

        [Obsolete("Use Add instead")]
        public static void AddOrUpdate(this IApproachRepository repo, Approach approach)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This Method is not used")]
        public static Approach FindAppoachByVersionIdPhaseOverlapAndDirection(this IApproachRepository repo, int versionId, int phaseNumber, bool isOverlap, int directionTypeId)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use GetList instead")]
        public static IReadOnlyList<Approach> GetAllApproaches(this IApproachRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Lookup instead")]
        public static Approach GetApproachByApproachID(this IApproachRepository repo, int approachID)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Remove instead")]
        public static void Remove(int approachID)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
