using ATSPM.Application.Analysis.ApproachVolume;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.PurdueCoordination
{
    public class PerdueCoordinationDiagramResult
    {
        //public PerdueCoordinationDiagramResult(
        //    string chartName,
        //    string signalId,
        //    string signalLocation,
        //    int phaseNumber,
        //    string phaseDescription,
        //    DateTime start,
        //    DateTime end,
        //    int totalOnGreenEvents,
        //    int totalDetectorHits,
        //    double percentArrivalOnGreen,
        //    ICollection<PerdueCoordinationPlan> plans,
        //    ICollection<VolumePerHour> volumePerHour,
        //    ICollection<CyclePcd> cycles)
        //{
        //    ChartName = chartName;
        //    SignalId = signalId;
        //    SignalLocation = signalLocation;
        //    PhaseNumber = phaseNumber;
        //    PhaseDescription = phaseDescription;
        //    Start = start;
        //    End = end;
        //    TotalOnGreenEvents = totalOnGreenEvents;
        //    TotalDetectorHits = totalDetectorHits;
        //    PercentArrivalOnGreen = percentArrivalOnGreen;
        //    Plans = plans;
        //    VolumePerHour = volumePerHour;
        //    Cycles = cycles;
        //}

        //public string ChartName { get; internal set; }
        public string SignalId { get; internal set; }
        //public string SignalLocation { get; internal set; }
        public int PhaseNumber { get; internal set; }
        //public string PhaseDescription { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }
        public int TotalOnGreenEvents { get; internal set; }
        public int TotalDetectorHits { get; internal set; }
        public double PercentArrivalOnGreen { get; internal set; }
        //public ICollection<PerdueCoordinationPlan> Plans { get; internal set; }
        //public ICollection<VolumePerHour> VolumePerHour { get; internal set; }
        //public ICollection<CyclePcd> Cycles { get; set; }

    }

    public class SignalPhase : StartEndRange
    {
        //public SignalPhase(
        //    VolumeCollection volume,
        //    List<PerdueCoordinationPlan> plans,
        //    List<CyclePcd> cycles,
        //    List<ControllerEventLog> detectorEvents,
        //    Approach approach,
        //    DateTime startDate,
        //    DateTime endDate
        //    )
        //{
        //    Volume = volume;
        //    Plans = plans;
        //    Cycles = cycles;
        //    DetectorEvents = detectorEvents;
        //    Approach = approach;
        //    StartDate = startDate;
        //    EndDate = endDate;
        //}

        //public SignalPhase()
        //{
        //}

        public DateTime Start { get; set; }
        public DateTime End { get; set; }


        //public VolumeCollection Volume { get; private set; }
        //public List<PerdueCoordinationPlan> Plans { get; private set; }
        public IReadOnlyList<CyclePcd> Cycles { get; set; }
        //private List<ControllerEventLog> DetectorEvents { get; set; }
        //public Approach Approach { get; }

        public double AvgDelay => TotalDelay / TotalVolume;

        public double PercentArrivalOnGreen => TotalVolume > 0 ? Math.Round(TotalArrivalOnGreen / TotalVolume * 100) : 0;
        //    get
        //    {
        //        if (TotalVolume > 0)
        //            return Math.Round(TotalArrivalOnGreen / TotalVolume * 100);
        //        return 0;
        //    }
        //}

        public double PercentGreen => TotalVolume > 0 ? Math.Round(TotalGreenTime / TotalTime * 100) : 0;
        //{
        //    get
        //    {
        //        if (TotalTime > 0)
        //            return Math.Round(TotalGreenTime / TotalTime * 100);
        //        return 0;
        //    }
        //}

        public double PlatoonRatio => TotalVolume > 0 ? Math.Round(PercentArrivalOnGreen / PercentGreen, 2) : 0;
        //{
        //    get
        //    {
        //        if (TotalVolume > 0)
        //            return Math.Round(PercentArrivalOnGreen / PercentGreen, 2);
        //        return 0;
        //    }
        //}

        public double TotalArrivalOnGreen => Cycles.Sum(d => d.TotalArrivalOnGreen);
        public double TotalArrivalOnYellow => Cycles.Sum(d => d.TotalArrivalOnYellow);
        public double TotalArrivalOnRed => Cycles.Sum(d => d.TotalArrivalOnRed);
        public double TotalDelay => Cycles.Sum(d => d.TotalDelay);
        public double TotalVolume => Cycles.Sum(d => d.TotalVolume);
        public double TotalGreenTime => Cycles.Sum(d => d.TotalGreenTime);
        public double TotalYellowTime => Cycles.Sum(d => d.TotalYellowTime);
        public double TotalRedTime => Cycles.Sum(d => d.TotalRedTime);
        public double TotalTime => Cycles.Sum(d => d.TotalTime);

        public override string ToString()
        {
            return $"{this.GetType().Name}: Start: {Start:yyyy-MM-dd'T'HH:mm:ss.f} End: {End:yyyy-MM-dd'T'HH:mm:ss.f}" +
                $" - {TotalArrivalOnGreen} - {TotalArrivalOnYellow} - {TotalArrivalOnRed} - {TotalVolume}" +
                $" - {TotalGreenTime} - {TotalYellowTime} - {TotalRedTime} - {TotalTime}" +
                $" - {PercentArrivalOnGreen} - {PercentGreen} - {PlatoonRatio}";
        }

        //public void ResetVolume()
        //{
        //    Volume = null;
        //}
    }

    public class CyclePcd : RedToRedCycle
    {
        public CyclePcd() { }

        public CyclePcd(RedToRedCycle redToRedCycle)
        {
            SignalId = redToRedCycle.SignalId;
            Phase = redToRedCycle.Phase;
            Start = redToRedCycle.Start;
            End = redToRedCycle.End;
            GreenEvent = redToRedCycle.GreenEvent;
            YellowEvent = redToRedCycle.YellowEvent;
        }

        public double TotalArrivalOnGreen => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnGreen);
        public double TotalArrivalOnYellow => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnYellow);
        public double TotalArrivalOnRed => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnRed);
        public double TotalDelay => Vehicles.Sum(d => d.Delay);
        public double TotalVolume => Vehicles.Count(d => InRange(d.CorrectedTimeStamp));

        //public double TotalArrivalOnGreen { get; set; }
        //public double TotalArrivalOnYellow { get; set; }
        //public double TotalArrivalOnRed { get; set; }
        //public double TotalDelay { get; set; }
        //public double TotalVolume { get; set; }

        public IReadOnlyList<Vehicle> Vehicles { get; set; }

        public override string ToString()
        {
            return $"{this.GetType().Name}: Signal: {SignalId} Phase: {Phase} Start: {Start:yyyy-MM-dd'T'HH:mm:ss.f} Green: {GreenEvent:yyyy-MM-dd'T'HH:mm:ss.f} Yellow: {YellowEvent:yyyy-MM-dd'T'HH:mm:ss.f} End: {End:yyyy-MM-dd'T'HH:mm:ss.f}" +
                $" - {TotalRedTime} - {TotalYellowTime} - {TotalGreenTime}" +
                $" - {TotalArrivalOnGreen} - {TotalArrivalOnYellow} - {TotalArrivalOnRed} - {TotalVolume}";
        }
    }

    public class CalculateVehicleArrivals : TransformProcessStepBase<Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>, IReadOnlyList<CyclePcd>>
    {
        public CalculateVehicleArrivals(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IReadOnlyList<CyclePcd>> Process(Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item2.Select(s => new CyclePcd(s)
            {
                Vehicles = input.Item1.Where(w => w.Detector.Approach?.Signal?.SignalId == s.SignalId && s.InRange(w.CorrectedTimeStamp))
                .Select(v => new Vehicle(v, s))
                .ToList()
            }).ToList();

            foreach (var r in result)
            {
                Console.WriteLine($"result: {r}");
                //foreach (var v in r.Vehicles)
                //{
                //    Console.WriteLine($"vehicle: {v}");
                //}
            }

            return Task.FromResult<IReadOnlyList<CyclePcd>>(result);
        }
    }

    public class CaclulatePhaseTotals : TransformProcessStepBase<IReadOnlyList<CyclePcd>, IReadOnlyList<SignalPhase>>
    {
        public CaclulatePhaseTotals(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IReadOnlyList<SignalPhase>> Process(IReadOnlyList<CyclePcd> input, CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => g.Phase)
                .Select(s => new SignalPhase()
                {
                    Cycles = s.ToList()
                }).ToList();

            foreach (var r in result)
            {
                Console.WriteLine($"result: {r}");
                //foreach (var v in r.Vehicles)
                //{
                //    Console.WriteLine($"vehicle: {v}");
                //}
            }

            return Task.FromResult<IReadOnlyList<SignalPhase>>(result);
        }
    }


    public class PurdueCoordinationWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, PerdueCoordinationDiagramResult>
    {
        protected JoinBlock<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> mergeVehicleArrivals;

        internal GetDetectorEvents GetDetectorEvents { get; private set; }

        public FilteredPhaseIntervalChanges FilteredPhaseIntervalChanges { get; private set; }

        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public CreateRedToRedCycles CreateRedToRedCycles { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public CalculateVehicleArrivals CalculateVehicleArrivals { get; private set; }
        public CaclulatePhaseTotals CaclulatePhaseTotals { get; private set; }
        //public GenerateApproachDelayResults GenerateApproachDelayResults { get; private set; }

        public override void InstantiateSteps()
        {
            FilteredPhaseIntervalChanges = new();
            FilteredDetectorData = new();
            CreateRedToRedCycles = new();
            IdentifyandAdjustVehicleActivations = new();
            mergeVehicleArrivals = new();
            CalculateVehicleArrivals = new();
            CaclulatePhaseTotals = new();
            //GenerateApproachDelayResults = new();

            GetDetectorEvents = new();
        }

        public override void AddStepsToTracker()
        {
            Steps.Add(FilteredPhaseIntervalChanges);
            Steps.Add(FilteredDetectorData);
            Steps.Add(CreateRedToRedCycles);
            Steps.Add(IdentifyandAdjustVehicleActivations);
            Steps.Add(mergeVehicleArrivals);
            Steps.Add(CalculateVehicleArrivals);
            Steps.Add(CaclulatePhaseTotals);
            //Steps.Add(GenerateApproachDelayResults);

            Steps.Add(GetDetectorEvents);
        }

        public override void LinkSteps()
        {
            Input.LinkTo(FilteredPhaseIntervalChanges, new DataflowLinkOptions() { PropagateCompletion = true });
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredPhaseIntervalChanges.LinkTo(CreateRedToRedCycles, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredDetectorData.LinkTo(GetDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            GetDetectorEvents.LinkTo(IdentifyandAdjustVehicleActivations, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyandAdjustVehicleActivations.LinkTo(mergeVehicleArrivals.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CreateRedToRedCycles.LinkTo(mergeVehicleArrivals.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            mergeVehicleArrivals.LinkTo(CalculateVehicleArrivals, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateVehicleArrivals.LinkTo(CaclulatePhaseTotals, new DataflowLinkOptions() { PropagateCompletion = true });
            //AssignCyclesToVehicles.LinkTo(GenerateApproachDelayResults, new DataflowLinkOptions() { PropagateCompletion = true });
            //GenerateApproachDelayResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
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
                        LatencyCorrection = 0,
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
