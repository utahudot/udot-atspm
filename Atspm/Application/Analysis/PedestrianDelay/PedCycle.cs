#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.PedestrianDelay/PedCycle.cs
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

namespace Utah.Udot.Atspm.Analysis.PedestrianDelay
{
    /// <summary>
    /// Represents a pedestrian cycle for analysis, aggregating related <see cref="IndianaEvent"/> objects
    /// such as walk interval start, detector requests, imputed calls, unique detections, and call registrations.
    /// Inherits start and end range properties from <see cref="StartEndRange"/>.
    /// Used to summarize and report pedestrian activity and delay metrics for a signalized intersection.
    /// </summary>
    public class PedCycle : StartEndRange
    {
        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> representing the start of the pedestrian walk interval.
        /// </summary>
        public DateTime PedestrianBeginWalk { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="IndianaEvent"/> representing pedestrian detector requests within the cycle.
        /// </summary>
        public IReadOnlyList<IndianaEvent> PedRequests { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="IndianaEvent"/> representing imputed pedestrian calls within the cycle.
        /// </summary>
        public int ImputedCalls { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="IndianaEvent"/> representing unique pedestrian detector activations within the cycle.
        /// </summary>
        public int UniquePedDetections { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="IndianaEvent"/> representing pedestrian call registrations within the cycle.
        /// </summary>
        public IReadOnlyList<IndianaEvent> PedCallsRegistered { get; set; }

        /// <summary>
        /// Returns a string representation of the pedestrian cycle, including start, walk interval, and event counts.
        /// </summary>
        /// <returns>
        /// A formatted string describing the pedestrian cycle, including the start time, walk interval start,
        /// counts for requests, imputed calls, unique detections, call registrations, and the end time.
        /// </returns>
        public override string ToString()
        {
            return $"{nameof(PedCycle)}: Start: {Start} BeginWalk: {PedestrianBeginWalk}, PedRequests: {PedRequests.Count}, ImputedCalls: {ImputedCalls} UniquePedDetections: {UniquePedDetections} PedCallsRegistered: {PedCallsRegistered.Count} End: {End}";
        }
    }
}
