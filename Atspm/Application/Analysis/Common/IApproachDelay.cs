#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Common/IApproachDelay.cs
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

namespace Utah.Udot.Atspm.Analysis.Common
{
    /// <summary>
    /// Defines the approach delay results
    /// </summary>
    public interface IApproachDelay : IStartEndRange
    {
        /// <summary>
        /// The average delay of <see cref="IndianaEnumerations.VehicleDetectorOn"/> arrival on red events
        /// </summary>
        double AverageDelay { get; }

        /// <summary>
        /// The total delay of <see cref="IndianaEnumerations.VehicleDetectorOn"/> arrival on red events
        /// </summary>
        double TotalDelay { get; }

        /// <summary>
        /// <see cref="IndianaEnumerations.VehicleDetectorOn"/> arrival on red events
        /// </summary>
        IReadOnlyList<Vehicle> Vehicles { get; }
    }
}
