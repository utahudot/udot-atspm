using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data.Models.EventLogModels;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.TimingAndActuation
{
    public class GlobalGetDataTimingAndActuationsService
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;

        public GlobalGetDataTimingAndActuationsService(IIndianaEventLogRepository controllerEventLogRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        public Dictionary<string, List<IndianaEvent>> GetGlobalGetDataTimingAndActuations(string locationId, TimingAndActuationsOptions options)
        {
            var globalCustomEventsDictionary = new Dictionary<string, List<IndianaEvent>>();
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
                        (
                            locationId,
                            options.Start,
                            options.End,
                            new List<short> { globalEventCode },
                            globalEventParam
                        )
                        .ToList();
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


