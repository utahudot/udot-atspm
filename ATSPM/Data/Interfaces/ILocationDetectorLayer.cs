#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Interfaces/ILocationDetectorLayer.cs
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
namespace ATSPM.Data.Interfaces
{
    /// <summary>
    /// For objects in the location and detector layer
    /// </summary>
    public interface ILocationDetectorLayer : ILocationLayer
    {
        /// <summary>
        /// Detector channel assigned to <see cref="ILocationLayer.LocationIdentifier"/>
        /// </summary>
        int DetectorChannel { get; set; }
    }
}
