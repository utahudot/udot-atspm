using ATSPM.Application.Extensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.OpenApi.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATSPM.Application.Business.Common
{
    public class PhaseDetail
    {
        public int PhaseNumber { get; set; }
        public bool UseOverlap { get; set; }
        public Approach Approach { get; set; }
        public bool IsPermissivePhase { get; set; }

        public string GetApproachDescription()
        {
            var movements = Approach.Detectors?.Select(d => d.MovementType).Distinct().ToList();
            string movementResult = GetMovementResult(movements);

            var descriptionResult = new StringBuilder($"{Approach.DirectionType.Description} ");
            if (!string.IsNullOrEmpty(movementResult) && movementResult.Contains("Left"))
            {
                descriptionResult.Append($"{movementResult} ");
                descriptionResult.Append(IsPermissivePhase ? "Permissive " : "Protected ");
            }
            descriptionResult.Append(UseOverlap ? "Overlap " : "Phase ");
            descriptionResult.Append(PhaseNumber);

            return descriptionResult.ToString();
        }

        private string GetMovementResult(List<MovementTypes> movements)
        {
            if (movements == null || !movements.Any())
            {
                return string.Empty;
            }

            return movements.Count == 1
                ? movements.First().GetDisplayName()
                : string.Join(",", movements.Select(m => m.GetDisplayName()));
        }


    }

}
