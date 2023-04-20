using System;
using System.Collections.Generic;
using System.CommandLine;
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

        public override string ToString() => $"{GetType().Name}-{SignalId}-{PreemptNumber}-{Start}-{End}-{Seconds}";
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

        public override string? ToString() => $"{GetType().Name}-{SignalId}-{PreemptNumber}-{Start}-{End}-{DwellTimes.Count()}-{TrackClearTimes?.Count()}-{ServiceTimes?.Count()}-{Delay?.Count()}-{GateDownTimes?.Count()}-{CallMaxOutTimes?.Count()}";

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
                    Console.WriteLine($"{item}");
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

    public abstract class WorkflowBase<T1, T2> : ServiceObjectBase, IExecuteWithProgress<T1, IAsyncEnumerable<T2>, int>
    {
        public event EventHandler CanExecuteChanged;

        public List<IDataflowBlock> Steps { get; set; }

        public BufferBlock<T1> Input { get; set; }
        public BufferBlock<T2> Output { get; set; }

        #region IExecuteWithProgress

        public virtual async IAsyncEnumerable<T2> Execute(T1 parameter, IProgress<int> progress = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            if (!IsInitialized)
                BeginInit();

            if (CanExecute(parameter))
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                //logMessages.LoggerStartedMessage(DateTime.Now, parameter.Count);

                //var signalSender = new BufferBlock<Signal>(new DataflowBlockOptions() { CancellationToken = cancelToken, NameFormat = "Signal Buffer" });

                //foreach (ITargetBlock<Signal> step in Steps.Where(f => f is ITargetBlock<Signal>))
                //{
                //    signalSender.LinkTo(step, new DataflowLinkOptions() { PropagateCompletion = true });
                //}

                //Steps.Add(signalSender);

                try
                {
                    //foreach (T1 item in parameter)
                    //{
                        await Input.SendAsync(parameter);
                    //}

                    Input.Complete();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Output.Completion.ContinueWith(t =>
                    {
                        Console.WriteLine($"Output is complete!");
                        this.IsInitialized = false;
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    await Task.WhenAll(Steps.Select(s => s.Completion)).ContinueWith(t => Console.WriteLine($"All steps are complete!"));

                    //return Steps.All(t => t.Completion.IsCompletedSuccessfully);
                }
                catch (Exception e)
                {
                    //logMessages.LoggerExecutionException(new ControllerLoggerExecutionException(this, "Exception running Signal Controller Logger Service", e));
                }
                finally
                {
                    //logMessages.LoggerCompletedMessage(DateTime.Now, sw.Elapsed);
                    sw.Stop();
                }

                await foreach (var item in Output.ReceiveAllAsync(cancelToken))
                    yield return item;
            }
            else
            {
                throw new ExecuteException();
            }
        }

        public virtual bool CanExecute(T1 parameter)
        {
            return this.IsInitialized;
        }

        public async IAsyncEnumerable<T2> Execute(T1 parameter, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await foreach (var item in Execute(parameter, default, cancelToken).WithCancellation(cancelToken))
            {
                yield return item;
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is T1 p)
                return CanExecute(p);
            return false;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is T1 p)
                Task.Run(() => Execute(p, default, default));
        }

        #endregion
    }

    public class PreemptionDetailsWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, PreemptDetailResult>
    {
        protected JoinBlock<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>> joinOne;
        protected JoinBlock<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>> joinTwo;
        protected JoinBlock<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>> joinThree;
        protected MergePreemptionTimes mergePreemptionTimes;

        public FilteredPreemptionData FilteredPreemptionData { get; private set; }
        public CalculateDwellTime CalculateDwellTime { get; private set; }
        public CalculateTrackClearTime CalculateTrackClearTime { get; private set; }
        public CalculateTimeToService CalculateTimeToService { get; private set; }
        public CalculateDelay CalculateDelay { get; private set; }
        public CalculateTimeToGateDown CalculateTimeToGateDown { get; private set; }
        public CalculateTimeToCallMaxOut CalculateTimeToCallMaxOut { get; private set; }
        public GeneratePreemptDetailResults GeneratePreemptDetailResults { get; private set; }

        public override void Initialize()
        {
            Steps = new();

            Input = new();
            Output = new();

            FilteredPreemptionData = new();
            
            CalculateDwellTime = new();
            CalculateTrackClearTime = new();
            CalculateTimeToService = new();
            CalculateDelay = new();
            CalculateTimeToGateDown = new();
            CalculateTimeToCallMaxOut = new();
            
            GeneratePreemptDetailResults = new();

            joinOne = new();
            joinTwo = new();
            joinThree = new();
            mergePreemptionTimes = new();

            Steps.Add(Input);
            Steps.Add(FilteredPreemptionData);
            Steps.Add(CalculateDwellTime);
            Steps.Add(CalculateTrackClearTime);
            Steps.Add(CalculateTimeToService);
            Steps.Add(CalculateDelay);
            Steps.Add(CalculateTimeToGateDown);
            Steps.Add(CalculateTimeToCallMaxOut);
            Steps.Add(GeneratePreemptDetailResults);
            Steps.Add(joinOne);
            Steps.Add(joinTwo);
            Steps.Add(joinThree);
            Steps.Add(mergePreemptionTimes);

            Input.LinkTo(FilteredPreemptionData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredPreemptionData.LinkTo(CalculateDwellTime, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(CalculateTrackClearTime, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(CalculateTimeToService, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(CalculateDelay, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(CalculateTimeToGateDown, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(CalculateTimeToCallMaxOut, new DataflowLinkOptions() { PropagateCompletion = true });

            joinOne.LinkTo(joinThree.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            joinTwo.LinkTo(joinThree.Target2, new DataflowLinkOptions() { PropagateCompletion = true });

            CalculateDwellTime.LinkTo(joinOne.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTrackClearTime.LinkTo(joinOne.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimeToService.LinkTo(joinOne.Target3, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateDelay.LinkTo(joinTwo.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimeToGateDown.LinkTo(joinTwo.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimeToCallMaxOut.LinkTo(joinTwo.Target3, new DataflowLinkOptions() { PropagateCompletion = true });

            joinThree.LinkTo(mergePreemptionTimes, new DataflowLinkOptions() { PropagateCompletion = true });
            mergePreemptionTimes.LinkTo(GeneratePreemptDetailResults, new DataflowLinkOptions() { PropagateCompletion = true });
            GeneratePreemptDetailResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });

            //var action = new ActionBlock<PreemptDetailResult>(test =>
            //{
            //    Console.WriteLine($"----------------------------------------------------------------------------------");
            //    Console.WriteLine($"{test.SignalId} - {test.PreemptNumber} - {test.Start} - {test.End} - {test.DwellTimes.Count()} - {test.TrackClearTimes.Count()} - {test.ServiceTimes.Count()} - {test.Delay.Count()} - {test.GateDownTimes.Count()} - {test.CallMaxOutTimes.Count()}");
            //});

            //Output.LinkTo(action, new DataflowLinkOptions() { PropagateCompletion = true });

            base.Initialize();
        }
    }

    public class MergePreemptionTimes : TransformProcessStepBase<Tuple<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>>, IEnumerable<PreempDetailValueBase>>
    {
        public MergePreemptionTimes(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<PreempDetailValueBase>> Process(Tuple<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item1.Item1.Union(input.Item1.Item2.Union(input.Item1.Item3)).Union(input.Item2.Item1.Union(input.Item2.Item2.Union(input.Item2.Item3)));

            return Task.FromResult(result);
        }
    }

    public class GeneratePreemptDetailResults : TransformManyProcessStepBase<IEnumerable<PreempDetailValueBase>, PreemptDetailResult>
    {
        public GeneratePreemptDetailResults(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<PreemptDetailResult>> Process(IEnumerable<PreempDetailValueBase> input, CancellationToken cancelToken = default)
        {
            var result = new List<PreemptDetailResult>();

            foreach (var signal in input.GroupBy(g => g.SignalId))
            {
                foreach (var item in signal.GroupBy(g => g.PreemptNumber))
                {
                    result.Add(new PreemptDetailResult()
                    {
                        SignalId = signal.Key,
                        PreemptNumber = item.Key,
                        Start = item.Min(m => m.Start),
                        End = item.Max(m => m.End),
                        DwellTimes = item.Where(w => w.GetType().Name == nameof(DwellTimeValue)).Cast<DwellTimeValue>(),
                        TrackClearTimes = item.Where(w => w.GetType().Name == nameof(TrackClearTimeValue)).Cast<TrackClearTimeValue>(),
                        ServiceTimes = item.Where(w => w.GetType().Name == nameof(TimeToServiceValue)).Cast<TimeToServiceValue>(),
                        Delay = item.Where(w => w.GetType().Name == nameof(DelayTimeValue)).Cast<DelayTimeValue>(),
                        GateDownTimes = item.Where(w => w.GetType().Name == nameof(TimeToGateDownValue)).Cast<TimeToGateDownValue>(),
                        CallMaxOutTimes = item.Where(w => w.GetType().Name == nameof(TimeToCallMaxOutValue)).Cast<TimeToCallMaxOutValue>()
                    });
                }
            }

            return Task.FromResult<IEnumerable<PreemptDetailResult>>(result);
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