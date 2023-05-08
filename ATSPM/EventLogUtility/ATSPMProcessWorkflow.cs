using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;
using ATSPM.Application;
using ATSPM.Application.Analysis;
using ATSPM.Application.Exceptions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using ATSPM.EventLogUtility;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace ATSPM.EventLogUtility
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

    public enum ArrivalType
    {
        Unknown,
        ArrivalOnGreen,
        ArrivalOnYellow,
        ArrivalOnRed
    }

    public class Vehicle
    {
        public string SignalId { get; set; }

        //public double YPoint { get; private set; }

        //public DateTime StartOfCycle { get; }

        //The actual time of the detector activation
        public DateTime TimeStamp { get; set; }

        //public DateTime YellowEvent { get; set; }

        //public DateTime GreenEvent { get; set; }

        //public DateTime RedEvent { get; set; }

        public double Delay => ArrivalType == ArrivalType.ArrivalOnRed ? (RedToRedCycle?.GreenEvent - TimeStamp).Value.TotalSeconds : 0;

        public int DetectorChannel { get; set; }

        public ArrivalType ArrivalType
        {
            get
            {
                if (TimeStamp < RedToRedCycle?.GreenEvent)
                {
                    return ArrivalType.ArrivalOnRed;
                }

                else if (TimeStamp >= RedToRedCycle?.GreenEvent && TimeStamp < RedToRedCycle?.YellowEvent)
                {
                    return ArrivalType.ArrivalOnGreen;
                }

                else if (TimeStamp >= RedToRedCycle?.YellowEvent)
                {
                    return ArrivalType.ArrivalOnYellow;
                }

                return ArrivalType.Unknown;
            }
        }

        public Detector Detector { get; set; }

        public RedToRedCycle RedToRedCycle { get; set; }

        public override string? ToString()
        {
            return $"{DetectorChannel}-{TimeStamp:yyyy-MM-dd'T'HH:mm:ss.f}-{ArrivalType}-{Delay}";
        }
    }

    public class DetectorEvent
    {
        //public DateTime Timestamp { get; set; }
        //public int ApproachSpeed { get; set; }
        //public int DistanceFromStopBar { get; set; }
        //public double LatencyCorrection { get; set; }


        public Detector Detector { get; set; }
        public IList<ControllerEventLog> EventLogs { get; set; } = new List<ControllerEventLog>();
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
                            //VehicleEvents = input.Where(w => w.EventCode == 82 && w.SignalId == signal.Key && w.Timestamp >= s.ElementAt(0).Timestamp && w.Timestamp <= s.ElementAt(3).Timestamp).ToList()
                        });

                    result.Add(group);
                }
            }

            return Task.FromResult<IEnumerable<IEnumerable<RedToRedCycle>>>(result);
        }
    }

    public class IdentifyandAdjustVehicleActivations : TransformManyProcessStepBase<Tuple<Detector, IEnumerable<ControllerEventLog>>, IEnumerable<Vehicle>>
    {
        public IdentifyandAdjustVehicleActivations(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<IEnumerable<Vehicle>>> Process(Tuple<Detector, IEnumerable<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            //TODO: I DONT KNOW IF THIS MATCHES UP OR NOT
            var result = input.Item2.Where(l => l.EventCode == (int)DataLoggerEnum.DetectorOn && l.EventParam == input.Item1.DetChannel)
                .GroupBy(g => g.SignalId).Select(s => s.ToList()).ToList()
                .Select(i => i.Select(s =>
                new Vehicle()
                {
                    SignalId = s.SignalId,
                    TimeStamp = s.Timestamp = AtspmMath.AdjustTimeStamp(s.Timestamp, input.Item1.Approach?.Mph ?? 0, input.Item1.DistanceFromStopBar ?? 0, input.Item1.LatencyCorrection),
                    DetectorChannel = s.EventParam,
                    Detector = input.Item1
                }));

            return Task.FromResult<IEnumerable<IEnumerable<Vehicle>>>(result);
        }
    }



    public class ApproachDelayWorkflow
    {
        public void Execute()
        {
            var FilteredDetectorData = new FilteredDetectorData();
            var FilteredPhaseIntervalChanges = new FilteredPhaseIntervalChanges();




        }
    }




    
    #region PreemptiveStuff
    

    

    #endregion











    //public class IdentifyUnknownTerminationTypes : ProcessStepBase<List<ControllerEventLog>, List<ControllerEventLog>>
    //{
    //    public IdentifyUnknownTerminationTypes(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions){}

    //    public override Task<List<ControllerEventLog>> Process(List<ControllerEventLog> input, CancellationToken cancelToken = default)
    //    {
    //        return Task.FromResult(input.Where(p => p.EventCode == (int)DataLoggerEnum.PhaseGreenTermination).ToList());
    //    }
    //}

    //public class IdentifyTerminationTypesAndTimes : ProcessStepBase<List<ControllerEventLog>, List<ControllerEventLog>>
    //{
    //    public IdentifyTerminationTypesAndTimes(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

    //    public override Task<List<ControllerEventLog>> Process(List<ControllerEventLog> input, CancellationToken cancelToken = default)
    //    {
    //        return Task.FromResult(input);
    //    }
    //}

    //public class IdentifyAllTerminationTypesandTimes : ProcessStepBase<Tuple<List<ControllerEventLog>, List<ControllerEventLog>>, List<ControllerEventLog>>
    //{
    //    public IdentifyAllTerminationTypesandTimes(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

    //    public override Task<List<ControllerEventLog>> Process(Tuple<List<ControllerEventLog>, List<ControllerEventLog>> input, CancellationToken cancelToken = default)
    //    {
    //        return Task.FromResult(input.Item1.Union(input.Item2).ToList());
    //    }
    //}

    //public class IdentifyPedActivity : ProcessStepBase<List<ControllerEventLog>, List<ControllerEventLog>>
    //{
    //    public IdentifyPedActivity(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

    //    public override Task<List<ControllerEventLog>> Process(List<ControllerEventLog> input, CancellationToken cancelToken = default)
    //    {
    //        return Task.FromResult(input);
    //    }
    //}

    //public class PhaseTerminationMeasureInformation : ProcessStepBase<Tuple<List<ControllerEventLog>, List<ControllerEventLog>>, AnalysisPhase>
    //{
    //    public PhaseTerminationMeasureInformation(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

    //    public override Task<AnalysisPhase> Process(Tuple<List<ControllerEventLog>, List<ControllerEventLog>> input, CancellationToken cancelToken = default)
    //    {
    //        return Task.FromResult(new AnalysisPhase() { Data = input.Item1.Count });
    //    }
    //}

    //public class AnalysisPhase
    //{
    //    public int Data { get; set; }
    //}


    //public class PhaseTerminationProcess : ServiceObjectBase
    //{
    //    public PhaseTerminationProcess() 
    //    {
    //    }

    //    public override void Initialize()
    //    {
    //        //Controller event logs EC7
    //        var FilteredTerminationStatus = new BroadcastBlock<List<ControllerEventLog>>(d => d.Where(i => i.EventCode == (int)DataLoggerEnum.PhaseGreenTermination).ToList());

    //        //Controller event logs EC 4,5,6
    //        var FilteredTerminationsData = new BroadcastBlock<List<ControllerEventLog>>(null);

    //        //Controller event logs EC 21,23
    //        var FilteredPedPhases = new BufferBlock<List<ControllerEventLog>>();

    //        //Identify unknown termination types
    //        //var IdentifyUnknownTerminationTypes = new TransformBlock<List<ControllerEventLog>, List<ControllerEventLog>>(d => IdentifyUnknownTerminationTypesProcess(d));
    //        var IdentifyUnknownTerminationTypes = new IdentifyUnknownTerminationTypes();

    //        //Identify termination types and times
    //        //var IdentifyTerminationTypesAndTimes = new TransformBlock<List<ControllerEventLog>, List<ControllerEventLog>>(d => IdentifyTerminationTypesAndTimesProcess(d));
    //        var IdentifyTerminationTypesAndTimes = new IdentifyTerminationTypesAndTimes();

    //        //Identify all termination types and times
    //        var IdentifyAllTerminationTypesandTimesJoin = new JoinBlock<List<ControllerEventLog>, List<ControllerEventLog>>();
    //        //var IdentifyAllTerminationTypesandTimes = new TransformBlock<Tuple<List<ControllerEventLog>, List<ControllerEventLog>>, List<ControllerEventLog>>(d => IdentifyAllTerminationTypesandTimesProcess(d));
    //        var IdentifyAllTerminationTypesandTimes = new IdentifyAllTerminationTypesandTimes();

    //        //Identify Ped Activity
    //        //var IdentifyPedActivity = new TransformBlock<List<ControllerEventLog>, List<ControllerEventLog>>(d => IdentifyPedActivityProcess(d));
    //        var IdentifyPedActivity = new IdentifyPedActivity();

    //        //Phase termination measure information
    //        var PhaseTerminationMeasureInformationJoin = new JoinBlock<List<ControllerEventLog>, List<ControllerEventLog>>();
    //        //var PhaseTerminationMeasureInformation = new TransformBlock<Tuple<List<ControllerEventLog>, List<ControllerEventLog>>, AnalysisPhase>(d => PhaseTerminationMeasureInformationProcess(d));
    //        var PhaseTerminationMeasureInformation = new PhaseTerminationMeasureInformation();

    //        //link FilteredTerminationStatus
    //        FilteredTerminationStatus.LinkTo(IdentifyUnknownTerminationTypes);

    //        //Link FilteredTerminationsData
    //        FilteredTerminationsData.LinkTo(IdentifyTerminationTypesAndTimes);

    //        //link IdentifyAllTerminationTypesandTimes
    //        IdentifyUnknownTerminationTypes.LinkTo(IdentifyAllTerminationTypesandTimesJoin.Target1);
    //        IdentifyTerminationTypesAndTimes.LinkTo(IdentifyAllTerminationTypesandTimesJoin.Target2);
    //        IdentifyAllTerminationTypesandTimesJoin.LinkTo(IdentifyAllTerminationTypesandTimes);

    //        //Link FilteredPedPhases
    //        FilteredPedPhases.LinkTo(IdentifyPedActivity);

    //        //link PhaseTerminationMeasureInformation
    //        IdentifyAllTerminationTypesandTimes.LinkTo(PhaseTerminationMeasureInformationJoin.Target1);
    //        IdentifyPedActivity.LinkTo(PhaseTerminationMeasureInformationJoin.Target2);
    //        PhaseTerminationMeasureInformationJoin.LinkTo(PhaseTerminationMeasureInformation);


    //        ActionBlock<AnalysisPhase> StartCharting = new ActionBlock<AnalysisPhase>(a => Console.WriteLine($"------------------------------------------------------AnalysisPhase Data: {a.Data}"));


    //        PhaseTerminationMeasureInformation.LinkTo(StartCharting);

    //        base.Initialize();
    //    }


    //}
}