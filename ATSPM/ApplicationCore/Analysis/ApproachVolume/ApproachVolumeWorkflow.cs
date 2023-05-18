using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static System.Net.Mime.MediaTypeNames;

namespace ATSPM.Application.Analysis.ApproachVolume
{
    public readonly struct OpposingDirection
    {
        private readonly DirectionTypes _direction;

        public OpposingDirection(DirectionTypes direction) => (_direction) = (direction);

        public static implicit operator DirectionTypes(OpposingDirection o) => o._direction switch
        {
            DirectionTypes.EB => DirectionTypes.WB,
            DirectionTypes.WB => DirectionTypes.EB,
            DirectionTypes.NB => DirectionTypes.SB,
            DirectionTypes.SB => DirectionTypes.NB,
            DirectionTypes.NE => DirectionTypes.SW,
            DirectionTypes.NW => DirectionTypes.NE,
            DirectionTypes.SE => DirectionTypes.NW,
            DirectionTypes.SW => DirectionTypes.NE,
            _ => DirectionTypes.NA,
        };

        public static explicit operator OpposingDirection(DirectionTypes d) => d switch
        {
            DirectionTypes.EB => new OpposingDirection(DirectionTypes.WB),
            DirectionTypes.WB => new OpposingDirection(DirectionTypes.EB),
            DirectionTypes.NB => new OpposingDirection(DirectionTypes.SB),
            DirectionTypes.SB => new OpposingDirection(DirectionTypes.NB),
            DirectionTypes.NE => new OpposingDirection(DirectionTypes.SW),
            DirectionTypes.NW => new OpposingDirection(DirectionTypes.NE),
            DirectionTypes.SE => new OpposingDirection(DirectionTypes.NW),
            DirectionTypes.SW => new OpposingDirection(DirectionTypes.NE),
            _ => new OpposingDirection(DirectionTypes.NA),
        };
    }

    public class TotalVolume: Volume
    {
        public Volume Primary { get; set; }
        public Volume Oppossing { get; set; }

        public override bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        public override string ToString()
        {
            return $"{Start}***{Primary} --- {Oppossing} --- {DetectorCount}***{End}";
        }
    }

    public class PeakVolume : Timeline<Volume>
    {
        public PeakVolume(TimelineOptions options) : base(options) { }

        public PeakVolume(IEnumerable<Volume> collection) : base(collection) { }

        public PeakVolume(TimelineOptions options, IEnumerable<Volume> collection) : base(options, collection) { }

        public double PHF => AtspmMath.PeakHourFactor(this.Sum(s => s.DetectorCount), this.Max(m => m.DetectorCount), 60 / Size);
        public double DFactor { get; set; }
        public double KFactor { get; set; }
    }

    public class Volume : StartEndRange
    {
        public int Phase { get; set; }
        public DirectionTypes Direction { get; set; }
        public int DetectorCount { get; set; }

        //public DirectionTypes OpposingDirection => new OpposingDirection(Direction);

        public override bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        public override string ToString()
        {
            return $"{Phase} - {Direction} - {Start} - {End} - {DetectorCount}";
        }
    }

    public class CalculatePhaseVolume : TransformProcessStepBase<IEnumerable<CorrectedDetectorEvent>, IReadOnlyList<Timeline<TotalVolume>>>
    {
        private readonly TimelineOptions _options;
        
        public CalculatePhaseVolume(TimelineOptions options, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) 
        {
            _options = options;
        }

        protected override Task<IReadOnlyList<Timeline<TotalVolume>>> Process(IEnumerable<CorrectedDetectorEvent> input, CancellationToken cancelToken = default)
        {
            //var result = input.GroupBy(g => g.Detector?.Approach, (k, v) =>
            //new Timeline<Volume>().GenerateTimeFrameInMinutes(_options.Start, _options.End, _options.Bin)
            //.Select((s, i) => new Volume()
            //{
            //    Phase = k.ProtectedPhaseNumber,
            //    Direction = k.DirectionTypeId,
            //    //StartTime = s.Start,
            //    //EndTime = s.End,
            //    DetectorCount = v.Where(w => w.CorrectedTimeStamp >= s.Start && w.CorrectedTimeStamp < s.End).Count()
            //})).SelectMany(m => m)
            //.ToList();

            var result = new List<Timeline<TotalVolume>>();

            var test = input.GroupBy(g => g.Detector.Approach);

            foreach (var t in test)
            {
                //Console.WriteLine($"approach: {t.Key.ProtectedPhaseNumber}-{t.Key.DirectionTypeId}");

                var c = new OpposingDirection(t.Key.DirectionTypeId);

                var o = test.Where(w => w.Key.DirectionTypeId == c).FirstOrDefault();

                var pv = Timeline.InMinutes<Volume>(_options.Start, _options.End, _options.Size);

                //Console.WriteLine($"pv count: {pv.Count}");

                pv.ForEach(a =>
                 {
                     a.Direction = t.Key.DirectionTypeId;
                     a.Phase = t.Key.ProtectedPhaseNumber;
                     a.DetectorCount = t.Where(w => a.InRange(w.CorrectedTimeStamp)).Count();
                 });

                //foreach (var v in pv)
                //{
                //    Console.WriteLine($"pv: {v}");
                //}

                var pvp = GetPeakVolume(pv);

                Console.WriteLine($"pvp PHF: {pvp.PHF}");
                foreach (var v in pvp)
                {
                    Console.WriteLine($"pv: {v}");
                }


                var ov = Timeline.InMinutes<Volume>(_options.Start, _options.End, _options.Size);

                //Console.WriteLine($"ov count: {ov.Count}");

                ov.ForEach(a =>
                {
                    a.Direction = o.Key.DirectionTypeId;
                    a.Phase = o.Key.ProtectedPhaseNumber;
                    a.DetectorCount = o.Where(w => a.InRange(w.CorrectedTimeStamp)).Count();
                });

                //foreach (var v in ov)
                //{
                //    Console.WriteLine($"ov: {v}");
                //}

                var zip = pv.Zip(ov, (p, o) => new TotalVolume() 
                { 
                    Phase = t.Key.ProtectedPhaseNumber,
                    DetectorCount = p.DetectorCount + o.DetectorCount,
                    Primary = p, 
                    Oppossing = o, 
                    Start = o.Start, 
                    End = o.End 
                });

                var final = new Timeline<TotalVolume>(zip);

                //foreach (var f in final)
                //{
                //    Console.WriteLine($"final: {f}");
                //}

                result.Add(final);


            }

            return Task.FromResult<IReadOnlyList<Timeline<TotalVolume>>>(result);
        }

        public PeakVolume GetPeakVolume(IEnumerable<Volume> volumes)
        {
            var chunks = 60 / 15;

            var blocks = new List<List<Volume>>();

            for (int i = 0; i < chunks; i++)
            {
                blocks.Add(volumes.Skip(i).Take(chunks).ToList());

                //var test = this.Skip(i).Take(chunks).Select(s => s.DetectorCount).Aggregate((a, b) => a + b);
                // Console.WriteLine($"t: {this[i].Start} - {test}");
            }

            //var max = blocks.Max(m => blocks.Select(s => s.DetectorCount).Aggregate((a, b) => a + b));

            var group = blocks.Aggregate((a, b) => a.Sum(s => s.DetectorCount) >= b.Sum(s => s.DetectorCount) ? a : b);

            //Console.WriteLine($"t: {max}");

            foreach (var g in group)
            {
                Console.WriteLine($"g: {g}");
            }



            return new PeakVolume(group);
        }
    }

    public class ApproachVolumeWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, IEnumerable<ApproachDelayResult>>
    {
        //protected JoinBlock<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> mergeCalculateDelayValues;

        //internal GetDetectorEvents GetDetectorEvents { get; private set; }

        //public FilteredPhaseIntervalChanges FilteredPhaseIntervalChanges { get; private set; }

        //public FilteredDetectorData FilteredDetectorData { get; private set; }
        //public CreateRedToRedCycles CreateRedToRedCycles { get; private set; }
        //public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        //public CalculateApproachDelay CalculateDelayValues { get; private set; }
        //public GenerateApproachDelayResults GenerateApproachDelayResults { get; private set; }

        public override void InstantiateSteps()
        {
            //FilteredPhaseIntervalChanges = new();
            //FilteredDetectorData = new();
            //CreateRedToRedCycles = new();
            //IdentifyandAdjustVehicleActivations = new();
            //mergeCalculateDelayValues = new();
            //CalculateDelayValues = new();
            //GenerateApproachDelayResults = new();

            //GetDetectorEvents = new();
        }

        public override void AddStepsToTracker()
        {
            //Steps.Add(FilteredPhaseIntervalChanges);
            //Steps.Add(FilteredDetectorData);
            //Steps.Add(CreateRedToRedCycles);
            //Steps.Add(IdentifyandAdjustVehicleActivations);
            //Steps.Add(mergeCalculateDelayValues);
            //Steps.Add(CalculateDelayValues);
            //Steps.Add(GenerateApproachDelayResults);

            //Steps.Add(GetDetectorEvents);
        }

        public override void LinkSteps()
        {
            //Input.LinkTo(FilteredPhaseIntervalChanges, new DataflowLinkOptions() { PropagateCompletion = true });
            //Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            //FilteredPhaseIntervalChanges.LinkTo(CreateRedToRedCycles, new DataflowLinkOptions() { PropagateCompletion = true });
            //FilteredDetectorData.LinkTo(GetDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            //GetDetectorEvents.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            //IdentifyandAdjustVehicleActivations.LinkTo(mergeCalculateDelayValues.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            //CreateRedToRedCycles.LinkTo(mergeCalculateDelayValues.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            //mergeCalculateDelayValues.LinkTo(CalculateDelayValues, new DataflowLinkOptions() { PropagateCompletion = true });
            //CalculateDelayValues.LinkTo(GenerateApproachDelayResults, new DataflowLinkOptions() { PropagateCompletion = true });
            //GenerateApproachDelayResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    internal class GetDetectorEvents : TransformProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<Tuple<Detector, IEnumerable<ControllerEventLog>>>>
    {
        public GetDetectorEvents(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<Tuple<Detector, IEnumerable<ControllerEventLog>>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = input.Where(l => l.EventCode == (int)DataLoggerEnum.DetectorOn)
                .GroupBy(g => g.SignalId)
                .Select(signal => signal.AsEnumerable()
                .GroupBy(g => g.EventParam)
                    .Select(s => Tuple.Create(new Detector()
                    {
                        DetChannel = s.Key,
                        DistanceFromStopBar = 340,
                        LatencyCorrection = 1.2,
                        Approach = new Approach()
                        {
                            Mph = 45,
                            Signal = new Signal() { SignalId = signal.Key }
                        }
                    }, s.AsEnumerable())))
                .SelectMany(s => s);

            return Task.FromResult(result);
        }
    }
}
