using ATSPM.Application.Analysis;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ApplicationCoreTests.Analysis
{
    public class GenerateApproachDelayResultsTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private StringWriter _consoleOut = new StringWriter();

        public GenerateApproachDelayResultsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait(nameof(GenerateApproachDelayResults), "Data Check")]
        public async void GenerateApproachDelayResultsCheckDataTest()
        {
            var sut = new GenerateApproachDelayResults();

            var testData = Enumerable.Range(1, 10).Select(s => new Vehicle()
            {
                //SignalId = (s + 1000).ToString(),
                //Phase = Random.Shared.Next(1, 8),
                //DetChannel = Random.Shared.Next(1, 8),
                SignalId = "1001",
                Phase = 1,
                DetChannel = 2,
                StartTime = GetStaticDateTime(s),
                GreenEvent = GetStaticDateTime(s + 1),
                YellowEvent = GetStaticDateTime(s + 2),
                EndTime = GetStaticDateTime(s + 3),
                TimeStamp = GetStaticDateTime(s + Random.Shared.Next(0, 3))
            }).ToList();

            var result = await sut.ExecuteAsync(testData);

            Assert.Equal("1001", result.SignalId);
            Assert.Equal(1, result.Phase);
            Assert.Equal(testData.Min(m => m.StartTime), result.Start);
            Assert.Equal(testData.Max(m => m.EndTime), result.End);
            Assert.Equal(testData.Average(a => a.Delay), result.AverageDelay);
            Assert.Equal(testData.Sum(s => s.Delay) / 3600, result.TotalDelay);
        }

        //TODO: verify that all phases and detector channels sum to a single result or if they need to be different
        [Fact]
        [Trait(nameof(GenerateApproachDelayResults), "Signal Grouping")]
        public async void GenerateApproachDelayResultsSignalGroupingTest()
        {
            var sut = new GenerateApproachDelayResults();

            var testData = Enumerable.Range(1, 10).Select(s => new Vehicle()
            {
                SignalId = (1000 + Random.Shared.Next(1, 3)).ToString(),
                Phase = 1,
                DetChannel = 2,
                StartTime = GetStaticDateTime(s),
                GreenEvent = GetStaticDateTime(s + 1),
                YellowEvent = GetStaticDateTime(s + 2),
                EndTime = GetStaticDateTime(s + 3),
                TimeStamp = GetStaticDateTime(s + Random.Shared.Next(0, 3))
            }).ToList();

            var result = await sut.ExecuteAsync(testData);

            Assert.False(true);
        }

        //TODO: verify that all phases and detector channels sum to a single result or if they need to be different
        [Fact]
        [Trait(nameof(GenerateApproachDelayResults), "Phase Grouping")]
        public async void GenerateApproachDelayResultsPhaseGroupingTest()
        {
            var sut = new GenerateApproachDelayResults();

            var testData = Enumerable.Range(1, 10).Select(s => new Vehicle()
            {
                SignalId = "1001",
                Phase = Random.Shared.Next(1, 3),
                DetChannel = 2,
                StartTime = GetStaticDateTime(s),
                GreenEvent = GetStaticDateTime(s + 1),
                YellowEvent = GetStaticDateTime(s + 2),
                EndTime = GetStaticDateTime(s + 3),
                TimeStamp = GetStaticDateTime(s + Random.Shared.Next(0, 3))
            }).ToList();

            var result = await sut.ExecuteAsync(testData);

            Assert.False(true);
        }

        //TODO: verify that all phases and detector channels sum to a single result or if they need to be different
        [Fact]
        [Trait(nameof(GenerateApproachDelayResults), "DetChannel Grouping")]
        public async void GenerateApproachDelayResultsDetChannelGroupingTest()
        {
            var sut = new GenerateApproachDelayResults();

            var testData = Enumerable.Range(1, 10).Select(s => new Vehicle()
            {
                SignalId = "1001",
                Phase = 1,
                DetChannel = Random.Shared.Next(1, 3),
                StartTime = GetStaticDateTime(s),
                GreenEvent = GetStaticDateTime(s + 1),
                YellowEvent = GetStaticDateTime(s + 2),
                EndTime = GetStaticDateTime(s + 3),
                TimeStamp = GetStaticDateTime(s + Random.Shared.Next(0, 3))
            }).ToList();

            var result = await sut.ExecuteAsync(testData);

            Assert.False(true);
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
