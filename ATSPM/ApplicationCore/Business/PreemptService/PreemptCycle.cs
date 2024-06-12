#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PreemptService/PreemptCycle.cs
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
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.PreemptService
{
    public class PreemptCycle
    {

        public PreemptCycle()
        {
            InputOn = new List<DateTime>();
            InputOff = new List<DateTime>();
            OtherPreemptStart = new List<DateTime>();
        }
        // public enum CycleState { InputOn, GateDown, InputOff, BeginTrackClearance, EntryStarted  };
        public List<DateTime> InputOff { get; set; }
        public List<DateTime> InputOn { get; set; }
        public List<DateTime> OtherPreemptStart { get; set; }
        public DateTime StartInputOn { get; set; }
        public DateTime CycleStart { get; set; }
        public DateTime CycleEnd { get; set; }
        public DateTime GateDown { get; set; }
        public DateTime EntryStarted { get; set; }
        public DateTime BeginTrackClearance { get; set; }
        public DateTime BeginDwellService { get; set; }
        public DateTime BeginExitInterval { get; set; }
        public DateTime LinkActive { get; set; }
        public DateTime LinkInactive { get; set; }
        public DateTime MaxPresenceExceeded { get; set; }
        public bool HasDelay { get; set; }
        public double Delay { get; set; }
        public double TimeToService { get; set; }
        public double DwellTime { get; set; }
        public double TimeToCallMaxOut { get; set; }
        public double TimeToEndOfEntryDelay { get; set; }
        public double TimeToTrackClear { get; set; }
        public double TimeToGateDown { get; set; }
    }
}