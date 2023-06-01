using ATSPM.Application.Analysis.ApproachVolume;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Application.Enums;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.PurdueCoordination
{
    public interface ISignalPhase
    {
        string SignalId { get; set; }
        int PhaseNumber { get; set; }
    }

    public interface IPlan : IStartEndRange
    {
        string SignalId { get; set; }
        int PlanNumber { get; set; }
    }
    
    public interface ICycle : IStartEndRange
    {
        //DateTime GreenEvent { get; set; }
        //DateTime YellowEvent { get; set; }

        double TotalGreenTime { get; }
        double TotalYellowTime { get; }
        double TotalRedTime { get; }
        double TotalTime { get; }
    }

    public interface ICycleVolume : IStartEndRange //: ICycle
    {
        double TotalDelay { get; }
        double TotalVolume { get; }
    }

    public interface ICycleArrivals : ICycle, ICycleVolume //: ICycleVolume
    {
        double TotalArrivalOnGreen { get; }
        double TotalArrivalOnYellow { get; }
        double TotalArrivalOnRed { get; }

        IReadOnlyList<Vehicle> Vehicles { get; }
    }
    
    public interface ICycleRatios : ICycleArrivals
    {
        double PercentArrivalOnGreen { get; }
        double PercentArrivalOnYellow { get; }
        double PercentArrivalOnRed { get; }

        double PercentGreen { get; }
        double PercentYellow { get; }
        double PercentRed { get; }

        double PlatoonRatio { get; }

        IReadOnlyList<ICycleArrivals> ArrivalCycles { get; }
    }

    public class Plan : StartEndRange, IPlan
    {
        public string SignalId { get; set; }
        public int PlanNumber { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class PerdueCoordinationPlan : Plan, ICycleRatios
    {
        private readonly List<ICycleArrivals> _arrivalCycles = new(); 
        
        public PerdueCoordinationPlan() {}

        public IReadOnlyList<ICycleArrivals> ArrivalCycles => _arrivalCycles;

        #region ICycleArrivals

        public double TotalArrivalOnGreen => _arrivalCycles.Sum(d => d.TotalArrivalOnGreen);

        public double TotalArrivalOnYellow => _arrivalCycles.Sum(d => d.TotalArrivalOnYellow);

        public double TotalArrivalOnRed => _arrivalCycles.Sum(d => d.TotalArrivalOnRed);

        public double TotalGreenTime => _arrivalCycles.Sum(d => d.TotalGreenTime);
       
        public double TotalYellowTime => _arrivalCycles.Sum(d => d.TotalYellowTime);
        
        public double TotalRedTime => _arrivalCycles.Sum(d => d.TotalRedTime);
       
        public double TotalTime => _arrivalCycles.Sum(d => d.TotalTime);

        public IReadOnlyList<Vehicle> Vehicles => _arrivalCycles.SelectMany(s => s.Vehicles).ToList();

        #endregion

        #region ICycleRatios

        public double PercentArrivalOnGreen => TotalVolume > 0 ? Math.Round(TotalArrivalOnGreen / TotalVolume * 100) : 0;

        public double PercentArrivalOnYellow => TotalVolume > 0 ? Math.Round(TotalArrivalOnYellow / TotalVolume * 100) : 0;

        public double PercentArrivalOnRed => TotalVolume > 0 ? Math.Round(TotalArrivalOnRed / TotalVolume * 100) : 0;

        public double PercentGreen => TotalVolume > 0 ? Math.Round(TotalGreenTime / TotalTime * 100) : 0;

        public double PercentYellow => TotalVolume > 0 ? Math.Round(TotalYellowTime / TotalTime * 100) : 0;

        public double PercentRed => TotalVolume > 0 ? Math.Round(TotalRedTime / TotalTime * 100) : 0;

        public double PlatoonRatio => TotalVolume > 0 ? Math.Round(PercentArrivalOnGreen / PercentGreen, 2) : 0;

        #endregion

        #region ICycleVolume

        public double TotalDelay => _arrivalCycles.Sum(d => d.TotalDelay);
        public double TotalVolume => _arrivalCycles.Sum(d => d.TotalVolume);

        #endregion

        public bool TryAssignToPlan(ICycleArrivals arrivalCycle)
        {
            if (InRange(arrivalCycle.Start) && InRange(arrivalCycle.End))
            {
                _arrivalCycles.Add(arrivalCycle);
                return true;
            }

            return false;
        }

        public override bool InRange(DateTime time)
        {
            return time >= Start && time <= End;
        }
    }

    public class PerdueCoordinationResult : StartEndRange, ICycleRatios
    {
        public PerdueCoordinationResult() {}

        public PerdueCoordinationResult(IEnumerable<PerdueCoordinationPlan> plans)
        {
            Plans = plans.ToList();
        }

        public IReadOnlyList<PerdueCoordinationPlan> Plans { get; set; } = new List<PerdueCoordinationPlan>();

        #region ICycleRatios

        public double PercentArrivalOnGreen => TotalVolume > 0 ? Math.Round(TotalArrivalOnGreen / TotalVolume * 100) : 0;

        public double PercentArrivalOnYellow => TotalVolume > 0 ? Math.Round(TotalArrivalOnYellow / TotalVolume * 100) : 0;

        public double PercentArrivalOnRed => TotalVolume > 0 ? Math.Round(TotalArrivalOnRed / TotalVolume * 100) : 0;

        public double PercentGreen => TotalVolume > 0 ? Math.Round(TotalGreenTime / TotalTime * 100) : 0;

        public double PercentYellow => TotalVolume > 0 ? Math.Round(TotalYellowTime / TotalTime * 100) : 0;

        public double PercentRed => TotalVolume > 0 ? Math.Round(TotalRedTime / TotalTime * 100) : 0;

        public double PlatoonRatio => TotalVolume > 0 ? Math.Round(PercentArrivalOnGreen / PercentGreen, 2) : 0;

        public IReadOnlyList<ICycleArrivals> ArrivalCycles => Plans.SelectMany(s => s.ArrivalCycles).ToList();

        public double TotalArrivalOnGreen => Plans.Sum(d => d.TotalArrivalOnGreen);

        public double TotalArrivalOnYellow => Plans.Sum(d => d.TotalArrivalOnYellow);

        public double TotalArrivalOnRed => Plans.Sum(d => d.TotalArrivalOnRed);

        public IReadOnlyList<Vehicle> Vehicles => Plans.SelectMany(s => s.Vehicles).ToList();

        public double TotalGreenTime => Plans.Sum(d => d.TotalGreenTime);

        public double TotalYellowTime => Plans.Sum(d => d.TotalYellowTime);

        public double TotalRedTime => Plans.Sum(d => d.TotalRedTime);

        public double TotalTime => Plans.Sum(d => d.TotalTime);

        public double TotalDelay => Plans.Sum(d => d.TotalDelay);

        public double TotalVolume => Plans.Sum(d => d.TotalVolume);

        #endregion
    }

    public class CycleArrivals : StartEndRange, ICycleArrivals, ISignalPhase
    {
        private readonly ICycle _cycle = new RedToRedCycle();
        
        public CycleArrivals() { }

        public CycleArrivals(ICycle cycle)
        {
            _cycle = cycle;

            if (cycle is ISignalPhase sp)
            {
                SignalId = sp.SignalId;
                PhaseNumber = sp.PhaseNumber;
            }
        }

        #region ICycle

        public string SignalId { get; set; }

        public int PhaseNumber { get; set; }

        public double TotalGreenTime => _cycle.TotalGreenTime;
        public double TotalYellowTime => _cycle.TotalYellowTime;
        public double TotalRedTime => _cycle.TotalRedTime;
        public double TotalTime => _cycle.TotalTime;

        #endregion

        #region ICycleArrivals

        public double TotalArrivalOnGreen => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnGreen);
        public double TotalArrivalOnYellow => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnYellow);
        public double TotalArrivalOnRed => Vehicles.Count(d => d.ArrivalType == ArrivalType.ArrivalOnRed);

        public double TotalDelay => Vehicles.Sum(d => d.Delay);
        public double TotalVolume => Vehicles.Count(d => InRange(d.CorrectedTimeStamp));

        public IReadOnlyList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        #endregion

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    public class CalculateVehicleArrivals : TransformProcessStepBase<Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>>, IReadOnlyList<CycleArrivals>>
    {
        public CalculateVehicleArrivals(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IReadOnlyList<CycleArrivals>> Process(Tuple<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item2.Select(s => new CycleArrivals(s)
            {
                Vehicles = input.Item1.Where(w => w.Detector.Approach?.Signal?.SignalId == s.SignalId && s.InRange(w.CorrectedTimeStamp))
                .Select(v => new Vehicle(v, s))
                .ToList()
            }).ToList();

            return Task.FromResult<IReadOnlyList<CycleArrivals>>(result);
        }
    }

    public class CaclulatePhaseTotals : TransformManyProcessStepBase<IReadOnlyList<CycleArrivals>, PerdueCoordinationResult>
    {
        public CaclulatePhaseTotals(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<PerdueCoordinationResult>> Process(IReadOnlyList<CycleArrivals> input, CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => g.PhaseNumber)
                .Select(s => new PerdueCoordinationResult()
                {
                    Cycles = s.ToList()
                });

            return Task.FromResult(result);
        }
    }


    public class PurdueCoordinationWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, PerdueCoordinationResult>
    {
        protected JoinBlock<IEnumerable<CorrectedDetectorEvent>, IEnumerable<RedToRedCycle>> mergeVehicleArrivals;

        internal GetDetectorEvents GetDetectorEvents { get; private set; }

        public FilteredPhaseIntervalChanges FilteredPhaseIntervalChanges { get; private set; }

        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public CreateRedToRedCycles CreateRedToRedCycles { get; private set; }
        public IdentifyandAdjustVehicleActivations IdentifyandAdjustVehicleActivations { get; private set; }
        public CalculateVehicleArrivals CalculateVehicleArrivals { get; private set; }
        public CaclulatePhaseTotals CaclulatePhaseTotals { get; private set; }

        public override void InstantiateSteps()
        {
            FilteredPhaseIntervalChanges = new();
            FilteredDetectorData = new();
            CreateRedToRedCycles = new();
            IdentifyandAdjustVehicleActivations = new();
            mergeVehicleArrivals = new();
            CalculateVehicleArrivals = new();
            CaclulatePhaseTotals = new();

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
            CaclulatePhaseTotals.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
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
