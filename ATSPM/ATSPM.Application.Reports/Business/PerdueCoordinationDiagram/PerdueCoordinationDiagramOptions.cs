using ATSPM.Data.Models;
using Legacy.Common.Business.WCFServiceLibrary;
using System;
using System.Collections.Generic;
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

        [Required]
        [Display(Name = "Volume Bin Size")]
        [DataMember]
        public int SelectedBinSize { get; set; }

        [DataMember]
        [Display(Name = "Show Plans")]
        public bool ShowPlanStatistics { get; set; }

        [DataMember]
        [Display(Name = "Show Volumes")]
        public bool ShowVolumes { get; set; }

        [DataMember] public bool ShowArrivalsOnGreen { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SignalId { get; set; }
        public int ApproachId { get; set; }

        

    }
}
