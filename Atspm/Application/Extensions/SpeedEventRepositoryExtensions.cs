#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/SpeedEventRepositoryExtensions.cs
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

using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ISpeedEventLogRepository"/>
    /// </summary>
    public static class SpeedEventRepositoryExtensions
    {
        /// <summary> 
        /// Gets <see cref="SpeedEvent"/> events by <paramref name="locationIdentifier"/> and <paramref name="detector"/>
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="locationIdentifier"></param>
        /// <param name="detector"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="minSpeedFilter">Minimum speed to include</param>
        /// <returns></returns>
        public static IReadOnlyList<SpeedEvent> GetSpeedEventsByDetector(this ISpeedEventLogRepository repo, string locationIdentifier, Detector detector, DateTime start, DateTime end, int minSpeedFilter = 5)
        {
            return repo.GetEventsBetweenDates(locationIdentifier, start, end)
                .Where(e => e.DetectorId == detector.DectectorIdentifier
                && e.Mph > minSpeedFilter
                ).ToList();

        }

        #region Obsolete

        //[Obsolete("This method isn't currently being used")]
        //public static IReadOnlyList<SpeedEvent> GetSpeedEventsByLocation(this ISpeedEventLogRepository repo, DateTime startDate, DateTime endDate, Approach approach)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}
