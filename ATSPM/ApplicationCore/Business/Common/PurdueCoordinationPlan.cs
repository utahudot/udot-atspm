using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.Common
{
    public class PurdueCoordinationPlan : Plan
    {
        public SortedDictionary<int, int> Splits { get; set; }

        public PurdueCoordinationPlan(DateTime start, DateTime end, string planNumber, List<CyclePcd> cyclesForPlan) : base(planNumber, start, end
            )
        {
            Splits = new SortedDictionary<int, int>();
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