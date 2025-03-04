#region license
// Copyright 2025 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.ApplicationTests.Business.Common/PhaseDetailTests.cs
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

using System.Collections.Generic;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Xunit;

namespace Utah.Udot.Atspm.ApplicationTests.Business.Common
{
    public class PhaseDetailTests
    {
        [Fact]
        public void GetApproachDescription_NoMovements_ReturnsDirectionPhaseNumber()
        {
            // Arrange
            var directionType = new DirectionType { Description = "North" };
            var approach = new Approach
            {
                DirectionType = directionType,
                ProtectedPhaseNumber = 1,
                Detectors = null
            };
            var phaseDetail = new PhaseDetail
            {
                PhaseNumber = approach.ProtectedPhaseNumber,
                UseOverlap = false,
                IsPermissivePhase = false,
                Approach = approach
            };

            // Act
            var result = phaseDetail.GetApproachDescription();

            // Assert
            Assert.Equal("North Phase 1", result);
        }

        [Fact]
        public void GetApproachDescription_SingleMovement_ReturnsDirectionMovementTypePhaseNumber()
        {
            // Arrange
            var directionType = new DirectionType { Description = "South" };
            var approach = new Approach
            {
                DirectionType = directionType,
                ProtectedPhaseNumber = 2,
                Detectors = new List<Detector>
                {
                    new Detector { MovementType = MovementTypes.L }
                }
            };
            var phaseDetail = new PhaseDetail
            {
                PhaseNumber = approach.ProtectedPhaseNumber,
                UseOverlap = false,
                IsPermissivePhase = true,
                Approach = approach
            };

            // Act
            var result = phaseDetail.GetApproachDescription();

            // Assert
            Assert.Equal("South Left Permissive Phase 2", result);
        }

        [Fact]
        public void GetApproachDescription_MultipleMovements_ReturnsDirectionMovementTypesPhaseNumber()
        {
            // Arrange
            var directionType = new DirectionType { Description = "East" };
            var approach = new Approach
            {
                DirectionType = directionType,
                ProtectedPhaseNumber = 3,
                Detectors = new List<Detector>
                {
                    new Detector { MovementType = MovementTypes.L },
                    new Detector { MovementType = MovementTypes.R }
                }
            };
            var phaseDetail = new PhaseDetail
            {
                PhaseNumber = approach.ProtectedPhaseNumber,
                UseOverlap = true,
                IsPermissivePhase = false,
                Approach = approach
            };

            // Act
            var result = phaseDetail.GetApproachDescription();

            // Assert
            Assert.Equal("East Left,Right Protected Overlap 3", result);
        }

        [Fact]
        public void GetApproachDescription_NoDetectors_ReturnsDirectionPhaseNumber()
        {
            // Arrange
            var directionType = new DirectionType { Description = "West" };
            var approach = new Approach
            {
                DirectionType = directionType,
                ProtectedPhaseNumber = 4,
                Detectors = new List<Detector>()
            };
            var phaseDetail = new PhaseDetail
            {
                PhaseNumber = approach.ProtectedPhaseNumber,
                UseOverlap = true,
                IsPermissivePhase = false,
                Approach = approach
            };

            // Act
            var result = phaseDetail.GetApproachDescription();

            // Assert
            Assert.Equal("West Overlap 4", result);
        }
    }
}
