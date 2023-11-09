using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.ReportApi.Business.Common
{
    public class PhaseService
    {
        public virtual List<PhaseDetail> GetPhases(Signal signal)
        {
            if (signal.Approaches == null || signal.Approaches.Count == 0)
            {
                return new List<PhaseDetail>();
            }

            var phaseDetails = new List<PhaseDetail>();
            foreach (var approach in signal.Approaches)
            {
                if (approach.ProtectedPhaseNumber != 0)
                {
                    phaseDetails.Add(new PhaseDetail
                    {
                        PhaseNumber = approach.ProtectedPhaseNumber,
                        UseOverlap = approach.IsProtectedPhaseOverlap,
                        Approach = approach
                    });
                }
                if (approach.PermissivePhaseNumber != null)
                {
                    phaseDetails.Add(new PhaseDetail
                    {
                        PhaseNumber = approach.PermissivePhaseNumber.Value,
                        UseOverlap = approach.IsPermissivePhaseOverlap,
                        Approach = approach
                    });
                }
            }

            var groupedPhaseDetails = phaseDetails
                .GroupBy(p => new { p.PhaseNumber, p.UseOverlap });

            // Create a new list to store combined phase details
            var combinedPhaseDetails = new List<PhaseDetail>();

            foreach (var group in groupedPhaseDetails)
            {
                //first item from each group. 
                var representative = group.First();
                combinedPhaseDetails.Add(representative);
            }

            return combinedPhaseDetails;
        }
    }

    public class PhaseDetail
    {
        public int PhaseNumber { get; set; }
        public bool UseOverlap { get; set; }
        public Approach Approach { get; set; }
        public bool IsPermissivePhase { get { if (Approach != null) { return PhaseNumber != Approach.ProtectedPhaseNumber; } else return false; } }
    }
}
