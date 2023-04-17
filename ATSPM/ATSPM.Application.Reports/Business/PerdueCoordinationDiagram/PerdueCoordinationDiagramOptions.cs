using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    [DataContract]
    public class PerdueCoordinationDiagramOptions
    {

        public PerdueCoordinationDiagramOptions(
            string signalID,
            DateTime startDate,
            DateTime endDate,
            int binSize,
            bool showPlanStatistics,
            bool showVolumes,
            bool showArrivalsOnGreen)
        {

            SignalId = signalID;
            SelectedBinSize = binSize;
            ShowPlanStatistics = showPlanStatistics;
            ShowVolumes = showVolumes;
            ShowArrivalsOnGreen = showArrivalsOnGreen;
            StartDate = startDate;
            EndDate = endDate;
        }
        public int SelectedBinSize { get; set; }
        public bool ShowPlanStatistics { get; set; }
        public bool ShowVolumes { get; set; }
        [DataMember] public bool ShowArrivalsOnGreen { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SignalId { get; set; }
        public int ApproachId { get; set; }
        public bool UsePermissivePhase { get; set; }
    }
}
