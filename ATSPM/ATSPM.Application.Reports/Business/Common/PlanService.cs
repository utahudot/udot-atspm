using ATSPM.Data.Models;
using ATSPM.Data.Enums;
using ATSPM.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.SplitFail;
using ATSPM.Application.Reports.Business.ApproachSpeed;

namespace ATSPM.Application.Reports.Business.Common
{
    public class PlanService
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly PlanSplitMonitorService planSplitMonitorService;

        public PlanService(IControllerEventLogRepository controllerEventLogRepository, PlanSplitMonitorService planSplitMonitorService)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.planSplitMonitorService = planSplitMonitorService;
        }

        public List<PerdueCoordinationPlan> GetPcdPlans(List<CyclePcd> cycles, DateTime startDate,
            DateTime endDate, Approach approach)
        {
            var planEvents = GetPlanEvents(startDate, endDate, approach.SignalId);
            var plans = new List<PerdueCoordinationPlan>();
            for (var i = 0; i < planEvents.Count; i++)
                if (planEvents.Count - 1 == i)
                {
                    if (planEvents[i].Timestamp != endDate)
                    {
                        var planCycles = cycles
                            .Where(c => c.StartTime >= planEvents[i].Timestamp && c.StartTime < endDate).ToList();
                        plans.Add(new PerdueCoordinationPlan(planEvents[i].Timestamp, endDate, planEvents[i].EventParam.ToString(), planCycles));
                    }
                }
                else
                {
                    if (planEvents[i].Timestamp != planEvents[i + 1].Timestamp)
                    {
                        var planCycles = cycles.Where(c =>
                                c.StartTime >= planEvents[i].Timestamp && c.StartTime < planEvents[i + 1].Timestamp)
                            .ToList();
                        plans.Add(new PerdueCoordinationPlan(planEvents[i].Timestamp, planEvents[i + 1].Timestamp,
                            planEvents[i].EventParam.ToString(), planCycles));
                    }
                }
            return plans;
        }

        public List<ControllerEventLog> GetPlanEvents(DateTime startDate, DateTime endDate, string signalId)
        {
            var planEvents = new List<ControllerEventLog>();
            var tempPlanEvents = controllerEventLogRepository.GetSignalEventsByEventCode(signalId, startDate, endDate, 131)
                .OrderBy(e => e.Timestamp).ToList();
            if (tempPlanEvents.Any() && tempPlanEvents.First().Timestamp != startDate)
            {
                GetFirstPlan(startDate, signalId, planEvents);
            }
            else if (!planEvents.Any())
            {
                GetFirstPlan(startDate, signalId, planEvents);
            }
            tempPlanEvents.Add(new ControllerEventLog { SignalId = signalId, EventCode = 131, EventParam = 254, Timestamp = endDate });

            for (var x = 0; x < tempPlanEvents.Count(); x++)
                if (x + 2 < tempPlanEvents.Count())
                {
                    if (tempPlanEvents[x].EventParam == tempPlanEvents[x + 1].EventParam)
                    {
                        planEvents.Add(tempPlanEvents[x]);
                        x++;
                    }
                    else
                    {
                        planEvents.Add(tempPlanEvents[x]);
                    }
                }
                else
                {
                    if (tempPlanEvents.Count >= 2 && tempPlanEvents.Last().EventCode ==
                        tempPlanEvents[tempPlanEvents.Count() - 2].EventCode)
                        planEvents.Add(tempPlanEvents[tempPlanEvents.Count() - 2]);
                    else
                        planEvents.Add(tempPlanEvents.Last());
                }

            return planEvents;
        }

        private void GetFirstPlan(DateTime startDate, string signalId, List<ControllerEventLog> planEvents)
        {
            var firstPlanEvent = controllerEventLogRepository.GetFirstEventBeforeDate(signalId, 131, startDate);
            if (firstPlanEvent != null)
            {
                firstPlanEvent.Timestamp = startDate;
                planEvents.Add(firstPlanEvent);
            }
            else
            {
                firstPlanEvent = new ControllerEventLog
                {
                    Timestamp = startDate,
                    EventCode = 131,
                    EventParam = 0,
                    SignalId = signalId
                };
                planEvents.Insert(0, firstPlanEvent);
            }
        }

        public List<Plan> GetBasicPlans(DateTime startDate, DateTime endDate, string signalId)
        {
            var planEvents = GetPlanEvents(startDate, endDate, signalId);
            var plans = new List<Plan>();
            for (var i = 0; i < planEvents.Count; i++)
                if (planEvents.Count - 1 == i)
                {
                    if (planEvents[i].Timestamp != endDate)
                        plans.Add(new Plan(planEvents[i].EventParam.ToString(),planEvents[i].Timestamp, endDate));
                }
                else
                {
                    if (planEvents[i].Timestamp != planEvents[i + 1].Timestamp)
                        plans.Add(new Plan(planEvents[i].EventParam.ToString(), planEvents[i].Timestamp, planEvents[i + 1].Timestamp
                            ));
                }
            return plans;
        }

        public List<PlanSplitMonitorData> GetSplitMonitorPlans(DateTime startDate, DateTime endDate, string signalId)
        {
            var planEvents = GetPlanEvents(startDate, endDate, signalId);
            var plans = new List<PlanSplitMonitorData>();
            for (var i = 0; i < planEvents.Count; i++)
                if (planEvents.Count - 1 == i)
                {
                    if (planEvents[i].Timestamp != endDate)
                        plans.Add(planSplitMonitorService.GetPlanSplitMonitor(planEvents[i].Timestamp, endDate, planEvents[i].EventParam));
                }
                else
                {
                    if (planEvents[i].Timestamp != planEvents[i + 1].Timestamp)
                        plans.Add(planSplitMonitorService.GetPlanSplitMonitor(planEvents[i].Timestamp, planEvents[i + 1].Timestamp,
                            planEvents[i].EventParam));
                }
            return plans;
        }

        public List<SpeedPlan> GetSpeedPlans(List<CycleSpeed> cycles, DateTime startDate,
            DateTime endDate, Approach approach)
        {
            var planEvents = GetPlanEvents(startDate, endDate, approach.SignalId);
            var plans = new List<SpeedPlan>();
            for (var i = 0; i < planEvents.Count; i++)
            {
                var planStart = new DateTime();
                var planEnd = new DateTime();
                var planNumber = 0;
                var averageSpeed = 0;
                var standardDeviation = 0;
                var eightyFifthPercentile = 0;
                var fifteenthPercentile = 0;
                if (planEvents.Count - 1 == i)
                {
                    if (planEvents[i].Timestamp != endDate)
                    {
                        var planCycles = cycles
                            .Where(c => c.StartTime >= planEvents[i].Timestamp && c.StartTime < endDate).ToList();
                        planStart = planEvents[i].Timestamp;
                        planEnd = endDate;
                        planNumber = planEvents[i].EventParam;
                        SetSpeedStatistics(
                            planCycles,
                            out averageSpeed,
                            out standardDeviation,
                            out eightyFifthPercentile,
                            out fifteenthPercentile);
                    }
                }
                else
                {
                    if (planEvents[i].Timestamp != planEvents[i + 1].Timestamp)
                    {
                        var planCycles = cycles.Where(c =>
                                c.StartTime >= planEvents[i].Timestamp && c.StartTime < planEvents[i + 1].Timestamp)
                            .ToList();
                        planStart = planEvents[i].Timestamp;
                        planEnd = planEvents[i + 1].Timestamp;
                        planNumber = planEvents[i].EventParam;
                        SetSpeedStatistics(
                            planCycles,
                            out averageSpeed,
                            out standardDeviation,
                            out eightyFifthPercentile,
                            out fifteenthPercentile);
                    }
                }
                plans.Add(new SpeedPlan(
                    planStart,
                    planEnd,
                    planNumber.ToString(),
                    averageSpeed,
                    standardDeviation,
                    eightyFifthPercentile,
                    fifteenthPercentile));
            }
            return plans;
        }

        public void SetSpeedStatistics(
            List<CycleSpeed> cycles,
            out int avgSpeed,
            out int stdDev,
            out int eightyFifth,
            out int fifteenth)
        {
            var rawSpeeds = new List<int>();
            foreach (var cycle in cycles)
                rawSpeeds.AddRange(cycle.SpeedEvents.Select(s => s.Mph));

            //find stddev of average
            if (rawSpeeds.Count > 0)
            {
                var rawaverage = rawSpeeds.Average();
                avgSpeed = Convert.ToInt32(Math.Round(rawaverage));
                stdDev =
                    Convert.ToInt32(Math.Round(Math.Sqrt(rawSpeeds.Average(v => Math.Pow(v - rawaverage, 2)))));
            }
            else
            {
                avgSpeed = 0;
                stdDev = 0;
            }
            eightyFifth = GetPercentile(rawSpeeds, .85);
            fifteenth = GetPercentile(rawSpeeds, .15);
        }

        private int GetPercentile(List<int> speeds, double percentile)
        {
            speeds.Sort();
            var percentileValue = 0;
            try
            {
                var tempPercentileIndex = speeds.Count * percentile - 1;

                if (speeds.Count > 3)
                {
                    var percentileIndex = 0;
                    if (tempPercentileIndex % 1 > 0)
                    {
                        percentileIndex = Convert.ToInt32(Math.Round(tempPercentileIndex + .5));
                        percentileValue = speeds[percentileIndex];
                    }
                    else
                    {
                        percentileIndex = Convert.ToInt32(tempPercentileIndex);
                        var speed1 = speeds[percentileIndex];
                        var speed2 = speeds[percentileIndex + 1];
                        double rawEightyfifth = (speed1 + speed2) / 2;
                        percentileValue = Convert.ToInt32(Math.Round(rawEightyfifth));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error creating Percentile");
            }
            return percentileValue;
        }

        public List<PlanSplitFail> GetSplitFailPlans(List<CycleSplitFail> cycles, SplitFailOptions options,
            Approach approach)
        {
            var planEvents = GetPlanEvents(options.StartDate, options.EndDate, approach.SignalId);
            var plans = new List<PlanSplitFail>();
            for (var i = 0; i < planEvents.Count; i++)
                if (planEvents.Count - 1 == i)
                {
                    if (planEvents[i].Timestamp != options.EndDate)
                    {
                        var planCycles = cycles.Where(c =>
                            c.StartTime >= planEvents[i].Timestamp && c.StartTime < options.EndDate).ToList();
                        plans.Add(new PlanSplitFail(planEvents[i].Timestamp, options.EndDate, planEvents[i].EventParam.ToString(),
                            planCycles));
                    }
                }
                else
                {
                    if (planEvents[i].Timestamp != planEvents[i + 1].Timestamp)
                    {
                        var planCycles = cycles.Where(c =>
                                c.StartTime >= planEvents[i].Timestamp && c.StartTime < planEvents[i + 1].Timestamp)
                            .ToList();
                        plans.Add(new PlanSplitFail(planEvents[i].Timestamp, planEvents[i + 1].Timestamp,
                            planEvents[i].EventParam.ToString(), planCycles));
                    }
                }
            return plans;
        }
    }
}