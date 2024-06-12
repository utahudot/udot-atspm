#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Common/PhaseTerminations.cs
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
using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Analysis.Common
{
    public class PhaseTerminations : ILocationPhaseLayer
    {
        public PhaseTerminations() { }

        public PhaseTerminations(IEnumerable<ControllerEventLog> terminationEvents)
        {
            TerminationEvents.AddRange(terminationEvents);
        }

        public string LocationIdentifier { get; set; }

        public int PhaseNumber { get; set; }

        public List<ControllerEventLog> TerminationEvents { get; set; } = new List<ControllerEventLog>();

        public IReadOnlyList<ITimestamp> GapOuts => TerminationEvents.Where(w => w.EventCode == (int)4).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> MaxOuts => TerminationEvents.Where(w => w.EventCode == (int)5).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> ForceOffs => TerminationEvents.Where(w => w.EventCode == (int)6).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> PedWalkBegins => TerminationEvents.Where(w => w.EventCode == (int)21).Cast<ITimestamp>().ToList();
        public IReadOnlyList<ITimestamp> UnknownTerminations => TerminationEvents.Where(w => w.EventCode == (int)IndianaEnumerations.PhaseGreenTermination).Cast<ITimestamp>().ToList();

        public override string ToString()
        {
            return $"{PhaseNumber} - {TerminationEvents.Count} - {GapOuts?.Count} - {MaxOuts?.Count} - {ForceOffs?.Count} - {PedWalkBegins?.Count} - {UnknownTerminations?.Count}";
        }
    }
}
