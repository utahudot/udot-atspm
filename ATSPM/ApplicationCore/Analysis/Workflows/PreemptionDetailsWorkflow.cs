using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.PreemptionDetails;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static System.Reflection.Metadata.BlobBuilder;
using System.Text.Json;
using static ATSPM.Application.Analysis.Workflows.PreemptiveStuff;

namespace ATSPM.Application.Analysis.Workflows
{
    public class InputOnValue : PreempDetailValueBase { }




    public class CreatePreemptCycleResult : TransformProcessStepBase<IEnumerable<PreemptCycle>, IEnumerable<PreemptCycleResult>>
    {
        public CreatePreemptCycleResult(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<PreemptCycleResult>> Process(IEnumerable<PreemptCycle> input, CancellationToken cancelToken = default)
        {
            var result = input.Select(s => new PreemptCycleResult()
            {
                CallMaxOut = s.TimeToCallMaxOut,
                Delay = s.Delay,
                TimeToService = s.TimeToService,
                DwellTime = s.DwellTime,
                TrackClear = s.TimeToTrackClear
            });

            return Task.FromResult(result);
        }
    }

    //public class PreemptiveTestStep : TransformProcessStepBase<IEnumerable<ControllerEventLog>, IReadOnlyList<PreemptCycle>>
    //{
    //    public PreemptiveTestStep(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

    //    protected override Task<IReadOnlyList<PreemptCycle>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
    //    {
    //        var cycles = PreemptDetailRange<PreemptCycle>(input, IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptionBeginExitInterval);

    //        //var inputon = PreemptDetailRange<DwellTimeValue>(input, IndianaEnumerations.PreemptCallInputOff, IndianaEnumerations.PreemptionBeginExitInterval);

    //        var dwell = PreemptDetailRange<DwellTimeValue>(input, IndianaEnumerations.PreemptionBeginDwellService, IndianaEnumerations.PreemptionBeginExitInterval);
    //        var trackclear = PreemptDetailRange<TrackClearTimeValue>(input, IndianaEnumerations.PreemptionBeginTrackClearance, IndianaEnumerations.PreemptionBeginDwellService);
    //        var timetoservice = PreemptDetailRange<TimeToServiceValue>(input, IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptionBeginDwellService);
    //        var delay = PreemptDetailRange<DelayTimeValue>(input, IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptEntryStarted);
    //        var gatedown = PreemptDetailRange<TimeToGateDownValue>(input, IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptGateDownInputReceived);
    //        var maxout = PreemptDetailRange<TimeToCallMaxOutValue>(input, IndianaEnumerations.PreemptCallInputOn, IndianaEnumerations.PreemptionMaxPresenceExceeded);

    //        var result = new List<PreemptCycle>();

    //        foreach (var c in cycles)
    //        {
    //            result.Add(new PreemptCycle()
    //            {
    //                StartInputOn = c.Start,
    //                BeginExitInterval = c.End,
    //                BeginDwellService = dwell.FirstOrDefault(w => c.InRange(w))?.Start,
    //                BeginTrackClearance = trackclear.FirstOrDefault(w => c.InRange(w))?.Start,
    //                EntryStarted = 

    //                TimeToService = timetoservice.FirstOrDefault(w => c.InRange(w))?.Seconds.TotalSeconds ?? 0,
    //                Delay = delay.FirstOrDefault(w => c.InRange(w))?.Seconds.TotalSeconds ?? 0,
    //                TimeToGateDown = gatedown.FirstOrDefault(w => c.InRange(w))?.Seconds.TotalSeconds ?? 0,
    //                TimeToTrackClear = trackclear.FirstOrDefault(w => c.InRange(w))?.Seconds.TotalSeconds ?? 0

    //            });

    //            //c.DwellTime = dwell.FirstOrDefault(w => c.InRange(w));
    //            //c.TrackClearTime = trackclear.FirstOrDefault(w => c.InRange(w));
    //            //c.ServiceTime = timetoservice.FirstOrDefault(w => c.InRange(w));
    //            //c.Delay = delay.FirstOrDefault(w => c.InRange(w));
    //            //c.GateDownTime = gatedown.FirstOrDefault(w => c.InRange(w));
    //            //c.CallMaxOutTime = maxout.FirstOrDefault(w => c.InRange(w));

    //            //_output.WriteLine($"c: {c}");
    //        }

    //        return Task.FromResult<IReadOnlyList<PreemptCycle>>(result);
    //    }

    //    private IEnumerable<T> PreemptDetailRange<T>(IEnumerable<ControllerEventLog> items, IndianaEnumerations first, IndianaEnumerations second) where T : PreempDetailValueBase, new()
    //    {
    //        var result = items.GroupBy(g => g.LocationIdentifier, (Location, l1) =>
    //        l1.GroupBy(g => g.EventParam, (preempt, l2) =>
    //        l2.TimeSpanFromConsecutiveCodes(first, second)
    //        .Select(s => new T()
    //        {
    //            LocationIdentifier = Location,
    //            PreemptNumber = preempt,
    //            Start = s.Item1[0].Timestamp,
    //            End = s.Item1[1].Timestamp,
    //            Seconds = s.Item2
    //        })).SelectMany(m => m)).SelectMany(m => m);

    //        return result;
    //    }
    //}


    public class PreemptiveStuff : TransformProcessStepBase<IEnumerable<ControllerEventLog>, IReadOnlyList<PreemptCycle>>
    {
        public PreemptiveStuff(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IReadOnlyList<PreemptCycle>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = input
                .GroupBy(g => g.SignalIdentifier, (Location, l1) => l1
                .GroupBy(g => g.EventParam, (preempt, l2) =>
                CreatePreemptCycle(l2.OrderBy(o => o.Timestamp).ToList())))
                .SelectMany(m => m)
                .SelectMany(m => m).ToList();

            return Task.FromResult<IReadOnlyList<PreemptCycle>>(result);
        }

        public List<PreemptCycle> CreatePreemptCycle(List<ControllerEventLog> preemptEvents)
        {
            var CycleCollection = new List<PreemptCycle>();
            PreemptCycle cycle = null;


            //foreach (MOE.Common.Models.Controller_Event_Log row in DTTB.Events)
            for (var x = 0; x < preemptEvents.Count; x++)
            {
                //It can happen that there is no defined terminaiton event.
                if (x + 1 < preemptEvents.Count)
                {
                    var timeBetweenEvents = preemptEvents[x + 1].Timestamp - preemptEvents[x].Timestamp;
                    if (cycle != null && timeBetweenEvents.TotalMinutes > 20 && preemptEvents[x].EventCode != 111 &&
                        preemptEvents[x].EventCode != 105)
                    {
                        EndCycle(cycle, preemptEvents[x], CycleCollection);
                        cycle = null;
                        continue;
                    }
                }

                switch (preemptEvents[x].EventCode)
                {
                    case 102:

                        //if (cycle != null)
                        //cycle.InputOn.Add(preemptEvents[x].Timestamp);

                        if (cycle == null && preemptEvents[x].Timestamp != preemptEvents[x + 1].Timestamp &&
                            preemptEvents[x + 1].EventCode == 105)
                            cycle = StartCycle(preemptEvents[x]);

                        break;

                    case 103:

                        if (cycle != null)
                            cycle.GateDown = preemptEvents[x].Timestamp;


                        break;

                    case 104:

                        //if (cycle != null)
                        //cycle.InputOff.Add(preemptEvents[x].Timestamp);

                        break;

                    case 105:


                        ////If we run into an entry start after cycle start (event 102)
                        if (cycle != null && cycle.HasDelay)
                        {
                            cycle.EntryStarted = preemptEvents[x].Timestamp;
                            break;
                        }

                        if (cycle != null)
                        {
                            EndCycle(cycle, preemptEvents[x], CycleCollection);
                            cycle = StartCycle(preemptEvents[x]);
                            break;
                        }

                        if (cycle == null)
                            cycle = StartCycle(preemptEvents[x]);
                        break;

                    case 106:
                        if (cycle != null)
                        {
                            cycle.BeginTrackClearance = preemptEvents[x].Timestamp;

                            if (x + 1 < preemptEvents.Count)
                                if (!DoesTrackClearEndNormal(preemptEvents, x))
                                    cycle.BeginDwellService = FindNext111Event(preemptEvents, x);
                        }
                        break;

                    case 107:

                        if (cycle != null)
                        {
                            cycle.BeginDwellService = preemptEvents[x].Timestamp;

                            if (x + 1 < preemptEvents.Count)
                                if (!DoesTheCycleEndNormal(preemptEvents, x))
                                {
                                    cycle.BeginExitInterval = preemptEvents[x + 1].Timestamp;

                                    EndCycle(cycle, preemptEvents[x + 1], CycleCollection);

                                    cycle = null;
                                }
                        }


                        break;

                    case 108:
                        if (cycle != null)
                            cycle.LinkActive = preemptEvents[x].Timestamp;
                        break;

                    case 109:
                        if (cycle != null)
                            cycle.LinkInactive = preemptEvents[x].Timestamp;

                        break;

                    case 110:
                        if (cycle != null)
                            cycle.MaxPresenceExceeded = preemptEvents[x].Timestamp;
                        break;

                    case 111:
                        // 111 can usually be considered "cycle complete"
                        if (cycle != null)
                        {
                            cycle.BeginExitInterval = preemptEvents[x].Timestamp;

                            EndCycle(cycle, preemptEvents[x], CycleCollection);


                            cycle = null;
                        }
                        break;
                }


                if (x + 1 >= preemptEvents.Count && cycle != null)
                {
                    cycle.BeginExitInterval = preemptEvents[x].Timestamp;
                    EndCycle(cycle, preemptEvents[x], CycleCollection);
                    break;
                }
            }

            return CycleCollection;
        }

        private DateTime FindNext111Event(List<ControllerEventLog> DTTB, int counter)
        {
            var Next111Event = new DateTime();
            for (var x = counter; x < DTTB.Count; x++)
                if (DTTB[x].EventCode == 111)
                {
                    Next111Event = DTTB[x].Timestamp;
                    x = DTTB.Count;
                }
            return Next111Event;
        }

        private bool DoesTheCycleEndNormal(List<ControllerEventLog> DTTB, int counter)
        {
            var foundEvent111 = false;

            for (var x = counter; x < DTTB.Count; x++)
                switch (DTTB[x].EventCode)
                {
                    case 102:
                        foundEvent111 = false;
                        x = DTTB.Count;
                        break;
                    case 105:
                        foundEvent111 = false;
                        x = DTTB.Count;
                        break;

                    case 111:
                        foundEvent111 = true;
                        x = DTTB.Count;
                        break;
                }

            return foundEvent111;
        }

        private bool DoesTrackClearEndNormal(List<ControllerEventLog> DTTB, int counter)
        {
            var foundEvent107 = false;

            for (var x = counter; x < DTTB.Count; x++)
                switch (DTTB[x].EventCode)
                {
                    case 107:
                        foundEvent107 = true;
                        x = DTTB.Count;
                        break;

                    case 111:
                        foundEvent107 = false;
                        x = DTTB.Count;
                        break;
                }

            return foundEvent107;
        }

        private void EndCycle(PreemptCycle cycle, ControllerEventLog controller_Event_Log,
            List<PreemptCycle> CycleCollection)
        {
            cycle.End = controller_Event_Log.Timestamp;
            //cycle.Delay = GetDelay(cycle.HasDelay, cycle.EntryStarted, cycle.Start);
            //cycle.TimeToService = GetTimeToService(
            //    cycle.HasDelay,
            //    cycle.BeginTrackClearance,
            //    cycle.Start,
            //    cycle.BeginDwellService,
            //    cycle.EntryStarted);
            //cycle.DwellTime = GetDwellTime(cycle.End, cycle.BeginDwellService);
            //cycle.TimeToCallMaxOut = GetTimeToCallMaxOut(cycle.Start, cycle.MaxPresenceExceeded);
            //cycle.TimeToEndOfEntryDelay = GetTimeToEndOfEntryDelay(cycle.EntryStarted, cycle.Start);
            //cycle.TimeToTrackClear = GetTimeToTrackClear(cycle.BeginDwellService, cycle.BeginTrackClearance);
            //cycle.TimeToGateDown = GetTimeToGateDown(cycle.Start, cycle.GateDown);
            CycleCollection.Add(cycle);
        }

        private double GetTimeToGateDown(DateTime cycleStart, DateTime gateDown)
        {
            if (cycleStart > DateTime.MinValue && gateDown > DateTime.MinValue && gateDown > cycleStart)
                return (gateDown - cycleStart).TotalSeconds;
            return 0;
        }

        private double GetTimeToTrackClear(DateTime beginDwellService, DateTime beginTrackClearance)
        {
            if (beginDwellService > DateTime.MinValue && beginTrackClearance > DateTime.MinValue &&
                    beginDwellService > beginTrackClearance)
                return (beginDwellService - beginTrackClearance).TotalSeconds;
            return 0;
        }

        private double GetTimeToEndOfEntryDelay(DateTime entryStarted, DateTime cycleStart)
        {
            if (cycleStart > DateTime.MinValue && entryStarted > DateTime.MinValue && entryStarted > cycleStart)
                return (entryStarted - cycleStart).TotalSeconds;
            return 0;
        }

        private double GetTimeToCallMaxOut(DateTime CycleStart, DateTime MaxPresenceExceeded)
        {
            if (CycleStart > DateTime.MinValue && MaxPresenceExceeded > DateTime.MinValue &&
                   MaxPresenceExceeded > CycleStart)
                return (MaxPresenceExceeded - CycleStart).TotalSeconds;
            return 0;
        }

        private double GetDwellTime(DateTime cycleEnd, DateTime beginDwellService)
        {
            if (cycleEnd > DateTime.MinValue && beginDwellService > DateTime.MinValue &&
                    cycleEnd >= beginDwellService)
                return (cycleEnd - beginDwellService).TotalSeconds;
            return 0;
        }

        private double GetTimeToService(
            bool hasDelay,
            DateTime beginTrackClearance,
            DateTime cycleStart,
            DateTime beginDwellService,
            DateTime entryStarted)
        {
            if (beginTrackClearance > DateTime.MinValue && cycleStart > DateTime.MinValue &&
                   beginTrackClearance >= cycleStart)
            {
                if (hasDelay)
                    return (beginTrackClearance - entryStarted).TotalSeconds;
                return (beginTrackClearance - cycleStart).TotalSeconds;
            }

            if (beginDwellService > DateTime.MinValue && cycleStart > DateTime.MinValue &&
                beginDwellService >= cycleStart)
            {
                if (hasDelay)
                    return (beginDwellService - entryStarted).TotalSeconds;
                return (beginDwellService - cycleStart).TotalSeconds;
            }

            return 0;
        }

        private double GetDelay(bool hasDelay, DateTime entryStarted, DateTime cycleStart)
        {
            if (hasDelay && entryStarted > DateTime.MinValue && cycleStart > DateTime.MinValue &&
                        entryStarted > cycleStart)
                return (entryStarted - cycleStart).TotalSeconds;

            return 0;
        }


        private PreemptCycle StartCycle(ControllerEventLog controller_Event_Log)
        {
            var cycle = new PreemptCycle();


            cycle.Start = controller_Event_Log.Timestamp;

            if (controller_Event_Log.EventCode == 105)
            {
                cycle.EntryStarted = controller_Event_Log.Timestamp;
                cycle.HasDelay = false;
            }

            if (controller_Event_Log.EventCode == 102)
            {
                cycle.StartInputOn = controller_Event_Log.Timestamp;
                cycle.HasDelay = true;
            }

            return cycle;
        }

        private IEnumerable<StartEndRange> TimeSpanFromConsecutiveCodes(IEnumerable<ControllerEventLog> items, IndianaEnumerations first, IndianaEnumerations second)
        {
            var preFilter = items.OrderBy(o => o.Timestamp)
                .Where(w => w.EventCode == (int)first || w.EventCode == (int)second)
                //.Where(w => w.Timestamp > DateTime.MinValue && w.Timestamp < DateTime.MaxValue)
                .ToList();

            var result = preFilter.Where((x, y) =>
                    (y < preFilter.Count - 1 && x.EventCode == (int)first && preFilter[y + 1].EventCode == (int)second) ||
                    (y > 0 && x.EventCode == (int)second && preFilter[y - 1].EventCode == (int)first))
                        .Chunk(2)
                        .Select(l => new StartEndRange() { Start = l[0].Timestamp, End = l[1].Timestamp });

            return result;
        }
    }

    public class PreemptCycleResult
    {
        public DateTime InputOff { get; set; }

        public DateTime InputOn { get; set; }

        public DateTime GateDown { get; set; }

        public double CallMaxOut { get; set; }

        public double Delay { get; set; }

        public double TimeToService { get; set; }

        public double DwellTime { get; set; }

        public double TrackClear { get; set; }
    }





    public class PreemptCycle : PreempDetailValueBase
    {

        public PreemptCycle()
        {
            //InputOn = new List<DateTime>();
            //InputOff = new List<DateTime>();
            //OtherPreemptStart = new List<DateTime>();
        }
        // public enum CycleState { InputOn, GateDown, InputOff, BeginTrackClearance, EntryStarted  };
        //public List<DateTime> InputOff { get; set; }
        // public List<DateTime> InputOn { get; set; }
        //public List<DateTime> OtherPreemptStart { get; set; }
        public List<DateTime?> InputOff { get; set; }
        public List<DateTime?> InputOn { get; set; }
        public List<DateTime?> OtherPreemptStart { get; set; }
        public DateTime? StartInputOn { get; set; }
        public DateTime? CycleStart { get; set; }
        public DateTime? CycleEnd { get; set; }
        public DateTime? GateDown { get; set; }
        public DateTime? EntryStarted { get; set; }
        public DateTime? BeginTrackClearance { get; set; }
        public DateTime? BeginDwellService { get; set; }
        public DateTime? BeginExitInterval { get; set; }
        public DateTime? LinkActive { get; set; }
        public DateTime? LinkInactive { get; set; }
        public DateTime? MaxPresenceExceeded { get; set; }
        public bool HasDelay { get; set; }
        public double Delay { get; set; }
        public double TimeToService { get; set; }
        public double DwellTime { get; set; }
        public double TimeToCallMaxOut { get; set; }
        public double TimeToEndOfEntryDelay { get; set; }
        public double TimeToTrackClear { get; set; }
        public double TimeToGateDown { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }


    /// <summary>
    /// Preemption is the interruption of normal operations to serve a preferred 
    /// vehicle(e.g., train, emergency vehicle). This measure can be used to determine if 
    /// preemption events are occurring as intended. “Preemption Details” refers to the
    /// duration of preemption intervals for each preemption event. The type of interval
    /// information available depends on the type of preempt (i.e., rail or emergency
    /// vehicle) and the availability of certain inputs. Some potential intervals that can be
    /// tracked include entry delay, track clearance, gate down, dwell, time to service, 
    /// max-out, and preempt input on/off (1).
    /// </summary>
    public class PreemptionDetailsWorkflow : WorkflowBase<IEnumerable<ControllerEventLog>, PreemptDetailResult>
    {
        protected JoinBlock<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>> joinOne;
        protected JoinBlock<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>> joinTwo;
        protected JoinBlock<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>> joinThree;
        internal MergePreemptionTimes mergePreemptionTimes;

        public FilteredPreemptionData FilteredPreemptionData { get; private set; }
        public CalculateDwellTime CalculateDwellTime { get; private set; }
        public CalculateTrackClearTime CalculateTrackClearTime { get; private set; }
        public CalculateTimeToService CalculateTimeToService { get; private set; }
        public CalculateDelay CalculateDelay { get; private set; }
        public CalculateTimeToGateDown CalculateTimeToGateDown { get; private set; }
        public CalculateTimeToCallMaxOut CalculateTimeToCallMaxOut { get; private set; }
        public GeneratePreemptDetailResults GeneratePreemptDetailResults { get; private set; }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
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
        }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
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
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            //Input.LinkTo(FilteredPreemptionData, new DataflowLinkOptions() { PropagateCompletion = true });

            //FilteredPreemptionData.LinkTo(CalculateDwellTime, new DataflowLinkOptions() { PropagateCompletion = true });
            //FilteredPreemptionData.LinkTo(CalculateTrackClearTime, new DataflowLinkOptions() { PropagateCompletion = true });
            //FilteredPreemptionData.LinkTo(CalculateTimeToService, new DataflowLinkOptions() { PropagateCompletion = true });
            //FilteredPreemptionData.LinkTo(CalculateDelay, new DataflowLinkOptions() { PropagateCompletion = true });
            //FilteredPreemptionData.LinkTo(CalculateTimeToGateDown, new DataflowLinkOptions() { PropagateCompletion = true });
            //FilteredPreemptionData.LinkTo(CalculateTimeToCallMaxOut, new DataflowLinkOptions() { PropagateCompletion = true });

            //joinOne.LinkTo(joinThree.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            //joinTwo.LinkTo(joinThree.Target2, new DataflowLinkOptions() { PropagateCompletion = true });

            //CalculateDwellTime.LinkTo(joinOne.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            //CalculateTrackClearTime.LinkTo(joinOne.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            //CalculateTimeToService.LinkTo(joinOne.Target3, new DataflowLinkOptions() { PropagateCompletion = true });
            //CalculateDelay.LinkTo(joinTwo.Target1, new DataflowLinkOptions() { PropagateCompletion = true });
            //CalculateTimeToGateDown.LinkTo(joinTwo.Target2, new DataflowLinkOptions() { PropagateCompletion = true });
            //CalculateTimeToCallMaxOut.LinkTo(joinTwo.Target3, new DataflowLinkOptions() { PropagateCompletion = true });

            //joinThree.LinkTo(mergePreemptionTimes, new DataflowLinkOptions() { PropagateCompletion = true });
            //mergePreemptionTimes.LinkTo(GeneratePreemptDetailResults, new DataflowLinkOptions() { PropagateCompletion = true });
            //GeneratePreemptDetailResults.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    internal class MergePreemptionTimes : TransformProcessStepBase<Tuple<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>>, IEnumerable<PreempDetailValueBase>>
    {
        internal MergePreemptionTimes(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IEnumerable<PreempDetailValueBase>> Process(Tuple<Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>, Tuple<IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>, IEnumerable<PreempDetailValueBase>>> input, CancellationToken cancelToken = default)
        {
            var result = input.Item1.Item1.Union(input.Item1.Item2.Union(input.Item1.Item3)).Union(input.Item2.Item1.Union(input.Item2.Item2.Union(input.Item2.Item3)));

            return Task.FromResult(result);
        }
    }
}
