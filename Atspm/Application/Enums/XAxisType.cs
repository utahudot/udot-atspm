#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Enums/XAxisType.cs
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

namespace Utah.Udot.Atspm.Enums
{
    /// <summary>
    /// X-axis type (for charting)
    /// </summary>
    public enum XAxisType
    {
        /// <summary>
        /// Time
        /// </summary>
        Time,

        /// <summary>
        /// Time of day
        /// </summary>
        TimeOfDay,

        /// <summary>
        /// Direction
        /// </summary>
        Direction,

        /// <summary>
        /// Approach
        /// </summary>
        Approach,

        /// <summary>
        /// Signal
        /// </summary>
        Signal,

        /// <summary>
        /// Detector
        /// </summary>
        Detector
    }
}