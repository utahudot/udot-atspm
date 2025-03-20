#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Enums/DetectionHardwareTypes.cs
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

using System.ComponentModel.DataAnnotations;

namespace Utah.Udot.Atspm.Data.Enums
{
    /// <summary>
    /// Detection hardware types
    /// </summary>
    public enum DetectionHardwareTypes
    {
        /// <summary>
        /// Not applicable
        /// </summary>
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,

        /// <summary>
        /// Wavetronics matrix
        /// </summary>
        [Display(Name = "Wavetronix Matrix", Order = 1)]
        WavetronixMatrix = 1,

        /// <summary>
        /// Wavetronix advance
        /// </summary>
        [Display(Name = "Wavetronix Advance", Order = 2)]
        WavetronixAdvance = 2,

        /// <summary>
        /// Inductive loops
        /// </summary>
        [Display(Name = "Inductive Loops", Order = 3)]
        InductiveLoops = 3,

        /// <summary>
        /// Sensys
        /// </summary>
        [Display(Name = "Sensys", Order = 4)]
        Sensys = 4,

        /// <summary>
        /// Video
        /// </summary>
        [Display(Name = "Video", Order = 5)]
        Video = 5,

        /// <summary>
        /// FLIR: thermal camera
        /// </summary>
        [Display(Name = "FLIR: Thermal Camera", Order = 6)]
        FLIRThermalCamera = 6,

        /// <summary>
        /// LiDar
        /// </summary> 
        [Display(Name = "LiDAR", Order = 7)]
        LiDar = 7,
    }
}
