using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;
using ATSPM.Application.Analysis;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.BaseClasses;
using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace ATSPM.EventLogUtility
{
    public class IdentifyUnknownTerminationTypes : ProcessStepBase<List<ControllerEventLog>, List<ControllerEventLog>>
    {
        public IdentifyUnknownTerminationTypes(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions){}

        public override Task<List<ControllerEventLog>> Process(List<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            return Task.FromResult(input.Where(p => p.EventCode == (int)DataLoggerEnum.PhaseGreenTermination).ToList());
        }
    }

    public class IdentifyTerminationTypesAndTimes : ProcessStepBase<List<ControllerEventLog>, List<ControllerEventLog>>
    {
        public IdentifyTerminationTypesAndTimes(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        public override Task<List<ControllerEventLog>> Process(List<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            return Task.FromResult(input);
        }
    }

    public class IdentifyAllTerminationTypesandTimes : ProcessStepBase<Tuple<List<ControllerEventLog>, List<ControllerEventLog>>, List<ControllerEventLog>>
    {
        public IdentifyAllTerminationTypesandTimes(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        public override Task<List<ControllerEventLog>> Process(Tuple<List<ControllerEventLog>, List<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            return Task.FromResult(input.Item1.Union(input.Item2).ToList());
        }
    }

    public class IdentifyPedActivity : ProcessStepBase<List<ControllerEventLog>, List<ControllerEventLog>>
    {
        public IdentifyPedActivity(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        public override Task<List<ControllerEventLog>> Process(List<ControllerEventLog> input, CancellationToken cancelToken = default)
        {
            return Task.FromResult(input);
        }
    }

    public class PhaseTerminationMeasureInformation : ProcessStepBase<Tuple<List<ControllerEventLog>, List<ControllerEventLog>>, AnalysisPhase>
    {
        public PhaseTerminationMeasureInformation(ExecutionDataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        public override Task<AnalysisPhase> Process(Tuple<List<ControllerEventLog>, List<ControllerEventLog>> input, CancellationToken cancelToken = default)
        {
            return Task.FromResult(new AnalysisPhase() { Data = input.Item1.Count });
        }
    }

    public class AnalysisPhase
    {
        public int Data { get; set; }
    }


    public class PhaseTerminationProcess : ServiceObjectBase
    {
        public PhaseTerminationProcess() 
        {
        }

        public override void Initialize()
        {
            //Controller event logs EC7
            var FilteredTerminationStatus = new BroadcastBlock<List<ControllerEventLog>>(d => d.Where(i => i.EventCode == (int)DataLoggerEnum.PhaseGreenTermination).ToList());

            //Controller event logs EC 4,5,6
            var FilteredTerminationsData = new BroadcastBlock<List<ControllerEventLog>>(null);

            //Controller event logs EC 21,23
            var FilteredPedPhases = new BufferBlock<List<ControllerEventLog>>();

            //Identify unknown termination types
            //var IdentifyUnknownTerminationTypes = new TransformBlock<List<ControllerEventLog>, List<ControllerEventLog>>(d => IdentifyUnknownTerminationTypesProcess(d));
            var IdentifyUnknownTerminationTypes = new IdentifyUnknownTerminationTypes();

            //Identify termination types and times
            //var IdentifyTerminationTypesAndTimes = new TransformBlock<List<ControllerEventLog>, List<ControllerEventLog>>(d => IdentifyTerminationTypesAndTimesProcess(d));
            var IdentifyTerminationTypesAndTimes = new IdentifyTerminationTypesAndTimes();

            //Identify all termination types and times
            var IdentifyAllTerminationTypesandTimesJoin = new JoinBlock<List<ControllerEventLog>, List<ControllerEventLog>>();
            //var IdentifyAllTerminationTypesandTimes = new TransformBlock<Tuple<List<ControllerEventLog>, List<ControllerEventLog>>, List<ControllerEventLog>>(d => IdentifyAllTerminationTypesandTimesProcess(d));
            var IdentifyAllTerminationTypesandTimes = new IdentifyAllTerminationTypesandTimes();

            //Identify Ped Activity
            //var IdentifyPedActivity = new TransformBlock<List<ControllerEventLog>, List<ControllerEventLog>>(d => IdentifyPedActivityProcess(d));
            var IdentifyPedActivity = new IdentifyPedActivity();

            //Phase termination measure information
            var PhaseTerminationMeasureInformationJoin = new JoinBlock<List<ControllerEventLog>, List<ControllerEventLog>>();
            //var PhaseTerminationMeasureInformation = new TransformBlock<Tuple<List<ControllerEventLog>, List<ControllerEventLog>>, AnalysisPhase>(d => PhaseTerminationMeasureInformationProcess(d));
            var PhaseTerminationMeasureInformation = new PhaseTerminationMeasureInformation();

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

            base.Initialize();
        }

        
    }
}