﻿using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.ApproachSpeed;
using ATSPM.ReportApi.Business.SplitFail;
using ATSPM.ReportApi.Business.YellowRedActivations;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.Business.Common
{
    public class PlanService
    {

        public PlanService()
        {
        }

        public List<PurdueCoordinationPlan> GetPcdPlans(List<CyclePcd> cycles, DateTime startDate,
            DateTime endDate, Approach approach, List<ControllerEventLog> events)
        {
            var planEvents = GetPlanEvents(startDate, endDate, approach.Signal.SignalIdentifier, events.OrderBy(e => e.Timestamp).ToList());
            var plans = new List<PurdueCoordinationPlan>();
            for (var i = 0; i < planEvents.Count; i++)
                if (planEvents.Count - 1 == i)
                {
                    if (planEvents[i].Timestamp != endDate)
                    {
                        var planCycles = cycles
                            .Where(c => c.StartTime >= planEvents[i].Timestamp && c.StartTime < endDate).ToList();
                        plans.Add(new PurdueCoordinationPlan(planEvents[i].Timestamp, endDate, planEvents[i].EventParam.ToString(), planCycles));
                    }
                }
                else
                {
                    if (planEvents[i].Timestamp != planEvents[i + 1].Timestamp)
                    {
                        var planCycles = cycles.Where(c =>
                                c.StartTime >= planEvents[i].Timestamp && c.StartTime < planEvents[i + 1].Timestamp)
                            .ToList();
                        plans.Add(new PurdueCoordinationPlan(planEvents[i].Timestamp, planEvents[i + 1].Timestamp,
                            planEvents[i].EventParam.ToString(), planCycles));
                    }
                }
            return plans;
        }

        public IReadOnlyList<ControllerEventLog> GetPlanEvents(
            DateTime startDate,
            DateTime endDate,
            string signalId,
            List<ControllerEventLog> tempPlanEvents)
        {
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Unspecified);
            if (tempPlanEvents.Any() && tempPlanEvents.First().Timestamp != startDate)
            {
                SetFirstPlan(startDate, signalId, tempPlanEvents);
            }
            else if (!tempPlanEvents.Any())
            {
                SetFirstPlan(startDate, signalId, tempPlanEvents);
            }
            SetLastPlan(startDate, endDate, tempPlanEvents);
            if (!tempPlanEvents.Any())
            {
                tempPlanEvents.Add(new ControllerEventLog { SignalIdentifier = signalId, EventCode = 131, EventParam = 254, Timestamp = endDate });
            }

            //var planEvents = tempPlanEvents
            //    .Select((x, i) => new { Log = x, Index = i })
            //    .Where(x => x.Index == 0 || x.Index + 1 == tempPlanEvents.Count || x.Log.EventParam != tempPlanEvents[x.Index + 1].EventParam)
            //    .Select(x => x.Log)
            //    .ToList();

            return tempPlanEvents;
        }

        private static void SetLastPlan(DateTime startDate, DateTime endDate, List<ControllerEventLog> tempPlanEvents)
        {
            // Find the index of the last event within the start and end dates
            var lastPlan = tempPlanEvents.Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate).OrderBy(e => e.Timestamp).Last();
            int lastIndex = tempPlanEvents.IndexOf(lastPlan);

            if (lastIndex >= 0)
            {
                // Remove events after the last event within the specified range
                tempPlanEvents.RemoveRange(lastIndex + 1, tempPlanEvents.Count - lastIndex - 1);
            }
            else
            {
                // If no event is found within the specified range, clear the list
                tempPlanEvents.Clear();
            }
        }

        private void SetFirstPlan(DateTime startDate, string signalId, List<ControllerEventLog> planEvents)
        {
            var firstPlanEvent = planEvents.Where(e => e.Timestamp < startDate).OrderByDescending(e => e.Timestamp).FirstOrDefault();

            if (firstPlanEvent != null)
            {
                // update the timestamp of the first event to match the start date
                firstPlanEvent.Timestamp = startDate;

                // remove all events before the first event
                var indexToRemove = planEvents.IndexOf(firstPlanEvent);
                if (indexToRemove != -1)
                {
                    planEvents.RemoveRange(0, indexToRemove);
                }
            }
            else
            {
                // create a new event with the specified properties and add it to the beginning of the list
                firstPlanEvent = new ControllerEventLog
                {
                    Timestamp = startDate,
                    EventCode = 131,
                    EventParam = 0,
                    SignalIdentifier = signalId
                };
                planEvents.Insert(0, firstPlanEvent);
            }
        }
        //private void SetFirstPlan(DateTime startDate, string signalId, List<ControllerEventLog> planEvents)
        //{
        //    var firstPlanEvent = controllerEventLogRepository.GetFirstEventBeforeDate(signalId, 131, startDate);
        //    if (firstPlanEvent != null)
        //    {
        //        firstPlanEvent.TimeStamp = startDate;
        //        planEvents.Add(firstPlanEvent);
        //    }
        //    else
        //    {
        //        firstPlanEvent = new ControllerEventLog
        //        {
        //            Timestamp = startDate,
        //            EventCode = 131,
        //            EventParam = 0,
        //            SignalId = signalId
        //        };
        //        planEvents.Insert(0, firstPlanEvent);
        //    }
        //}

        public IReadOnlyList<Plan> GetBasicPlans(
            DateTime startDate,
            DateTime endDate,
            string signalId,
            IReadOnlyList<ControllerEventLog> events)
        {
            var planEvents = GetPlanEvents(startDate, endDate, signalId, events.ToList());
            var plans = planEvents.Select((x, i) => i == planEvents.Count - 1
                                        ? new Plan(x.EventParam.ToString(), x.Timestamp, endDate)
                                        : new Plan(x.EventParam.ToString(), x.Timestamp, planEvents[i + 1].Timestamp))
                                  .ToList();
            return plans;
        }

        public IReadOnlyList<YellowRedActivationPlan> GetYellowRedActivationPlans(
            DateTime startDate,
            DateTime endDate,
            List<YellowRedActivationsCycle> cycles,
            string signalIdentifier,
            double severeRedLightViolationSeconds,
            IReadOnlyList<ControllerEventLog> planEvents)
        {
            var plans = GetBasicPlans(
                startDate,
                endDate,
                signalIdentifier,
                planEvents).ToList();
            if (plans.Count == 0)
                plans.Add(new Plan("0", startDate, endDate));
            return plans.Select(plan => new YellowRedActivationPlan(
                plan.Start,
                plan.End,
                plan.PlanNumber,
                cycles.Where(c => c.StartTime >= plan.Start && c.StartTime < plan.End).ToList(),
                severeRedLightViolationSeconds)).ToList();
        }

        public IReadOnlyList<PlanSplitMonitorData> GetSplitMonitorPlans(
            DateTime startDate,
            DateTime endDate,
            string signalId,
            IList<ControllerEventLog> events)
        {
            var planEvents = GetPlanEvents(startDate, endDate, signalId, events.ToList());
            var plans = new List<PlanSplitMonitorData>();
            for (var i = 0; i < planEvents.Count; i++)
                if (planEvents.Count - 1 == i)
                {
                    if (planEvents[i].Timestamp != endDate)
                        plans.Add(new PlanSplitMonitorData(planEvents[i].Timestamp, endDate, planEvents[i].EventParam.ToString()));
                }
                else
                {
                    if (planEvents[i].Timestamp != planEvents[i + 1].Timestamp)
                        plans.Add(new PlanSplitMonitorData(planEvents[i].Timestamp, planEvents[i + 1].Timestamp,
                            planEvents[i].EventParam.ToString()));
                }
            return plans;
        }

        /// <summary>
        /// Needs event code 131
        /// </summary>
        /// <param name="cycles"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="approach"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        public List<SpeedPlan> GetSpeedPlans(List<CycleSpeed> cycles, DateTime startDate,
            DateTime endDate, Approach approach, List<ControllerEventLog> events)
        {
            if (events.IsNullOrEmpty())
            {
                return new List<SpeedPlan>();
            }
            var planEvents = GetPlanEvents(startDate, endDate, approach.Signal.SignalIdentifier, events);
            var plans = new List<SpeedPlan>();
            try
            {
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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

        private int GetPercentile(IReadOnlyList<int> speeds, double percentile)
        {
            if (speeds.IsNullOrEmpty())
                return -1;
            var sortedSpeeds = speeds.OrderBy(x => x).ToList();
            var tempPercentileIndex = sortedSpeeds.Count * percentile - 1;
            var percentileIndex = 0;
            var percentileValue = 0;
            if (sortedSpeeds.Count > 3)
            {
                percentileIndex = Convert.ToInt32(Math.Round(tempPercentileIndex + 0.5));
                percentileValue = sortedSpeeds[percentileIndex];
            }
            else
            {
                percentileIndex = Convert.ToInt32(tempPercentileIndex);
                var speed1 = speeds[percentileIndex];
                var speed2 = speeds[percentileIndex + 1];
                double rawPercentile = (speed1 + speed2) / 2;
                percentileValue = Convert.ToInt32(Math.Round(rawPercentile));

            }
            return percentileValue;
        }

        public List<PlanSplitFail> GetSplitFailPlans(
            List<CycleSplitFail> cycles,
            SplitFailOptions options,
            Approach approach,
            IReadOnlyList<ControllerEventLog> events)
        {
            var planEvents = GetPlanEvents(options.Start, options.End, approach.Signal.SignalIdentifier, events.ToList());
            var plans = planEvents.Select((x, i) =>
            {
                var planCycles = cycles.Where(c => c.StartTime >= x.Timestamp && c.StartTime < (i + 1 < planEvents.Count ? planEvents[i + 1].Timestamp : options.End)).ToList();
                return new PlanSplitFail(x.Timestamp, i + 1 < planEvents.Count ? planEvents[i + 1].Timestamp : options.End, x.EventParam.ToString(), planCycles);
            })
                .ToList();
            return plans;
        }

    }
}