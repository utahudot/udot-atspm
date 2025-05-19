#region license
// Copyright 2025 Utah Departement of Transportation
// for ApplicationTests - Utah.Udot.Atspm.Business.TransitSignalPriorityRequest.Tests/TransitSignalPriorityServiceTests.cs
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
using System.Linq;
using Utah.Udot.Atspm.Business.TransitSignalPriority;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Xunit;

namespace Utah.Udot.Atspm.Business.TransitSignalPriorityRequest.Tests
{
    public class CalculateTransitSignalPriorityMaxTests
    {
        [Fact]
        public void CalculateTransitSignalPriorityMax_Given_AllZeroPhaseValues_Should_SetRecommendedTSPMaxToNull()
        {
            // Arrange
            var input = CreateTestInput(programmedSplit: 0, minTime: 0, percentile50th: 0, percentile85th: 0, percentSkips: 0, minGreen: 0, yellow: 0, redClearance: 0);

            // Act
            var result = TransitSignalPriorityService.CalculateTransitSignalPriorityMax(input);

            // Assert
            Assert.Null(result.Item2[0].Phases[0].RecommendedTSPMax);
            Assert.Equal("Phase not in use", result.Item2[0].Phases[0].Notes);
        }

        [Fact]
        public void CalculateTransitSignalPriorityMax_Given_ProgrammedSplitZero_Should_SetRecommendedTSPMaxToNull()
        {
            // Arrange
            var input = CreateTestInput(programmedSplit: 0, minTime: 10);

            // Act
            var result = TransitSignalPriorityService.CalculateTransitSignalPriorityMax(input);

            // Assert
            Assert.Null(result.Item2[0].Phases[0].RecommendedTSPMax);
            Assert.Equal("Programmed split is zero, manually calculate", result.Item2[0].Phases[0].Notes);
        }

        [Fact]
        public void CalculateTransitSignalPriorityMax_Given_SkipsGreaterThan70_Should_SetRecommendedTSPMaxToSkipsGreaterThan70TSPMax()
        {
            // Arrange
            var input = CreateTestInput(programmedSplit: 50, minTime: 20, percentSkips: 75);

            // Act
            var result = TransitSignalPriorityService.CalculateTransitSignalPriorityMax(input);

            // Assert
            var phase = result.Item2[0].Phases[0];
            Assert.Equal(30, phase.RecommendedTSPMax);
            Assert.Equal("Skips greater than 70%", phase.Notes);
        }

        [Fact]
        public void CalculateTransitSignalPriorityMax_Given_PercentMaxOutsForceOffs_LessThan40_Should_SetRecommendedTSPMaxToForceOffsGreaterThan40TSPMax()
        {
            // Arrange
            var input = CreateTestInput(programmedSplit: 50, minTime: 20, percentile50th: 30, percentMaxOutsForceOffs: 30);

            // Act
            var result = TransitSignalPriorityService.CalculateTransitSignalPriorityMax(input);

            // Assert
            var phase = result.Item2[0].Phases[0];
            Assert.Equal(25, phase.RecommendedTSPMax);
            Assert.Equal("Force offs less than 40%", phase.Notes);
        }

        [Fact]
        public void CalculateTransitSignalPriorityMax_Given_PercentMaxOutsForceOffs_LessThan60_Should_SetRecommendedTSPMaxToForceOffsLessThan60TSPMax()
        {
            // Arrange
            var input = CreateTestInput(programmedSplit: 50, minTime: 20, percentile50th: 30, percentMaxOutsForceOffs: 50);

            // Act
            var result = TransitSignalPriorityService.CalculateTransitSignalPriorityMax(input);

            // Assert
            var phase = result.Item2[0].Phases[0];
            Assert.Equal(20, phase.RecommendedTSPMax);
            Assert.Equal("Force offs less than 60%", phase.Notes);
        }

        [Fact]
        public void CalculateTransitSignalPriorityMax_Given_PercentMaxOutsForceOffs_LessThan80_Should_SetRecommendedTSPMaxToForceOffsLessThan80TSPMax()
        {
            // Arrange
            var input = CreateTestInput(programmedSplit: 50, minTime: 20, percentile50th: 30, percentMaxOutsForceOffs: 70, percentile85th: 30);

            // Act
            var result = TransitSignalPriorityService.CalculateTransitSignalPriorityMax(input);

            // Assert
            var phase = result.Item2[0].Phases[0];
            Assert.Equal(20, phase.RecommendedTSPMax);
            Assert.Equal("Force offs less than 80%", phase.Notes);
        }

        [Fact]
        public void CalculateTransitSignalPriorityMax_NoRecommendedTSPMax()
        {
            // Arrange
            var input = CreateTestInput(percentSkips: 50, programmedSplit: 50, minTime: 20, percentile50th: 30, percentMaxOutsForceOffs: 820, percentile85th: 30);

            // Act
            var result = TransitSignalPriorityService.CalculateTransitSignalPriorityMax(input);

            // Assert
            var phase = result.Item2[0].Phases[0];
            Assert.Equal(null, phase.RecommendedTSPMax);
            Assert.Equal("No recommended TSP Max", phase.Notes);
        }



        [Fact]
        public void Given_NegativeRecommendedTSPMax_Should_SetToNull()
        {
            // Arrange
            var input = CreateTestInput(programmedSplit: 10, minTime: 20);

            // Act
            var result = TransitSignalPriorityService.CalculateTransitSignalPriorityMax(input);

            // Assert
            var phase = result.Item2[0].Phases[0];
            Assert.Null(phase.RecommendedTSPMax);
            Assert.Equal("No recommended TSP Max", phase.Notes);
        }

        private (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>) CreateTestInput(
            int programmedSplit = 50, int minTime = 20, int percentile50th = 25, int percentile85th = 30,
            double percentSkips = 0, double percentMaxOutsForceOffs = 0, int minGreen = 5, int yellow = 3, int redClearance = 2)
        {
            var phase = new TransitSignalPhase
            {
                ProgrammedSplit = programmedSplit,
                MinTime = minTime,
                PercentileSplit50th = percentile50th,
                PercentileSplit85th = percentile85th,
                PercentSkips = percentSkips,
                PercentMaxOutsForceOffs = percentMaxOutsForceOffs,
                MinGreen = minGreen,
                Yellow = yellow,
                RedClearance = redClearance
            };

            var plan = new TransitSignalPriorityPlan { Phases = new List<TransitSignalPhase> { phase } };

            return (new TransitSignalLoadParameters(), new List<TransitSignalPriorityPlan> { plan });
        }
    }

    public class CalculateMaxExtensionTests
    {
        [Fact]
        public void Given_AllZeroPhaseValues_Should_SetMaxReductionAndMaxExtensionToZero()
        {
            // Arrange
            var input = CreateTestInput(
                programmedSplit: 0, minTime: 0, percentile50th: 0, percentile85th: 0, percentSkips: 0,
                minGreen: 0, yellow: 0, redClearance: 0, recommendedTSPMax: null, phaseNumber: 1
            );

            // Act
            var result = TransitSignalPriorityService.CalculateMaxExtension(input);

            // Assert
            var phase = result.Item2[0].Phases[0];
            Assert.Equal(0, phase.MaxReduction);
            Assert.Equal(0, phase.MaxExtension);
            Assert.Equal(0, phase.PriorityMin);
            Assert.Equal(0, phase.PriorityMax);
        }

        [Fact]
        public void Given_DesignatedPhase_Should_CalculateMaxExtensionBasedOnNonDesignatedPhases()
        {
            // Arrange
            var input = CreateTestInput(
                programmedSplit: 40, minTime: 10, recommendedTSPMax: 10, phaseNumber: 1,
                designatedPhases: new List<int> { 1 } // Phase 1 is designated
            );

            // Add additional non-designated phases with TSP Max values
            input.Item2[0].Phases.AddRange(new List<TransitSignalPhase>
        {
            new TransitSignalPhase { PhaseNumber = 2, RecommendedTSPMax = 5 },
            new TransitSignalPhase { PhaseNumber = 3, RecommendedTSPMax = 7 }
        });

            // Act
            var result = TransitSignalPriorityService.CalculateMaxExtension(input);

            // Assert
            var phase = result.Item2[0].Phases.First(p => p.PhaseNumber == 1);
            Assert.Equal(10, phase.MaxReduction);
            Assert.Equal(12, phase.MaxExtension); // Sum of 5 and 7
            Assert.Equal(30, phase.PriorityMin); // ProgrammedSplit - RecommendedTSPMax (40 - 10)
            Assert.Equal(50, phase.PriorityMax); // ProgrammedSplit + RecommendedTSPMax (40 + 10)
        }

        [Fact]
        public void Given_NonDesignatedPhase_Should_HaveZeroMaxExtension()
        {
            // Arrange
            var input = CreateTestInput(
                programmedSplit: 40, minTime: 10, recommendedTSPMax: 10, phaseNumber: 5,
                designatedPhases: new List<int> { 1 } // Phase 5 is NOT designated
            );

            // Act
            var result = TransitSignalPriorityService.CalculateMaxExtension(input);

            // Assert
            var phase = result.Item2[0].Phases.First(p => p.PhaseNumber == 5);
            Assert.Equal(10, phase.MaxReduction);
            Assert.Equal(0, phase.MaxExtension); // Not a designated phase, so should be 0
            Assert.Equal(30, phase.PriorityMin);
            Assert.Equal(50, phase.PriorityMax);
        }

        [Fact]
        public void Given_EmptyDesignatedPhases_Should_NotAffectMaxExtensionCalculation()
        {
            // Arrange
            var input = CreateTestInput(
                programmedSplit: 50, minTime: 15, recommendedTSPMax: 12, phaseNumber: 3,
                designatedPhases: new List<int>() // No designated phases
            );

            // Act
            var result = TransitSignalPriorityService.CalculateMaxExtension(input);

            // Assert
            var phase = result.Item2[0].Phases.First(p => p.PhaseNumber == 3);
            Assert.Equal(12, phase.MaxReduction);
            Assert.Equal(0, phase.MaxExtension); // No designated phases should result in 0 MaxExtension
            Assert.Equal(38, phase.PriorityMin); // ProgrammedSplit - RecommendedTSPMax
            Assert.Equal(62, phase.PriorityMax); // ProgrammedSplit + RecommendedTSPMax
        }

        [Fact]
        public void Given_NegativeRecommendedTSPMax_Should_SetMaxReductionToZero()
        {
            // Arrange
            var input = CreateTestInput(
                programmedSplit: 30, minTime: 20, recommendedTSPMax: -5, phaseNumber: 7
            );

            // Act
            var result = TransitSignalPriorityService.CalculateMaxExtension(input);

            // Assert
            var phase = result.Item2[0].Phases[0];
            Assert.Equal(0, phase.MaxReduction); // Should not allow negative values
            Assert.Equal(0, phase.PriorityMin);
            Assert.Equal(0, phase.PriorityMax);
            Assert.Equal(0, phase.MaxExtension);
        }

        private (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>) CreateTestInput(
            int programmedSplit = 50, int minTime = 20, int percentile50th = 25, int percentile85th = 30,
            double percentSkips = 0, double percentMaxOutsForceOffs = 0, int minGreen = 5, int yellow = 3, int redClearance = 2,
            double? recommendedTSPMax = null, int phaseNumber = 1, List<int> designatedPhases = null)
        {
            var phase = new TransitSignalPhase
            {
                PhaseNumber = phaseNumber,
                ProgrammedSplit = programmedSplit,
                MinTime = minTime,
                PercentileSplit50th = percentile50th,
                PercentileSplit85th = percentile85th,
                PercentSkips = percentSkips,
                PercentMaxOutsForceOffs = percentMaxOutsForceOffs,
                MinGreen = minGreen,
                Yellow = yellow,
                RedClearance = redClearance,
                RecommendedTSPMax = recommendedTSPMax
            };

            var plan = new TransitSignalPriorityPlan { Phases = new List<TransitSignalPhase> { phase } };

            var parameters = new TransitSignalLoadParameters
            {
                LocationPhases = new LocationPhases
                {
                    DesignatedPhases = designatedPhases ?? new List<int>()
                }
            };

            return (parameters, new List<TransitSignalPriorityPlan> { plan });
        }
    }


}