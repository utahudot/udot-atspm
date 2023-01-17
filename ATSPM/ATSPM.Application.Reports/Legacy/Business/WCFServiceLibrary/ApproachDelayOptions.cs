using ATSPM.Data.Models;
using System;
using System.Runtime.Serialization;

namespace Legacy.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class ApproachDelayOptions : MetricOptions
    {
        public ApproachDelayOptions(
            Approach approach,
            bool getPermissivePhase,
            DateTime startDate, 
            DateTime endDate,
            int binSize, 
            bool showPlanStatistics,
            bool showDelayPerVehicle)
        {
            MetricTypeId = 8;
            Approach = approach;
            GetPermissivePhase = getPermissivePhase;
            StartDate = startDate;
            EndDate = endDate;
            BinSize = binSize;
            ShowPlanStatistics = showPlanStatistics;
            ShowDelayPerVehicle = showDelayPerVehicle;
        }

        public Approach Approach { get; }
        public bool GetPermissivePhase { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public int BinSize { get; }
        public bool ShowPlanStatistics { get; }
        public bool ShowDelayPerVehicle { get; }
    }
}