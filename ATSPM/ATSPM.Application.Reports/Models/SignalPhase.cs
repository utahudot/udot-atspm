using ATSPM.Application.Reports.ViewModels.TurningMovementCounts;
using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Data.Models;
using ATSPM.Application.Reports.Business;
using Legacy.Common.Business;

namespace ATSPM.Application.Reports.Models
{
    public class SignalPhase
    {
        public VolumeCollection Volume { get; private set; }

        public List<PerdueCoordinationPlan> Plans { get; private set; }
        public List<CyclePcd> Cycles { get; private set; }
        private List<ControllerEventLog> DetectorEvents { get; set; }
        public bool GetPermissivePhase { get; }
        public ATSPM.Data.Models.Approach Approach { get; }
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


        private readonly bool _showVolume;
        private readonly int _binSize;
        private readonly int _metricTypeId;
        private readonly int _pcdCycleTime = 0;



        public SignalPhase(VolumeCollection volume)
        {
            Volume = volume;
        }
    }
}
