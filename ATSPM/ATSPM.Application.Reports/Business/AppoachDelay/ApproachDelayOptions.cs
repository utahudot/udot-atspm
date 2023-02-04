using ATSPM.Data.Models;
using System;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.AppoachDelay
{
    public class ApproachDelayOptions
    {
        //public ApproachDelayOptions(
        //    int approach,
        //    bool getPermissivePhase,
        //    DateTime startDate,
        //    DateTime endDate,
        //    int binSize,
        //    bool showPlanStatistics,
        //    bool showDelayPerVehicle)
        //{
        //    MetricTypeId = 8;
        //    ApproachId = approach;
        //    GetPermissivePhase = getPermissivePhase;
        //    StartDate = startDate;
        //    EndDate = endDate;
        //    BinSize = binSize;
        //    ShowPlanStatistics = showPlanStatistics;
        //    ShowDelayPerVehicle = showDelayPerVehicle;
        //}

        public int MetricTypeId { get; }
        public int ApproachId { get; }
        public bool GetPermissivePhase { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public int BinSize { get; }
        public bool ShowPlanStatistics { get; }
        public bool ShowDelayPerVehicle { get; }
        public string SignalId { get; internal set; }
        public bool ShowDelayPerHour { get; internal set; }
    }
}