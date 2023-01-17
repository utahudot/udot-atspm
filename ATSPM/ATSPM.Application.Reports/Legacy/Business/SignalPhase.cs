using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Legacy.Common.Business
{
    public class SignalPhase
    {

        public SignalPhase(
            VolumeCollection volume, 
            List<PlanPcd> plans, 
            List<CyclePcd> cycles,
            List<ControllerEventLog> detectorEvents,
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

        public VolumeCollection Volume { get; private set; }
        public List<PlanPcd> Plans { get; private set; }
        public List<CyclePcd> Cycles { get; private set; }
        private List<ControllerEventLog> DetectorEvents { get; set; }
        public Approach Approach { get; }
        public double AvgDelay => TotalDelay / TotalVolume;

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
                    return Math.Round(TotalGreenTime / TotalTime * 100);
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
        public double TotalDelay => Cycles.Sum(d => d.TotalDelay);
        public double TotalVolume => Cycles.Sum(d => d.TotalVolume);
        public double TotalGreenTime => Cycles.Sum(d => d.TotalGreenTime);
        public double TotalYellowTime => Cycles.Sum(d => d.TotalYellowTime);
        public double TotalRedTime => Cycles.Sum(d => d.TotalRedTime);
        public double TotalTime => Cycles.Sum(d => d.TotalTime);
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public void ResetVolume()
        {
            Volume = null;
        }
    }
}