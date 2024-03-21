using ATSPM.Application.Business.Aggregation.FilterExtensions;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.Aggregation
{
    public class AggregationOptions
    {
        public List<string> LocationIdentifiers { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string BinSize { get; set; }
        public AggregationType AggregationType { get; set; }
        public DetectionTypes DetectionType { get; set; }
        public int SeriesWidth { get; set; }
        public int DataType { get; set; }

        public TimeOptions TimeOptions { get; set; }


        public AggregationCalculationType SelectedAggregationType { get; set; }


        public XAxisType SelectedXAxisType { get; set; }


        public SeriesType SelectedSeries { get; set; }


        public List<FilterSignal> FilterSignals { get; set; } = new List<FilterSignal>();


        public List<FilterDirection> FilterDirections { get; set; }


        public List<FilterMovement> FilterMovements { get; set; }

        public List<Location> Signals { get; set; } = new List<Location>();

        public string YAxisTitle { get; }

        public bool ShowEventCount { get; set; }

        public string ChartTitle
        {
            get
            {
                string chartTitle;
                chartTitle = "AggregationChart\n";
                chartTitle += TimeOptions.Start.ToString();
                if (TimeOptions.End > TimeOptions.Start)
                    chartTitle += " to " + TimeOptions.End + "\n";
                if (TimeOptions.DaysOfWeek != null)
                    foreach (var dayOfWeek in TimeOptions.DaysOfWeek)
                        chartTitle += dayOfWeek + " ";
                if (TimeOptions.TimeOfDayStartHour != null && TimeOptions.TimeOfDayStartMinute != null &&
                    TimeOptions.TimeOfDayEndHour != null && TimeOptions.TimeOfDayEndMinute != null)
                    chartTitle += "Limited to: " +
                                  new TimeSpan(0, TimeOptions.TimeOfDayStartHour.Value,
                                      TimeOptions.TimeOfDayStartMinute.Value, 0) + " to " + new TimeSpan(0,
                                      TimeOptions.TimeOfDayEndHour.Value,
                                      TimeOptions.TimeOfDayEndMinute.Value, 0) + "\n";
                chartTitle += TimeOptions.SelectedBinSize + " bins ";
                chartTitle += SelectedXAxisType + " Aggregation ";
                chartTitle += SelectedAggregationType.ToString();
                return chartTitle;
            }
        }

    }
}