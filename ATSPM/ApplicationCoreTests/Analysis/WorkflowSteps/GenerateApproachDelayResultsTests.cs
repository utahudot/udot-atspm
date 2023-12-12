using ATSPM.Application.Analysis.ApproachDelay;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis.WorkflowSteps
{
    public class GenerateApproachDelayResultsTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public GenerateApproachDelayResultsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait(nameof(GenerateApproachDelayResults), "Data Check")]
        public void GenerateApproachDelayResultsCheckDataTest()
        {
            var sut = new GenerateApproachDelayResults();

            var testData = Enumerable.Range(1, 10).Select(s => new Vehicle()
            {
                locationIdentifier = "1001",
                PhaseNumber = 1,
                DetectorChannel = 2,
                Start = GetStaticDateTime(s),
                GreenEvent = GetStaticDateTime(s + 1),
                YellowEvent = GetStaticDateTime(s + 2),
                End = GetStaticDateTime(s + 3),
                CorrectedTimeStamp = GetStaticDateTime(s + Random.Shared.Next(0, 3))
            }).ToList();

            var result = sut.ExecuteAsync(testData).Result.First();

            Assert.Equal("1001", result.locationIdentifier);
            Assert.Equal(1, result.PhaseNumber);
            Assert.Equal(testData.Min(m => m.Start), result.Start);
            Assert.Equal(testData.Max(m => m.End), result.End);
            Assert.Equal(testData.Average(a => a.Delay), result.AverageDelay);
            Assert.Equal(testData.Sum(s => s.Delay) / 3600, result.TotalDelay);
        }

        [Fact]
        [Trait(nameof(GenerateApproachDelayResults), "Signal Grouping")]
        public async void GenerateApproachDelayResultsSignalGroupingTest()
        {
            var sut = new GenerateApproachDelayResults();

            var testData = Enumerable.Range(1, 10).Select(s => new Vehicle()
            {
                locationIdentifier = (1000 + Random.Shared.Next(1, 4)).ToString(),
                PhaseNumber = 1,
                DetectorChannel = 2,
                Start = GetStaticDateTime(s),
                GreenEvent = GetStaticDateTime(s + 1),
                YellowEvent = GetStaticDateTime(s + 2),
                End = GetStaticDateTime(s + 3),
                CorrectedTimeStamp = GetStaticDateTime(s + Random.Shared.Next(0, 3))
            }).ToList();

            var result = await sut.ExecuteAsync(testData);

            var actual = result.GroupBy(g => g.locationIdentifier).Select(s => s.Key);
            var expected = testData.GroupBy(g => g.locationIdentifier).Select(s => s.Key);

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(GenerateApproachDelayResults), "Phase Grouping")]
        public async void GenerateApproachDelayResultsPhaseGroupingTest()
        {
            var sut = new GenerateApproachDelayResults();

            var testData = Enumerable.Range(1, 10).Select(s => new Vehicle()
            {
                locationIdentifier = "1001",
                PhaseNumber = Random.Shared.Next(1, 4),
                DetectorChannel = 2,
                Start = GetStaticDateTime(s),
                GreenEvent = GetStaticDateTime(s + 1),
                YellowEvent = GetStaticDateTime(s + 2),
                End = GetStaticDateTime(s + 3),
                CorrectedTimeStamp = GetStaticDateTime(s + Random.Shared.Next(0, 3))
            }).ToList();

            var result = await sut.ExecuteAsync(testData);

            var actual = result.GroupBy(g => g.PhaseNumber).Select(s => s.Key);
            var expected = testData.GroupBy(g => g.PhaseNumber).Select(s => s.Key);

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(nameof(GenerateApproachDelayResults), "DetChannel Grouping")]
        public async void GenerateApproachDelayResultsDetChannelGroupingTest()
        {
            var sut = new GenerateApproachDelayResults();

            var testData = Enumerable.Range(1, 10).Select(s => new Vehicle()
            {
                locationIdentifier = "1001",
                PhaseNumber = 1,
                DetectorChannel = Random.Shared.Next(1, 4),
                Start = GetStaticDateTime(s),
                GreenEvent = GetStaticDateTime(s + 1),
                YellowEvent = GetStaticDateTime(s + 2),
                End = GetStaticDateTime(s + 3),
                CorrectedTimeStamp = GetStaticDateTime(s + Random.Shared.Next(0, 3))
            }).ToList();

            var result = await sut.ExecuteAsync(testData);

            var actual = result.Count();
            var expected = testData.GroupBy(g => g.DetectorChannel).Count();

            Assert.Equal(expected, actual);
        }

        private DateTime GetStaticDateTime(int offset)
        {
            return DateTime.Now.AddSeconds(offset);
        }

        public void Dispose()
        {
        }
    }
}
