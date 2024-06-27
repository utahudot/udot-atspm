using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Linq;

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

        public string GetApproachDescription(int metricTypeId)
        {
            var detectors = Approach.GetDetectorsForMetricType(metricTypeId);
            var movements = detectors.Select(d => d.MovementType).Distinct().ToList();
            string movementResult;

            if (movements.Count == 1)
            {
                movementResult = movements.FirstOrDefault().GetDescription();
            }
            else
            {
                movementResult = string.Join(",", movements.Select(m => m.GetDescription()));
            }
            return $"{Approach.DirectionType.Description} "
                + $"{movementResult} "
                + (IsPermissivePhase ? "Permissive " : "Protected ")
                + (UseOverlap ? "Overlap " : "Phase ")
                + PhaseNumber;
        }
    }
}
