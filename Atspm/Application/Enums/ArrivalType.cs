#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Enums/ArrivalType.cs
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

namespace Utah.Udot.Atspm.Enums
{
    /// <summary>
    /// Arrival type of <see cref="IndianaEnumerations.VehicleDetectorOn"/> events
    /// </summary>
    public enum ArrivalType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// <see cref="IndianaEnumerations.VehicleDetectorOn"/> arrival on green
        /// </summary>
        ArrivalOnGreen,

        /// <summary>
        /// <see cref="IndianaEnumerations.VehicleDetectorOn"/> arrival on yellow
        /// </summary>
        ArrivalOnYellow,

        /// <summary>
        /// <see cref="IndianaEnumerations.VehicleDetectorOn"/> arrival on red
        /// </summary>
        ArrivalOnRed
    }
}
