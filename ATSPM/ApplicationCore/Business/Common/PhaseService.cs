#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/PhaseService.cs
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
using ATSPM.Data.Models;
using System.Collections.Generic;

namespace ATSPM.Application.Business.Common
{
    public class PhaseService
    {
        public virtual List<PhaseDetail> GetPhases(Location Location)
        {
            if (Location.Approaches == null || Location.Approaches.Count == 0)
            {
                return new List<PhaseDetail>();
            }

            var phaseDetails = new List<PhaseDetail>();
            foreach (var approach in Location.Approaches)
            {
                if (approach.ProtectedPhaseNumber != 0)
                {
                    phaseDetails.Add(new PhaseDetail
                    {
                        PhaseNumber = approach.ProtectedPhaseNumber,
                        UseOverlap = approach.IsProtectedPhaseOverlap,
                        IsPermissivePhase = false,
                        Approach = approach
                    });
                }
                if (approach.PermissivePhaseNumber != null)
                {
                    phaseDetails.Add(new PhaseDetail
                    {
                        PhaseNumber = approach.PermissivePhaseNumber.Value,
                        UseOverlap = approach.IsPermissivePhaseOverlap,
                        IsPermissivePhase = true,
                        Approach = approach
                    });
                }
            }
            return phaseDetails;

            //This compbines the phases and removes permissive phases. Not sure this is what we want in all scenarios.
            //var groupedPhaseDetails = phaseDetails
            //    .GroupBy(p => new { p.PhaseNumber, p.UseOverlap });

            //// Create a new list to store combined phase details
            //var combinedPhaseDetails = new List<PhaseDetail>();

            //foreach (var group in groupedPhaseDetails)
            //{
            //    //first item from each group. 
            //    var representative = group.First();
            //    combinedPhaseDetails.Add(representative);
            //}

            //return combinedPhaseDetails;
        }
    }

    public class PhaseDetail
    {
        public int PhaseNumber { get; set; }
        public bool UseOverlap { get; set; }
        public Approach Approach { get; set; }
        public bool IsPermissivePhase { get; set; }
    }
}
