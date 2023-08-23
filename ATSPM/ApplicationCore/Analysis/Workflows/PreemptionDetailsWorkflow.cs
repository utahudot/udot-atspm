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

namespace ATSPM.Application.Analysis.Workflows
{
    public class PreemptiveStuff : TransformProcessStepBase<IEnumerable<ControllerEventLog>, IReadOnlyList<PreemptCycle>>
    {
        public PreemptiveStuff(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        protected override Task<IReadOnlyList<PreemptCycle>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            var result = CreatePreemptCycle(input.ToList());

            return Task.FromResult<IReadOnlyList<PreemptCycle>>(result);
        }



        //protected override Task<IReadOnlyList<PreemptCycle>> Process(IEnumerable<ControllerEventLog> input, CancellationToken cancelToken = default)
        //{
        //    //var result = TimeSpanFromConsecutiveCodes(input, DataLoggerEnum.PreemptCallInputOn, DataLoggerEnum.PreemptionBeginExitInterval)
        //    //    .Select(s => new PreemptCycle() { Start = s.Start, End = s.End }).ToList();

        //    //foreach (var r in result)
        //    //{
        //    //    r.GateDown = (DateTime)(input.FirstOrDefault(w => w.EventCode == 103 && r.InRange(w.Timestamp))?.Timestamp);
        //    //    //r. = input.FirstOrDefault(w => w.EventCode == 104 && r.InRange(w.Timestamp)).Timestamp;
        //    //    r.EntryStarted = (DateTime)input.FirstOrDefault(w => w.EventCode == 105 && r.InRange(w.Timestamp))?.Timestamp;
        //    //    r.BeginTrackClearance = (DateTime)input.FirstOrDefault(w => w.EventCode == 106 && r.InRange(w.Timestamp))?.Timestamp;
        //    //    r.BeginDwellService = (DateTime)input.FirstOrDefault(w => w.EventCode == 107 && r.InRange(w.Timestamp))?.Timestamp;
        //    //    r.MaxPresenceExceeded = (DateTime)input.FirstOrDefault(w => w.EventCode == 110 && r.InRange(w.Timestamp))?.Timestamp;
        //    //}

        //    var result = new List<PreemptCycle>();

        //    var logs = input.ToList();

        //    for(int i = 0; i < logs.Count; i++)
        //    {
        //        if (logs[i].EventCode == 102)
        //        {
        //            var cycle = new PreemptCycle() { Start = logs[i].Timestamp };
        //            var on = logs.FindIndex(i + 1, p => p.EventCode == 102);
        //            //var entry = logs.FindIndex(i + 1, p => p.EventCode == 105);
        //            var off = logs.FindIndex(i + 1 , p => p.EventCode == 104);
        //            var end = logs.FindIndex(i + 1, p => p.EventCode == 111);

        //            //check to see if the next 102 event is after the 111, otherwise need to end it
        //            if (on == -1 || on > end)
        //            {
        //                //make sure there is an 111 event
        //                if (end != -1)
        //                {
        //                    cycle.End = logs[end].Timestamp;
        //                    ////see if there is a 105 inbetween start and end
        //                    //if (entry > 0 && entry < end)
        //                    //{
        //                    //    cycle.EntryStarted = logs[entry].Timestamp;
        //                    //}
        //                }
        //            }

        //            //if there is a 102 before the 111
        //            if (on > -1 && on < end)
        //            {
        //                cycle.End = logs[on].Timestamp;
        //            }

        //            result.Add(cycle);
        //        }
        //    }


        //    return Task.FromResult<IReadOnlyList<PreemptCycle>>(result);
        //}

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
                    var timeBetweenEvents = preemptEvents[x + 1].TimeStamp - preemptEvents[x].TimeStamp;
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

                        if (cycle == null && preemptEvents[x].TimeStamp != preemptEvents[x + 1].TimeStamp &&
                            preemptEvents[x + 1].EventCode == 105)
                            cycle = StartCycle(preemptEvents[x]);

                        break;

                    case 103:

                        if (cycle != null)
                            cycle.GateDown = preemptEvents[x].TimeStamp;


                        break;

                    case 104:

                        //if (cycle != null)
                            //cycle.InputOff.Add(preemptEvents[x].Timestamp);

                        break;

                    case 105:


                        ////If we run into an entry start after cycle start (event 102)
                        if (cycle != null && cycle.HasDelay)
                        {
                            cycle.EntryStarted = preemptEvents[x].TimeStamp;
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
                            cycle.BeginTrackClearance = preemptEvents[x].TimeStamp;

                            if (x + 1 < preemptEvents.Count)
                                if (!DoesTrackClearEndNormal(preemptEvents, x))
                                    cycle.BeginDwellService = FindNext111Event(preemptEvents, x);
                        }
                        break;

                    case 107:

                        if (cycle != null)
                        {
                            cycle.BeginDwellService = preemptEvents[x].TimeStamp;

                            if (x + 1 < preemptEvents.Count)
                                if (!DoesTheCycleEndNormal(preemptEvents, x))
                                {
                                    cycle.BeginExitInterval = preemptEvents[x + 1].TimeStamp;

                                    EndCycle(cycle, preemptEvents[x + 1], CycleCollection);

                                    cycle = null;
                                }
                        }


                        break;

                    case 108:
                        if (cycle != null)
                            cycle.LinkActive = preemptEvents[x].TimeStamp;
                        break;

                    case 109:
                        if (cycle != null)
                            cycle.LinkInactive = preemptEvents[x].TimeStamp;

                        break;

                    case 110:
                        if (cycle != null)
                            cycle.MaxPresenceExceeded = preemptEvents[x].TimeStamp;
                        break;

                    case 111:
                        // 111 can usually be considered "cycle complete"
                        if (cycle != null)
                        {
                            cycle.BeginExitInterval = preemptEvents[x].TimeStamp;

                            EndCycle(cycle, preemptEvents[x], CycleCollection);


                            cycle = null;
                        }
                        break;
                }


                if (x + 1 >= preemptEvents.Count && cycle != null)
                {
                    cycle.BeginExitInterval = preemptEvents[x].TimeStamp;
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
                    Next111Event = DTTB[x].TimeStamp;
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
            cycle.End = controller_Event_Log.TimeStamp;
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


            cycle.Start = controller_Event_Log.TimeStamp;

            if (controller_Event_Log.EventCode == 105)
            {
                cycle.EntryStarted = controller_Event_Log.TimeStamp;
                cycle.HasDelay = false;
            }

            if (controller_Event_Log.EventCode == 102)
            {
                cycle.StartInputOn = controller_Event_Log.TimeStamp;
                cycle.HasDelay = true;
            }

            return cycle;
        }

        private IEnumerable<StartEndRange> TimeSpanFromConsecutiveCodes(IEnumerable<ControllerEventLog> items, DataLoggerEnum first, DataLoggerEnum second)
        {
            var preFilter = items.OrderBy(o => o.TimeStamp)
                .Where(w => w.EventCode == (int)first || w.EventCode == (int)second)
                //.Where(w => w.Timestamp > DateTime.MinValue && w.Timestamp < DateTime.MaxValue)
                .ToList();

            var result = preFilter.Where((x, y) =>
                    (y < preFilter.Count - 1 && x.EventCode == (int)first && preFilter[y + 1].EventCode == (int)second) ||
                    (y > 0 && x.EventCode == (int)second && preFilter[y - 1].EventCode == (int)first))
                        .Chunk(2)
                        .Select(l => new StartEndRange() { Start = l[0].TimeStamp, End = l[1].TimeStamp });

            return result;
        }
    }

    public class PreemptCycle : StartEndRange
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
        
        
        /// <summary>
        /// 102
        /// </summary>
        public DateTime? StartInputOn { get; set; }
        
        /// <summary>
        /// start of cycle, either from 102 or 105
        /// </summary>
        //public DateTime CycleStart { get; set; }
        
        
        //public DateTime CycleEnd { get; set; }
        public DateTime? GateDown { get; set; }
        public DateTime? EntryStarted { get; set; }
        public DateTime? BeginTrackClearance { get; set; }
        public DateTime? BeginDwellService { get; set; }
        public DateTime? BeginExitInterval { get; set; }
        public DateTime? LinkActive { get; set; }
        public DateTime? LinkInactive { get; set; }
        public DateTime? MaxPresenceExceeded { get; set; }

        //
        public bool HasDelay { get; set; }
        //public double Delay { get; set; }
        //public double TimeToService { get; set; }
        //public double DwellTime { get; set; }
        //public double TimeToCallMaxOut { get; set; }
        //public double TimeToEndOfEntryDelay { get; set; }
        //public double TimeToTrackClear { get; set; }
        //public double TimeToGateDown { get; set; }

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
        public override void InstantiateSteps()
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
        public override void AddStepsToTracker()
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
        public override void LinkSteps()
        {
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
