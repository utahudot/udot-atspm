using Legacy.Common.Business.SplitFail;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Legacy.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class SplitFailOptions
    {
        public SplitFailOptions(string signalID, DateTime startDate, DateTime endDate,
            int metricTypeID, int firstSecondsOfRed, bool showFailLines, bool showAvgLines, bool showPercentFailLine)
        {
            SignalId = signalID;
            StartDate = startDate;
            EndDate = endDate;
            MetricTypeId = metricTypeID;
            FirstSecondsOfRed = firstSecondsOfRed;
            ShowFailLines = showFailLines;
            ShowAvgLines = showAvgLines;
            ShowPercentFailLines = showPercentFailLine;
        }

        public SplitFailOptions()
        {
            MetricTypeId = 12;
            //SetDefaults();
        }

        [Required]
        [DataMember]
        [Display(Name = "First Seconds Of Red")]
        public int FirstSecondsOfRed { get; set; }

        [DataMember]
        [Display(Name = "Show Fail Lines")]
        public bool ShowFailLines { get; set; }

        [DataMember]
        [Display(Name = "Show Average Lines")]
        public bool ShowAvgLines { get; set; }

        [DataMember]
        [Display(Name = "Show Percent Fail Lines")]
        public bool ShowPercentFailLines { get; set; }
        public DateTime StartDate { get; internal set; }
        public DateTime EndDate { get; internal set; }
        public int MetricTypeId { get; }
        public string SignalId { get; internal set; }

        //public override List<string> CreateMetric()
        //{
        //    base.CreateMetric();
        //    //EndDate = EndDate.AddSeconds(59);
        //    var returnString = new List<string>();
        //    var sr = SignalsRepositoryFactory.Create();
        //    var signal = sr.GetVersionOfSignalByDate(SignalId, StartDate);
        //    var metricApproaches = signal.GetApproachesForSignalThatSupportMetric(MetricTypeId);
        //    if (metricApproaches.Count > 0)
        //    {
        //        List<SplitFailPhaseService> splitFailPhases = new List<SplitFailPhaseService>();
        //        foreach (Approach approach in metricApproaches)
        //        {
        //            if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber > 0)
        //            {
        //                splitFailPhases.Add(new SplitFailPhase(approach, this, true));
        //            }
        //            if (approach.ProtectedPhaseNumber > 0)
        //            {
        //                splitFailPhases.Add(new SplitFailPhase(approach, this, false));
        //            }
        //        }
        //        splitFailPhases = splitFailPhases.OrderBy(s => s.PhaseNumberSort).ToList();
        //        foreach (var splitFailPhase in splitFailPhases)
        //        {
        //            GetChart(splitFailPhase, returnString);
        //        }
        //    }
        //    return returnString;
        //}

        //private void GetChart(SplitFailPhaseService splitFailPhase, List<string> returnString)
        //{
        //    var sfChart = new SplitFailChart(this, splitFailPhase, splitFailPhase.GetPermissivePhase);
        //    var detector = splitFailPhase.Approach.GetDetectorsForMetricType(12).FirstOrDefault();
        //    if (detector != null)
        //    {
        //        if (splitFailPhase.GetPermissivePhase)
        //        {
        //            sfChart.Chart.BackColor = Color.LightGray;
        //        }
        //    }
        //    //Thread.Sleep(300);
        //    string chartName = CreateFileName();
        //    chartName = chartName.Replace(".", splitFailPhase.Approach.DirectionType.Description + ".");
        //    try
        //    {
        //        sfChart.Chart.SaveImage(MetricFileLocation + chartName, ChartImageFormat.Jpeg);
        //    }
        //    catch (Exception ex)
        //    {
        //        try
        //        {
        //            sfChart.Chart.SaveImage(MetricFileLocation + chartName, ChartImageFormat.Jpeg);
        //        }
        //        catch
        //        {
        //            var appEventRepository =
        //                ApplicationEventRepositoryFactory.Create();
        //            var applicationEvent = new ApplicationEvent();
        //            applicationEvent.ApplicationName = "SPM Website";
        //            applicationEvent.Description = MetricType.ChartName + ex.Message + " Failed While Saving File";
        //            applicationEvent.SeverityLevel = ApplicationEvent.SeverityLevels.Medium;
        //            applicationEvent.Timestamp = DateTime.Now;
        //            appEventRepository.Add(applicationEvent);
        //        }
        //    }
        //    returnString.Add(MetricWebPath + chartName);
        //}
    }
}