﻿using ATSPM.Application.Reports.ViewModels.ApproachDelay;
using ATSPM.Application.Reports.ViewModels.ApproachVolume;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproachDelayController : ControllerBase
    {
        [HttpGet("test")]
        public ApproachDelayChart Test()
        {
            Fixture fixture = new();
            ApproachDelayChart viewModel = fixture.Create<ApproachDelayChart>();
            return viewModel;
        }

        [HttpPost("chart")]
        public ApproachDelayChart Chart(
            string signalId,
            int phaseNumber,
            DateTime start,
            DateTime end,

            )
        {
            var signalPhase = new SignalPhase
            var delayChart = new ApproachDelayChart(
                "Approach Delay",
                signalId,
                "",
                phaseNumber,
                "",
                start,
                end,
                );
            Fixture fixture = new();
            ApproachDelayChart viewModel = fixture.Create<ApproachDelayChart>();
            return viewModel;
        }

        private void SetChartTitles(SignalPhase signalPhase, Dictionary<string, string> statistics)
        {
            Chart.Titles.Add(ChartTitleFactory.GetChartName(Options.MetricTypeID));
            Chart.Titles.Add(ChartTitleFactory.GetSignalLocationAndDateRange(
                Options.SignalID, Options.StartDate, Options.EndDate));
            Chart.Titles.Add(ChartTitleFactory.GetPhaseAndPhaseDescriptions(
                signalPhase.Approach, signalPhase.GetPermissivePhase));
            Chart.Titles.Add(ChartTitleFactory.GetStatistics(statistics));
            Chart.Titles.Add(ChartTitleFactory.GetTitle(
                "Simplified Approach Delay. Displays time between approach activation during the red phase and when the phase turns green."
                + " \n Does NOT account for start up delay, deceleration, or queue length that exceeds the detection zone."));
            Chart.Titles.LastOrDefault().Docking = Docking.Bottom;
        }


        protected void AddDataToChart(Chart chart, SignalPhase signalPhase, int binSize, bool showDelayPerHour,
            bool showDelayPerVehicle)
        {
            var dt = signalPhase.StartDate;
            while (dt < signalPhase.EndDate)
            {
                var pcdsInBin = from item in signalPhase.Cycles
                                where item.StartTime >= dt && item.StartTime < dt.AddMinutes(binSize)
                                select item;

                var binDelay = pcdsInBin.Sum(d => d.TotalDelay);
                var binVolume = pcdsInBin.Sum(d => d.TotalVolume);
                double bindDelaypervehicle = 0;
                double bindDelayperhour = 0;

                if (binVolume > 0 && pcdsInBin.Any())
                    bindDelaypervehicle = binDelay / binVolume;
                else
                    bindDelaypervehicle = 0;

                bindDelayperhour = binDelay * (60 / binSize) / 60 / 60;

                if (showDelayPerVehicle)
                    chart.Series["Approach Delay Per Vehicle"].Points.AddXY(dt, bindDelaypervehicle);
                if (showDelayPerHour)
                    chart.Series["Approach Delay"].Points.AddXY(dt, bindDelayperhour);

                dt = dt.AddMinutes(binSize);
            }
            var statistics = new Dictionary<string, string>();
            statistics.Add("Average Delay Per Vehicle (AD)", Math.Round(signalPhase.AvgDelay) + " seconds");
            statistics.Add("Total Delay For Selected Period (TD)", Math.Round(signalPhase.TotalDelay / 60 / 60, 1) + " hours");
            SetChartTitles(signalPhase, statistics);
        }


        protected void SetPlanStrips(List<PlanPcd> planCollection, Chart chart, DateTime graphStartDate,
            bool showPlanStatistics)
        {
            var backGroundColor = 1;
            foreach (var plan in planCollection)
            {
                var stripline = new StripLine();
                //Creates alternating backcolor to distinguish the plans
                if (backGroundColor % 2 == 0)
                    stripline.BackColor = Color.FromArgb(120, Color.LightGray);
                else
                    stripline.BackColor = Color.FromArgb(120, Color.LightBlue);

                //Set the stripline properties
                stripline.IntervalOffsetType = DateTimeIntervalType.Hours;
                stripline.Interval = 1;
                stripline.IntervalOffset = (plan.StartTime - graphStartDate).TotalHours;
                stripline.StripWidth = (plan.EndTime - plan.StartTime).TotalHours;
                stripline.StripWidthType = DateTimeIntervalType.Hours;

                chart.ChartAreas["ChartArea1"].AxisX.StripLines.Add(stripline);

                //Add a corrisponding custom label for each strip
                var Plannumberlabel = new CustomLabel();
                Plannumberlabel.FromPosition = plan.StartTime.ToOADate();
                Plannumberlabel.ToPosition = plan.EndTime.ToOADate();
                switch (plan.PlanNumber)
                {
                    case 254:
                        Plannumberlabel.Text = "Free";
                        break;
                    case 255:
                        Plannumberlabel.Text = "Flash";
                        break;
                    case 0:
                        Plannumberlabel.Text = "Unknown";
                        break;
                    default:
                        Plannumberlabel.Text = "Plan " + plan.PlanNumber;

                        break;
                }

                Plannumberlabel.ForeColor = Color.Black;
                Plannumberlabel.RowIndex = 3;

                chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(Plannumberlabel);


                var avgDelay = Math.Round(plan.AvgDelay, 0);
                var totalDelay = Math.Round(plan.TotalDelay);

                if (showPlanStatistics)
                {
                    var aogLabel = new CustomLabel();
                    aogLabel.FromPosition = plan.StartTime.ToOADate();
                    aogLabel.ToPosition = plan.EndTime.ToOADate();
                    aogLabel.Text = Math.Round(totalDelay / 60 / 60, 1) + " TD";
                    aogLabel.LabelMark = LabelMarkStyle.LineSideMark;
                    aogLabel.ForeColor = Color.Red;
                    aogLabel.RowIndex = 1;
                    chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(aogLabel);

                    var statisticlabel = new CustomLabel();
                    statisticlabel.FromPosition = plan.StartTime.ToOADate();
                    statisticlabel.ToPosition = plan.EndTime.ToOADate();
                    //statisticlabel.LabelMark = LabelMarkStyle.LineSideMark;
                    statisticlabel.Text = avgDelay + " AD\n";
                    statisticlabel.ForeColor = Color.Blue;
                    statisticlabel.RowIndex = 2;
                    chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(statisticlabel);
                }
                //Change the background color counter for alternating color
                backGroundColor++;
            }
        }

    }
}