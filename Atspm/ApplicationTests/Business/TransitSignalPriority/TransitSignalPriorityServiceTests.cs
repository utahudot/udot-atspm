using Xunit;
using Utah.Udot.Atspm.Business.TransitSignalPriorityRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Business.TransitSignalPriority;

namespace Utah.Udot.Atspm.Business.TransitSignalPriorityRequest.Tests
{
    public class TransitSignalPriorityServiceTests
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
}