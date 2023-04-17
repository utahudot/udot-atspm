using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.Common
{
    public class AnalysisPhaseCollectionData
    {
        public List<PlanSplitMonitorData> Plans { get; set; }
        public int MaxPhaseInUse { get; set; }
        public string SignalId { get; set; }
        public List<AnalysisPhaseData> AnalysisPhases { get; set; }
        public Signal Signal { get; internal set; }
    }

    public class AnalysisPhaseCollectionService
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository _signalRepository;
        private readonly PlanService planService;
        private readonly AnalysisPhaseService analysisPhaseService;
        private readonly PlanSplitMonitorService planSplitMonitorService;

        public AnalysisPhaseCollectionService(
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository,
            PlanService planService,
            AnalysisPhaseService analysisPhaseService,
            PlanSplitMonitorService planSplitMonitorService
            )
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            _signalRepository = signalRepository;
            this.planService = planService;
            this.analysisPhaseService = analysisPhaseService;
            this.planSplitMonitorService = planSplitMonitorService;
        }

        //public AnalysisPhaseCollectionData GetAnalysisPhaseCollectionData(
        //    string signalId,
        //    DateTime startTime,
        //    DateTime endTime,
        //    int consecutivecount)
        //{
        //    var signal = _signalRepository.GetLatestVersionOfSignal(signalId, startTime);
        //    var analysisPhaseCollectionData = new AnalysisPhaseCollectionData();
        //    analysisPhaseCollectionData.SignalId = signalId;
        //    var ptedt = controllerEventLogRepository.GetSignalEventsByEventCodes(
        //        analysisPhaseCollectionData.SignalId,
        //        startTime,
        //        endTime,
        //        new List<int> { 1, 11, 4, 5, 6, 7, 21, 23 });
        //    var dapta = controllerEventLogRepository.GetSignalEventsByEventCodes(
        //        analysisPhaseCollectionData.SignalId,
        //        startTime,
        //        endTime,
        //        new List<int> { 1 });
        //    ptedt = ptedt.OrderByDescending(i => i.Timestamp).ToList();
        //    var phasesInUse = dapta.Where(r => r.EventCode == 1).Select(r => r.EventParam).Distinct();
        //    analysisPhaseCollectionData.Plans = planService.GetSplitMonitorPlans(startTime, endTime, analysisPhaseCollectionData.SignalId);
        //    foreach (var row in phasesInUse)
        //    {
        //        var aPhase = analysisPhaseService.GetAnalysisPhaseData(row, ptedt, consecutivecount, signal);
        //        analysisPhaseCollectionData.AnalysisPhases.Add(aPhase);
        //    }
        //    analysisPhaseCollectionData.AnalysisPhases = analysisPhaseCollectionData.AnalysisPhases.OrderBy(i => i.PhaseNumber).ToList();
        //    analysisPhaseCollectionData.MaxPhaseInUse = FindMaxPhase(analysisPhaseCollectionData.AnalysisPhases);
        //    return analysisPhaseCollectionData;
        //}

        public AnalysisPhaseCollectionData GetAnalysisPhaseCollectionData(string signalId, DateTime startTime, DateTime endTime, List<ControllerEventLog> planEvents)
        {
            var signal = _signalRepository.GetLatestVersionOfSignal(signalId, startTime);
            var analysisPhaseCollectionData = new AnalysisPhaseCollectionData();
            var ptedt = controllerEventLogRepository.GetSignalEventsByEventCodes(signalId, startTime, endTime,
                new List<int> { 1, 11, 4, 5, 6, 7, 21, 23 }).ToList();
            var dapta = controllerEventLogRepository.GetSignalEventsByEventCodes(signalId, startTime, endTime, new List<int> { 1 });
            var phasesInUse = dapta.Where(d => d.EventCode == 1).Select(d => d.EventParam).Distinct();
            analysisPhaseCollectionData.Plans = planService.GetSplitMonitorPlans(startTime, endTime, signalId, planEvents);
            foreach (var row in phasesInUse)
            {
                var aPhase = analysisPhaseService.GetAnalysisPhaseData(row, signal, ptedt);
                analysisPhaseCollectionData.AnalysisPhases.Add(aPhase);
            }
            analysisPhaseCollectionData.AnalysisPhases = analysisPhaseCollectionData.AnalysisPhases.OrderBy(i => i.PhaseNumber).ToList();
            analysisPhaseCollectionData.Signal = signal;
            if (analysisPhaseCollectionData.Plans.Count > 0)
            {
                foreach (var plan in analysisPhaseCollectionData.Plans)
                {
                    planSplitMonitorService.SetProgrammedSplits(signalId, plan);
                    planSplitMonitorService.SetHighCycleCount(analysisPhaseCollectionData, plan);
                    var highestSplit = planSplitMonitorService.FindHighestRecordedSplitPhase(plan);
                    planSplitMonitorService.FillMissingSplits(highestSplit, plan);
                }
            }
            return analysisPhaseCollectionData;
        }

        private int FindMaxPhase(List<AnalysisPhaseData> analysisPhases)
        {
            return analysisPhases.Select(phase => phase.PhaseNumber).Concat(new[] { 0 }).Max();
        }
    }
}