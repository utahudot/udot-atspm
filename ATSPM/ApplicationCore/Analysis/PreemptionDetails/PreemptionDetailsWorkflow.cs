using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.PreemptionDetails
{
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

        public override string ToString() => $"{GetType().Name}-{SignalId}-{PreemptNumber}-{Start}-{End}-{DwellTimes.Count()}-{TrackClearTimes?.Count()}-{ServiceTimes?.Count()}-{Delay?.Count()}-{GateDownTimes?.Count()}-{CallMaxOutTimes?.Count()}";

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

        public PreemptiveProcessBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<IEnumerable<T>>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = input.GroupBy(g => g.SignalId)
                .SelectMany(s => s.GroupBy(g => g.EventParam)
                .Select(s => s.TimeSpanFromConsecutiveCodes(first, second)
                .Select(s => new T()
                {
                    SignalId = s.Item1[0].SignalId == s.Item1[1].SignalId ? s.Item1[0].SignalId : string.Empty,
                    PreemptNumber = Convert.ToInt32(s.Item1.Average(a => a.EventParam)),
                    Start = s.Item1[0].Timestamp,
                    End = s.Item1[1].Timestamp,
                    Seconds = s.Item2
                })));

            return Task.FromResult(result);
        }
    }


    public class CalculateDwellTime : PreemptiveProcessBase<DwellTimeValue>
    {
        public CalculateDwellTime(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptionBeginDwellService;
            second = DataLoggerEnum.PreemptionBeginExitInterval;
        }
    }

    public class CalculateTrackClearTime : PreemptiveProcessBase<TrackClearTimeValue>
    {
        public CalculateTrackClearTime(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptionBeginTrackClearance;
            second = DataLoggerEnum.PreemptionBeginDwellService;
        }
    }

    public class CalculateTimeToService : PreemptiveProcessBase<TimeToServiceValue>
    {
        public CalculateTimeToService(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptCallInputOn;
            second = DataLoggerEnum.PreemptionBeginDwellService;
        }
    }

    public class CalculateDelay : PreemptiveProcessBase<DelayTimeValue>
    {
        public CalculateDelay(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptCallInputOn;
            second = DataLoggerEnum.PreemptEntryStarted;
        }
    }

    public class CalculateTimeToGateDown : PreemptiveProcessBase<TimeToGateDownValue>
    {
        public CalculateTimeToGateDown(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptCallInputOn;
            second = DataLoggerEnum.PreemptGateDownInputReceived;
        }
    }

    public class CalculateTimeToCallMaxOut : PreemptiveProcessBase<TimeToCallMaxOutValue>
    {
        public CalculateTimeToCallMaxOut(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            first = DataLoggerEnum.PreemptCallInputOn;
            second = DataLoggerEnum.PreemptionMaxPresenceExceeded;
        }
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

            Input = new(null);
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

            base.Initialize();
        }
    }

    public class MergePreemptionTimes : TransformProcessStepBase<Tuple<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>>, IEnumerable<PreempDetailValueBase>>
    {
        public MergePreemptionTimes(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<PreempDetailValueBase>> Process(Tuple<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item1.Item1.Union(input.Item1.Item2.Union(input.Item1.Item3)).Union(input.Item2.Item1.Union(input.Item2.Item2.Union(input.Item2.Item3)));

            return Task.FromResult(result);
        }
    }

    public class GeneratePreemptDetailResults : TransformManyProcessStepBase<IEnumerable<PreempDetailValueBase>, PreemptDetailResult>
    {
        public GeneratePreemptDetailResults(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

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
}
