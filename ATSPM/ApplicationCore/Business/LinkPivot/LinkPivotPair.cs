#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.LinkPivot/LinkPivotPair.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Application.Business.Common;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.LinkPivot
{

    public class LinkPivotPair
    {
        public LinkPivotPair()
        {
        }

        public double SecondsAdded { get; set; }
        public double MaxArrivalOnGreen { get; set; }
        public double MaxPercentAog { get; set; }
        public List<LocationPhase> UpstreamPcd { get; set; } = new List<LocationPhase>();
        public List<LocationPhase> DownstreamPcd { get; set; } = new List<LocationPhase>();
        public int PaogUpstreamBefore { get; set; } = 0;
        public int PaogDownstreamBefore { get; set; }
        public int PaogDownstreamPredicted { get; set; }
        public int PaogUpstreamPredicted { get; set; }
        public double AogUpstreamBefore { get; set; }
        public double AogDownstreamBefore { get; set; }
        public double AogDownstreamPredicted { get; set; }
        public double AogUpstreamPredicted { get; set; }
        public double TotalVolumeUpstream { get; set; }
        public double TotalVolumeDownstream { get; set; }
        public string ResultChartLocation { get; private set; }
        public Dictionary<int, double> ResultsGraph { get; set; } = new Dictionary<int, double>();
        public Dictionary<int, double> UpstreamResultsGraph { get; set; } = new Dictionary<int, double>();
        public Dictionary<int, double> DownstreamResultsGraph { get; set; } = new Dictionary<int, double>();
        //public List<LinkPivotPCDDisplay> Display { get; set; } = new List<LinkPivotPCDDisplay>();
        public double AogTotalBefore { get; set; }
        public int PaogTotalBefore { get; set; }
        public double AogTotalPredicted { get; set; }
        public int PaogTotalPredicted { get; set; }
        public Approach UpstreamLocationApproach { get; set; }
        public Approach DownstreamLocationApproach { get; set; }
        public DateOnly StartDate { get; set; }
        public int LinkNumber { get; set; }

        

        //private void GetNewResultsChart()
        //{

        //    var chart = new Chart();

        //    //Set the chart properties
        //    ChartFactory.SetImageProperties(chart);
        //    chart.ImageStorageMode = ImageStorageMode.UseImageLocation;

        //    //Set the chart title
        //    var title = new Title();
        //    title.Text = "Max Arrivals On Green By Second";
        //    title.Font = new Font(FontFamily.GenericSansSerif, 20);
        //    chart.Titles.Add(title);

        //    //Create the chart legend
        //    var chartLegend = new Legend();
        //    chartLegend.Name = "MainLegend";
        //    //chartLegend.LegendStyle = LegendStyle.Table;
        //    chartLegend.Docking = Docking.Left;
        //    //chartLegend.CustomItems.Add(Color.Blue, "AoG - Arrival On Green");
        //    //chartLegend.CustomItems.Add(Color.Blue, "GT - Green Time");
        //    //chartLegend.CustomItems.Add(Color.Maroon, "PR - Platoon Ratio");
        //    //LegendCellColumn a = new LegendCellColumn();
        //    //a.ColumnType = LegendCellColumnType.Text;
        //    //a.Text = "test";
        //    //chartLegend.CellColumns.Add(a);
        //    chart.Legends.Add(chartLegend);


        //    //Create the chart area
        //    var chartArea = new ChartArea();
        //    chartArea.Name = "ChartArea1";
        //    chartArea.AxisY.Minimum = 0;
        //    chartArea.AxisY.Title = "Arrivals On Green";
        //    chartArea.AxisY.TitleFont = new Font(FontFamily.GenericSansSerif, 20);
        //    chartArea.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;
        //    chartArea.AxisY.LabelStyle.Font = new Font(FontFamily.GenericSansSerif, 20);


        //    chartArea.AxisX.Minimum = 0;
        //    chartArea.AxisX.Title = "Adjustment(seconds)";
        //    chartArea.AxisX.TitleFont = new Font(FontFamily.GenericSansSerif, 20);
        //    chartArea.AxisX.Interval = 10;
        //    chartArea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
        //    chartArea.AxisX.LabelStyle.Font = new Font(FontFamily.GenericSansSerif, 20);

        //    chart.ChartAreas.Add(chartArea);

        //    //Add the line series
        //    var lineSeries = new Series();
        //    lineSeries.ChartType = SeriesChartType.Line;
        //    lineSeries.Color = Color.Black;
        //    lineSeries.Name = "Total AOG";
        //    lineSeries.XValueType = ChartValueType.Int32;
        //    lineSeries.BorderWidth = 5;
        //    chart.Series.Add(lineSeries);

        //    foreach (var d in ResultsGraph)
        //        chart.Series["Total AOG"].Points.AddXY(
        //            d.Key,
        //            d.Value);

        //    //Add the line series
        //    var downstreamLineSeries = new Series();
        //    downstreamLineSeries.ChartType = SeriesChartType.Line;
        //    downstreamLineSeries.Color = Color.Blue;
        //    downstreamLineSeries.Name = "Downstream AOG";
        //    downstreamLineSeries.XValueType = ChartValueType.Int32;
        //    downstreamLineSeries.BorderWidth = 3;
        //    chart.Series.Add(downstreamLineSeries);

        //    foreach (var d in DownstreamResultsGraph)
        //        chart.Series["Downstream AOG"].Points.AddXY(
        //            d.Key,
        //            d.Value);

        //    //Add the line series
        //    var upstreamLineSeries = new Series();
        //    upstreamLineSeries.ChartType = SeriesChartType.Line;
        //    upstreamLineSeries.Color = Color.Green;
        //    upstreamLineSeries.Name = "Upstream AOG";
        //    upstreamLineSeries.XValueType = ChartValueType.Int32;
        //    upstreamLineSeries.BorderWidth = 3;
        //    chart.Series.Add(upstreamLineSeries);

        //    foreach (var d in UpstreamResultsGraph)
        //        chart.Series["Upstream AOG"].Points.AddXY(
        //            d.Key,
        //            d.Value);

        //    var chartName = "LinkPivot-" + UpstreamLocationApproach.SignalID + UpstreamLocationApproach.DirectionType.Abbreviation +
        //                    DownstreamLocationApproach.SignalID + DownstreamLocationApproach.DirectionType.Abbreviation +
        //                    DateTime.Now.Day +
        //                    DateTime.Now.Hour +
        //                    DateTime.Now.Minute +
        //                    DateTime.Now.Second +
        //                    ".jpg";
        //    var settingsRepository = MOE.Common.Models.Repositories.ApplicationSettingsRepositoryFactory.Create();
        //    var settings = settingsRepository.GetGeneralSettings();
        //    chart.SaveImage(settings.ImagePath + chartName,
        //        ChartImageFormat.Jpeg);
        //    ResultChartLocation = settings.ImageUrl + chartName;
        //}


        ///// <summary>
        /////     Creates a pcd chart specific to the Link Pivot
        ///// </summary>
        ///// <param name="sp"></param>
        ///// <param name="startDate"></param>
        ///// <param name="endDate"></param>
        ///// <param name="location"></param>
        ///// <param name="chartNameSuffix"></param>
        ///// <param name="chartLocation"></param>
        ///// <returns></returns>
        //private string CreateChart(SignalPhase sp, DateTime startDate, DateOnly endDate, string location,
        //    string chartNameSuffix, string chartLocation)
        //{
        //    var chart = new Chart();
        //    //Display the PDC chart
        //    chart = GetNewChart(startDate, endDate, sp.Approach.SignalID, sp.Approach.ProtectedPhaseNumber,
        //        sp.Approach.DirectionType.Description,
        //        location, sp.Approach.IsProtectedPhaseOverlap, 150, 2000, false, 2);

        //    AddDataToChart(chart, sp, startDate, false, true);

        //    //Create the File Name
        //    var chartName = "LinkPivot-" +
        //                    sp.Approach.SignalID +
        //                    "-" +
        //                    sp.Approach.ProtectedPhaseNumber +
        //                    "-" +
        //                    startDate.Year +
        //                    startDate.ToString("MM") +
        //                    startDate.ToString("dd") +
        //                    startDate.ToString("HH") +
        //                    startDate.ToString("mm") +
        //                    "-" +
        //                    endDate.Year +
        //                    endDate.ToString("MM") +
        //                    endDate.ToString("dd") +
        //                    endDate.ToString("HH") +
        //                    endDate.ToString("mm-") +
        //                    chartNameSuffix +
        //                    ".jpg";


        //    //Save an image of the chart
        //    chart.SaveImage(chartLocation + chartName, ChartImageFormat.Jpeg);
        //    return chartName;
        //}

        ///// <summary>
        /////     Gets a new chart for the pcd Diagram
        ///// </summary>
        ///// <param name="graphStartDate"></param>
        ///// <param name="graphEndDate"></param>
        ///// <param name="signalId"></param>
        ///// <param name="phase"></param>
        ///// <param name="direction"></param>
        ///// <param name="location"></param>
        ///// <returns></returns>
        //private Chart GetNewChart(DateOnly graphStartDate, DateTime graphEndDate, string signalId,
        //    int phase, string direction, string location, bool isOverlap, double y1AxisMaximum,
        //    double y2AxisMaximum, bool showVolume, int dotSize)
        //{
        //    var chart = new Chart();
        //    var extendedDirection = string.Empty;
        //    var movementType = "Phase";
        //    if (isOverlap)
        //        movementType = "Overlap";


        //    //Gets direction for the title
        //    switch (direction)
        //    {
        //        case "SB":
        //            extendedDirection = "Southbound";
        //            break;
        //        case "NB":
        //            extendedDirection = "Northbound";
        //            break;
        //        default:
        //            extendedDirection = direction;
        //            break;
        //    }

        //    //Set the chart properties
        //    ChartFactory.SetImageProperties(chart);

        //    //Set the chart title
        //    var title = new Title();
        //    title.Text = location + "Signal " + signalId + " "
        //                 + movementType + ": " + phase +
        //                 " " + extendedDirection + "\n" + graphStartDate.ToString("f") +
        //                 " - " + graphEndDate.ToString("f");
        //    title.Font = new Font(FontFamily.GenericSansSerif, 20);
        //    chart.Titles.Add(title);

        //    //Create the chart legend
        //    //Legend chartLegend = new Legend();
        //    //chartLegend.Name = "MainLegend";
        //    ////chartLegend.LegendStyle = LegendStyle.Table;
        //    //chartLegend.Docking = Docking.Left;
        //    //chartLegend.CustomItems.Add(Color.Blue, "AoG - Arrival On Green");
        //    //chartLegend.CustomItems.Add(Color.Blue, "GT - Green Time");
        //    //chartLegend.CustomItems.Add(Color.Maroon, "PR - Platoon Ratio");
        //    ////LegendCellColumn a = new LegendCellColumn();
        //    ////a.ColumnType = LegendCellColumnType.Text;
        //    ////a.Text = "test";
        //    ////chartLegend.CellColumns.Add(a);
        //    //chart.Legends.Add(chartLegend);


        //    //Create the chart area
        //    var chartArea = new ChartArea();
        //    chartArea.Name = "ChartArea1";
        //    chartArea.AxisY.Maximum = y1AxisMaximum;
        //    chartArea.AxisY.Minimum = 0;
        //    chartArea.AxisY.Title = "Cycle Time (Seconds) ";
        //    chartArea.AxisY.TitleFont = new Font(FontFamily.GenericSansSerif, 20);
        //    chartArea.AxisY.LabelAutoFitStyle = LabelAutoFitStyles.None;
        //    chartArea.AxisY.LabelStyle.Font = new Font(FontFamily.GenericSansSerif, 20);

        //    if (showVolume)
        //    {
        //        chartArea.AxisY2.Enabled = AxisEnabled.True;
        //        chartArea.AxisY2.MajorTickMark.Enabled = true;
        //        chartArea.AxisY2.MajorGrid.Enabled = false;
        //        chartArea.AxisY2.IntervalType = DateTimeIntervalType.Number;
        //        chartArea.AxisY2.Interval = 500;
        //        chartArea.AxisY2.Maximum = y2AxisMaximum;
        //        chartArea.AxisY2.Title = "Volume Per Hour ";
        //    }

        //    chartArea.AxisX.Title = "Time (Hour of Day)";
        //    chartArea.AxisX.TitleFont = new Font(FontFamily.GenericSansSerif, 20);
        //    chartArea.AxisX.Interval = 1;
        //    chartArea.AxisX.IntervalType = DateTimeIntervalType.Hours;
        //    chartArea.AxisX.LabelStyle.Format = "HH";
        //    chartArea.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
        //    chartArea.AxisX.LabelStyle.Font = new Font(FontFamily.GenericSansSerif, 20);

        //    //chartArea.AxisX.Minimum = 0;

        //    chartArea.AxisX2.Enabled = AxisEnabled.True;
        //    chartArea.AxisX2.MajorTickMark.Enabled = true;
        //    chartArea.AxisX2.IntervalType = DateTimeIntervalType.Hours;
        //    chartArea.AxisX2.LabelStyle.Format = "HH";
        //    chartArea.AxisX2.LabelAutoFitStyle = LabelAutoFitStyles.None;
        //    chartArea.AxisX2.Interval = 1;
        //    chartArea.AxisX2.LabelStyle.Font = new Font(FontFamily.GenericSansSerif, 20);
        //    //chartArea.AxisX.Minimum = 0;

        //    chart.ChartAreas.Add(chartArea);

        //    //Add the point series
        //    var pointSeries = new Series();
        //    pointSeries.ChartType = SeriesChartType.Point;
        //    pointSeries.Color = Color.Black;
        //    pointSeries.Name = "Detector Activation";
        //    pointSeries.XValueType = ChartValueType.DateOnly;
        //    pointSeries.MarkerSize = dotSize;
        //    chart.Series.Add(pointSeries);

        //    //Add the green series
        //    var greenSeries = new Series();
        //    greenSeries.ChartType = SeriesChartType.Line;
        //    greenSeries.Color = Color.DarkGreen;
        //    greenSeries.Name = "Change to Green";
        //    greenSeries.XValueType = ChartValueType.DateTime;
        //    greenSeries.BorderWidth = 1;
        //    chart.Series.Add(greenSeries);

        //    //Add the yellow series
        //    var yellowSeries = new Series();
        //    yellowSeries.ChartType = SeriesChartType.Line;
        //    yellowSeries.Color = Color.Yellow;
        //    yellowSeries.Name = "Change to Yellow";
        //    yellowSeries.XValueType = ChartValueType.DateTime;
        //    chart.Series.Add(yellowSeries);

        //    //Add the red series
        //    var redSeries = new Series();
        //    redSeries.ChartType = SeriesChartType.Line;
        //    redSeries.Color = Color.Red;
        //    redSeries.Name = "Change to Red";
        //    redSeries.XValueType = ChartValueType.DateTime;
        //    chart.Series.Add(redSeries);

        //    //Add the red series
        //    var volumeSeries = new Series();
        //    volumeSeries.ChartType = SeriesChartType.Line;
        //    volumeSeries.Color = Color.Black;
        //    volumeSeries.Name = "Volume Per Hour";
        //    volumeSeries.XValueType = ChartValueType.DateTime;
        //    volumeSeries.YAxisType = AxisType.Secondary;
        //    chart.Series.Add(volumeSeries);


        //    //Add points at the start and and of the x axis to ensure
        //    //the graph covers the entire period selected by the user
        //    //whether there is data or not
        //    chart.Series["Detector Activation"].Points.AddXY(graphStartDate, 0);
        //    chart.Series["Detector Activation"].Points.AddXY(graphEndDate, 0);
        //    return chart;
        //}

        ///// <summary>
        /////     Adds data points to a graph with the series GreenLine, YellowLine, Redline
        /////     and Points already added.
        ///// </summary>
        ///// <param name="chart"></param>
        ///// <param name="signalPhase"></param>
        ///// <param name="startDate"></param>
        //private void AddDataToChart(Chart chart, SignalPhase signalPhase, DateTime startDate, bool showVolume,
        //    bool showArrivalOnGreen)
        //{
        //    decimal totalDetectorHits = 0;
        //    decimal totalOnGreenArrivals = 0;
        //    decimal percentArrivalOnGreen = 0;
        //    foreach (var cycle in signalPhase.Cycles)
        //    {
        //        chart.Series["Change to Green"].Points.AddXY(
        //            //pcd.StartTime,
        //            cycle.GreenEvent,
        //            cycle.GreenLineY);
        //        chart.Series["Change to Yellow"].Points.AddXY(
        //            //pcd.StartTime,
        //            cycle.YellowEvent,
        //            cycle.YellowLineY);
        //        chart.Series["Change to Red"].Points.AddXY(
        //            //pcd.StartTime, 
        //            cycle.EndTime,
        //            cycle.RedLineY);
        //        totalDetectorHits += cycle.DetectorEvents.Count;
        //        foreach (var detectorPoint in cycle.DetectorEvents)
        //        {
        //            chart.Series["Detector Activation"].Points.AddXY(
        //                //pcd.StartTime, 
        //                detectorPoint.TimeStamp,
        //                detectorPoint.YPoint);
        //            if (detectorPoint.YPoint > cycle.GreenLineY && detectorPoint.YPoint < cycle.RedLineY)
        //                totalOnGreenArrivals++;
        //        }
        //    }

        //    if (showVolume)
        //        foreach (var v in signalPhase.Volume.Items)
        //            chart.Series["Volume Per Hour"].Points.AddXY(v.XAxis, v.YAxis);

        //    //if arrivals on green is selected add the data to the chart
        //    if (showArrivalOnGreen)
        //    {
        //        if (totalDetectorHits > 0)
        //            percentArrivalOnGreen = totalOnGreenArrivals / totalDetectorHits * 100;
        //        else
        //            percentArrivalOnGreen = 0;
        //        var title = new Title();
        //        title.Text = Math.Round(percentArrivalOnGreen) + "% AoG";
        //        title.Font = new Font(FontFamily.GenericSansSerif, 20);
        //        chart.Titles.Add(title);
        //        SetPlanStrips(signalPhase.Plans, chart, startDate);
        //    }
        //}


        ///// <summary>
        /////     Adds plan strips to the chart
        ///// </summary>
        ///// <param name="planCollection"></param>
        ///// <param name="chart"></param>
        ///// <param name="graphStartDate"></param>
        //protected void SetPlanStrips(List<PlanPcd> planCollection, Chart chart, DateTime graphStartDate)
        //{
        //    var backGroundColor = 1;
        //    foreach (var plan in planCollection)
        //    {
        //        var stripline = new StripLine();
        //        //Creates alternating backcolor to distinguish the plans
        //        if (backGroundColor % 2 == 0)
        //            stripline.BackColor = Color.FromArgb(120, Color.LightGray);
        //        else
        //            stripline.BackColor = Color.FromArgb(120, Color.LightBlue);

        //        //Set the stripline properties
        //        stripline.IntervalOffset = (plan.StartTime - graphStartDate).TotalHours;
        //        stripline.IntervalOffsetType = DateTimeIntervalType.Hours;
        //        stripline.Interval = 1;
        //        stripline.IntervalType = DateTimeIntervalType.Days;
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

        //        Plannumberlabel.ForeColor = Color.Black;
        //        Plannumberlabel.RowIndex = 3;
        //        chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(Plannumberlabel);

        //        var aogLabel = new CustomLabel();
        //        aogLabel.FromPosition = plan.StartTime.ToOADate();
        //        aogLabel.ToPosition = plan.EndTime.ToOADate();
        //        aogLabel.Text = plan.PercentArrivalOnGreen + "% AoG\n" +
        //                        plan.PercentGreenTime + "% GT";

        //        aogLabel.LabelMark = LabelMarkStyle.LineSideMark;
        //        aogLabel.ForeColor = Color.Blue;
        //        aogLabel.RowIndex = 2;
        //        chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(aogLabel);

        //        var statisticlabel = new CustomLabel();
        //        statisticlabel.FromPosition = plan.StartTime.ToOADate();
        //        statisticlabel.ToPosition = plan.EndTime.ToOADate();
        //        statisticlabel.Text =
        //            plan.PlatoonRatio + " PR";
        //        statisticlabel.ForeColor = Color.Maroon;
        //        statisticlabel.RowIndex = 1;
        //        chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(statisticlabel);

        //        //CustomLabel PlatoonRatiolabel = new CustomLabel();
        //        //PercentGreenlabel.FromPosition = plan.StartTime.ToOADate();
        //        //PercentGreenlabel.ToPosition = plan.EndTime.ToOADate();
        //        //PercentGreenlabel.Text = plan.PlatoonRatio.ToString() + " PR";
        //        //PercentGreenlabel.ForeColor = Color.Black;
        //        //PercentGreenlabel.RowIndex = 1;
        //        //chart.ChartAreas["ChartArea1"].AxisX2.CustomLabels.Add(PercentGreenlabel);

        //        //Change the background color counter for alternating color
        //        backGroundColor++;
        //    }
        //}
    }
}
