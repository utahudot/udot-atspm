#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Extensions/DetectorCommentRepositoryExtensions.cs
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
    public static class DetectorCommentRepositoryExtensions
    {
        public static DetectorComment GetMostRecentDetectorCommentByDetectorID(this IDetectorCommentRepository repo, int id)
        {
            return repo.GetList().Where(r => r.DetectorId == id).OrderByDescending(r => r.TimeStamp).FirstOrDefault();
        }

        #region Obsolete

        [Obsolete("Use GetList in the BaseClass", true)]
        public static List<DetectorComment> GetAllDetectorComments(this IDetectorCommentRepository repo)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Lookup in the BaseClass", true)]
        public static DetectorComment GetDetectorCommentByDetectorCommentID(this IDetectorCommentRepository repo, int detectorCommentID)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Add in the BaseClass")]
        public static void AddOrUpdate(this IDetectorCommentRepository repo, DetectorComment detectorComment)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Add in the BaseClass", true)]
        public static void Add(this IDetectorCommentRepository repo, DetectorComment detectorComment)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use Remove in the BaseClass", true)]
        public static void Remove(this IDetectorCommentRepository repo, DetectorComment detectorComment)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
