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
    public static class AtspmExtensions
    {
        public static IReadOnlyList<T> GetPeakVolumes<T>(this IEnumerable<T> volumes, int chunks) where T : VolumeBase
        {
            return volumes.Where((w, i) => i <= volumes.Count() - chunks)
                .Select((s, i) => volumes.Skip(i).Take(chunks))
                .Aggregate((a, b) => a.Sum(s => s.DetectorCount) >= b.Sum(s => s.DetectorCount) ? a : b).ToList();
        }
    }

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

    public class TotalVolume : VolumeBase
    {
        public Volume Primary { get; set; }
        public Volume Opposing { get; set; }

        public int DetectorCount => Primary?.DetectorCount + Opposing?.DetectorCount ?? 0;

        public override bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        public override string ToString()
        {
            return $"{Start}***{Primary} --- {Opposing} --- {DetectorCount}***{End}";
        }
    }

    public class ApproachVolumeResult
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public int PrimaryTotalVolume { get; set; }
        public int PrimaryPeakVolume { get; set; }
        public double PrimaryPHF { get; set; }
        public double PrimaryDFactor { get; set; }
        public double PrimaryKFactor { get; set; }

        public int OpposingTotalVolume { get; set; }
        public int OpposingPeakVolume { get; set; }
        public double OpposingPHF { get; set; }
        public double OpposingDFactor { get; set; }
        public double OpposingKFactor { get; set; }

        public int TotalVolume { get; set; }
        public int TotalPeakVolume { get; set; }
        public double TotalPHF { get; set; }
        public double TotalKFactor { get; set; }

        public override string ToString()
        {
            return $"{Start} " +
                $"- ptv:{PrimaryTotalVolume} - ppv:{PrimaryPeakVolume} - pphf:{PrimaryPHF} - pdf:{PrimaryDFactor} - pkf:{PrimaryKFactor} " +
                $"- otv:{OpposingTotalVolume} - opv:{OpposingPeakVolume} - ophf:{OpposingPHF} - odf:{OpposingDFactor} - okf:{OpposingKFactor} " +
                $"- tv:{TotalVolume} - tpv:{TotalPeakVolume} - tphf:{TotalPHF} - tkf:{TotalKFactor} " +
                $"- {End}";
        }
    }

    public class TotalVolumes : Timeline<TotalVolume>
    {
        public TotalVolumes(TimelineOptions options) : base(options) { }

        public TotalVolumes(Timeline<TotalVolume> collection) : base(collection) { }

        //public TotalVolumes(TimelineOptions options, IEnumerable<TotalVolume> collection) : base(options, collection) { }

        public int DetectorCount => this.Sum(s => s.DetectorCount);

        public override bool InRange(DateTime time)
        {
            return time >= Start && time < End;
        }

        public override string ToString()
        {
            return $"{Start}***{DetectorCount}***{End}";
        }
    }

    public abstract class VolumeBase : StartEndRange
    {
        public int DetectorCount { get; set; }
    }

    public class Volume : VolumeBase
    {
        public int Phase { get; set; }
        public DirectionTypes Direction { get; set; }
        //public int DetectorCount { get; set; }

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

    public class CalculateTotalVolumes : TransformManyProcessStepBase<IEnumerable<CorrectedDetectorEvent>, TotalVolumes>
    {
        private readonly TimelineOptions _options;
        
        public CalculateTotalVolumes(TimelineOptions options, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) 
        {
            _options = options;
        }

        protected override Task<IEnumerable<TotalVolumes>> Process(IEnumerable<CorrectedDetectorEvent> input, CancellationToken cancelToken = default)
        {
            var result = new List<TotalVolumes>();

            var test = input.GroupBy(g => g.Detector.Approach);

            foreach (var t in test)
            {
                var total = new TotalVolumes(_options);

                var c = new OpposingDirection(t.Key.DirectionTypeId);

                var o = test.Where(w => w.Key.DirectionTypeId == c).FirstOrDefault();

                total.ForEach(f =>
                {
                    f.Primary = new Volume()
                    {
                        Start = f.Start,
                        End = f.End,
                        Direction = t.Key.DirectionTypeId,
                        Phase = t.Key.ProtectedPhaseNumber,
                        DetectorCount = t.Where(w => f.InRange(w.CorrectedTimeStamp)).Count()
                    };

                    f.Opposing = new Volume()
                    {
                        Start = f.Start,
                        End = f.End,
                        Direction = o.Key.DirectionTypeId,
                        Phase = o.Key.ProtectedPhaseNumber,
                        DetectorCount = o.Where(w => f.InRange(w.CorrectedTimeStamp)).Count()
                    };
                });

                result.Add(total);

                foreach (var a in total)
                {
                    Console.WriteLine($"p: {a.Primary}");
                    Console.WriteLine($"o: {a.Opposing}");
                }
            }

            return Task.FromResult<IEnumerable<TotalVolumes>>(result);
        }
    }

    public class GenerateApproachVolumeResults : TransformProcessStepBase<TotalVolumes, ApproachVolumeResult>
    {
        private readonly TimelineOptions _options;

        public GenerateApproachVolumeResults(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<ApproachVolumeResult> Process(TotalVolumes input, CancellationToken cancelToken = default)
        {
            var chunks = 60 / input.Size;

            var peakA = input.Select(s => s.Primary).GetPeakVolumes(chunks).ToList();
            var o = input.Where(w => peakA.Contains(w.Primary)).Select(s => s.Opposing).Sum(s => s.DetectorCount);

            var peakB = input.Select(s => s.Opposing).GetPeakVolumes(chunks);
            var p = input.Where(w => peakB.Contains(w.Opposing)).Select(s => s.Primary).Sum(s => s.DetectorCount);

            var peakTotal = input.GetPeakVolumes(chunks);

            var result = new ApproachVolumeResult();

            result.Start = input.Start;
            result.End = input.End;

            result.PrimaryTotalVolume = input.Select(s => s.Primary).Sum(s => s.DetectorCount);
            result.PrimaryPeakVolume = peakA.Sum(s => s.DetectorCount);
            result.PrimaryPHF = AtspmMath.PeakHourFactor(result.PrimaryPeakVolume, peakA.Max(m => m.DetectorCount), chunks);

            result.OpposingTotalVolume = input.Select(s => s.Opposing).Sum(s => s.DetectorCount);
            result.OpposingPeakVolume = peakB.Sum(s => s.DetectorCount);
            result.OpposingPHF = AtspmMath.PeakHourFactor(result.OpposingPeakVolume, peakB.Max(m => m.DetectorCount), chunks);

            result.PrimaryDFactor = AtspmMath.PeakHourDFactor(result.PrimaryPeakVolume, o);
            result.OpposingDFactor = AtspmMath.PeakHourDFactor(result.OpposingPeakVolume, p);
            result.PrimaryKFactor = AtspmMath.PeakHourKFactor(result.PrimaryTotalVolume, result.PrimaryPeakVolume, result.OpposingTotalVolume, o);
            result.OpposingKFactor = AtspmMath.PeakHourKFactor(result.OpposingTotalVolume, result.OpposingPeakVolume, result.PrimaryTotalVolume, p);

            result.TotalVolume = input.DetectorCount;
            result.TotalPeakVolume = peakTotal.Sum(s => s.DetectorCount);
            result.TotalPHF = AtspmMath.PeakHourFactor(result.TotalPeakVolume, peakTotal.Max(m => m.DetectorCount), chunks);
            result.TotalKFactor = Math.Round(Convert.ToDouble(result.TotalPeakVolume) / Convert.ToDouble(result.TotalVolume), 3);

            return Task.FromResult(result);
        }
    }

    public class ApproachVolumeWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, ApproachVolumeResult>
    {
        //protected JoinBlock<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> mergeCalculateDelayValues;

        internal GetDetectorEvents GetDetectorEvents { get; private set; }

        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public CalculateTotalVolumes CalculateTotalVolumes { get; private set; }
        public GenerateApproachVolumeResults GenerateApproachVolumeResults { get; private set; }

        public override void InstantiateSteps()
        {
            FilteredDetectorData = new();
            IdentifyandAdjustVehicleActivations = new();
            //HACK: figure this out!
            CalculateTotalVolumes = new(new TimelineOptions()
            {
                Start = DateTime.Parse("4/17/2023 8:00:0.0"),
                End = DateTime.Parse("4/17/2023 10:00:0.0"),
                Size = 15
            });
            GenerateApproachVolumeResults = new();

            GetDetectorEvents = new();
        }

        public override void AddStepsToTracker()
        {
            Steps.Add(FilteredDetectorData);
            Steps.Add(IdentifyandAdjustVehicleActivations);
            Steps.Add(CalculateTotalVolumes);
            Steps.Add(GenerateApproachVolumeResults);

            Steps.Add(GetDetectorEvents);
        }

        public override void LinkSteps()
        {
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredDetectorData.LinkTo(GetDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            GetDetectorEvents.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyandAdjustVehicleActivations.LinkTo(CalculateTotalVolumes, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTotalVolumes.LinkTo(GenerateApproachVolumeResults, new DataflowLinkOptions() { PropagateCompletion = true });
            GenerateApproachVolumeResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    //HACK: figure this out! can't do this with only one detector because you can't figure out opposing
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
