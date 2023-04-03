using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using Google.Protobuf.WellKnownTypes;

namespace ATSPM.EventLogUtility
{
    public abstract class ProcessWorkflowStepBase<T1, T2, T3> : IExecuteAsyncWithProgress<T1, T2, T3>, IPropagatorBlock<T1, T2>, IDataflowBlock, ISourceBlock<T2>, ITargetBlock<T1>
    {
        public event EventHandler? CanExecuteChanged;

        private readonly IPropagatorBlock<T1, T2> _workflowProcess;

        public ProcessWorkflowStepBase(ExecutionDataflowBlockOptions dataflowBlockOptions = default, IProgress<T3> progress = default)
        {
            dataflowBlockOptions.NameFormat = this.GetType().Name;
            _workflowProcess = new TransformBlock<T1, T2>(p => ExecuteAsync(p, progress, dataflowBlockOptions.CancellationToken), dataflowBlockOptions);
        }

        #region IPropagatorBlock

        #region IDataflowBlock

        public Task Completion => _workflowProcess.Completion;

        public void Complete()
        {
            _workflowProcess.Complete();
        }

        public void Fault(Exception exception)
        {
            _workflowProcess.Fault(exception);
        }

        #endregion

        #region ISourceBlock

        public T2? ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T2> target, out bool messageConsumed)
        {
            return _workflowProcess.ConsumeMessage(messageHeader, target, out messageConsumed);
        }

        public IDisposable LinkTo(ITargetBlock<T2> target, DataflowLinkOptions linkOptions)
        {
            return _workflowProcess.LinkTo(target, linkOptions);
        }

        public void ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T2> target)
        {
            _workflowProcess?.ReleaseReservation(messageHeader, target);
        }

        public bool ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T2> target)
        {
            return _workflowProcess.ReserveMessage(messageHeader, target);
        }

        #endregion

        #region ITargetBlock

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, T1 messageValue, ISourceBlock<T1>? source, bool consumeToAccept)
        {
            return _workflowProcess.OfferMessage(messageHeader,messageValue, source, consumeToAccept);
        }

        #endregion

        #endregion

        #region IExecuteAsyncWithProgress

        public abstract Task<T2> ExecuteAsync(T1 parameter, IProgress<T3>? progress = null, CancellationToken cancelToken = default);

        public virtual bool CanExecute(T1 parameter)
        {
            return true;
        }

        public virtual Task<T2> ExecuteAsync(T1 parameter, CancellationToken cancelToken = default)
        {
            return ExecuteAsync(parameter, default, cancelToken);
        }

        Task? IExecuteAsync.ExecuteAsync(object parameter)
        {
            if (parameter is T1 p)
                return Task.Run(() => ExecuteAsync(p, default, default));
            return default;
        }

        bool ICommand.CanExecute(object? parameter)
        {
            if (parameter is T1 p)
                return CanExecute(p);
            return default;
        }

        void ICommand.Execute(object? parameter)
        {
            if (parameter is T1 p)
                Task.Run(() => ExecuteAsync(p, default, default));
        }

        #endregion
    }

    public class IdentifyUnknownTerminationTypes<T> : ProcessWorkflowStepBase<List<ControllerEventLog>, List<ControllerEventLog>, T>
    {
        public IdentifyUnknownTerminationTypes(ExecutionDataflowBlockOptions dataflowBlockOptions = default, IProgress<T> progress = default) : base(dataflowBlockOptions, progress){}

        public override Task<List<ControllerEventLog>> ExecuteAsync(List<ControllerEventLog> parameter, IProgress<T>? progress = null, CancellationToken cancelToken = default)
        {
            if (cancelToken.IsCancellationRequested)
                return Task.FromCanceled<List<ControllerEventLog>>(cancelToken);

            if (!CanExecute(parameter))
                return Task.FromException<List<ControllerEventLog>>(new ExecuteException());

            return Task.FromResult(parameter.Where(p => p.EventCode == (int)DataLoggerEnum.PhaseGreenTermination).ToList());
        }
    }





    public class AnalysisPhase
    {
        public int Data { get; set; }
    }


    public class ATSPMProcessWorkflow
    {
        public TransformBlock<Tuple<List<ControllerEventLog>, List<ControllerEventLog>>, AnalysisPhase> PhaseTerminationMeasureInformation { get; set; }

        public BroadcastBlock<List<ControllerEventLog>> FilteredTerminationStatus { get; set; } 


        public ATSPMProcessWorkflow() 
        {
            //Controller event logs EC7
            FilteredTerminationStatus = new BroadcastBlock<List<ControllerEventLog>>(d => d.Where(i => i.EventCode == (int)DataLoggerEnum.PhaseGreenTermination).ToList());

            //Controller event logs EC 4,5,6
            var FilteredTerminationsData = new BroadcastBlock<List<ControllerEventLog>>(null);

            //Controller event logs EC 21,23
            var FilteredPedPhases = new BufferBlock<List<ControllerEventLog>>();

            //Identify unknown termination types
            var IdentifyUnknownTerminationTypes = new TransformBlock<List<ControllerEventLog>, List<ControllerEventLog>>(d => IdentifyUnknownTerminationTypesProcess(d));

            //Identify termination types and times
            var IdentifyTerminationTypesAndTimes = new TransformBlock<List<ControllerEventLog>, List<ControllerEventLog>>(d => IdentifyTerminationTypesAndTimesProcess(d));

            //Identify all termination types and times
            var IdentifyAllTerminationTypesandTimesJoin = new JoinBlock<List<ControllerEventLog>, List<ControllerEventLog>>();
            var IdentifyAllTerminationTypesandTimes = new TransformBlock<Tuple<List<ControllerEventLog>, List<ControllerEventLog>>, List<ControllerEventLog>>(d => IdentifyAllTerminationTypesandTimesProcess(d));

            //Identify Ped Activity
            var IdentifyPedActivity = new TransformBlock<List<ControllerEventLog>, List<ControllerEventLog>>(d => IdentifyPedActivityProcess(d));

            //Phase termination measure information
            var PhaseTerminationMeasureInformationJoin = new JoinBlock<List<ControllerEventLog>, List<ControllerEventLog>>();
            var PhaseTerminationMeasureInformation = new TransformBlock<Tuple<List<ControllerEventLog>, List<ControllerEventLog>>, AnalysisPhase>(d => PhaseTerminationMeasureInformationProcess(d));

            //link FilteredTerminationStatus
            FilteredTerminationStatus.LinkTo(IdentifyUnknownTerminationTypes);

            //Link FilteredTerminationsData
            FilteredTerminationsData.LinkTo(IdentifyTerminationTypesAndTimes);

            //link IdentifyAllTerminationTypesandTimes
            IdentifyUnknownTerminationTypes.LinkTo(IdentifyAllTerminationTypesandTimesJoin.Target1);
            IdentifyTerminationTypesAndTimes.LinkTo(IdentifyAllTerminationTypesandTimesJoin.Target2);
            IdentifyAllTerminationTypesandTimesJoin.LinkTo(IdentifyAllTerminationTypesandTimes);

            //Link FilteredPedPhases
            FilteredPedPhases.LinkTo(IdentifyPedActivity);

            //link PhaseTerminationMeasureInformation
            IdentifyAllTerminationTypesandTimes.LinkTo(PhaseTerminationMeasureInformationJoin.Target1);
            IdentifyPedActivity.LinkTo(PhaseTerminationMeasureInformationJoin.Target2);
            PhaseTerminationMeasureInformationJoin.LinkTo(PhaseTerminationMeasureInformation);


            ActionBlock<AnalysisPhase> StartCharting = new ActionBlock<AnalysisPhase>(a => Console.WriteLine($"------------------------------------------------------AnalysisPhase Data: {a.Data}"));


            PhaseTerminationMeasureInformation.LinkTo(StartCharting);

            

            //FilteredTerminationStatus.Post(list1);
            //FilteredTerminationsData.Post(list2);
            //FilteredPedPhases.Post(list3);
        }

        public List<ControllerEventLog> IdentifyUnknownTerminationTypesProcess(List<ControllerEventLog> list)
        {
            foreach (var i in list)
            {
                Console.WriteLine($"IdentifyUnknownTerminationTypesProcess: {i}");
            }

            return list;
        }

        public List<ControllerEventLog> IdentifyTerminationTypesAndTimesProcess(List<ControllerEventLog> list)
        {
            foreach (var i in list)
            {
                Console.WriteLine($"IdentifyTerminationTypesAndTimesProcess: {i}");
            }

            return list;
        }

        public List<ControllerEventLog> IdentifyPedActivityProcess(List<ControllerEventLog> list)
        {
            foreach (var i in list)
            {
                Console.WriteLine($"IdentifyPedActivityProcess: {i}");
            }

            return list;
        }

        public List<ControllerEventLog> IdentifyAllTerminationTypesandTimesProcess(Tuple<List<ControllerEventLog>, List<ControllerEventLog>> lists)
        {
            foreach (var i in lists.Item1)
            {
                Console.WriteLine($"IdentifyAllTerminationTypesandTimesProcess: item1 - {i}");
            }

            foreach (var i in lists.Item2)
            {
                Console.WriteLine($"IdentifyAllTerminationTypesandTimesProcess: item2 - {i}");
            }

            var list = lists.Item1.Union(lists.Item2).ToList();

            foreach (var i in list)
            {
                Console.WriteLine($"IdentifyAllTerminationTypesandTimesProcess: {i}");
            }

            return list;
        }

        public AnalysisPhase PhaseTerminationMeasureInformationProcess(Tuple<List<ControllerEventLog>, List<ControllerEventLog>> lists)
        {
            foreach (var i in lists.Item1)
            {
                Console.WriteLine($"PhaseTerminationMeasureInformationProcess: item1 - {i}");
            }

            foreach (var i in lists.Item2)
            {
                Console.WriteLine($"PhaseTerminationMeasureInformationProcess: item2 - {i}");
            }

            Console.WriteLine($"********************************************************************");

            foreach(var phase in lists.Item1.Union(lists.Item2).Select(s => s.EventParam).Distinct().OrderBy(o => o))
            {
                Console.WriteLine($"Events for phase {phase}: {lists.Item1.Where(w => w.EventParam == phase).Count()} - {lists.Item2.Where(w => w.EventParam == phase).Count()}");
            }

            Console.WriteLine($"********************************************************************");

            foreach (var phase in lists.Item1.Union(lists.Item2).GroupBy(g => g.EventParam).OrderBy(o => o.Key))
            {
                Console.WriteLine($"Events for phase {phase.Key}: {phase.Count()}");
            }

            Console.WriteLine($"********************************************************************");

            return new AnalysisPhase() { Data = lists.Item1.Count};
        }
    }
}