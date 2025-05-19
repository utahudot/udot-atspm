#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.Common/PlanService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.Atspm.Business.ApproachSpeed;
using Utah.Udot.Atspm.Business.SplitFail;
using Utah.Udot.Atspm.Business.TransitSignalPriority;
using Utah.Udot.Atspm.Business.YellowRedActivations;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;

namespace Utah.Udot.Atspm.Business.Common
{
    public class PlanService
    {

        public PlanService()
        {
        }

        public List<PurdueCoordinationPlan> GetPcdPlans(List<CyclePcd> cycles, DateTime startDate,
            DateTime endDate, Approach approach, List<IndianaEvent> events)
        {
            var planEvents = SetFirstAndLastPlan(startDate, endDate, approach.Location.LocationIdentifier, events.OrderBy(e => e.Timestamp).ToList());
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

        public List<IndianaEvent> SetFirstAndLastPlan(
            DateTime startDate,
            DateTime endDate,
            string locationId,
            List<IndianaEvent> tempPlanEvents)
        {
            tempPlanEvents = tempPlanEvents.OrderBy(t => t.Timestamp).ToList();
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Unspecified);
            if (!tempPlanEvents.IsNullOrEmpty() && tempPlanEvents[0].Timestamp != startDate)
            {
                SetFirstPlan(startDate, locationId, tempPlanEvents);
            }
            else if (!tempPlanEvents.Any())
            {
                SetFirstPlan(startDate, locationId, tempPlanEvents);
            }
            SetLastPlan(startDate, endDate, tempPlanEvents);
            if (!tempPlanEvents.Any())
            {
                tempPlanEvents.Add(new IndianaEvent { LocationIdentifier = locationId, EventCode = 131, EventParam = 254, Timestamp = endDate });
            }

            return tempPlanEvents;
        }

        private static void SetLastPlan(DateTime startDate, DateTime endDate, List<IndianaEvent> tempPlanEvents)
        {
            // Find the index of the last event within the start and end dates
            var lastPlan = tempPlanEvents.Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate).OrderBy(e => e.Timestamp).Last();
            int lastIndex = tempPlanEvents.IndexOf(lastPlan);

            if (lastIndex >= 0)
            {
                // Remove planEvents after the last event within the specified range
                tempPlanEvents.RemoveRange(lastIndex + 1, tempPlanEvents.Count - lastIndex - 1);
            }
            else
            {
                // If no event is found within the specified range, clear the list
                tempPlanEvents.Clear();
            }
        }

        private static void SetFirstPlan(DateTime startDate, string locationId, List<IndianaEvent> planEvents)
        {
            // Check if first plan has exact startDate as value in cleanPlanEvents
            var firstPlanEvent = planEvents.Find(e => e.Timestamp == startDate);
            firstPlanEvent ??= planEvents.Where(e => e.Timestamp < startDate).OrderByDescending(e => e.Timestamp).FirstOrDefault();

            if (firstPlanEvent != null)
            {
                // update the timestamp of the first event to match the start date
                firstPlanEvent.Timestamp = startDate;

                // remove all planEvents before the first event
                var indexToRemove = planEvents.IndexOf(firstPlanEvent);
                if (indexToRemove != -1)
                {
                    planEvents.RemoveRange(0, indexToRemove);
                }
            }
            else
            {
                // create a new event with the specified properties and add it to the beginning of the list
                firstPlanEvent = new IndianaEvent
                {
                    Timestamp = startDate,
                    EventCode = 131,
                    EventParam = 0,
                    LocationIdentifier = locationId
                };
                planEvents.Insert(0, firstPlanEvent);
            }
        }

        public IReadOnlyList<Plan> GetBasicPlans(
            DateTime startDate,
            DateTime endDate,
            string locationId,
            IReadOnlyList<IndianaEvent> events)
        {
            var planEvents = SetFirstAndLastPlan(startDate, endDate, locationId, events.ToList());
            var plans = planEvents.Select((x, i) => i == planEvents.Count - 1
                                        ? new Plan(x.EventParam.ToString(), x.Timestamp, endDate)
                                        : new Plan(x.EventParam.ToString(), x.Timestamp, planEvents[i + 1].Timestamp))
                                  .ToList();
            return plans;
        }

        public List<TransitSignalPriorityBasicPlan> GetTransitSignalPriorityBasicPlans(
            DateTime startDate,
            DateTime endDate,
            string locationId,
            IReadOnlyList<IndianaEvent> planEvents)
        {
            // Validate input parameters
            if (startDate >= endDate)
                throw new ArgumentException("startDate must be earlier than endDate.", nameof(startDate));
            if (string.IsNullOrWhiteSpace(locationId))
                throw new ArgumentException("locationId cannot be null or empty.", nameof(locationId));
            var plans = CreatePlansFromEvents(planEvents.ToList(), startDate, endDate);
            return plans.Select(p => new TransitSignalPriorityBasicPlan(p.PlanNumber, p.Start, p.End)).ToList();
        }
        /// <summary>
        /// Creates plan objects based on cleaned events and the given time range.
        /// </summary>
        private List<Plan> CreatePlansFromEvents(List<IndianaEvent> planEvents, DateTime startDate, DateTime endDate)
        {
            planEvents = planEvents.OrderBy(e => e.Timestamp).ToList();
            var plans = new List<Plan>();

            for (int i = 0; i < planEvents.Count; i++)
            {
                DateTime planStart = planEvents[i].Timestamp;
                DateTime planEnd = (i == planEvents.Count - 1) ? endDate : planEvents[i + 1].Timestamp;

                // If the plan's duration is longer than a day, adjust the end to be at the start of the next day.
                if ((planEnd - planStart).TotalDays > 1)
                {
                    planEnd = planStart.Date.AddDays(1);
                }

                plans.Add(new Plan(planEvents[i].EventParam.ToString(), planStart, planEnd));
            }

            return plans;
        }

        public IReadOnlyList<YellowRedActivationPlan> GetYellowRedActivationPlans(
            DateTime startDate,
            DateTime endDate,
            List<YellowRedActivationsCycle> cycles,
            string locationIdentifier,
            double severeRedLightViolationSeconds,
            IReadOnlyList<IndianaEvent> planEvents)
        {
            var plans = GetBasicPlans(
                startDate,
                endDate,
                locationIdentifier,
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
            string locationId,
            IList<IndianaEvent> planEvents)
        {
            //var planEvents = SetFirstAndLastPlan(startDate, endDate, locationId, events.ToList());
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
            DateTime endDate, Approach approach, List<IndianaEvent> events)
        {
            if (events.IsNullOrEmpty())
            {
                return new List<SpeedPlan>();
            }
            var planEvents = SetFirstAndLastPlan(startDate, endDate, approach.Location.LocationIdentifier, events);
            var plans = new List<SpeedPlan>();
            try
            {
                for (var i = 0; i < planEvents.Count; i++)
                {
                    var planStart = new DateTime();
                    var planEnd = new DateTime();
                    var planNumber = 0;
                    int? averageSpeed = null;
                    int? standardDeviation = null;
                    int? eightyFifthPercentile = null;
                    int? fifteenthPercentile = null;
                    List<int> speedEvents = null;
                    if (cycles
                        .SelectMany(c => c.SpeedEvents)
                        .Any(c => c.Timestamp >= planEvents[i].Timestamp && c.Timestamp < endDate))
                    {
                        speedEvents = cycles
                            .SelectMany(c => c.SpeedEvents)
                            .Where(c => c.Timestamp >= planEvents[i].Timestamp && c.Timestamp < endDate)
                            .Select(c => c.Mph)
                            .ToList();
                    }

                    if (planEvents.Count - 1 == i)
                    {
                        if (planEvents[i].Timestamp != endDate)
                        {
                            planStart = planEvents[i].Timestamp;
                            planEnd = endDate;
                            planNumber = planEvents[i].EventParam;
                            SetSpeedStatistics(
                                speedEvents,
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
                            planStart = planEvents[i].Timestamp;
                            planEnd = planEvents[i + 1].Timestamp;
                            planNumber = planEvents[i].EventParam;
                            SetSpeedStatistics(
                                speedEvents,
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
            List<int> speeds,
            out int? avgSpeed,
            out int? stdDev,
            out int? eightyFifth,
            out int? fifteenth)
        {
            avgSpeed = null;
            stdDev = null;
            eightyFifth = null;
            fifteenth = null;
            if (speeds != null && speeds.Count > 0)
            {
                var rawaverage = speeds.Average();
                avgSpeed = Convert.ToInt32(Math.Round(rawaverage));
                stdDev =
                    Convert.ToInt32(Math.Round(Math.Sqrt(speeds.Average(v => Math.Pow(v - rawaverage, 2)))));
                eightyFifth = GetPercentile(speeds, .85);
                fifteenth = GetPercentile(speeds, .15);
            }
        }

        private static int? GetPercentile(IReadOnlyList<int> speeds, double percentile)
        {
            if (speeds.IsNullOrEmpty())
                return null;
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
                var speed1 = (double)speeds[percentileIndex];
                var speed2 = (double)speeds[percentileIndex + 1];
                double rawPercentile = (speed1 + speed2) / 2;
                percentileValue = Convert.ToInt32(Math.Round(rawPercentile));

            }
            return percentileValue;
        }

        public List<PlanSplitFail> GetSplitFailPlans(
            List<CycleSplitFail> cycles,
            SplitFailOptions options,
            Approach approach,
            IReadOnlyList<IndianaEvent> events)
        {
            var planEvents = SetFirstAndLastPlan(options.Start, options.End, approach.Location.LocationIdentifier, events.ToList());
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