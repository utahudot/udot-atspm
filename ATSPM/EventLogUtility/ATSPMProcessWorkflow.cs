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
    public class FilteredPreemptionData : FilterStepBase
    {
        public FilteredPreemptionData(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PreemptCallInputOn);
            filteredList.Add((int)DataLoggerEnum.PreemptGateDownInputReceived);
            filteredList.Add((int)DataLoggerEnum.PreemptCallInputOff);
            filteredList.Add((int)DataLoggerEnum.PreemptEntryStarted);
            filteredList.Add((int)DataLoggerEnum.PreemptionBeginDwellService);
            filteredList.Add((int)DataLoggerEnum.PreemptionMaxPresenceExceeded);
            filteredList.Add((int)DataLoggerEnum.PreemptionBeginExitInterval);
        }
    }

    public class FilteredIndicationData : FilterStepBase
    {
        public FilteredIndicationData(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int) DataLoggerEnum.PhaseBeginGreen);
            filteredList.Add((int) DataLoggerEnum.PhaseBeginRedClearance);
        }
    }

    public class FilteredDetectorData : FilterStepBase
    {
        public FilteredDetectorData(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.DetectorOff);
            filteredList.Add((int)DataLoggerEnum.DetectorOn);
        }
    }

    public class FilteredPedPhases : FilterStepBase
    {
        public FilteredPedPhases(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginSolidDontWalk);
        }
    }

    public class FilteredTerminationStatus : FilterStepBase
    {
        public FilteredTerminationStatus(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseGreenTermination);
        }
    }

    public class FilteredTerminations : FilterStepBase
    {
        public FilteredTerminations(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseGapOut);
            filteredList.Add((int)DataLoggerEnum.PhaseMaxOut);
            filteredList.Add((int)DataLoggerEnum.PhaseForceOff);
        }
    }

    public class FilteredSplitsData : FilterStepBase
    {
        public FilteredSplitsData(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            for(int i = 134; i <= 149; i++)
            {
                filteredList.Add(i);
            }
        }
    }

    public class FilteredPhaseIntervalChanges : FilterStepBase
    {
        public FilteredPhaseIntervalChanges(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseBeginGreen);
            filteredList.Add((int)DataLoggerEnum.PhaseBeginYellowChange);
            filteredList.Add((int)DataLoggerEnum.PhaseEndYellowChange);
            filteredList.Add((int)DataLoggerEnum.PhaseEndRedClearance);
        }
    }

    public class FilteredCallStatus : FilterStepBase
    {
        public FilteredCallStatus(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseCallRegistered);
            filteredList.Add((int)DataLoggerEnum.PhaseCallDropped);
        }
    }

    public class FilteredPedCalls : FilterStepBase
    {
        public FilteredPedCalls(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PedestrianCallRegistered);
            filteredList.Add((int)DataLoggerEnum.PedDetectorOn);
        }
    }

    public class FilteredPedPhaseData : FilterStepBase
    {
        public FilteredPedPhaseData(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginChangeInterval);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginClearance);
        }
    }

    public class FilteredTimingActuationData : FilterStepBase
    {
        public FilteredTimingActuationData(DataflowBlockOptions? dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            filteredList.Add((int)DataLoggerEnum.PhaseBeginGreen);
            filteredList.Add((int)DataLoggerEnum.PhaseMinComplete);
            filteredList.Add((int)DataLoggerEnum.PhaseMaxOut);
            filteredList.Add((int)DataLoggerEnum.PhaseEndYellowChange);
            filteredList.Add((int)DataLoggerEnum.PhaseEndRedClearance);
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginChangeInterval);
            filteredList.Add((int)DataLoggerEnum.PedestrianBeginSolidDontWalk);
            filteredList.Add((int)DataLoggerEnum.OverlapBeginGreen);
            filteredList.Add((int)DataLoggerEnum.OverlapBeginTrailingGreenExtension);
            filteredList.Add((int)DataLoggerEnum.OverlapBeginYellow);
            filteredList.Add((int)DataLoggerEnum.OverlapBeginRedClearance);
            filteredList.Add((int)DataLoggerEnum.OverlapOffInactivewithredindication);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginWalk);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginClearance);
            filteredList.Add((int)DataLoggerEnum.PedestrianOverlapBeginSolidDontWalk);
            filteredList.Add((int)DataLoggerEnum.DetectorOff);
            filteredList.Add((int)DataLoggerEnum.DetectorOn);
            filteredList.Add((int)DataLoggerEnum.PedDetectorOff);
            filteredList.Add((int)DataLoggerEnum.PedDetectorOn);
        }
    }



























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