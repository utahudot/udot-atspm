using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;
using ATSPM.Application;
using ATSPM.Application.Analysis;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using ATSPM.EventLogUtility;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace ATSPM.EventLogUtility
{
    

    public class DetectorEvent
    {
        //public DateTime Timestamp { get; set; }
        //public int ApproachSpeed { get; set; }
        //public int DistanceFromStopBar { get; set; }
        //public double LatencyCorrection { get; set; }


        public Detector Detector { get; set; }
        public IList<ControllerEventLog> EventLogs { get; set; } = new List<ControllerEventLog>();
    }

    public class IdentifyandAdjustVehicleActivations : TransformProcessStepBase<DetectorEvent, DetectorEvent>
    {
        public IdentifyandAdjustVehicleActivations(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<DetectorEvent> Process(DetectorEvent input, CancellationToken cancelToken = default)
        {
            foreach(var log in input.EventLogs)
            {
                log.Timestamp = AtspmMath.AdjustTimeStamp(
                    log.Timestamp,
                    input.Detector.Approach?.Mph ?? 0,
                    input.Detector.DistanceFromStopBar ?? 0,
                    input.Detector.LatencyCorrection);
            }

            return Task.FromResult(input);
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
    public abstract class PreempDetailValueBase
    {
        public string SignalId { get; set; }
        public int PreemptNumber { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Seconds { get; set; }
    }


    public class DwellTimeValue : PreempDetailValueBase { }
    public class TrackClearTimeValue : PreempDetailValueBase { }
    public class TimeToServiceValue : PreempDetailValueBase { }
    public class DelayTimeValue : PreempDetailValueBase { }
    public class TimeToGateDownValue : PreempDetailValueBase { }
    public class TimeToCallMaxOutValue : PreempDetailValueBase { }

    public class PreemptDetailResult
    {
        public string SignalId { get; set; }
        public int PreemptNumber { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public IEnumerable<DwellTimeValue> DwellTimes { get; set; }
        public IEnumerable<TrackClearTimeValue> TrackClearTimes { get; set; }
        public IEnumerable<TimeToServiceValue> ServiceTimes { get; set; }
        public IEnumerable<DelayTimeValue> Delay { get; set; }
        public IEnumerable<TimeToGateDownValue> GateDownTimes { get; set; }
        public IEnumerable<TimeToCallMaxOutValue> CallMaxOutTimes { get; set; }

        //public string ChartName { get; set; }
        //public string SignalLocation { get; set; }

        //public ICollection<Plan> Plans { get; set; }

        //public ICollection<InputOn> InputOns { get; set; }
        //public ICollection<InputOff> InputOffs { get; set; }
    }

    public abstract class PreemptiveProcessBase<T> : TransformManyProcessStepBase<IEnumerable<ControllerEventLog>, IEnumerable<T>> where T : PreempDetailValueBase, new()
    {
        protected DataLoggerEnum first;
        protected DataLoggerEnum second;

        public PreemptiveProcessBase(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<IEnumerable<T>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => g.SignalId)
                .SelectMany(s => s.GroupBy(g => g.EventParam)
                .Select(s => s.TimeSpanFromConsecutiveCodes(first, second)
                .Select(s => new T()
                {
                    SignalId = (s.Item1[0].SignalId == s.Item1[1].SignalId) ? s.Item1[0].SignalId : string.Empty,
                    PreemptNumber = Convert.ToInt32(s.Item1.Average(a => a.EventParam)),
                    Start = s.Item1[0].Timestamp,
                    End = s.Item1[1].Timestamp,
                    Seconds = s.Item2
                })));





            foreach (var group in result)
            {
                Console.WriteLine($"{this.GetType().Name} {group.Count()}------------------------------------------------------------------------");
                foreach (var item in group)
                {
                    Console.WriteLine($"{item.GetType().Name} - {item.SignalId} - {item.PreemptNumber} - {item.Start} - {item.End} - {item.Seconds}");
                }
            }






            return Task.FromResult(result);
        }
    }


    public class CalculateDwellTime : PreemptiveProcessBase<DwellTimeValue>
    {
        public CalculateDwellTime(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptionBeginDwellService;
            second = DataLoggerEnum.PreemptionBeginExitInterval;
        }
    }

    public class CalculateTrackClearTime : PreemptiveProcessBase<TrackClearTimeValue>
    {
        public CalculateTrackClearTime(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptionBeginTrackClearance;
            second = DataLoggerEnum.PreemptionBeginDwellService;
        }
    }

    public class CalculateTimeToService : PreemptiveProcessBase<TimeToServiceValue>
    {
        public CalculateTimeToService(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptCallInputOn;
            second = DataLoggerEnum.PreemptionBeginDwellService;
        }
    }

    public class CalculateDelay : PreemptiveProcessBase<DelayTimeValue>
    {
        public CalculateDelay(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptCallInputOn;
            second = DataLoggerEnum.PreemptEntryStarted;
        }
    }

    public class CalculateTimeToGateDown : PreemptiveProcessBase<TimeToGateDownValue>
    {
        public CalculateTimeToGateDown(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptCallInputOn;
            second = DataLoggerEnum.PreemptGateDownInputReceived;
        }
    }

    public class CalculateTimeToCallMaxOut : PreemptiveProcessBase<TimeToCallMaxOutValue>
    {
        public CalculateTimeToCallMaxOut(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptCallInputOn;
            second = DataLoggerEnum.PreemptionMaxPresenceExceeded;
        }
    }

    public class PreemptionDetailsWorkflow
    {
        public FilteredPreemptionData FilteredPreemptionData { get; private set; }
        public CalculateDwellTime CalculateDwellTime { get; private set; }
        public CalculateTrackClearTime CalculateTrackClearTime { get; private set; }
        public CalculateTimeToService CalculateTimeToService { get; private set; }
        public CalculateDelay CalculateDelay { get; private set; }
        public CalculateTimeToGateDown CalculateTimeToGateDown { get; private set; }
        public CalculateTimeToCallMaxOut CalculateTimeToCallMaxOut { get; private set; }

        //public TransformManyBlock<IEnumerable<ControllerEventLog>, IEnumerable<ControllerEventLog>> GroupBySignal { get; private set; }
        //public TransformManyBlock<IEnumerable<ControllerEventLog>, IEnumerable<ControllerEventLog>> GroupByPreemptNumber { get; private set; }

        public void Execute(List<ControllerEventLog> logs)
        {
            FilteredPreemptionData = new FilteredPreemptionData();

            CalculateDwellTime = new();
            CalculateTrackClearTime = new();
            CalculateTimeToService = new();
            CalculateDelay = new();
            CalculateTimeToGateDown = new();
            CalculateTimeToCallMaxOut = new();

            //GroupBySignal = new TransformManyBlock<IEnumerable<ControllerEventLog>, IEnumerable<ControllerEventLog>>(l => l.GroupBy(g => g.SignalId));
            //GroupByPreemptNumber = new TransformManyBlock<IEnumerable<ControllerEventLog>, IEnumerable<ControllerEventLog>>(l => l.GroupBy(g => g.EventParam));

            var broadcast = new BroadcastBlock<IEnumerable<ControllerEventLog>>(null);


            //FilteredPreemptionData.LinkTo(GroupBySignal, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupBySignal.LinkTo(GroupByPreemptNumber, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupByPreemptNumber.LinkTo(broadcast, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(broadcast, new DataflowLinkOptions() { PropagateCompletion = true });

            broadcast.LinkTo(CalculateDwellTime, new DataflowLinkOptions() { PropagateCompletion = true });
            broadcast.LinkTo(CalculateTrackClearTime, new DataflowLinkOptions() { PropagateCompletion = true });
            broadcast.LinkTo(CalculateTimeToService, new DataflowLinkOptions() { PropagateCompletion = true });
            broadcast.LinkTo(CalculateDelay, new DataflowLinkOptions() { PropagateCompletion = true });
            broadcast.LinkTo(CalculateTimeToGateDown, new DataflowLinkOptions() { PropagateCompletion = true });
            broadcast.LinkTo(CalculateTimeToCallMaxOut, new DataflowLinkOptions() { PropagateCompletion = true });

            var joinOne = new JoinBlock<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>();
            var joinTwo = new JoinBlock<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>();
            var joinThree = new JoinBlock<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>>();

            joinOne.LinkTo(joinThree.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            joinTwo.LinkTo(joinThree.Target2, new DataflowLinkOptions() { PropagateCompletion = true });

            CalculateDwellTime.LinkTo(joinOne.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTrackClearTime.LinkTo(joinOne.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimeToService.LinkTo(joinOne.Target3, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateDelay.LinkTo(joinTwo.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimeToGateDown.LinkTo(joinTwo.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimeToCallMaxOut.LinkTo(joinTwo.Target3, new DataflowLinkOptions() { PropagateCompletion = true });

            var GeneratePreemptDetailResults = new TransformManyBlock<Tuple<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>>, PreemptDetailResult>(p =>
            {
               


                var result = new List<PreemptDetailResult>();

                var signals = p.Item1.Item1.Union(p.Item1.Item2.Union(p.Item1.Item3))
                .Union(p.Item2.Item1.Union(p.Item2.Item2.Union(p.Item2.Item3)))
                .GroupBy(g => g.SignalId);

                foreach (var signal in signals)
                {
                    Console.WriteLine($"{signal.Key}===============================================================");

                    foreach (var item in signal.GroupBy(g => g.PreemptNumber))
                    {
                        Console.WriteLine($"{item.Key}******************************************************************");

                        var test = new PreemptDetailResult()
                        {
                            SignalId = signal.Key,
                            PreemptNumber = item.Key,
                            Start = item.Min(m => m.Start),
                            End = item.Max(m => m.End),
                            DwellTimes = (IEnumerable<DwellTimeValue>)p.Item1.Item1,
                            TrackClearTimes = (IEnumerable<TrackClearTimeValue>)p.Item1.Item2,
                            ServiceTimes = (IEnumerable<TimeToServiceValue>)p.Item1.Item3,
                            Delay = (IEnumerable<DelayTimeValue>)p.Item2.Item1,
                            GateDownTimes = (IEnumerable<TimeToGateDownValue>)p.Item2.Item2,
                            CallMaxOutTimes = (IEnumerable<TimeToCallMaxOutValue>)p.Item2.Item3
                        };

                        Console.WriteLine($"{test.SignalId} - {test.PreemptNumber} - {test.Start} - {test.End} - {test.DwellTimes.Count()} - {test.TrackClearTimes.Count()} - {test.ServiceTimes.Count()} - {test.Delay.Count()} - {test.GateDownTimes.Count()} - {test.CallMaxOutTimes.Count()}");

                        result.Add(test);
                    }
                }

                //var signal = p.Item1.Item1.Union(p.Item1.Item2.Union(p.Item1.Item3))
                //.Union(p.Item2.Item1.Union(p.Item2.Item2.Union(p.Item2.Item3)))
                //.GroupBy(g => g.PreemptNumber);





                return result;
            });

            joinThree.LinkTo(GeneratePreemptDetailResults, new DataflowLinkOptions() { PropagateCompletion = true });

            var action = new ActionBlock<PreemptDetailResult>(test =>
            {


                //Console.WriteLine($"{test.SignalId} - {test.PreemptNumber} - {test.Start} - {test.End} - " + $"{test.DwellTimes.Count()} - {test.TrackClearTimes.Count()} - {test.ServiceTimes.Count()} - {test.Delay.Count()} - {test.GateDownTimes.Count()} - {test.CallMaxOutTimes.Count()}");
            });

            GeneratePreemptDetailResults.LinkTo(action, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredPreemptionData.Post(logs);
            FilteredPreemptionData.Complete();
        }
    }



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