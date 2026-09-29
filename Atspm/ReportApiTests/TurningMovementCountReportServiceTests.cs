#region license
// Copyright 2026 Utah Departement of Transportation
// for ReportApiTests - ReportApiTests/TurningMovementCountReportServiceTests.cs
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

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Moq;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.ReportApi.ReportServices;

namespace ReportApiTests;

public class TurningMovementCountReportServiceTests
{
    private static readonly DateTime Start = new(2026, 4, 1, 8, 0, 0);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(7)]
    [InlineData(61)]
    public async Task InvalidBinSize_IsRejectedBeforeReadingData(int binSize)
    {
        var service = new TurningMovementCountReportService(
            new Mock<IIndianaEventLogRepository>(MockBehavior.Strict).Object,
            new TurningMovementCountsService(),
            new Mock<ILocationRepository>(MockBehavior.Strict).Object,
            new PlanService());

        await Assert.ThrowsAsync<ValidationException>(() => service.ExecuteAsync(
            new TurningMovementCountsOptions { Start = Start, End = Start.AddHours(1), BinSize = binSize }, null, default));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task InvalidTimeRange_IsRejectedBeforeReadingData(int duration)
    {
        var service = new TurningMovementCountReportService(
            new Mock<IIndianaEventLogRepository>(MockBehavior.Strict).Object,
            new TurningMovementCountsService(),
            new Mock<ILocationRepository>(MockBehavior.Strict).Object,
            new PlanService());

        await Assert.ThrowsAsync<ValidationException>(() => service.ExecuteAsync(
            new TurningMovementCountsOptions { Start = Start, End = Start.AddMinutes(duration), BinSize = 15 }, null, default));
    }

    [Theory]
    [InlineData(5, 30, 1.0)]
    [InlineData(15, 30, 1.0)]
    [InlineData(60, 30, 1.0)]
    [InlineData(5, 90, 0.5)]
    [InlineData(15, 90, 0.5)]
    [InlineData(60, 90, 0.5)]
    public async Task ChartAndTable_UseFifteenMinutePhfRegardlessOfDisplayBin(int binSize, int firstQuarter, double expectedFactor)
    {
        var result = await Run(new[] { Detector(1) }, Events(1, firstQuarter, 30, 30, 30), binSize);
        var chart = Assert.Single(result.Charts);
        Assert.Equal(firstQuarter + 90, result.PeakHour!.Value.Value);
        Assert.Equal(expectedFactor, result.PeakHourFactor);
        Assert.Equal(expectedFactor, chart.PeakHourFactor);
        Assert.Equal(result.PeakHour.Value.Value, chart.PeakHourVolume);
        Assert.Equal(result.PeakHour.Value.Value, Assert.Single(result.Table).PeakHourVolume.Value);
    }

    [Fact]
    public async Task MissingLaneNumber_PreservesCountsAndOtherMovements()
    {
        var unassigned = Detector(2, null, MovementTypes.R);
        var result = await Run(new[] { Detector(1), unassigned },
            Events(1, 10, 10, 10, 10).Concat(Events(2, 1, 1, 1, 1)).ToList());

        Assert.Equal(2, result.Charts.Count);
        Assert.Equal(44, result.PeakHour!.Value.Value);
        var chart = Assert.Single(result.Charts.Where(c => c.MovementType == "Right"));
        Assert.Equal(4, chart.TotalVolume);
        Assert.Null(Assert.Single(chart.Lanes).LaneNumber);
        Assert.Null(chart.LaneUtilizationFactor);
    }

    [Fact]
    public async Task MultipleDetectorsInOneLane_DoNotReduceLaneUtilization()
    {
        var result = await Run(new[] { Detector(1), Detector(2) },
            Events(1, 10, 10, 10, 10).Concat(Events(2, 10, 10, 10, 10)).ToList());
        var chart = Assert.Single(result.Charts);
        Assert.Single(chart.Lanes);
        Assert.Equal(80, chart.TotalVolume);
        Assert.Equal(1, chart.LaneUtilizationFactor);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SharedChannel_IsCountedOnceWithinMovement(bool combine)
    {
        var result = await Run(new[] { Detector(1), Detector(1, 2, combine ? MovementTypes.TR : MovementTypes.T) },
            Events(1, 10, 10, 10, 10), combine: combine);
        var chart = Assert.Single(result.Charts);
        Assert.Equal(40, chart.TotalVolume);
        Assert.Equal(40, result.PeakHour!.Value.Value);
        Assert.Null(Assert.Single(chart.Lanes).LaneNumber);
        Assert.Equal(combine ? "Thru + Thru-Right" : "Thru", chart.MovementType);
    }

    [Theory]
    [InlineData(60)]
    [InlineData(61)]
    public async Task LatencyCorrection_FiltersUsingCorrectedTimestamps(int duration)
    {
        var detector = Detector(1);
        detector.LatencyCorrection = 1;
        var end = Start.AddMinutes(duration);
        var result = await Run(new[] { detector }, new List<IndianaEvent>
        {
            Event(1, Start.AddMilliseconds(500)),
            Event(1, Start.AddMilliseconds(1500)),
            Event(1, end.AddMilliseconds(500)),
            Event(1, end.AddMilliseconds(1500))
        }, duration: duration);

        Assert.Equal(2, Assert.Single(result.Charts).TotalVolume);
        Assert.Equal(2, Assert.Single(result.Table).Volumes.Sum(v => v.Value));
    }

    [Fact]
    public async Task ForwardCorrection_DoesNotCountPastPartialFinalBin()
    {
        var detector = Detector(1);
        detector.LatencyCorrection = -1;
        var result = await Run(new[] { detector }, new List<IndianaEvent>
        {
            Event(1, Start.AddMilliseconds(-500)),
            Event(1, Start.AddMinutes(61).AddMilliseconds(-500))
        }, duration: 61);

        Assert.Equal(1, Assert.Single(result.Charts).TotalVolume);
    }

    [Theory]
    [InlineData(30, 10)]
    [InlineData(60, 0)]
    public async Task ShortOrZeroTrafficReport_HasNoPeakHour(int duration, int count)
    {
        var result = await Run(new[] { Detector(1) }, Events(1, count), duration: duration);
        var chart = Assert.Single(result.Charts);
        Assert.Equal(count, chart.TotalVolume);
        Assert.Null(result.PeakHour);
        Assert.Null(result.PeakHourFactor);
        Assert.Null(chart.PeakHour);
        Assert.Null(chart.PeakHourVolume);
        Assert.Null(chart.PeakHourFactor);
        Assert.Null(Assert.Single(result.Table).PeakHourVolume);
    }

    [Fact]
    public async Task PartialFinalHour_IsNotAValidPeakWindow()
    {
        var result = await Run(new[] { Detector(1) }, Events(1, 0, 10, 10, 10, 100), duration: 61);
        Assert.Equal(Start, result.PeakHour!.Value.Key);
        Assert.Equal(30, result.PeakHour.Value.Value);
        var chart = Assert.Single(result.Charts);
        Assert.Equal(130, chart.TotalVolume);
        Assert.Equal("08:00 - 09:00", chart.PeakHour);
        Assert.Equal(30, chart.PeakHourVolume);
        Assert.Equal(0.75, chart.PeakHourFactor);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LeftThruRightMovement_IsIncluded(bool combine)
    {
        var result = await Run(new[] { Detector(1, 1, MovementTypes.LTR) }, Events(1, 10, 10, 10, 10), combine: combine);
        Assert.Equal("left-thru-right", Assert.Single(result.Charts).MovementType);
        Assert.Equal(40, Assert.Single(result.Table).PeakHourVolume.Value);
    }

    [Fact]
    public async Task VehiclePeak_ExcludesOtherLaneTypes()
    {
        var bike = Detector(2, 1, MovementTypes.R);
        bike.LaneType = LaneTypes.Bike;
        var result = await Run(new[] { Detector(1), bike },
            Events(1, 25, 25, 25, 25).Concat(Events(2, 100, 100, 100, 100)).ToList());
        Assert.Equal(100, result.PeakHour!.Value.Value);
        Assert.Equal(100, Assert.Single(result.Table.Where(r => r.LaneType == "Vehicle")).PeakHourVolume.Value);
    }

    [Fact]
    public async Task ZeroVehicleTrafficWithBikeTraffic_DoesNotInventYearOnePeak()
    {
        var bike = Detector(2);
        bike.LaneType = LaneTypes.Bike;
        var result = await Run(new[] { Detector(1), bike }, Events(2, 1, 1, 1, 1));
        Assert.Null(result.PeakHour);
        Assert.Null(Assert.Single(result.Charts.Where(c => c.LaneType == "Vehicle")).PeakHour);
        Assert.NotNull(Assert.Single(result.Charts.Where(c => c.LaneType == "Bike")).PeakHour);
    }

    [Fact]
    public async Task MultiDayPeak_IdentifiesItsDateAndKeepsMinuteCountsOffTheWire()
    {
        var result = await Run(new[] { Detector(1) }, new List<IndianaEvent> { Event(1, Start.AddDays(1)) }, 60, 1500);
        var chart = Assert.Single(result.Charts);
        Assert.Equal("2026-04-02 08:00 - 2026-04-02 09:00", chart.PeakHour);
        Assert.DoesNotContain("MinuteVolumes", JsonSerializer.Serialize(result));
    }

    [Theory]
    [InlineData(-60)]
    [InlineData(60)]
    [InlineData(120)]
    public async Task LogsOnlyOutsideRequestedInterval_ReturnNoData(int eventMinute)
    {
        var result = await Run(new[] { Detector(1) }, new List<IndianaEvent>
        {
            new() { LocationIdentifier = "1001", EventCode = 131, EventParam = 1, Timestamp = Start.AddHours(-2) },
            Event(1, Start.AddMinutes(eventMinute))
        }, includePlanEvent: false);

        Assert.Empty(result.Charts);
        Assert.Empty(result.Table);
        Assert.Null(result.PeakHour);
        Assert.Null(result.PeakHourFactor);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30)]
    public async Task ControllerActivityWithoutDetectorCounts_PreservesZeroTraffic(int eventMinute)
    {
        var result = await Run(new[] { Detector(1) }, new List<IndianaEvent>
        {
            new() { LocationIdentifier = "1001", EventCode = 1, EventParam = 1, Timestamp = Start.AddMinutes(eventMinute) }
        }, includePlanEvent: false);

        var chart = Assert.Single(result.Charts);
        Assert.Equal(0, chart.TotalVolume);
        Assert.All(chart.TotalVolumes, volume => Assert.Equal(0, volume.Value));
        Assert.All(Assert.Single(result.Table).Volumes, volume => Assert.Equal(0, volume.Value));
        Assert.Null(result.PeakHour);
        Assert.Null(result.PeakHourFactor);
    }

    [Theory]
    [InlineData(-1, -0.5)]
    [InlineData(1, 3600.5)]
    public async Task CorrectedBoundaryCountsWithoutRawEventsInRange_ArePreserved(double latency, double eventSecond)
    {
        var detector = Detector(1);
        detector.LatencyCorrection = latency;
        var result = await Run(new[] { detector }, new List<IndianaEvent>
        {
            new() { LocationIdentifier = "1001", EventCode = 131, EventParam = 1, Timestamp = Start.AddHours(-2) },
            Event(1, Start.AddSeconds(eventSecond))
        }, includePlanEvent: false);

        Assert.Equal(1, Assert.Single(result.Charts).TotalVolume);
        Assert.Equal(1, Assert.Single(result.Table).Volumes.Sum(v => v.Value));
        Assert.Equal(1, result.PeakHour!.Value.Value);
    }

    private static Detector Detector(int channel, int? lane = 1, MovementTypes movement = MovementTypes.T) => new()
    {
        Id = channel,
        DetectorChannel = channel,
        LaneNumber = lane,
        MovementType = movement,
        LaneType = LaneTypes.V,
        DetectionTypes = new List<DetectionType>
        {
            new() { Id = DetectionTypes.LLC, MeasureTypes = new List<MeasureType> { new() { Id = 5 } } }
        }
    };

    private static IndianaEvent Event(short channel, DateTime timestamp) => new()
    {
        LocationIdentifier = "1001", EventCode = 82, EventParam = channel, Timestamp = timestamp
    };

    private static List<IndianaEvent> Events(short channel, params int[] quarters) =>
        quarters.SelectMany((count, quarter) => Enumerable.Range(0, count)
            .Select(i => Event(channel, Start.AddMinutes(quarter * 15).AddMilliseconds(i + 1)))).ToList();

    private static Task<TurningMovementCountsResult> Run(
        IEnumerable<Detector> detectors, List<IndianaEvent> events, int binSize = 15, int duration = 60, bool combine = false, bool includePlanEvent = true)
    {
        var location = new Location { LocationIdentifier = "1001", PrimaryName = "Main", SecondaryName = "State" };
        var approach = new Approach { Location = location, DirectionTypeId = DirectionTypes.NB, Detectors = detectors.ToList() };
        foreach (var detector in approach.Detectors)
            detector.Approach = approach;
        location.Approaches = new List<Approach> { approach };
        var locations = new Mock<ILocationRepository>();
        locations.Setup(r => r.GetLatestVersionOfLocation("1001", Start)).Returns(location);
        var allEvents = events.ToList();
        if (includePlanEvent)
            allEvents.Add(new IndianaEvent
            {
                LocationIdentifier = "1001", EventCode = 131, EventParam = 1, Timestamp = Start
            });
        var repository = new Mock<IIndianaEventLogRepository>();
        repository.Setup(r => r.GetEventsBetweenDates("1001", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns((string id, DateTime start, DateTime end) => allEvents.Where(e => e.Timestamp >= start && e.Timestamp < end).ToList());

        return new TurningMovementCountReportService(repository.Object, new TurningMovementCountsService(),
            locations.Object, new PlanService()).ExecuteAsync(new TurningMovementCountsOptions
            {
                LocationIdentifier = "1001", Start = Start, End = Start.AddMinutes(duration),
                BinSize = binSize, CombineThruRight = combine
            }, null, default);
    }
}
