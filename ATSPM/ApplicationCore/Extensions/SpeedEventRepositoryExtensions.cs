#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Extensions/SpeedEventRepositoryExtensions.cs
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
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Extensions
{
    public static class SpeedEventRepositoryExtensions
    {
        public static IReadOnlyList<SpeedEvent> GetSpeedEventsByDetector(this ISpeedEventLogRepository repo, string locationIdentifier, Detector detector, DateTime start, DateTime end, int minSpeedFilter = 5)
        {
            return repo.GetEventsBetweenDates(locationIdentifier, start, end)
                .Where(e => e.DetectorId == detector.DectectorIdentifier
                && e.Mph > minSpeedFilter
                ).ToList();

        }
        #region Obsolete

        [Obsolete("This method isn't currently being used")]
        public static IReadOnlyList<SpeedEvent> GetSpeedEventsByLocation(this ISpeedEventLogRepository repo, DateTime startDate, DateTime endDate, Approach approach)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
