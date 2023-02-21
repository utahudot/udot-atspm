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

        public int MetricTypeId { get; set; }
        public int ApproachId { get; set; }
        public bool GetPermissivePhase { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int BinSize { get; set; }
        public bool ShowPlanStatistics { get; set; }
        public bool ShowDelayPerVehicle { get; set; }
        public string SignalId { get; set; }
        public bool ShowDelayPerHour { get; set; }
    }
}