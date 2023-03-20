using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Google.Protobuf.WellKnownTypes;

namespace ATSPM.EventLogUtility
{
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
