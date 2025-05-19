#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.TransitSignalPriority/TransitSignalPriorityCycle.cs
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

namespace Utah.Udot.Atspm.Business.TransitSignalPriority
{
    public class TransitSignalPriorityCycle
    {
        public int PhaseNumber { get; set; }
        public DateTime GreenEvent { get; set; }
        public DateTime MinGreen { get; set; }
        public DateTime YellowEvent { get; set; }
        public DateTime RedEvent { get; set; }
        public DateTime EndRedClearanceEvent { get; set; }
        public Double DurationSeconds { get { return (RedEvent - GreenEvent).TotalSeconds; } }
        public Double MinTime { get { return MinGreenDurationSeconds + YellowDurationSeconds + RedDurationSeconds; } }
        public Double GreenDurationSeconds { get { return (YellowEvent - GreenEvent).TotalSeconds; } }
        public Double MinGreenDurationSeconds { get { return (MinGreen - GreenEvent).TotalSeconds; } }
        public Double YellowDurationSeconds { get { return (RedEvent - YellowEvent).TotalSeconds; } }
        public Double RedDurationSeconds { get { return (EndRedClearanceEvent - RedEvent).TotalSeconds; } }
        public short? TerminationEvent { get; set; }
    }
}
