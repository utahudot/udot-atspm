using System.Runtime.Serialization;
using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.NetStandardToolkit.Common;

namespace Utah.Udot.Atspm.Data.Models.MeasureOptions
{
    public interface IBinSize
    {
        public int BinSize { get; set; }
    }
    
    public abstract class MeasureOptionsBase : StartEndRange, ILocationLayer
    {
        //private DateTime _start;
        //private DateTime _end;

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        //public DateTime Start
        //{
        //    get
        //    {
        //        return _start;
        //    }
        //    set
        //    {
        //        _start = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        //    }
        //}
        //public DateTime End
        //{
        //    get
        //    {
        //        return _end;
        //    }
        //    set
        //    {
        //        _end = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        //    }
        //}
    }

    public class ApproachDelayOptions : MeasureOptionsBase, IBinSize
    {
        public int BinSize { get; set; }
        public bool GetPermissivePhase { get; set; }
        public bool GetVolume { get; set; } = true;

        public override string? ToString()
        {
            return $"{LocationIdentifier} - {Start} - {End} - {BinSize} - {GetPermissivePhase} - {GetVolume}";
        }
        //public string LocationIdentifier { get; set; }
    }

    public class ApproachSpeedOptions : MeasureOptionsBase, IBinSize
    {
        public int BinSize { get; set; }
        public int MetricTypeId { get; } = 10;
        //public string LocationIdentifier { get; set; }
    }

    public class ApproachVolumeOptions : MeasureOptionsBase, IBinSize
    {
        public int BinSize { get; set; }
        public bool ShowDirectionalSplits { get; set; }
        public bool GetVolume { get; set; } = true;
        public bool ShowNbEbVolume { get; set; }
        public bool ShowSbWbVolume { get; set; }
        public bool ShowTMCDetection { get; set; }
        public bool ShowAdvanceDetection { get; set; }
        public int MetricTypeId { get; internal set; } = 7;
        //public string LocationIdentifier { get; set; }
    }

    public class ArrivalOnRedOptions : MeasureOptionsBase, IBinSize
    {
        public int BinSize { get; set; }
        public bool GetPermissivePhase { get; set; }
        //public string LocationIdentifier { get; set; }
    }


    public class TimeSpaceDiagramOptions : MeasureOptionsBase
    {
        public int RouteId { get; set; }
        public int? SpeedLimit { get; set; }

        public double ExtendStartStopSearch { get; set; }
        public bool ShowAllLanesInfo { get; set; }
       // public string LocationIdentifier { get; set; }
    }

    public class GreenTimeUtilizationOptions : MeasureOptionsBase
    {
        public int MetricTypeId { get; set; } = 36;
        public int XAxisBinSize { get; set; }
        public int YAxisBinSize { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class LeftTurnGapAnalysisOptions : MeasureOptionsBase, IBinSize
    {
        public const int EVENT_GREEN = 1;
        public const int EVENT_RED = 10;
        public const int EVENT_DET = 81;


        public double Gap1Min { get; set; } = 0;
        public double Gap1Max { get; set; } = 1;
        public double Gap2Min { get; set; } = 1;
        public double Gap2Max { get; set; } = 3.3;
        public double Gap3Min { get; set; } = 3.3;
        public double Gap3Max { get; set; } = 3.7;
        public double Gap4Min { get; set; } = 3.7;
        public double? Gap4Max { get; set; }
        public double? Gap5Min { get; set; }
        public double? Gap5Max { get; set; }
        public double? Gap6Min { get; set; }
        public double? Gap6Max { get; set; }
        public double? Gap7Min { get; set; }
        public double? Gap7Max { get; set; }
        public double? Gap8Min { get; set; }
        public double? Gap8Max { get; set; }
        public double? Gap9Min { get; set; }
        public double? Gap9Max { get; set; }
        public double? Gap10Min { get; set; }
        public double? Gap10Max { get; set; }
        public double? SumDurationGap1 { get; set; }
        public double? SumDurationGap2 { get; set; }
        public double? SumDurationGap3 { get; set; }
        public double TrendLineGapThreshold { get; set; } = 7.4;
        public int BinSize { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class GapDurationOptions : MeasureOptionsBase
    {
        public int ApproachId { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public int[] DaysOfWeek { get; set; }
       // public string LocationIdentifier { get; set; }
    }

    public class LeftTurnGapDataCheckOptions : MeasureOptionsBase
    {
        public int ApproachId { get; set; }
        public int VolumePerHourThreshold { get; set; }
        public double GapOutThreshold { get; set; }
        public double PedestrianThreshold { get; set; }
        public int[] DaysOfWeek { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class LeftTurnSplitFailOptions : MeasureOptionsBase
    {
        public int ApproachId { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public int[] DaysOfWeek { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class PeakHourOptions : MeasureOptionsBase
    {
        public int ApproachId { get; set; }
        public int[] DaysOfWeek { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class PedActuationOptions : MeasureOptionsBase
    {
        public int ApproachId { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public int[] DaysOfWeek { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class VolumeOptions : MeasureOptionsBase
    {
        public int ApproachId { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public int[] DaysOfWeek { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    [DataContract]
    public class PedDelayOptions : MeasureOptionsBase
    {
        public int TimeBuffer { get; set; }
        public bool ShowPedBeginWalk { get; set; }
        public bool ShowCycleLength { get; set; }
        public bool ShowPercentDelay { get; set; }
        public bool ShowPedRecall { get; set; }
        public int PedRecallThreshold { get; set; }
       // public string LocationIdentifier { get; set; }
    }

    public class PurduePhaseTerminationOptions : MeasureOptionsBase
    {
        public int SelectedConsecutiveCount { get; set; }
        public int SelectedPhaseNumber { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class PreemptDetailOptions : MeasureOptionsBase
    {
        //public string LocationIdentifier { get; set; }
    }

    public class PreemptServiceOptions : MeasureOptionsBase
    {
        //public string LocationIdentifier { get; set; }
    }

    public class PreemptServiceRequestOptions : MeasureOptionsBase
    {
        //public string LocationIdentifier { get; set; }

    }

    public class PurdueCoordinationDiagramOptions : MeasureOptionsBase, IBinSize
    {
        public int BinSize { get; set; }
        public bool GetVolume { get; set; } = true;
        public bool ShowPlanStatistics { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class RampMeteringOptions : MeasureOptionsBase
    {
        public bool CombineLanes { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class SplitFailOptions : MeasureOptionsBase
    {
        public int FirstSecondsOfRed { get; set; }
        public int MetricTypeId { get; set; } = 12;
        public bool GetPermissivePhase { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class SplitMonitorOptions : MeasureOptionsBase
    {
        public int PercentileSplit { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class TimingAndActuationsOptions : MeasureOptionsBase
    {
        //public string LocationIdentifier { get; set; }
        public List<short> GlobalEventCodesList { get; set; }
        public List<short> GlobalEventParamsList { get; set; }
        public List<short> PhaseEventCodesList { get; set; }
    }

    public class TurningMovementCountsOptions : MeasureOptionsBase, IBinSize
    {
        public int BinSize { get; set; }
        public int MetricTypeId { get; internal set; } = 5;
        //public string LocationIdentifier { get; set; }
    }

    public class WaitTimeOptions : MeasureOptionsBase, IBinSize
    {
        public int BinSize { get; set; }
        //public string LocationIdentifier { get; set; }
    }

    public class YellowRedActivationsOptions : MeasureOptionsBase
    {
        public double SevereLevelSeconds { get; set; }
        public int MetricTypeId { get; set; } = 11;
        //public string LocationIdentifier { get; set; }
    }

    public class TransitSignalPriorityOptions : MeasureOptionsBase

    {
        public IEnumerable<LocationPhases> LocationsAndPhases { get; set; }
        public IEnumerable<DateTime> Dates { get; set; }
    }

    public class LocationPhases
    {
        public string LocationIdentifier { get; set; }
        public List<int> DesignatedPhases { get; set; }
    }
}
