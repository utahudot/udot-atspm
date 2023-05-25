using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.SplitMonitor
{
    public class SplitMonitorOptions
    {

        public SplitMonitorOptions(string signalID,int approachId, int phaseNumber, 
            DateTime startDate, DateTime endDate,
            int percentileSplit, bool showPlanStripes, bool showPedActivity,
            bool showAverageSplit, bool showPercentMaxOutForceOff, bool showPercentGapOuts, bool showPercentSkip)
        {
            SignalId = signalID;
            ApproachId = approachId;
            PhaseNumber = phaseNumber;
            Start = startDate;
            End = endDate;
            SelectedPercentileSplit = percentileSplit;
            ShowPlanStripes = showPlanStripes;
            ShowPedActivity = showPedActivity;
            ShowAverageSplit = showAverageSplit;
            ShowPercentMaxOutForceOff = showPercentMaxOutForceOff;
            ShowPercentGapOuts = showPercentGapOuts;
            ShowPercentSkip = showPercentSkip;
        }

        public SplitMonitorOptions()
        {
        }
        public int? SelectedPercentileSplit { get; set; }
        public bool ShowPlanStripes { get; set; }
        public bool ShowPedActivity { get; set; }
        public bool ShowAverageSplit { get; set; }
        public bool ShowPercentMaxOutForceOff { get; set; }
        public bool ShowPercentGapOuts { get; set; }
        public bool ShowPercentSkip { get; set; }
        public bool AdjustYAxis { get; set; }
        public string SignalId { get; private set; }
        public int ApproachId { get; private set; }
        public int PhaseNumber { get; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }



        //public override List<string> CreateMetric()
        //{
        //    base.CreateMetric();
        //    var analysisPhaseCollection = new AnalysisPhaseCollection(SignalId, StartDate, EndDate);
        //    //If there are phases in the collection add the charts
        //    if (analysisPhaseCollection.Items.Count > 0)
        //    {
        //        foreach (var plan in analysisPhaseCollection.Plans)
        //        {
        //            plan.SetProgrammedSplits(SignalId);
        //            plan.SetHighCycleCount(analysisPhaseCollection);
        //        }

        //        //If there are phases in the collection add the charts


        //        //dummy chart to create a legend for the entire split monitor page.
        //        var dummychart = new Chart();
        //        var chartarea1 = new ChartArea();
        //        ChartFactory.SetImageProperties(dummychart);
        //        dummychart.BorderlineDashStyle = ChartDashStyle.Dot;

        //        dummychart.Height = 100;

        //        var PedActivity = new Series();
        //        var GapoutSeries = new Series();
        //        var MaxOutSeries = new Series();
        //        var ForceOffSeries = new Series();
        //        var ProgramedSplit = new Series();
        //        var UnknownSeries = new Series();

        //        PedActivity.Name = "Ped Activity";
        //        GapoutSeries.Name = "Gap Out";
        //        MaxOutSeries.Name = "Max Out";
        //        ForceOffSeries.Name = "Force Off";
        //        ProgramedSplit.Name = "Programmed Split";
        //        UnknownSeries.Name = "Unknown Termination Cause";


        //        PedActivity.MarkerStyle = MarkerStyle.Cross;
        //        GapoutSeries.MarkerStyle = MarkerStyle.Circle;
        //        MaxOutSeries.MarkerStyle = MarkerStyle.Circle;
        //        ForceOffSeries.MarkerStyle = MarkerStyle.Circle;
        //        ProgramedSplit.BorderDashStyle = ChartDashStyle.Solid;
        //        UnknownSeries.MarkerStyle = MarkerStyle.Circle;

        //        GapoutSeries.Color = Color.OliveDrab;
        //        PedActivity.Color = Color.DarkGoldenrod;
        //        MaxOutSeries.Color = Color.Red;
        //        ForceOffSeries.Color = Color.MediumBlue;
        //        ProgramedSplit.Color = Color.OrangeRed;
        //        UnknownSeries.Color = Color.Black;

        //        dummychart.Series.Add(ProgramedSplit);
        //        dummychart.Series.Add(GapoutSeries);
        //        dummychart.Series.Add(MaxOutSeries);
        //        dummychart.Series.Add(ForceOffSeries);
        //        dummychart.Series.Add(UnknownSeries);
        //        dummychart.Series.Add(PedActivity);

        //        dummychart.ChartAreas.Add(chartarea1);

        //        var dummychartLegend = new Legend();
        //        dummychartLegend.Name = "DummyLegend";

        //        dummychartLegend.IsDockedInsideChartArea = true;

        //        dummychartLegend.Title = "Chart Legend";
        //        dummychartLegend.Docking = Docking.Bottom;
        //        dummychartLegend.Alignment = StringAlignment.Center;
        //        dummychart.Legends.Add(dummychartLegend);
        //        var removethese = new List<Title>();

        //        foreach (var t in dummychart.Titles)
        //            if (string.IsNullOrEmpty(t.Text))
        //                removethese.Add(t);
        //        foreach (var t in removethese)
        //            dummychart.Titles.Remove(t);


        //        var dummyChartFileName = CreateFileName();
        //        dummychart.SaveImage(MetricFileLocation + dummyChartFileName);
        //        ReturnList.Add(MetricWebPath + dummyChartFileName);

        //        if (analysisPhaseCollection.Items.Count > 0)
        //        {
        //            var phasesInOrder = (analysisPhaseCollection.Items.Select(r => r)).OrderBy(r => r.PhaseNumber);
        //            foreach (var phase in phasesInOrder)
        //            {
        //                var chart = GetNewSplitMonitorChart(StartDate, EndDate,
        //                    phase.PhaseNumber);
        //                AddSplitMonitorDataToChart(chart, phase, analysisPhaseCollection.Plans);
        //                if (ShowPlanStripes)
        //                {
        //                    SetSimplePlanStrips(analysisPhaseCollection.Plans, chart, StartDate);
        //                    SetSplitMonitorStatistics(analysisPhaseCollection.Plans, phase, chart);
        //                }
        //                var chartFileName = CreateFileName();
        //                removethese = new List<Title>();

        //                foreach (var t in chart.Titles)
        //                    if (string.IsNullOrEmpty(t.Text))
        //                        removethese.Add(t);
        //                foreach (var t in removethese)
        //                    chart.Titles.Remove(t);
        //                chart.SaveImage(MetricFileLocation + chartFileName);
        //                ReturnList.Add(MetricWebPath + chartFileName);
        //            }
        //        }
        //    }
        //    return ReturnList;
        //}

        //private void SetSplitMonitorStatistics(List<PlanSplitMonitor> plans, AnalysisPhase phase, Chart chart)
        //{
        //    //find the phase Cycles that occure during the plan.
        //    foreach (var plan in plans)
        //    {
        //        var Cycles = from cycle in phase.Cycles.Items
        //                     where cycle.StartTime >= plan.StartTime && cycle.EndTime < plan.EndTime
        //                     orderby cycle.Duration
        //                     select cycle;

        //        // find % Skips
        //        if (ShowPercentSkip)
        //            if (plan.CycleCount > 0)
        //            {
        //                double CycleCount = plan.CycleCount;
        //                double SkippedPhases = plan.CycleCount - Cycles.Count();
        //                double SkipPercent = 0;
        //                if (CycleCount > 0)
        //                    SkipPercent = SkippedPhases / CycleCount;


        //                var skipLabel = ChartTitleFactory.GetCustomLabelForTitle(
        //                    $"{SkipPercent:0.0%} Skips", plan.StartTime.ToOADate(),
        //                    plan.EndTime.ToOADate(), 1, Color.Black);

        //                //new CustomLabel();
        //                //skipLabel.FromPosition = plan.StartTime.ToOADate();
        //                //skipLabel.ToPosition = plan.EndTime.ToOADate();
        //                //skipLabel.Text = string.Format("{0:0.0%} Skips", SkipPercent);
        //                //skipLabel.LabelMark = LabelMarkStyle.LineSideMark;
        //                //skipLabel.ForeColor = Color.Black;
        //                //skipLabel.RowIndex = 1;
        //                chart.ChartAreas[0].AxisX2.CustomLabels.Add(skipLabel);
        //            }

        //        // find % GapOuts
        //        if (ShowPercentGapOuts)
        //        {
        //            var GapOuts = from cycle in Cycles
        //                          where cycle.TerminationEvent == 4
        //                          select cycle;

        //            double CycleCount = plan.CycleCount;
        //            double gapouts = GapOuts.Count();
        //            double GapPercent = 0;
        //            if (CycleCount > 0)
        //                GapPercent = gapouts / CycleCount;


        //            var gapLabel = new CustomLabel();
        //            gapLabel.FromPosition = plan.StartTime.ToOADate();
        //            gapLabel.ToPosition = plan.EndTime.ToOADate();
        //            gapLabel.Text = string.Format("{0:0.0%} GapOuts", GapPercent);
        //            gapLabel.LabelMark = LabelMarkStyle.LineSideMark;
        //            gapLabel.ForeColor = Color.OliveDrab;
        //            gapLabel.RowIndex = 2;
        //            chart.ChartAreas[0].AxisX2.CustomLabels.Add(gapLabel);
        //        }

        //        //Set Max Out
        //        if (ShowPercentMaxOutForceOff && plan.PlanNumber == 254)
        //        {
        //            var MaxOuts = from cycle in Cycles
        //                          where cycle.TerminationEvent == 5
        //                          select cycle;

        //            double CycleCount = plan.CycleCount;
        //            double maxouts = MaxOuts.Count();
        //            double MaxPercent = 0;
        //            if (CycleCount > 0)
        //                MaxPercent = maxouts / CycleCount;


        //            var maxLabel = new CustomLabel();
        //            maxLabel.FromPosition = plan.StartTime.ToOADate();
        //            maxLabel.ToPosition = plan.EndTime.ToOADate();
        //            maxLabel.Text = string.Format("{0:0.0%} MaxOuts", MaxPercent);
        //            maxLabel.LabelMark = LabelMarkStyle.LineSideMark;
        //            maxLabel.ForeColor = Color.Red;
        //            maxLabel.RowIndex = 3;
        //            chart.ChartAreas[0].AxisX2.CustomLabels.Add(maxLabel);
        //        }

        //        // Set Force Off
        //        if (ShowPercentMaxOutForceOff && plan.PlanNumber != 254
        //        )
        //        {
        //            var ForceOffs = from cycle in Cycles
        //                            where cycle.TerminationEvent == 6
        //                            select cycle;

        //            double CycleCount = plan.CycleCount;
        //            double forceoffs = ForceOffs.Count();
        //            double ForcePercent = 0;
        //            if (CycleCount > 0)
        //                ForcePercent = forceoffs / CycleCount;


        //            var forceLabel = new CustomLabel();
        //            forceLabel.FromPosition = plan.StartTime.ToOADate();
        //            forceLabel.ToPosition = plan.EndTime.ToOADate();
        //            forceLabel.Text = string.Format("{0:0.0%} ForceOffs", ForcePercent);
        //            forceLabel.LabelMark = LabelMarkStyle.LineSideMark;
        //            forceLabel.ForeColor = Color.MediumBlue;
        //            forceLabel.RowIndex = 3;
        //            chart.ChartAreas[0].AxisX2.CustomLabels.Add(forceLabel);
        //        }

        //        //Average Split
        //        if (ShowAverageSplit)
        //        {
        //            double runningTotal = 0;
        //            double averageSplits = 0;
        //            foreach (var Cycle in Cycles)
        //                runningTotal = runningTotal + Cycle.Duration.TotalSeconds;

        //            if (Cycles.Count() > 0)
        //                averageSplits = runningTotal / Cycles.Count();


        //            var avgLabel = new CustomLabel();
        //            avgLabel.FromPosition = plan.StartTime.ToOADate();
        //            avgLabel.ToPosition = plan.EndTime.ToOADate();
        //            avgLabel.Text = string.Format("{0: 0.0} Avg. Split", averageSplits);
        //            avgLabel.LabelMark = LabelMarkStyle.LineSideMark;
        //            avgLabel.ForeColor = Color.Black;
        //            avgLabel.RowIndex = 4;
        //            chart.ChartAreas[0].AxisX2.CustomLabels.Add(avgLabel);

        //            //Percentile Split
        //            if (SelectedPercentileSplit != null && Cycles.Count() > 2)
        //            {
        //                double percentileResult = 0;
        //                var Percentile = Convert.ToDouble(SelectedPercentileSplit) / 100;
        //                var setCount = Cycles.Count();


        //                var PercentilIndex = Percentile * setCount;
        //                if (PercentilIndex % 1 == 0)
        //                {
        //                    percentileResult = Cycles.ElementAt(Convert.ToInt16(PercentilIndex) - 1).Duration
        //                        .TotalSeconds;
        //                }
        //                else
        //                {
        //                    var indexMod = PercentilIndex % 1;
        //                    //subtracting .5 leaves just the integer after the convert.
        //                    //There was probably another way to do that, but this is easy.
        //                    int indexInt = Convert.ToInt16(PercentilIndex - .5);

        //                    var step1 = Cycles.ElementAt(Convert.ToInt16(indexInt) - 1).Duration.TotalSeconds;
        //                    var step2 = Cycles.ElementAt(Convert.ToInt16(indexInt)).Duration.TotalSeconds;
        //                    var stepDiff = step2 - step1;
        //                    var step3 = stepDiff * indexMod;
        //                    percentileResult = step1 + step3;
        //                }

        //                var percentileLabel = new CustomLabel();
        //                percentileLabel.FromPosition = plan.StartTime.ToOADate();
        //                percentileLabel.ToPosition = plan.EndTime.ToOADate();
        //                percentileLabel.Text = string.Format("{0: 0.0} - {1} Percentile Split", percentileResult,
        //                    Convert.ToDouble(SelectedPercentileSplit));
        //                percentileLabel.LabelMark = LabelMarkStyle.LineSideMark;
        //                percentileLabel.ForeColor = Color.Purple;
        //                percentileLabel.RowIndex = 5;
        //                chart.ChartAreas[0].AxisX2.CustomLabels.Add(percentileLabel);
        //            }
        //        }
        //    }
        //}


        //private void AddSplitMonitorDataToChart(Chart chart, AnalysisPhase phase, List<PlanSplitMonitor> plans)
        //{
        //    //Table 
        //    if (phase.Cycles.Items.Count > 0)
        //    {
        //        var maxSplitLength = 0;
        //        foreach (var plan in plans)
        //        {
        //            var highestSplit = plan.FindHighestRecordedSplitPhase();
        //            plan.FillMissingSplits(highestSplit);
        //            try
        //            {
        //                chart.Series["Programed Split"].Points.AddXY(plan.StartTime, plan.Splits[phase.PhaseNumber]);
        //                chart.Series["Programed Split"].Points.AddXY(plan.EndTime, plan.Splits[phase.PhaseNumber]);
        //                if (plan.Splits[phase.PhaseNumber] > maxSplitLength)
        //                    maxSplitLength = plan.Splits[phase.PhaseNumber];
        //            }
        //            catch
        //            {
        //                //System.Windows.MessageBox.Show(ex.ToString());
        //            }
        //        }
        //        foreach (var Cycle in phase.Cycles.Items)
        //        {
        //            if (Cycle.TerminationEvent == 4)
        //                chart.Series["GapOut"].Points.AddXY(Cycle.StartTime, Cycle.Duration.TotalSeconds);
        //            if (Cycle.TerminationEvent == 5)
        //                chart.Series["MaxOut"].Points.AddXY(Cycle.StartTime, Cycle.Duration.TotalSeconds);
        //            if (Cycle.TerminationEvent == 6)
        //                chart.Series["ForceOff"].Points.AddXY(Cycle.StartTime, Cycle.Duration.TotalSeconds);
        //            if (Cycle.TerminationEvent == 0)
        //                chart.Series["Unknown"].Points.AddXY(Cycle.StartTime, Cycle.Duration.TotalSeconds);
        //            if (Cycle.HasPed && ShowPedActivity)
        //            {
        //                if (Cycle.PedDuration == 0)
        //                {
        //                    if (Cycle.PedStartTime == DateTime.MinValue)
        //                        Cycle.SetPedStart(Cycle.StartTime);
        //                    if (Cycle.PedEndTime == DateTime.MinValue)
        //                        Cycle.SetPedEnd(Cycle.YellowEvent);
        //                }
        //                chart.Series["PedActivity"].Points.AddXY(Cycle.PedStartTime, Cycle.PedDuration);
        //            }
        //        }
        //        SetYAxisMaxAndInterval(chart, phase, maxSplitLength);
        //    }
        //}


    }
}