#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.Common/PhaseDetail.cs
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

using System.Text;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Business.Common
{
    public class PhaseDetail
    {
        public int PhaseNumber { get; set; }
        public bool UseOverlap { get; set; }
        public Approach Approach { get; set; }
        public bool IsPermissivePhase { get; set; }

        public string GetApproachDescriptionWithMovements(DetectionTypes detectionTypes)
        {
            var movements = Approach.Detectors?
                .Where(d => d.DetectionTypes.Select(d => d.Id).Contains(detectionTypes))
                .Select(d => d.MovementType)
                .Distinct()
                .ToList();
            string movementResult = GetMovementResult(movements);
            return BuildApproachDescription(movementResult);
        }

        public string GetApproachDescription()
        {
            var movements = Approach.Detectors?
                .Select(d => d.MovementType)
                .Distinct()
                .ToList();
            string movementResult = GetMovementResult(movements);
            return BuildApproachDescription(movementResult);
        }

        private string BuildApproachDescription(string movementResult)
        {
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
