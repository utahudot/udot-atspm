#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/PurdueCoordinationPlan.cs
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.Common
{
    public class PurdueCoordinationPlan : Plan
    {
        public SortedDictionary<int, int> Splits = new SortedDictionary<int, int>();

        public PurdueCoordinationPlan(DateTime start, DateTime end, string planNumber, List<CyclePcd> cyclesForPlan) : base(planNumber, start, end
            )
        {
            TotalTime = cyclesForPlan.Sum(d => d.TotalTimeSeconds);
            TotalRedTime = cyclesForPlan.Sum(d => d.TotalRedTimeSeconds);
            TotalYellowTime = cyclesForPlan.Sum(d => d.TotalYellowTimeSeconds);
            TotalGreenTime = cyclesForPlan.Sum(d => d.TotalGreenTimeSeconds);
            TotalVolume = cyclesForPlan.Sum(d => d.TotalVolume);
            TotalDelay = cyclesForPlan.Sum(d => d.TotalDelaySeconds);
            TotalArrivalOnRed = cyclesForPlan.Sum(d => d.TotalArrivalOnRed);
            TotalArrivalOnYellow = cyclesForPlan.Sum(d => d.TotalArrivalOnYellow);
            TotalArrivalOnGreen = cyclesForPlan.Sum(d => d.TotalArrivalOnGreen);
            TotalCycles = cyclesForPlan.Count;
            TotalDetectorHits = cyclesForPlan.Sum(c => c.TotalVolume);
        }

        public double TotalDetectorHits { get; }

        public double PercentGreenTime
        {
            get
            {
                if (TotalTime > 0)
                    return Math.Round(TotalGreenTime / TotalTime * 100);
                return 0;
            }
        }

        public double PercentRedTime
        {
            get
            {
                if (TotalTime > 0)
                    return Math.Round(TotalRedTime / TotalTime * 100);
                return 0;
            }
        }

        public double AvgDelay
        {
            get
            {
                if (TotalVolume > 0)
                    return TotalDelay / TotalVolume;
                return 0;
            }
        }

        public double PercentArrivalOnGreen
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(TotalArrivalOnGreen / TotalVolume * 100);
                return 0;
            }
        }

        public double PercentArrivalOnRed
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(TotalArrivalOnRed / TotalVolume * 100);
                return 0;
            }
        }

        public double PlatoonRatio
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(PercentArrivalOnGreen / PercentGreenTime, 2);
                return 0;
            }
        }

        public double TotalArrivalOnGreen { get; }
        public double TotalArrivalOnYellow { get; }
        public double TotalArrivalOnRed { get; }
        public double TotalDelay { get; }
        public double TotalVolume { get; }
        public double TotalGreenTime { get; }
        public double TotalYellowTime { get; }
        public double TotalRedTime { get; }
        public double TotalTime { get; }
        public int TotalCycles { get; }
    }
}