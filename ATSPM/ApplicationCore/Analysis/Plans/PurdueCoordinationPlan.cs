#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Plans/PurdueCoordinationPlan.cs
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
using ATSPM.Application.Analysis.Common;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static Google.Api.Distribution.Types;

namespace ATSPM.Application.Analysis.Plans
{
    /// <summary>
    /// Plan specific to purdue coordination information
    /// </summary>
    public class PurdueCoordinationPlan : Plan, ICycleRatios
    {
        private readonly List<ICycleArrivals> _arrivalCycles = new();

        public PurdueCoordinationPlan() { }

        #region ICycle

        /// <inheritdoc/>
        public double TotalGreenTime => _arrivalCycles.Sum(d => d.TotalGreenTime);

        /// <inheritdoc/>
        public double TotalYellowTime => _arrivalCycles.Sum(d => d.TotalYellowTime);

        /// <inheritdoc/>
        public double TotalRedTime => _arrivalCycles.Sum(d => d.TotalRedTime);

        /// <inheritdoc/>
        public double TotalTime => _arrivalCycles.Sum(d => d.TotalTime);

        #endregion

        #region ICycleArrivals

        /// <inheritdoc/>
        public double TotalArrivalOnGreen => _arrivalCycles.Sum(d => d.TotalArrivalOnGreen);

        /// <inheritdoc/>
        public double TotalArrivalOnYellow => _arrivalCycles.Sum(d => d.TotalArrivalOnYellow);

        /// <inheritdoc/>
        public double TotalArrivalOnRed => _arrivalCycles.Sum(d => d.TotalArrivalOnRed);

        /// <inheritdoc/>
        public IReadOnlyList<Vehicle> Vehicles => _arrivalCycles.SelectMany(s => s.Vehicles).ToList();

        #endregion

        #region ICycleRatios

        /// <inheritdoc/>
        public double PercentArrivalOnGreen => TotalVolume > 0 ? Math.Round(TotalArrivalOnGreen / TotalVolume * 100) : 0;

        /// <inheritdoc/>
        public double PercentArrivalOnYellow => TotalVolume > 0 ? Math.Round(TotalArrivalOnYellow / TotalVolume * 100) : 0;

        /// <inheritdoc/>
        public double PercentArrivalOnRed => TotalVolume > 0 ? Math.Round(TotalArrivalOnRed / TotalVolume * 100) : 0;

        /// <inheritdoc/>
        public double PercentGreen => TotalVolume > 0 ? Math.Round(TotalGreenTime / TotalTime * 100) : 0;

        /// <inheritdoc/>
        public double PercentYellow => TotalVolume > 0 ? Math.Round(TotalYellowTime / TotalTime * 100) : 0;

        /// <inheritdoc/>
        public double PercentRed => TotalVolume > 0 ? Math.Round(TotalRedTime / TotalTime * 100) : 0;

        /// <inheritdoc/>
        public double PlatoonRatio => TotalVolume > 0 ? Math.Round(PercentArrivalOnGreen / PercentGreen, 2) : 0;

        /// <inheritdoc/>
        public IReadOnlyList<ICycleArrivals> ArrivalCycles
        {
            get { return _arrivalCycles; }
            set { AssignToPlan(value); }
        }

        #endregion

        #region ICycleVolume

        /// <inheritdoc/>
        public double TotalDelay => _arrivalCycles.Sum(d => d.TotalDelay);

        /// <inheritdoc/>
        public double TotalVolume => _arrivalCycles.Sum(d => d.TotalVolume);

        #endregion

        #region IPlan

        /// <inheritdoc/>
        public override void AssignToPlan<T>(IEnumerable<T> range)
        {
            _arrivalCycles.AddRange(range.Cast<ICycleArrivals>().Where(w => InRange(w.Start) && InRange(w.End)));
        }

        #endregion

        #region IStartEndRange

        /// <inheritdoc/>
        public override bool InRange(DateTime time)
        {
            return time >= Start && time <= End;
        }

        #endregion
    }
}
