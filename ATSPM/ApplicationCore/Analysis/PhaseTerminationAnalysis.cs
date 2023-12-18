using ATSPM.Application.Exceptions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.BaseClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis
{
    public class PhaseTerminationData
    {
        public int ForceOffs { get; set; }
        public int MaxOuts { get; set; }
        public int GapOuts { get; set; }
        public int UnknownTerminationTypes { get; set; }
    }

    public class PhaseTerminationAnalysis : ServiceObjectBase
    {
        protected CancellationToken token;

        public List<IDataflowBlock> Steps { get; set; } = new();

        public override void Initialize()
        {
            var stepOptions = new ExecutionDataflowBlockOptions()
            {
                CancellationToken = token,
                //NameFormat = blockName,
                //MaxDegreeOfParallelism = _options.Value.MaxDegreeOfParallelism,
                //BoundedCapacity = capcity,
                //MaxMessagesPerTask = ?,
                SingleProducerConstrained = true,
                EnsureOrdered = false
            };

            //create steps
            var step1 = CreateTransformManyStep<Location, string>(t => DownloadLogs(t, token), "Determine Phase Termination Time", stepOptions);
            var step2 = CreateTransformManyStep<string, string>(t => DownloadLogs(t, token), "Determine Unknown Phase Termination", stepOptions);
            var step3 = CreateTransformManyStep<string, string>(t => DownloadLogs(t, token), "Identify Pedestrian Events", stepOptions);

            //step linking
            step1.LinkTo(step2, new DataflowLinkOptions() { PropagateCompletion = true });
            step2.LinkTo(step3, new DataflowLinkOptions() { PropagateCompletion = true });

            base.Initialize();
        }

        public List<ControllerEventLog> FindTerminationEvents(List<ControllerEventLog> logs, int phaseNumber)
        {
            var events = logs.Where(l => l.EventParam == phaseNumber && (
            l.EventCode == (int)DataLoggerEnum.PhaseGapOut ||
            l.EventCode == (int)DataLoggerEnum.PhaseMaxOut ||
            l.EventCode == (int)DataLoggerEnum.PhaseForceOff ||
            l.EventCode == (int)DataLoggerEnum.PhaseGreenTermination))
                .OrderBy(o => o.Timestamp).ThenBy(t => t.EventCode).ToList();

            var duplicateList = new List<ControllerEventLog>();
            for (int i = 0; i < events.Count - 1; i++)
            {
                var event1 = events[i];
                var event2 = events[i + 1];
                if (event1.Timestamp == event2.Timestamp)
                {
                    if (event1.EventCode == (int)DataLoggerEnum.PhaseGreenTermination)
                        duplicateList.Add(event1);
                    if (event2.EventCode == (int)DataLoggerEnum.PhaseGreenTermination)
                        duplicateList.Add(event2);
                }
            }

            foreach (var e in duplicateList)
            {
                events.Remove(e);
            }

            return events;
        }

        private List<ControllerEventLog> FindConsecutiveEvents(List<ControllerEventLog> logs, DataLoggerEnum eventType, int consecutiveCount)
        {
            var result = new List<ControllerEventLog>();
            var count = 0;

            foreach (var log in logs.OrderBy(o => o.Timestamp))
            {
                if (log.EventCode != (int)DataLoggerEnum.PhaseGreenTermination)
                {
                    count = log.EventCode == (int)eventType ? count + 1 : 0;

                    if (count >= consecutiveCount)
                        result.Add(log);
                }
            }
                
            return result;
        }

        private List<ControllerEventLog> FindUnknownTerminationEvents(List<ControllerEventLog> logs)
        {
            return logs.Where(t => t.EventCode == (int)DataLoggerEnum.PhaseGreenTermination).ToList();
        }

        public List<ControllerEventLog> FindPedEvents(List<ControllerEventLog> logs, int phaseNumber)
        {
            var result = logs.Where(w => w.EventParam == phaseNumber && 
            (w.EventCode == (int)DataLoggerEnum.PedestrianBeginWalk || 
            w.EventCode == (int)DataLoggerEnum.PedestrianBeginSolidDontWalk))
                .OrderBy(o => o.Timestamp).ToList();

            return result;
        }

        protected ITargetBlock<T> CreateActionStep<T>(Action<T> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;
            options.SingleProducerConstrained = false;

            var block = new ActionBlock<T>(p =>
            {
                try
                {
                    process?.Invoke(p);
                }
                catch (Exception e)
                {
                    logMessages.StepExecutionException(processName, new ControllerLoggerStepExecutionException<T>(this, processName, p, null, e));
                }
            }, options);

            block.Completion.ContinueWith(t => logMessages.StepCompletedMessage(block.ToString(), t.Status), options.CancellationToken);

            Steps.Add(block);

            return block;
        }

        protected ITargetBlock<T> CreateActionStep<T>(Func<T, Task> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;
            options.SingleProducerConstrained = false;

            var block = new ActionBlock<T>(async p =>
            {
                try
                {
                    await process?.Invoke(p);
                }
                catch (Exception e)
                {
                    logMessages.StepExecutionException(processName, new ControllerLoggerStepExecutionException<T>(this, processName, p, null, e));
                }
            }, options);

            block.Completion.ContinueWith(t => logMessages.StepCompletedMessage(block.ToString(), t.Status), options.CancellationToken);

            Steps.Add(block);

            return block;
        }

        protected IPropagatorBlock<T1, T2> CreateTransformManyStep<T1, T2>(Func<T1, IEnumerable<T2>> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;

            var block = new TransformManyBlock<T1, T2>(p =>
            {
                try
                {
                    return process?.Invoke(p);
                }
                catch (Exception e)
                {
                    logMessages.StepExecutionException(processName, new ControllerLoggerStepExecutionException<T1>(this, processName, p, null, e));
                }

                return null;

            }, options);

            block.Completion.ContinueWith(t => logMessages.StepCompletedMessage(block.ToString(), t.Status), options.CancellationToken);

            Steps.Add(block);

            return block;
        }

        protected IPropagatorBlock<T1, T2> CreateTransformManyStep<T1, T2>(Func<T1, Task<IEnumerable<T2>>> process, string processName, ExecutionDataflowBlockOptions options = default)
        {
            options.NameFormat = processName;

            var block = new TransformManyBlock<T1, T2>(async p =>
            {
                try
                {
                    return await process?.Invoke(p);
                }
                catch (Exception e)
                {
                    logMessages.StepExecutionException(processName, new ControllerLoggerStepExecutionException<T1>(this, processName, p, null, e));
                }

                return null;

            }, options);

            block.Completion.ContinueWith(t => logMessages.StepCompletedMessage(block.ToString(), t.Status), options.CancellationToken);

            Steps.Add(block);

            return block;
        }
    }
}
