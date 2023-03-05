using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PhaseTermination;
using ATSPM.Data.Models;
using Legacy.Common.Business.WCFServiceLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Legacy.Common.Business
{
    class WaitTimeChart
    {
        private readonly WaitTimeOptions _waitTimeOptions;
        private readonly Signal _signal;
        private readonly Approach _approach;
        private readonly IEnumerable<ControllerEventLog> _phaseEvents;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly AnalysisPhaseData _phaseInfo;
        private readonly List<PlanSplitMonitorService> _plans;

        public class WaitTimeTracker
        {
            public DateTime Time;
            public double WaitTimeSeconds;
        }

        public WaitTimeChart(WaitTimeOptions waitTimeOptions, Signal signal, Approach approach,
            IEnumerable<ControllerEventLog> phaseEvents, DateTime startDate, DateTime endDate,
            AnalysisPhaseData phaseInfo, List<PlanSplitMonitorService> plans)
        {
            _waitTimeOptions = waitTimeOptions;
            _signal = signal;
            _approach = approach;
            _phaseEvents = phaseEvents;
            _startDate = startDate;
            _endDate = endDate;
            _phaseInfo = phaseInfo;
            _plans = plans;

            string detectionTypesForApproach;
            var hasAdvanceDetection = approach.GetAllDetectorsOfDetectionType(ATSPM.Data.Enums.DetectionTypes.AC).Any();
            var hasStopBarDetection = approach.GetAllDetectorsOfDetectionType(ATSPM.Data.Enums.DetectionTypes.SBP).Any();
            bool useDroppingAlgorithm;

            if (hasAdvanceDetection && hasStopBarDetection)
            {
                detectionTypesForApproach = "Advance + Stop Bar Detection";
                useDroppingAlgorithm = true;
            }
            else if (hasAdvanceDetection)
            {
                detectionTypesForApproach = "Advance Detection";
                useDroppingAlgorithm = false;
            }
            else if (hasStopBarDetection)
            {
                detectionTypesForApproach = "Stop Bar Detection";
                useDroppingAlgorithm = true;
            }
            else
                return;
                        
            //AddDataToChart(useDroppingAlgorithm);
        }       
       

        //protected void AddDataToChart(bool useDroppingAlgorithm)
        //{
        //    var waitTimeSeries = _waitTimeOptions.CreateChartSeries(Color.Blue, SeriesChartType.Point,
        //        ChartValueType.DateTime, AxisType.Primary, "Wait Time (s)");
        //    _waitTimeOptions.CreateVolumeSeries(Chart);

        //    var redList = _phaseEvents.Where(x => x.EventCode == WaitTimeOptions.PHASE_END_RED_CLEARANCE)
        //        .OrderBy(x => x.Timestamp);
        //    var greenList = _phaseEvents.Where(x => x.EventCode == WaitTimeOptions.PHASE_BEGIN_GREEN)
        //        .OrderBy(x => x.Timestamp);
        //    var orderedPhaseRegisterList = _phaseEvents.Where(x =>
        //        x.EventCode == WaitTimeOptions.PHASE_CALL_REGISTERED ||
        //        x.EventCode == WaitTimeOptions.PHASE_CALL_DROPPED);

        //    var waitTimeTrackerList = new List<WaitTimeTracker>();

            

        //    try
        //    {
        //        foreach (var red in redList)
        //        {
        //            //Find the corresponding green
        //            var green = greenList.Where(x => x.Timestamp > red.Timestamp).OrderBy(x => x.Timestamp)
        //                .FirstOrDefault();
        //            if (green == null)
        //                continue;

        //            //Find all events between the red and green
        //            var phaseCallList = orderedPhaseRegisterList
        //                .Where(x => x.Timestamp >= red.Timestamp && x.Timestamp < green.Timestamp)
        //                .OrderBy(x => x.Timestamp).ToList();

        //            if (!phaseCallList.Any())
        //                continue;

        //            var exportList = new List<string>();
        //            foreach (var row in phaseCallList)
        //            {
        //                exportList.Add($"{row.SignalID}, {row.Timestamp}, {row.EventCode}, {row.EventParam}");
        //            }

        //            WaitTimeTracker waitTimeTrackerToFill = null;
        //            if (useDroppingAlgorithm &&
        //                phaseCallList.Any(x => x.EventCode == WaitTimeOptions.PHASE_CALL_DROPPED))
        //            {
        //                var lastDroppedPhaseCall =
        //                    phaseCallList.LastOrDefault(x => x.EventCode == WaitTimeOptions.PHASE_CALL_DROPPED);
        //                if (lastDroppedPhaseCall != null)
        //                {
        //                    var lastIndex = phaseCallList.IndexOf(lastDroppedPhaseCall);
        //                    if (lastIndex + 1 >= phaseCallList.Count)
        //                        continue;
        //                    var nextPhaseCall = phaseCallList[lastIndex + 1];

        //                    waitTimeTrackerToFill = new WaitTimeTracker
        //                    {
        //                        Time = green.Timestamp,
        //                        WaitTimeSeconds = (green.Timestamp - nextPhaseCall.Timestamp).TotalSeconds
        //                    };
        //                }
        //            }
        //            else if (phaseCallList.Any(x => x.EventCode == WaitTimeOptions.PHASE_CALL_REGISTERED))
        //            {
        //                var firstPhaseCall = phaseCallList.First(x => x.EventCode == WaitTimeOptions.PHASE_CALL_REGISTERED);
        //                //waitTimeTrackerList.Add(new WaitTimeTracker { Time = green.Timestamp, WaitTimeSeconds = (green.Timestamp - firstPhaseCall.Timestamp).TotalSeconds });
        //                waitTimeTrackerToFill = new WaitTimeTracker
        //                {
        //                    Time = green.Timestamp,
        //                    WaitTimeSeconds = (green.Timestamp - firstPhaseCall.Timestamp).TotalSeconds
        //                };
        //            }
        //            else
        //            {
        //                continue;
        //            }

        //            //Toss anything longer than 6 minutes - usually a bad value as a result of missing data
        //            if (waitTimeTrackerToFill.WaitTimeSeconds > 360)
        //                continue;

        //            var priorPhase = _phaseInfo.Cycles.Items.FirstOrDefault(x => x.EndTime == red.Timestamp);
        //            if (priorPhase != null)
        //            {
        //                waitTimeTrackerList.Add(waitTimeTrackerToFill);
        //                switch (priorPhase.TerminationEvent)
        //                {
        //                    case 4: //Gap Out
        //                        GapoutSeries.Points.AddXY(waitTimeTrackerToFill.Time,
        //                            waitTimeTrackerToFill.WaitTimeSeconds);
        //                        break;
        //                    case 5: //Max Out
        //                        MaxOutSeries.Points.AddXY(waitTimeTrackerToFill.Time,
        //                            waitTimeTrackerToFill.WaitTimeSeconds);
        //                        break;
        //                    case 6: //Force Off
        //                        ForceOffSeries.Points.AddXY(waitTimeTrackerToFill.Time,
        //                            waitTimeTrackerToFill.WaitTimeSeconds);
        //                        break;
        //                    case 0:
        //                        UnknownSeries.Points.AddXY(waitTimeTrackerToFill.Time,
        //                            waitTimeTrackerToFill.WaitTimeSeconds);
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    try
        //    {
        //        if (!waitTimeTrackerList.Any())
        //            return;

        //        for (var lowerTimeLimit = _startDate;
        //            lowerTimeLimit < _endDate;
        //            lowerTimeLimit = lowerTimeLimit.AddMinutes(15))
        //        {
        //            var upperTimeLimit = lowerTimeLimit.AddMinutes(15);
        //            var items = waitTimeTrackerList.Where(x => x.Time > lowerTimeLimit && x.Time < upperTimeLimit);
        //            if (items.Any())
        //            {
        //                var avg = items.Average(x => x.WaitTimeSeconds);
        //                AverageSeries.Points.AddXY(upperTimeLimit, avg);
        //            }
        //        }

        //        //Round to the nearest minute
        //        Chart.ChartAreas[0].AxisY.Maximum =
        //            Math.Ceiling(waitTimeTrackerList.Max(x => x.WaitTimeSeconds) / 60d) * 60;
        //        Chart.Series.Add(GapoutSeries);
        //        Chart.Series.Add(MaxOutSeries);
        //        Chart.Series.Add(ForceOffSeries);
        //        Chart.Series.Add(UnknownSeries);
        //        Chart.Series.Add(AverageSeries);

        //        var maxSplitLength = 0;
        //        foreach (var plan in _plans)
        //        {
        //            var highestSplit = plan.FindHighestRecordedSplitPhase();
        //            plan.FillMissingSplits(highestSplit);
        //            try
        //            {
        //                ProgrammedSplitSeries.Points.AddXY(plan.StartTime, plan.Splits[_phaseInfo.PhaseNumber]);
        //                ProgrammedSplitSeries.Points.AddXY(plan.EndTime, plan.Splits[_phaseInfo.PhaseNumber]);
        //            }
        //            catch
        //            {
        //            }
        //        }

        //        Chart.Series.Add(ProgrammedSplitSeries);

        //        var signalPhase = new SignalPhase(_startDate, _endDate, _approach, true, 15, 32, false);
        //        if (_waitTimeOptions.ShowPlanStripes)
        //        {
        //            SetSimplePlanStrips(_plans, Chart, _startDate, waitTimeTrackerList);
        //        }

        //        AddVolumeToChart(Chart, signalPhase.Volume);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        //private void SetSimplePlanStrips(List<PlanSplitMonitor> plans, Chart chart, DateTime graphStartDate,
        //    List<WaitTimeTracker> waitTimeTrackerList)
        //{
        //    var backGroundColor = 1;
        //    foreach (var plan in plans)
        //    {
        //        var stripline = new StripLine();
        //        //Creates alternating backcolor to distinguish the plans
        //        if (backGroundColor % 2 == 0)
        //            stripline.BackColor = Color.FromArgb(120, Color.LightGray);
        //        else
        //            stripline.BackColor = Color.FromArgb(120, Color.LightBlue);

        //        //Set the stripline properties
        //        stripline.IntervalOffsetType = DateTimeIntervalType.Hours;
        //        stripline.Interval = 1;
        //        stripline.IntervalOffset = (plan.StartTime - graphStartDate).TotalHours;
        //        stripline.StripWidth = (plan.EndTime - plan.StartTime).TotalHours;
        //        stripline.StripWidthType = DateTimeIntervalType.Hours;

        //        chart.ChartAreas["ChartArea1"].AxisX.StripLines.Add(stripline);

        //        //Add a corrisponding custom label for each strip
        //        var Plannumberlabel = new CustomLabel();
        //        Plannumberlabel.FromPosition = plan.StartTime.ToOADate();
        //        Plannumberlabel.ToPosition = plan.EndTime.ToOADate();
        //        switch (plan.PlanNumber)
        //        {
        //            case 254:
        //                Plannumberlabel.Text = "Free";
        //                break;
        //            case 255:
        //                Plannumberlabel.Text = "Flash";
        //                break;
        //            case 0:
        //                Plannumberlabel.Text = "Unknown";
        //                break;
        //            default:
        //                Plannumberlabel.Text = "Plan " + plan.PlanNumber;

        //                break;
        //        }

        //        Plannumberlabel.LabelMark = LabelMarkStyle.LineSideMark;
        //        Plannumberlabel.ForeColor = Color.Black;
        //        Plannumberlabel.RowIndex = 6;


        //        chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(Plannumberlabel);

        //        if (waitTimeTrackerList.Any())
        //        {
        //            var waitTimeSubset =
        //                waitTimeTrackerList.Where(x => x.Time > plan.StartTime && x.Time < plan.EndTime);
        //            if (waitTimeSubset.Any())
        //            {
        //                var avgWaitTime = waitTimeSubset.Average(x => x.WaitTimeSeconds);
        //                var maxWaitTime = waitTimeSubset.Max(x => x.WaitTimeSeconds);

        //                var avgLabel = ChartTitleFactory.GetCustomLabelForTitle(
        //                    $"Avg Wait: {avgWaitTime.ToString("F1")} s", plan.StartTime.ToOADate(),
        //                    plan.EndTime.ToOADate(), 5, Color.Black);
        //                chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(avgLabel);

        //                var maxLabel = ChartTitleFactory.GetCustomLabelForTitle(
        //                    $"Max Wait: {maxWaitTime.ToString("F1")} s", plan.StartTime.ToOADate(),
        //                    plan.EndTime.ToOADate(), 4, Color.Black);
        //                chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(maxLabel);
        //            }
        //        }

        //        backGroundColor++;
        //    }
        //}

        //void AddVolumeToChart(Chart chart, VolumeCollection volumeCollection)
        //{
        //    foreach (var v in volumeCollection.Items)
        //        chart.Series["Volume Per Hour"].Points.AddXY(v.XAxis, v.YAxis);
        //}
    }
}