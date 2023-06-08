using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.PreempDetail
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
            List<ControllerEventLog> preemptEvents)
        {
            var uniquePreemptNumbers = preemptEvents.Select(x => x.EventParam).Distinct().ToList();   
            var preemptDetails = new List<PreemptDetail>();
            foreach(var preemptNumber in uniquePreemptNumbers)
            {
                var tempEvents = preemptEvents
                    .Where(x => x.EventParam == preemptNumber).ToList();
                var cycles = cycleService.CreatePreemptCycle(tempEvents);
                preemptDetails.Add( new PreemptDetail(
                    preemptDetailOptions.SignalId,
                    preemptDetailOptions.StartDate,
                    preemptDetailOptions.EndDate,
                    preemptNumber,
                    cycles));
            }

            return new PreemptDetailResult(
                preemptDetailOptions.SignalId,
                preemptDetailOptions.StartDate,
                preemptDetailOptions.EndDate,
                preemptDetails
                );
        }
        
    }
}