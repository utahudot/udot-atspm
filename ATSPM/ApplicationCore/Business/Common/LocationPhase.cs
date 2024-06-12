#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/LocationPhase.cs
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
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.Common
{
    public class LocationPhase
    {

        public LocationPhase(
            VolumeCollection volume,
            List<PurdueCoordinationPlan> plans,
            List<CyclePcd> cycles,
            List<IndianaEvent> detectorEvents,
            Approach approach,
            DateTime startDate,
            DateTime endDate
            )
        {
            Volume = volume;
            Plans = plans;
            Cycles = cycles;
            DetectorEvents = detectorEvents;
            Approach = approach;
            StartDate = startDate;
            EndDate = endDate;
        }

        public LocationPhase()
        {
        }

        public VolumeCollection Volume { get; private set; }
        public List<PurdueCoordinationPlan> Plans { get; private set; }
        public List<CyclePcd> Cycles { get; private set; }
        private List<IndianaEvent> DetectorEvents { get; set; }
        public Approach Approach { get; }
        public double AvgDelaySeconds => (TotalDelaySeconds == 0 || TotalVolume == 0) ? 0 : TotalDelaySeconds / TotalVolume;

        public double PercentArrivalOnGreen
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(TotalArrivalOnGreen / TotalVolume * 100);
                return 0;
            }
        }

        public double PercentGreen
        {
            get
            {
                if (TotalTime > 0)
                    return Math.Round(TotalGreenTimeSeconds / TotalTime * 100);
                return 0;
            }
        }

        public double PlatoonRatio
        {
            get
            {
                if (TotalVolume > 0)
                    return Math.Round(PercentArrivalOnGreen / PercentGreen, 2);
                return 0;
            }
        }

        public double TotalArrivalOnGreen => Cycles.Sum(d => d.TotalArrivalOnGreen);
        public double TotalArrivalOnRed => Cycles.Sum(d => d.TotalArrivalOnRed);
        public double TotalArrivalOnYellow => Cycles.Sum(d => d.TotalArrivalOnYellow);
        public double TotalDelaySeconds => Cycles.Sum(d => d.TotalDelaySeconds);
        public double TotalVolume => Cycles.Sum(d => d.TotalVolume);
        public double TotalGreenTimeSeconds => Cycles.Sum(d => d.TotalGreenTimeSeconds);
        public double TotalYellowTimeSeconds => Cycles.Sum(d => d.TotalYellowTimeSeconds);
        public double TotalRedTimeSeconds => Cycles.Sum(d => d.TotalRedTimeSeconds);
        public double TotalTime => Cycles.Sum(d => d.TotalTimeSeconds);
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public void ResetVolume()
        {
            Volume = null;
        }
    }
}