﻿using System;
using System.Runtime.Serialization;

namespace Legacy.Common.Business.WCFServiceLibrary

{
    [DataContract]
    public class PreemptServiceMetricOptions
    {
        public PreemptServiceMetricOptions(string signalId, DateTime startDate, DateTime endDate, double yAxisMax)
        {
            SignalId = signalId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public string SignalId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        //public override List<string> CreateMetric()
        //{
        //    base.CreateMetric();
        //    var returnString = new List<string>();

        //    var eventsTable = new ControllerEventLogs();

        //    eventsTable.FillforPreempt(SignalID, StartDate, EndDate);
        //    if (eventsTable.Events.Count > 0)
        //    {
        //        var psChart = new PreemptServiceMetric(this, eventsTable);
        //        var chart = psChart.ServiceChart;
        //        var chartName = CreateFileName();
        //        chart.SaveImage(MetricFileLocation + chartName, ChartImageFormat.Jpeg);
        //        returnString.Add(MetricWebPath + chartName);
        //    }
        //    return returnString;
        //}
    }
}