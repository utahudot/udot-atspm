#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/DoubleExtensions.cs
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

using Microsoft.OpenApi.Extensions;
using System.ComponentModel.DataAnnotations;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Extensions for <see cref="PhaseDetail"/>
    /// </summary>
    public static class PhaseDetailsExtensions
    {
        private static string GetApproachDescription(this PhaseDetail phaseDetail)
        {
            DirectionTypes direction = phaseDetail.Approach.DirectionTypeId;
            string directionTypeName = direction.GetAttributeOfType<DisplayAttribute>().Name;
            var ignoreDetectionTypes = new List<DetectionTypes> { DetectionTypes.AC, DetectionTypes.AS, DetectionTypes.AP };
            var filteredDetectors = phaseDetail.Approach.Detectors.Where(d => d.DetectionTypes.Any(t => !ignoreDetectionTypes.Contains(t.Id)));
            string approachDescription = "";
            if (filteredDetectors.Any())
            {
                MovementTypes movementType = filteredDetectors.ToList()[0].MovementType;
                string movementTypeName = movementType.GetAttributeOfType<DisplayAttribute>().Name;
                approachDescription = $"{directionTypeName} {movementTypeName} Ph{phaseDetail.PhaseNumber}";
            }
            else
            {
                approachDescription = $"{directionTypeName} Ph{phaseDetail.PhaseNumber}";
            }
            return approachDescription;
        }
    }
}
