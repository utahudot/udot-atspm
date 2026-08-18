#region license
// Copyright 2026 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/AggregationModelBase.cs
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

using Utah.Udot.Atspm.Data.Interfaces;

#nullable disable

#pragma warning disable 

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Represents the total number of signal controller events recorded for a location
    /// during the aggregation period.  
    /// This metric helps traffic engineers assess controller activity levels,
    /// detect unusual event patterns, and validate system communication.
    /// </summary>
    public partial class SignalEventCountAggregation : AggregationModelBase, ILocationLayer
    {
        /// <summary>
        /// Total number of signal controller events captured.  
        /// Useful for monitoring controller health, logging frequency,
        /// and identifying periods of abnormal activity.
        /// </summary>
        public int EventCount { get; set; }
    }
}