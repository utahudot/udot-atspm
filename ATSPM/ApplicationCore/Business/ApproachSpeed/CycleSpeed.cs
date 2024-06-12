#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.ApproachSpeed/CycleSpeed.cs
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

using ATSPM.Application.Business.Common;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.ApproachSpeed
{
    public class CycleSpeed : RedToRedCycle
    {
        public CycleSpeed(DateTime firstRedEvent, DateTime greenEvent, DateTime yellowEvent, DateTime lastRedEvent) :
            base(firstRedEvent, greenEvent, yellowEvent, lastRedEvent)
        {
        }

        public List<SpeedEvent> SpeedEvents { get; set; }

        public void FindSpeedEventsForCycle(List<SpeedEvent> speeds)
        {
            if (speeds.IsNullOrEmpty() || speeds.Count == 0)
                SpeedEvents = new List<SpeedEvent>();
            else
                SpeedEvents = speeds.Where(s =>
                    s.Timestamp >= GreenEvent.AddSeconds(15) && s.Timestamp < YellowEvent && s.Mph >= 5).ToList();
        }
    }
}