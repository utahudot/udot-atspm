using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace Legacy.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class WaitTimeOptions 
    {
        public const int PHASE_BEGIN_GREEN = 1;
        public const int PHASE_END_RED_CLEARANCE = 11;
        public const int PHASE_CALL_REGISTERED = 43;
        public const int PHASE_CALL_DROPPED = 44;

        public WaitTimeOptions(bool showPlanStripes)
        {
            ShowPlanStripes = showPlanStripes;
        }

        public WaitTimeOptions()
        {
        }

        [DataMember]
        [Display(Name = "Show Plan Stripes")]
        public bool ShowPlanStripes { get; set; }

        //public override List<string> CreateMetric()
        //{
        //    base.CreateMetric();
        //    var signalsRepository = SignalsRepositoryFactory.Create();
        //    var signal = signalsRepository.GetVersionOfSignalByDate(SignalId, StartDate);
        //    var analysisPhaseCollection = new AnalysisPhaseCollection(SignalId, StartDate, EndDate);
        //    foreach (var plan in analysisPhaseCollection.Plans)
        //    {
        //        plan.SetProgrammedSplits(SignalId);
        //        plan.SetHighCycleCount(analysisPhaseCollection);
        //    }

        //    var eventLogs = new ControllerEventLogService(SignalId, StartDate, EndDate,
        //        new List<int>
        //            { PHASE_BEGIN_GREEN, PHASE_END_RED_CLEARANCE, PHASE_CALL_REGISTERED, PHASE_CALL_DROPPED });
        //    foreach (var approach in signal.Approaches.OrderBy(x => x.ProtectedPhaseNumber))
        //    {
        //        var phaseInfo =
        //            analysisPhaseCollection.Items.FirstOrDefault(x => x.PhaseNumber == approach.ProtectedPhaseNumber);
        //        CreateChart(approach, eventLogs, signal, phaseInfo, analysisPhaseCollection.Plans);
        //    }

        //    return ReturnList;
        //}

        //public void CreateChart(Approach approach, ControllerEventLogService eventLogs, Signal signal,
        //    AnalysisPhase phaseInfo, List<PlanSplitMonitor> plans)
        //{
        //    var phaseEvents = eventLogs.Events.Where(x => x.EventParam == approach.ProtectedPhaseNumber);

        //    if (phaseEvents.Any())
        //    {
        //        var waitTimeChart = new WaitTimeChart(this, signal, approach, phaseEvents, StartDate, EndDate,
        //            phaseInfo, plans);
        //        var chart = waitTimeChart.Chart;
        //        var chartName = CreateFileName();
        //        chart.SaveImage(MetricFileLocation + chartName);
        //        ReturnList.Add(MetricWebPath + chartName);
        //    }

        //}

       

       

      
    }
}