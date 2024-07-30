using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System.Collections.Generic;
using Xunit;

namespace ATSPM.Application.Business.Common.Tests
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
