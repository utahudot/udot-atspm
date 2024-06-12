#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Enums/DeviceTypes.cs
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
namespace ATSPM.Data.Enums
{
    /// <summary>
    /// Device types enum
    /// </summary>
    public enum DeviceTypes
    {
        /// <summary>
        /// Unknown device type
        /// </summary>
        Unknown,

        /// <summary>
        /// Signal controller
        /// </summary>
        SignalController,

        /// <summary>
        /// Ramp controller
        /// </summary>
        RampController,

        /// <summary>
        /// A.I. Camera
        /// </summary>
        AICamera,

        /// <summary>
        /// F.I.R. Camera
        /// </summary>
        FIRCamera,

        /// <summary>
        /// LIDAR Sensor
        /// </summary>
        LidarSensor,

        /// <summary>
        /// Wavetronix Speed sensor
        /// </summary>
        WavetronixSpeed
    }
}
