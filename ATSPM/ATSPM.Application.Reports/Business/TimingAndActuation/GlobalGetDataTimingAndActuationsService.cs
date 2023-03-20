using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.TimingAndActuation
{
    public class GlobalGetDataTimingAndActuationsService
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public GlobalGetDataTimingAndActuationsService(IControllerEventLogRepository controllerEventLogRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        public Dictionary<string, List<ControllerEventLog>> GetGlobalGetDataTimingAndActuations(string signalId, TimingAndActuationsOptions options)
        {
            var globalCustomEventsDictionary = new Dictionary<string, List<ControllerEventLog>>();
            if (options.GlobalEventCodesList != null && options.GlobalEventParamsList != null &&
                options.GlobalEventCodesList.Any() && options.GlobalEventCodesList.Count > 0 &&
                options.GlobalEventParamsList.Any() && options.GlobalEventParamsList.Count > 0)
            {
                foreach (var globalEventCode in options.GlobalEventCodesList)
                {
                    foreach (var globalEventParam in options.GlobalEventParamsList)
                    {
                        options.GlobalEventCounter = 1;
                        var globalCustomEvents = controllerEventLogRepository.GetEventsByEventCodesParam
                        (signalId, options.Start, options.End,
                            new List<int> { globalEventCode }, globalEventParam).ToList();
                        if (globalCustomEvents.Count > 0)
                        {
                            globalCustomEventsDictionary.Add("Global Events: Code: " + globalEventCode + " Param: " +
                                    globalEventParam,
                                    globalCustomEvents);
                        }
                    }
                }
            }
            return globalCustomEventsDictionary;
        }
    }
}


