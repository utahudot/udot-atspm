﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PreempDetail/PreemptDetailService.cs
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

using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.PreemptService;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.TempModels;

namespace Utah.Udot.Atspm.Business.PreempDetail
{
    public class PreemptDetailService
    {
        private readonly CycleService cycleService;

        public PreemptDetailService(
            CycleService cycleService)
        {
            this.cycleService = cycleService;
        }

        public PreemptDetailResult GetChartData(
            PreemptDetailOptions preemptDetailOptions,
            List<IndianaEvent> preemptEvents)
        {
            var preemptServiceEventCodes = new List<short>() { 105 };
            var preemptServiceRequestEventCodes = new List<short>() { 102 };

            var uniquePreemptNumbers = preemptEvents.Select(x => x.EventParam).Distinct().ToList();
            var preemptDetails = new List<PreemptDetail>();
            PreemptRequestAndServices preemptSummary = new PreemptRequestAndServices(preemptDetailOptions.LocationIdentifier,
                preemptDetailOptions.Start,
                preemptDetailOptions.End);
            var requestAndServices = new List<RequestAndServices>();

            var preemptServiceEvents = preemptEvents.Where(e => preemptServiceEventCodes.Contains(e.EventCode));
            var preemptServiceRequestEvents = preemptEvents.Where(e => preemptServiceRequestEventCodes.Contains(e.EventCode));


            foreach (var preemptNumber in uniquePreemptNumbers)
            {
                var serviceEventsForNumber = preemptServiceEvents.Where(e => e.EventParam == preemptNumber).ToList();
                var requestEventsForNumber = preemptServiceRequestEvents.Where(e => e.EventParam == preemptNumber).ToList();
                var tempEvents = preemptEvents
                    .Where(x => x.EventParam == preemptNumber).ToList();
                var cycles = cycleService.CreatePreemptCycle(tempEvents);
                var cycleToCycleResult = createResultCycles(cycles, preemptDetailOptions);

                var requests = requestEventsForNumber.Select(e => e.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss")).ToList();
                var services = serviceEventsForNumber.Select(e => e.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss")).ToList();
                requestAndServices.Add(new RequestAndServices() { PreemptionNumber = preemptNumber, Requests = requests, Services = services });

                preemptDetails.Add(new PreemptDetail(
                    preemptDetailOptions.LocationIdentifier,
                    preemptDetailOptions.Start,
                    preemptDetailOptions.End,
                    preemptNumber,
                    cycleToCycleResult));
            }

            preemptSummary.RequestAndServices = requestAndServices;

            return new PreemptDetailResult(
                preemptDetails,
                preemptSummary
                );
        }

        private List<PreemptCycleResult> createResultCycles(List<PreemptCycle> cycles, PreemptDetailOptions options)
        {
            var result = cycles.Select(c => new PreemptCycleResult()
            {
                InputOff = c.InputOff != null && c.InputOff.Count > 0 ? c.InputOff[0] : null,
                InputOn = c.EntryStarted >= options.Start && c.EntryStarted <= options.End ? c.EntryStarted : c.InputOn[0],
                GateDown = c.GateDown >= options.Start && c.GateDown <= options.End ? c.GateDown : null,
                CallMaxOut = c.TimeToCallMaxOut,
                Delay = c.Delay,
                TimeToService = c.TimeToService,
                DwellTime = c.DwellTime,
                TrackClear = c.TimeToTrackClear,
            }).ToList();

            return result;
        }
    }
}