#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Common/CycleArrivals.cs
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
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// A cycle with <see cref="IndianaEnumerations.VehicleDetectorOn"/> event arrivals
    /// </summary>
    public class CycleArrivals : StartEndRange, ICycleArrivals, ILocationPhaseLayer
    {
        private readonly ICycleTotal _cycle = new RedToRedCycle();

        public CycleArrivals() { }

        public CycleArrivals(ICycleTotal cycle)
        {
            _cycle = cycle;
            Start = _cycle.Start;
            End = _cycle.End;

            if (cycle is ILocationPhaseLayer sp)
            {
                LocationIdentifier = sp.LocationIdentifier;
                PhaseNumber = sp.PhaseNumber;
            }
        }

        #region ILocationPhaseLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        #region ICycle

        /// <inheritdoc/>
        public double TotalGreenTime => _cycle.TotalGreenTime;

        /// <inheritdoc/>
        public double TotalYellowTime => _cycle.TotalYellowTime;

        /// <inheritdoc/>
        public double TotalRedTime => _cycle.TotalRedTime;

        /// <inheritdoc/>
        public double TotalTime => _cycle.TotalTime;

        #endregion

        #region ICycleArrivals

        /// <inheritdoc/>
        public double TotalArrivalOnGreen => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnGreen);

        /// <inheritdoc/>
        public double TotalArrivalOnYellow => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnYellow);

        /// <inheritdoc/>
        public double TotalArrivalOnRed => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnRed);

        /// <inheritdoc/>
        [JsonIgnore]
        public IReadOnlyList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        #region ICycleVolume

        /// <inheritdoc/>
        public double TotalDelay => Vehicles.Sum(d => d.Delay);

        /// <inheritdoc/>
        public double TotalVolume => Vehicles.Count(d => InRange(d.Timestamp));

        #endregion

        #endregion

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
