#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.PedestrianDelay/PedDelayCycle.cs
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

namespace Utah.Udot.Atspm.Analysis.PedestrianDelay
{
    /// <summary>
    /// Represents a single pedestrian cycle, including the timing of walk interval and detector activation.
    /// Used to calculate pedestrian delay and analyze pedestrian service intervals at a signalized intersection.
    /// Inherits start and end range properties from <see cref="StartEndRange"/>.
    /// </summary>
    public class PedDelayCycle : StartEndRange
    {
        /// <summary>
        /// Gets or sets the timestamp when the pedestrian walk interval begins.
        /// </summary>
        public DateTime BeginWalk { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the pedestrian detector is activated.
        /// </summary>
        public DateTime PedDetectorOn { get; set; }

        /// <summary>
        /// Gets the calculated pedestrian delay in seconds.
        /// Returns zero if the detector activation occurs after the walk interval begins; otherwise, returns the absolute difference in seconds.
        /// </summary>
        public double PedDelay => PedDetectorOn > BeginWalk ? 0 : Math.Abs((BeginWalk - PedDetectorOn).TotalSeconds);

        /// <summary>
        /// Returns a string representation of the pedestrian cycle, including walk start, detector activation, and delay.
        /// </summary>
        /// <returns>A formatted string describing the pedestrian cycle.</returns>
        public override string ToString()
        {
            return $"{nameof(PedDelayCycle)}: Start: {Start} BeginWalk: {BeginWalk}, PedDetectorOn: {PedDetectorOn}, PedDelay: {PedDelay} seconds End: {End}";
        }
    }
}
