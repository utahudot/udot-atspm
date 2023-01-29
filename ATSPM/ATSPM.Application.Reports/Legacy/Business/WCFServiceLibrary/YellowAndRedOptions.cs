using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Legacy.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class YellowAndRedOptions
    {
        public YellowAndRedOptions(string signalID, DateTime startDate, DateTime endDate, double yAxisMax,
            double y2AxisMax,
            int binSize, int metricTypeID, double severeLevelSeconds, bool showRedLightViolations,
            bool showSevereRedLightViolations,
            bool showPercentRedLightViolations, bool showPercentSevereRedLightViolations,
            bool showAverageTimeRedLightViolations,
            bool showYellowLightOccurrences, bool showPercentYellowLightOccurrences,
            bool showAverageTimeYellowOccurences)
        {
            SignalId = signalID;
            StartDate = startDate;
            EndDate = endDate;
            MetricTypeId = metricTypeID;
            SevereLevelSeconds = severeLevelSeconds;
            ShowRedLightViolations = showRedLightViolations;
            ShowSevereRedLightViolations = showSevereRedLightViolations;
            ShowPercentRedLightViolations = showPercentRedLightViolations;
            ShowPercentSevereRedLightViolations = showPercentSevereRedLightViolations;
            ShowAverageTimeRedLightViolations = showAverageTimeRedLightViolations;
            ShowYellowLightOccurrences = showYellowLightOccurrences;
            ShowPercentYellowLightOccurrences = showPercentYellowLightOccurrences;
            ShowAverageTimeYellowOccurences = showAverageTimeYellowOccurences;
            BinSize = binSize;
        }

        public YellowAndRedOptions()
        {
            MetricTypeId = 11;
            BinSize = 15;
        }

        [DataMember]
        [Display(Name = "Severe Red Light Violations")]
        public double SevereLevelSeconds { get; set; }

        [DataMember]
        public int BinSize { get; set; }

        [DataMember]
        [Display(Name = "Red Light Violations")]
        public bool ShowRedLightViolations { get; set; }

        [DataMember]
        [Display(Name = "Severe Red Light Violations")]
        public bool ShowSevereRedLightViolations { get; set; }

        [DataMember]
        [Display(Name = "Percent Red Light Violations")]
        public bool ShowPercentRedLightViolations { get; set; }

        [DataMember]
        [Display(Name = "Percent Severe Red Light Violations")]
        public bool ShowPercentSevereRedLightViolations { get; set; }

        [DataMember]
        [Display(Name = "Average Time Red Light Violations")]
        public bool ShowAverageTimeRedLightViolations { get; set; }

        [DataMember]
        [Display(Name = "Yellow Light Occurrences")]
        public bool ShowYellowLightOccurrences { get; set; }

        [DataMember]
        [Display(Name = "Percent Yellow Light Occurrences")]
        public bool ShowPercentYellowLightOccurrences { get; set; }

        [DataMember]
        [Display(Name = "Average Time Yellow Occurences")]
        public bool ShowAverageTimeYellowOccurences { get; set; }
        public string SignalId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public int MetricTypeId { get; private set; }

        //public override List<string> CreateMetric()
        //{
        //    base.CreateMetric();
        //    var returnList = new List<string>();


        //    var signalphasecollection =
        //        new RLMSignalPhaseCollection(StartDate, EndDate, SignalId, BinSize, SevereLevelSeconds);

        //    if (signalphasecollection.SignalPhaseList.Count > 0)
        //        foreach (var signalPhase in signalphasecollection.SignalPhaseList)
        //        {
        //            var location = GetSignalLocation();
        //            var rlmChart = new RLMChart();
        //            var chart = rlmChart.GetChart(signalPhase, this);

        //            if (signalPhase.GetPermissivePhase)
        //            {
        //                chart.BackColor = Color.LightGray;
        //                chart.Titles[2].Text = "Permissive " + chart.Titles[2].Text + " " +
        //                                       signalPhase.Approach.Detectors.FirstOrDefault().MovementType.Description;
        //            }
        //            else
        //            {
        //                chart.Titles[2].Text = "Protected " + chart.Titles[2].Text + " " +
        //                                       signalPhase.Approach.Detectors.FirstOrDefault().MovementType.Description;
        //            }


        //            var chartName = CreateFileName();

        //            chart.SaveImage(MetricFileLocation + chartName, ChartImageFormat.Jpeg);
        //            returnList.Add(MetricWebPath + chartName);
        //        }

        //    return returnList;
        //}
    }
}