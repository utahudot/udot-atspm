#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.TimingAndActuation/GlobalGetDataTimingAndActuationsService.cs
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


