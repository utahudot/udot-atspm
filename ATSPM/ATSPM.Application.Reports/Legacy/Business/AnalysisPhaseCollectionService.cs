using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Legacy.Common.Business
{
    public class AnalysisPhaseCollectionData
    {        
        public List<PlanSplitMonitorData> Plans { get; set; }
        public int MaxPhaseInUse { get; set; }
        public string SignalId { get; set; }
        public List<AnalysisPhase> AnalysisPhases { get; set; }
    }
    
    public class AnalysisPhaseCollectionService
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository _signalRepository;
        private readonly PlanService planService;

        public AnalysisPhaseCollectionService(
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository,
            PlanService planService
            )
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            _signalRepository = signalRepository;
            this.planService = planService;
        }

        public AnalysisPhaseCollectionData GetAnalysisPhaseCollection(
            string signalId,
            DateTime startTime,
            DateTime endTime,
            int consecutivecount)
        {
            var analysisPhaseCollectionData = new AnalysisPhaseCollectionData();
            analysisPhaseCollectionData.SignalId = signalId;
            var ptedt = controllerEventLogRepository.GetSignalEventsByEventCodes(
                analysisPhaseCollectionData.SignalId,
                startTime,
                endTime,
                new List<int> { 1, 11, 4, 5, 6, 7, 21, 23 });
            var dapta = controllerEventLogRepository.GetSignalEventsByEventCodes(
                analysisPhaseCollectionData.SignalId,
                startTime,
                endTime,
                new List<int> { 1 });
            ptedt = ptedt.OrderByDescending(i => i.Timestamp).ToList();
            var phasesInUse = dapta.Where(r => r.EventCode == 1).Select(r => r.EventParam).Distinct();
            analysisPhaseCollectionData.Plans = planService.GetSplitMonitorPlans(startTime, endTime, analysisPhaseCollectionData.SignalId);
            foreach (var row in phasesInUse)
            {
                var aPhase = new AnalysisPhase(row, ptedt, consecutivecount, _signalRepository);
                analysisPhaseCollectionData.AnalysisPhases.Add(aPhase);
            }
            analysisPhaseCollectionData.AnalysisPhases = analysisPhaseCollectionData.AnalysisPhases.OrderBy(i => i.PhaseNumber).ToList(); 
            analysisPhaseCollectionData.MaxPhaseInUse = FindMaxPhase(analysisPhaseCollectionData.AnalysisPhases);
            return analysisPhaseCollectionData;
        }

        public AnalysisPhaseCollectionData GetAnalysisPhaseCollection(string signalId, DateTime startTime, DateTime endTime)
        {
            var analysisPhaseCollectionData = new AnalysisPhaseCollectionData();
            var ptedt = controllerEventLogRepository.GetSignalEventsByEventCodes(signalId, startTime, endTime,
                new List<int> {1, 11, 4, 5, 6, 7, 21, 23}).ToList();
            var dapta = controllerEventLogRepository.GetSignalEventsByEventCodes(signalId, startTime, endTime, new List<int> {1});
            var phasesInUse = dapta.Where(d => d.EventCode == 1).Select(d => d.EventParam).Distinct();
            analysisPhaseCollectionData.Plans = planService.GetSplitMonitorPlans(startTime, endTime, signalId);
            foreach (var row in phasesInUse)
            {
                var aPhase = new AnalysisPhase(row, signalId, ptedt);
                analysisPhaseCollectionData.AnalysisPhases.Add(aPhase);
            }
            analysisPhaseCollectionData.AnalysisPhases = analysisPhaseCollectionData.AnalysisPhases.OrderBy(i => i.PhaseNumber).ToList();
            return analysisPhaseCollectionData;
        }

        private int FindMaxPhase(List<AnalysisPhase> analysisPhases)
        {
            return analysisPhases.Select(phase => phase.PhaseNumber).Concat(new[] {0}).Max();
        }
    }
}