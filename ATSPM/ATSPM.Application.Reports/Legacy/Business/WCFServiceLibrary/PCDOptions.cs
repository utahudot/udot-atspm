using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Legacy.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class PCDOptions
    {


        public PCDOptions(string signalID, DateTime startDate, DateTime endDate, double yAxisMax, double y2AxisMax,
            int binSize, int dotSize, bool showPlanStatistics, bool showVolumes, int metricTypeID,
            bool showArrivalsOnGreen)
        {

            SignalId = signalID;
            SelectedBinSize = binSize;
            SelectedDotSize = dotSize;
            ShowPlanStatistics = showPlanStatistics;
            ShowVolumes = showVolumes;
            ShowArrivalsOnGreen = showArrivalsOnGreen;
            MetricTypeId = 6;
            StartDate = startDate;
            EndDate = endDate;
        }

        public PCDOptions()
        {
            VolumeBinSizeList = new List<int>() { 5, 15 };
            DotSizeList = new List<DotSizeItem>();
            DotSizeList.Add(new DotSizeItem(1, "Small"));
            DotSizeList.Add(new DotSizeItem(2, "Large"));
            MetricTypeId = 6;
            ShowArrivalsOnGreen = true;
        }

        [Required]
        [Display(Name = "Volume Bin Size")]
        [DataMember]
        public int SelectedBinSize { get; set; }

        public List<int> VolumeBinSizeList { get; set; }

        [Required]
        [Display(Name = "Dot Size")]
        [DataMember]
        public int SelectedDotSize { get; set; }

        [Required]
        [Display(Name = "Line Size")]
        [DataMember]
        public int SelectedLineSize { get; set; }

        public List<DotSizeItem> DotSizeList { get; set; }

        [DataMember]
        [Display(Name = "Show Plans")]
        public bool ShowPlanStatistics { get; set; }

        [DataMember]
        [Display(Name = "Show Volumes")]
        public bool ShowVolumes { get; set; }

        [DataMember] public bool ShowArrivalsOnGreen { get; set; }
        public int MetricTypeId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public Signal Signal { get; set; }
        public string SignalId { get; private set; }

        //public override List<string> CreateMetric()
        //{
        //    base.CreateMetric();
        //    var signalRepository = SignalsRepositoryFactory.Create();
        //    Signal = signalRepository.GetVersionOfSignalByDate(SignalId, StartDate);
        //    MetricTypeId = 6;
        //    var chart = new Chart();
        //    var metricApproaches = Signal.GetApproachesForSignalThatSupportMetric(MetricTypeId);
        //    if (metricApproaches.Count > 0)
        //        foreach (var approach in metricApproaches)
        //        {
        //            var signalPhase = new SignalPhase(StartDate, EndDate, approach, ShowVolumes, SelectedBinSize,
        //                MetricTypeId, false);
        //            chart = GetNewChart();
        //            chart.ChartAreas[0].AxisX.Minimum = signalPhase.Cycles.Any()? signalPhase.Cycles.First().StartTime.ToOADate():StartDate.ToOADate();
        //            chart.ChartAreas[0].AxisX.Maximum = signalPhase.Cycles.Any() ? signalPhase.Cycles.Last().EndTime.ToOADate():EndDate.ToOADate();
        //            AddDataToChart(chart, signalPhase);
        //            var chartName = CreateFileName();
        //            chart.ImageLocation = MetricFileLocation + chartName;
        //            chart.SaveImage(MetricFileLocation + chartName, ChartImageFormat.Jpeg);
        //            ReturnList.Add(MetricWebPath + chartName);
        //        }

        //    return ReturnList;
        //}






        //    void AddDataToChart(Chart chart, SignalPhaseService signalPhase)
        //    {
        //        double totalDetectorHits = 0;
        //        double totalOnGreenArrivals = 0;
        //        foreach (var cycle in signalPhase.Cycles)
        //        {
        //            totalOnGreenArrivals += AddCycleToChart(chart, cycle);
        //            totalDetectorHits += cycle.DetectorEvents.Count;
        //        }

        //        if (ShowVolumes)
        //            AddVolumeToChart(chart, signalPhase.Volume);
        //        if (ShowArrivalsOnGreen)
        //            AddArrivalOnGreen(chart, totalOnGreenArrivals, totalDetectorHits, signalPhase);
        //        if (ShowPlanStatistics)
        //            SetPlanStrips(signalPhase.Plans, chart);
        //    }

        //    void AddArrivalOnGreen(Chart chart, double totalOnGreenArrivals, double totalDetectorHits,
        //        SignalPhaseService signalPhase)
        //    {
        //        double percentArrivalOnGreen = 0;
        //        if (totalDetectorHits > 0)
        //            percentArrivalOnGreen = totalOnGreenArrivals / totalDetectorHits * 100;
        //        var statistics = new Dictionary<string, string>();
        //        statistics.Add("AoG", Math.Round(percentArrivalOnGreen) + "%");
        //        SetChartTitle(chart, signalPhase, statistics);
        //    }

        //    void AddVolumeToChart(Chart chart, VolumeCollection volumeCollection)
        //    {
        //        foreach (var v in volumeCollection.Items)
        //            chart.Series["Volume Per Hour"].Points.AddXY(v.XAxis, v.YAxis);
        //    }

        //    double AddCycleToChart(Chart chart, CyclePcd cycle)
        //    {
        //        chart.Series["Change to Green"].Points.AddXY(cycle.GreenEvent, cycle.GreenLineY);
        //        chart.Series["Change to Yellow"].Points.AddXY(cycle.YellowEvent, cycle.YellowLineY);
        //        chart.Series["Change to Red"].Points.AddXY(cycle.EndTime, cycle.RedLineY);
        //        foreach (var detectorPoint in cycle.DetectorEvents)
        //        {
        //            chart.Series["Detector Activation"].Points.AddXY(
        //                detectorPoint.TimeStamp,
        //                detectorPoint.YPoint);
        //        }

        //    return cycle.TotalArrivalOnGreen;
        //}

    }
}
