#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Common/CorrectedDetectorEvent.cs
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
using ATSPM.Data.Enums;
using System;
using System.Text.Json;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Events that coorelate to <see cref="IndianaEnumerations.VehicleDetectorOn"/>
    /// and that have been timestamp corrected for detector distances and latency
    /// using the <see cref="ATSPM.Application.AtspmMath.AdjustTimeStamp"/> calculation.
    /// </summary>
    public class CorrectedDetectorEvent : IDetectorEvent
    {
        #region IDetectorEvent

        #region ILocationPhaseLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        /// <inheritdoc/>
        public int DetectorChannel { get; set; }

        /// <inheritdoc/>
        public DirectionTypes Direction { get; set; }

        /// <summary>
        /// Coreected timestamp of event using the <see cref="ATSPM.Application.AtspmMath.AdjustTimeStamp"/> calculation.
        /// </summary>
        public DateTime Timestamp { get; set; }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
