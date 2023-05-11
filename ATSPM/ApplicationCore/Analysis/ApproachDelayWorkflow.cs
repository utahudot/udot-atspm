using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis
{
    public class ApproachDelayResult
    {
        //public string ChartName { get; }
        public string SignalId { get; set; }
        //public string SignalLocation { get; }
        public int Phase { get; set; }
        //public string PhaseDescription { get; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public double AverageDelay => Vehicles.Average(a => a.Delay);
        public double TotalDelay => Vehicles.Sum(s => s.Delay) / 3600;
        //public List<ApproachDelayPlan> Plans { get; }
        //public List<ApproachDelayDataPoint> ApproachDelayDataPoints { get; }
        //public List<ApproachDelayPerVehicleDataPoint> ApproachDelayPerVehicleDataPoints { get; }

        public List<Vehicle> Vehicles { get; set; } = new();

        public override string? ToString()
        {
            return $"Signal: {SignalId} Phase: {Phase} Start: {Start:yyyy-MM-dd'T'HH:mm:ss.f} End: {End:yyyy-MM-dd'T'HH:mm:ss.f} Avg: {AverageDelay:0.00} Total: {TotalDelay:0.0h} Vehicles: {Vehicles.Count}";
        }
    }

    public class RedToRedCycle
    {
        //public RedToRedCycle(DateTime firstRedEvent, DateTime greenEvent, DateTime yellowEvent, DateTime lastRedEvent)
        //{
        //    StartTime = firstRedEvent;
        //    GreenEvent = greenEvent;
        //    GreenLineY = (greenEvent - StartTime).TotalSeconds;
        //    YellowEvent = yellowEvent;
        //    YellowLineY = (yellowEvent - StartTime).TotalSeconds;
        //    EndTime = lastRedEvent;
        //    RedLineY = (lastRedEvent - StartTime).TotalSeconds;
        //    //PreemptCollection = new List<DetectorDataPoint>();
        //}

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        //public double GreenLineY { get; }
        //public double YellowLineY { get; }
        //public double RedLineY { get; }
        public DateTime GreenEvent { get; set; }
        public DateTime YellowEvent { get; set; }

        public string SignalId { get; set; }
        public int Phase { get; set; }

        //public ICollection<ControllerEventLog> VehicleEvents { get; set; } = new List<ControllerEventLog>();


        public double TotalGreenTime => (YellowEvent - GreenEvent).TotalSeconds;
        public double TotalYellowTime => (EndTime - YellowEvent).TotalSeconds;
        public double TotalRedTime => (GreenEvent - StartTime).TotalSeconds;
        public double TotalTime => (EndTime - StartTime).TotalSeconds;
        public double TotalGreenTimeMilliseconds => (YellowEvent - GreenEvent).TotalMilliseconds;
        public double TotalYellowTimeMilliseconds => (EndTime - YellowEvent).TotalMilliseconds;
        public double TotalRedTimeMilliseconds => (GreenEvent - StartTime).TotalMilliseconds;
        public double TotalTimeMilliseconds => (EndTime - StartTime).TotalMilliseconds;

        public override string? ToString()
        {
            return $"Signal: {SignalId} Phase: {Phase} Start: {StartTime:yyyy-MM-dd'T'HH:mm:ss.f} Green: {GreenEvent:yyyy-MM-dd'T'HH:mm:ss.f} Yellow: {YellowEvent:yyyy-MM-dd'T'HH:mm:ss.f} End: {EndTime:yyyy-MM-dd'T'HH:mm:ss.f}";
        }
    }

    public class CorrectedDetectorEvent
    {
        public string SignalId { get; set; }
        public DateTime TimeStamp { get; set; }
        public int DetChannel { get; set; }

        //public Detector Detector { get; set; }
    }

    public enum ArrivalType
    {
        Unknown,
        ArrivalOnGreen,
        ArrivalOnYellow,
        ArrivalOnRed
    }

    public class Vehicle : CorrectedDetectorEvent
    {
        public Vehicle() { }
        
        public Vehicle(CorrectedDetectorEvent detectorEvent, RedToRedCycle redToRedCycle)
        {
            if (detectorEvent.SignalId == redToRedCycle.SignalId)
            {
                SignalId = detectorEvent.SignalId;
                TimeStamp = detectorEvent.TimeStamp;
                DetChannel = detectorEvent.DetChannel;
                Phase = redToRedCycle.Phase;
                StartTime = redToRedCycle.StartTime;
                EndTime = redToRedCycle.EndTime;
                YellowEvent = redToRedCycle.YellowEvent;
                GreenEvent = redToRedCycle.GreenEvent;
            }
        }

        public int Phase { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime GreenEvent { get; set; }
        public DateTime YellowEvent { get; set; }

        public double Delay => ArrivalType == ArrivalType.ArrivalOnRed ? (GreenEvent - TimeStamp).TotalSeconds : 0;

        public ArrivalType ArrivalType
        {
            get
            {
                if (TimeStamp < GreenEvent && TimeStamp >= StartTime)
                {
                    return ArrivalType.ArrivalOnRed;
                }

                else if (TimeStamp >= GreenEvent && TimeStamp < YellowEvent)
                {
                    return ArrivalType.ArrivalOnGreen;
                }

                else if (TimeStamp >= YellowEvent && TimeStamp <= EndTime)
                {
                    return ArrivalType.ArrivalOnYellow;
                }

                return ArrivalType.Unknown;
            }
        }

        //public Detector Detector { get; set; }

        //public RedToRedCycle RedToRedCycle { get; set; }

        public override string? ToString()
        {
            return $"{DetChannel}-{TimeStamp:yyyy-MM-dd'T'HH:mm:ss.f}-{ArrivalType}-{Delay}";
        }
    }

    public class CreateRedToRedCycles : TransformManyProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<RedToRedCycle>>
    {
        public CreateRedToRedCycles(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<IEnumerable<RedToRedCycle>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = new List<IEnumerable<RedToRedCycle>>();

            var signalFilter = input.Where(l => l.EventCode == 1 || l.EventCode == 8 || l.EventCode == 9)
                .OrderBy(o => o.Timestamp)
                .GroupBy(g => g.SignalId);

            foreach (var signal in signalFilter)
            {
                foreach (var phase in signal.GroupBy(g => g.EventParam))
                {
                    var items = phase.Select(s => s).ToList();
                    var group = items
                        .Where((w, i) => i <= items.Count - 3 && w.EventCode == 9 && items[i + 1].EventCode == 1 && items[i + 2].EventCode == 8 && items[i + 3].EventCode == 9)
                        .Select((s, i) => new { s, i = items.IndexOf(s) })
                        .Select(s => items.Skip(s.i).Take(4))
                        .Select(s => new RedToRedCycle()
                        {
                            StartTime = s.ElementAt(0).Timestamp,
                            EndTime = s.ElementAt(3).Timestamp,
                            GreenEvent = s.ElementAt(1).Timestamp,
                            YellowEvent = s.ElementAt(2).Timestamp,
                            Phase = phase.Key,
                            SignalId = signal.Key
                        });

                        result.Add(group);
                }
            }

            return Task.FromResult<IEnumerable<IEnumerable<RedToRedCycle>>>(result);
        }
    }

    //HACK: this doesn't verify that all ControllerEventLogs EventParams match the Detectors DetChannel
    public class IdentifyandAdjustVehicleActivations : TransformProcessStepBase<Tuple<Detector, IEnumerable<ControllerEventLog>>, IEnumerable<CorrectedDetectorEvent>>
    {
        public IdentifyandAdjustVehicleActivations(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<CorrectedDetectorEvent>> Process(Tuple<Detector, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            //var result = input.Item2.Where(l => l.EventCode == (int)DataLoggerEnum.DetectorOn && input.Item1.Approach?.Signal?.SignalId == l.SignalId && l.EventParam == input.Item1.DetChannel)
            //    .GroupBy(g => g.SignalId).Select(s => s.ToList()).ToList()
            //    .Select(i => i.Select(s =>
            //    new Vehicle()
            //    {
            //        SignalId = s.SignalId,
            //        TimeStamp = s.Timestamp = AtspmMath.AdjustTimeStamp(s.Timestamp, input.Item1.Approach?.Mph ?? 0, input.Item1.DistanceFromStopBar ?? 0, input.Item1.LatencyCorrection),
            //        DetectorChannel = s.EventParam,
            //        Detector = input.Item1
            //    }));

            var result = input.Item2.Where(l => l.EventCode == (int)DataLoggerEnum.DetectorOn && input.Item1.Approach?.Signal?.SignalId == l.SignalId && l.EventParam == input.Item1.DetChannel)
                .Select(s =>
                new CorrectedDetectorEvent()
                {
                    SignalId = s.SignalId,
                    TimeStamp = s.Timestamp = AtspmMath.AdjustTimeStamp(s.Timestamp, input.Item1.Approach?.Mph ?? 0, input.Item1.DistanceFromStopBar ?? 0, input.Item1.LatencyCorrection),
                    DetChannel = s.EventParam
                    //Detector = input.Item1
                });

            return Task.FromResult<IEnumerable<CorrectedDetectorEvent>>(result);
        }
    }

    //HACK: this doesn't filter out different signals, phases or detchannels
    public class CalculateDelayValues : TransformProcessStepBase<Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>, IEnumerable<Vehicle>>
    {
        public CalculateDelayValues(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<Vehicle>> Process(Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> input, CancellationToken cancelToken = default)
        {
            var result = new List<Vehicle>();

            //var redToRedCycles = input.Item2.ToList();

            foreach (var v in input.Item1)
            {
                var redCycle = input.Item2?.FirstOrDefault(w => w.SignalId == v.SignalId && v.TimeStamp >= w.StartTime && v.TimeStamp <= w.EndTime);

                if (redCycle != null)
                {
                    result.Add(new Vehicle(v, redCycle));
                }
                    
            }

            return Task.FromResult<IEnumerable<Vehicle>>(result);
        }
    }

    //HACK: this doesn't filter out different signals, phases or detchannels
    public class GenerateApproachDelayResults : TransformProcessStepBase<IEnumerable<Vehicle>, ApproachDelayResult>
    {
        public GenerateApproachDelayResults(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<ApproachDelayResult> Process(IEnumerable<Vehicle> input, CancellationToken cancelToken = default)
        {
            var result = new ApproachDelayResult()
            {
                Start = input.Min(m => m.StartTime),
                End = input.Max(m => m.EndTime),
                SignalId = input.GroupBy(g => g.SignalId).FirstOrDefault().Key,
                Phase = input.GroupBy(g => g.Phase).FirstOrDefault().Key,
                Vehicles = input.ToList()
            };

            return Task.FromResult<ApproachDelayResult>(result);
        }
    }

    public class ApproachDelayWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, ApproachDelayResult>
    {
        protected JoinBlock<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> mergeCalculateDelayValues;

        protected GetDetectorEvents GetDetectorEvents { get; private set; }

        public FilteredPhaseIntervalChanges FilteredPhaseIntervalChanges { get; private set; }
        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public CreateRedToRedCycles CreateRedToRedCycles { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public CalculateDelayValues CalculateDelayValues { get; private set; }
        public GenerateApproachDelayResults GenerateApproachDelayResults { get; private set; }

        public override void Initialize()
        {
            Steps = new();

            Input = new(null);
            Output = new();

            FilteredPhaseIntervalChanges = new();
            FilteredDetectorData = new();

            CreateRedToRedCycles = new();
            IdentifyandAdjustVehicleActivations = new();
            mergeCalculateDelayValues = new();
            CalculateDelayValues = new();
            GenerateApproachDelayResults = new();

            GetDetectorEvents = new();

            Steps.Add(Input);
            Steps.Add(FilteredPhaseIntervalChanges);
            Steps.Add(FilteredDetectorData);
            Steps.Add(CreateRedToRedCycles);
            Steps.Add(IdentifyandAdjustVehicleActivations);
            Steps.Add(mergeCalculateDelayValues);
            Steps.Add(CalculateDelayValues);
            Steps.Add(GenerateApproachDelayResults);

            Steps.Add(GetDetectorEvents);

            Input.LinkTo(FilteredPhaseIntervalChanges, new DataflowLinkOptions() { PropagateCompletion = true });
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredPhaseIntervalChanges.LinkTo(CreateRedToRedCycles, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredDetectorData.LinkTo(GetDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            GetDetectorEvents.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyandAdjustVehicleActivations.LinkTo(mergeCalculateDelayValues.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CreateRedToRedCycles.LinkTo(mergeCalculateDelayValues.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            mergeCalculateDelayValues.LinkTo(CalculateDelayValues, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateDelayValues.LinkTo(GenerateApproachDelayResults, new DataflowLinkOptions() { PropagateCompletion = true });
            GenerateApproachDelayResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });

            base.Initialize();
        }
    }

    public class GetDetectorEvents : TransformManyProcessStepBase<IEnumerable<ControllerEventLog>, Tuple<Detector, IEnumerable<ControllerEventLog>>>
    {
        public GetDetectorEvents(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<Tuple<Detector, IEnumerable<ControllerEventLog>>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = input.Where(l => l.EventCode == (int)DataLoggerEnum.DetectorOn)
                .GroupBy(g => g.EventParam)
                .Select(s => s.AsEnumerable())
                .Select(s => Tuple.Create(new Detector() { DetChannel = 2, DistanceFromStopBar = 340, LatencyCorrection = 1.2, Approach = new Approach() { Mph = 45 } }, s));

            return Task.FromResult(result);
        }
    }
}
