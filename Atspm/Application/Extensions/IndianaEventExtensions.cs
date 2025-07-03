#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/IndianaEventExtensions.cs
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

using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Provides extension methods for working with collections of <see cref="IndianaEvent"/> objects,
    /// including filtering sequential events and grouping events by parameter type.
    /// </summary>
    public static class IndianaEventExtensions
    {
        /// <summary>
        /// Returns a list of <see cref="IndianaEvent"/> where, for the specified <paramref name="eventCode"/>,
        /// only the first occurrence in any sequence of consecutive events with that code is kept.
        /// Other event codes are included as-is.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to filter.</param>
        /// <param name="eventCode">The event code to filter for first-in-sequence occurrences.</param>
        /// <returns>
        /// A list of <see cref="IndianaEvent"/> with only the first event of each consecutive sequence
        /// of the specified event code included.
        /// </returns>
        public static IList<IndianaEvent> KeepFirstSequentialEvent(this IEnumerable<IndianaEvent> events, IndianaEnumerations eventCode)
        {
            var sort = events.OrderBy(o => o.Timestamp).ToList();

            return sort.Where((w, i) => i == 0 || w.EventCode != (int)eventCode || (w.EventCode == (int)eventCode && w.EventCode != sort[i - 1].EventCode)).ToList();
        }

        /// <summary>
        /// Groups a collection of <see cref="IndianaEvent"/> by their parameter type, as determined by the
        /// <see cref="IndianaEventLayerAttribute"/> on the corresponding <see cref="IndianaEnumerations"/> value.
        /// </summary>
        /// <param name="events">The collection of <see cref="IndianaEvent"/> to group.</param>
        /// <returns>
        /// An <see cref="ILookup{TKey, TElement}"/> where the key is the parameter type as a string,
        /// and the value is a collection of <see cref="IndianaEvent"/> with that parameter type.
        /// </returns>
        public static ILookup<string, IndianaEvent> GroupEventsByParamType(this IEnumerable<IndianaEvent> events)
        {
            var codeToParamType = Enum.GetValues(typeof(IndianaEnumerations))
                .Cast<IndianaEnumerations>()
                .ToDictionary(
                    e => (short)e,
                    e => e.GetAttributeOfType<IndianaEventLayerAttribute>()?.IndianaEventParamType.ToString() ?? IndianaEventParamType.None.ToString()
                );

            return events
                .Where(e => codeToParamType.ContainsKey(e.EventCode))
                .ToLookup(e => codeToParamType[e.EventCode]);
        }
    }
}
