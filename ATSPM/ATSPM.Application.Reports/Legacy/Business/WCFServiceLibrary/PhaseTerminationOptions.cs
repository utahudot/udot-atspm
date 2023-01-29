using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;

namespace Legacy.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class PhaseTerminationOptions 
    {
        public PhaseTerminationOptions(DateTime startDate,
            double yAxisMax,
            DateTime endDate,
            string signalId,
            bool showPedActivity,
            int consecutiveCount,
            bool showPlanStripes)
        {
            SignalId = signalId;
            MetricTypeId = 1;
            //ConsecutiveCount = consecutiveCount;
            ShowPedActivity = showPedActivity;
            ShowPlanStripes = showPlanStripes;
        }

        public PhaseTerminationOptions()
        {
            ConsecutiveCountList = new List<int>() { 1, 2, 3, 4, 5 };
            MetricTypeId = 1;
            ShowArrivalsOnGreen = true;
        }

        [Required]
        [DataMember]
        [Display(Name = "Consecutive Count")]
        public int SelectedConsecutiveCount { get; set; }

        [DataMember]
        public List<int> ConsecutiveCountList { get; set; }

        [DataMember]
        [Display(Name = "Show Ped Activity")]
        public bool ShowPedActivity { get; set; }

        [DataMember]
        [Display(Name = "Show Plans")]
        public bool ShowPlanStripes { get; set; }

        [DataMember]
        public bool ShowArrivalsOnGreen { get; set; }
        public string SignalId { get; private set; }
        public int MetricTypeId { get; private set; }

       

        //public override List<string> CreateMetric()
        //{
        //    base.CreateMetric();
        //    var location = GetSignalLocation();
        //    var chart = ChartFactory.CreateDefaultChartNoX2Axis(this);

        //    CreateLegend();
        //    var analysisPhaseCollection =
        //        new AnalysisPhaseCollection(SignalId, StartDate,
        //            EndDate, SelectedConsecutiveCount);

        //    //If there are phases in the collection add the charts
        //    if (analysisPhaseCollection.Items.Count > 0)
        //    {
        //        chart = GetNewTermEventChart(StartDate, EndDate, SignalId, location,
        //            SelectedConsecutiveCount, analysisPhaseCollection.MaxPhaseInUse, ShowPedActivity);

        //        AddTermEventDataToChart(chart, StartDate, EndDate, analysisPhaseCollection, SignalId,
        //            ShowPedActivity, ShowPlanStripes);
        //    }

        //    var chartName = CreateFileName();
        //    var removethese = new List<Title>();

        //    foreach (var t in chart.Titles)
        //        if (t.Text == "" || t.Text == null)
        //            removethese.Add(t);
        //    foreach (var t in removethese)
        //        chart.Titles.Remove(t);

        //    //Save an image of the chart
        //    chart.SaveImage(MetricFileLocation + chartName, ChartImageFormat.Jpeg);

        //    ReturnList.Add(MetricWebPath + chartName);

        //    return ReturnList;
        //}

      

    

        //protected void AddTermEventDataToChart(Chart chart, DateTime startDate,
        //    DateTime endDate, AnalysisPhaseCollection analysisPhaseCollection,
        //    string signalId, bool showVolume, bool showPlanStripes)
        //{
        //    foreach (var phase in analysisPhaseCollection.Items)
        //    {
        //        if (phase.TerminationEvents.Count > 0)
        //        {
        //            foreach (var TermEvent in phase.ConsecutiveGapOuts)
        //                chart.Series["GapOut"].Points.AddXY(TermEvent.Timestamp, phase.PhaseNumber);

        //            foreach (var TermEvent in phase.ConsecutiveMaxOut)
        //                chart.Series["MaxOut"].Points.AddXY(TermEvent.Timestamp, phase.PhaseNumber);

        //            foreach (var TermEvent in phase.ConsecutiveForceOff)
        //                chart.Series["ForceOff"].Points.AddXY(TermEvent.Timestamp, phase.PhaseNumber);

        //            foreach (var TermEvent in phase.UnknownTermination)
        //                chart.Series["Unknown"].Points.AddXY(TermEvent.Timestamp, phase.PhaseNumber);

        //            if (ShowPedActivity)
        //                foreach (var PedEvent in phase.PedestrianEvents)
        //                    if (PedEvent.EventCode == 23)
        //                        chart.Series["Ped Walk Begin"].Points.AddXY(PedEvent.Timestamp, phase.PhaseNumber + .3);
        //        }
        //        if (showPlanStripes)
        //            SetSimplePlanStrips(analysisPhaseCollection.Plans, chart, startDate);
        //        if (YAxisMax != null)
        //            chart.ChartAreas[0].AxisY.Maximum = YAxisMax.Value + .5;
        //    }
        //}
        
    }
}