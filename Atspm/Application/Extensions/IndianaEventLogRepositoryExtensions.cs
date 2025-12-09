#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/IndianaEventLogRepositoryExtensions.cs
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
    /// Extensions for <see cref="IIndianaEventLogRepository"/>
    /// </summary>
    public static class IndianaEventLogRepositoryExtensions
    {
        /// <summary>
        /// Gets <see cref="IIndianaEventLogRepository"/> events by <paramref name="eventCodes"/> and <paramref name="param"/>
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="locationId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="eventCodes"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodesParam(this IIndianaEventLogRepository repo, string locationId, DateTime startTime, DateTime endTime, IEnumerable<short> eventCodes, int param)
        {
            var result = repo.GetEventsBetweenDates(locationId, startTime, endTime)
                .Where(w => eventCodes.Contains(w.EventCode))
                .Where(w => w.EventParam == (short)param)
                .ToList();

            return result;
        }

        /// <summary>
        /// Gets <see cref="IIndianaEventLogRepository"/> events by <paramref name="eventCodes"/>
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="locationIdentifier"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="eventCodes"></param>
        /// <returns></returns>
        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodes(this IIndianaEventLogRepository repo, string locationIdentifier, DateTime startTime, DateTime endTime, IEnumerable<short> eventCodes)
        {
            var result = repo.GetEventsBetweenDates(locationIdentifier, startTime, endTime)
                .Where(w => eventCodes.Contains(w.EventCode))
                .OrderBy(o => o.EventParam)
                .ToList();

            return result;
        }

    }
}
