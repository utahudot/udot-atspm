#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Interfaces/IPlanLayer.cs
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

namespace Utah.Udot.Atspm.Data.Interfaces
{
    /// <summary>
    /// For objects in the plan layer
    /// </summary>
    public interface IPlanLayer : ILocationLayer
    {
        /// <summary>
        /// Plan number as derrived from the event parameter on <see cref="IndianaEnumerations.CoordPatternChange"/> event
        /// </summary>
        int PlanNumber { get; set; }
    }
}
