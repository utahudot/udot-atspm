using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;

namespace Legacy.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class PedsVsFailuresOptions
    {
        public PedsVsFailuresOptions(
            string signalID,
            DateTime startDate,
            DateTime endDate,
            double yAxisMax,
            double yAxisMin,
            int metricTypeID,
            Dictionary<DateTime, double> percentCyclesWithPeds,
            Dictionary<DateTime, double> percentCyclesWithSplitFails)
        {
            SignalId = signalID;
            StartDate = startDate;
            EndDate = endDate;
            MetricTypeId = metricTypeID;
            PercentPedsList = percentCyclesWithPeds;
            PercentFailuresList = percentCyclesWithSplitFails;
        }
        

        public int MetricTypeId { get; private set; }
        [Required]
        [DataMember]
        [Display(Name = "Percent of Cycles w/ Peds")]
        public Dictionary<DateTime, double> PercentPedsList { get; internal set; }
        [Required]
        [DataMember]
        [Display(Name = "Percent Cycles w/ Split Failure")]
        public Dictionary<DateTime, double> PercentFailuresList { get; internal set; }
        public string SignalId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }


       

       

      

        //protected void AddDataToChart(Chart chart)
        //{
        //    if (PercentPedsList != null)
        //    {
        //        foreach (var bucket in PercentPedsList)
        //        {
        //            chart.Series["% of Cycles w/ Peds"].Points.AddXY(bucket.Key.ToOADate(), bucket.Value * 100);
        //        }
        //    }
        //    if (PercentFailuresList != null)
        //    {
        //        foreach (var bucket in PercentFailuresList)
        //        {
        //            chart.Series["% of Cycles w/ Split Failure"].Points.AddXY(bucket.Key.ToOADate(), bucket.Value * 100);
        //        }
        //    }
        //}

    }
}