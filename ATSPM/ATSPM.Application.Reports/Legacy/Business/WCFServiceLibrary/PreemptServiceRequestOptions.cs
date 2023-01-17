using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Legacy.Common.Business.Preempt;

namespace Legacy.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class PreemptServiceRequestOptions : MetricOptions
    {
        public PreemptServiceRequestOptions(string signalId, DateTime startDate, DateTime endDate)
        {
            SignalId = signalId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public override List<string> CreateMetric()
        {
            base.CreateMetric();
            var returnList = new List<string>();
            var eventsTable = new ControllerEventLogService();
            eventsTable.FillforPreempt(SignalId, StartDate, EndDate);

            var psrChart = new PreemptRequestChart(this, eventsTable);
            var chart = psrChart.PreemptServiceRequestChart;
            var chartName = CreateFileName();
            chart.SaveImage(MetricFileLocation + chartName, ChartImageFormat.Jpeg);
            returnList.Add(MetricWebPath + chartName);
            return returnList;
        }
    }
}