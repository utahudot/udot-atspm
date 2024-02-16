using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;

namespace ATSPM.ReportApi.Business.Common
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
        public double AvgDelaySeconds => TotalDelaySeconds / TotalVolume;

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