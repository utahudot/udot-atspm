using ATSPM.Application.Business.Aggregation.FilterExtensions;
using ATSPM.Application.Business.Bins;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.Aggregation
{
    public class AggregationOptions
    {
        public List<string> LocationIdentifiers { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public AggregationType AggregationType { get; set; }
        public DetectionTypes? DetectionType { get; set; }
        public int DataType { get; set; }
        public TimeOptions TimeOptions { get; set; }
        public AggregationCalculationType SelectedAggregationType { get; set; }
        public XAxisType SelectedXAxisType { get; set; }
        public SeriesType SelectedSeries { get; set; }
        public List<FilterSignal> Locations { get; set; } = new List<FilterSignal>();
        public List<FilterDirection> FilterDirections { get; set; }
        public List<FilterMovement> FilterMovements { get; set; }
    }
}