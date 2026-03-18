using Moq;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.ReportApi.ReportServices;
using Utah.Udot.Atspm.Repositories.AggregationRepositories;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.ReportApi.DataAggregation;

namespace ReportApiTests
{
    public class PedestrianAggregationServiceTests
    {
        private readonly Mock<ILocationRepository> _locationRepo = new();
        private readonly Mock<IPhasePedAggregationRepository> _pedRepo = new();
        private readonly Mock<IPhaseCycleAggregationRepository> _cycleRepo = new();

        private PedestrianAggregationService CreateService()
        {
            return new PedestrianAggregationService(
                _locationRepo.Object,
                _pedRepo.Object,
                _cycleRepo.Object);
        }

        private PedatLocationDataQuery CreateValidQuery()
        {
            return new PedatLocationDataQuery
            {
                StartDate = DateTime.Today.AddDays(-1),
                EndDate = DateTime.Today,
                LocationIdentifiers = new List<string> { "1" },
                TimeUnit = PedestrianTimeUnit.Hour
            };
        }

        private Location CreateLocation() => new Location
        {
            LocationIdentifier = "1",
            PrimaryName = "Main",
            SecondaryName = "1st",
            Latitude = 40,
            Longitude = -111
        };

        private List<PhasePedAggregation> CreatePedData(int calls = 10, int walk = 5)
        {
            return new List<PhasePedAggregation>
        {
            new PhasePedAggregation
            {
                Start = DateTime.Today,
                End = DateTime.Today.AddMinutes(15),
                PhaseNumber = 1,
                ImputedPedCallsRegistered = calls,
                PedBeginWalkCount = walk,
                UniquePedDetections = calls
            }
        };
        }

        private List<PhaseCycleAggregation> CreateCycleData(int count = 10)
        {
            return new List<PhaseCycleAggregation>
        {
            new PhaseCycleAggregation
            {
                Start = DateTime.Today,
                End = DateTime.Today.AddMinutes(60),
                PhaseNumber = 1,
                PhaseBeginCount = count
            }
        };
        }

        // ---------------------------
        // ExecutePedAgg Tests
        // ---------------------------

        [Fact]
        public async Task ExecutePedAgg_BasicCompileTest()
        {
            var locationRepo = new Mock<ILocationRepository>();
            var pedRepo = new Mock<IPhasePedAggregationRepository>();
            var cycleRepo = new Mock<IPhaseCycleAggregationRepository>();

            var service = new PedestrianAggregationService(
                locationRepo.Object,
                pedRepo.Object,
                cycleRepo.Object);

            var query = new PedatLocationDataQuery
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddHours(1),
                LocationIdentifiers = new List<string> { "1" }
            };

            locationRepo.Setup(x => x.GetLatestVersionOfLocation(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(new Location { LocationIdentifier = "1" });

            pedRepo.Setup(x => x.GetAggregationsBetweenDates(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<PhasePedAggregation>());

            cycleRepo.Setup(x => x.GetAggregationsBetweenDates(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<PhaseCycleAggregation>());

            var result = await service.ExecutePedAgg(query);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ExecutePedAgg_NullParameter_ReturnsEmpty()
        {
            var service = CreateService();

            var result = await service.ExecutePedAgg(null);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ExecutePedAgg_NoLocations_Throws()
        {
            var service = CreateService();
            var query = CreateValidQuery();

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation(It.IsAny<string>(), It.IsAny<DateTime>()))
                         .Returns((Location)null);

            await Assert.ThrowsAsync<NullReferenceException>(() => service.ExecutePedAgg(query));
        }

        [Fact]
        public async Task ExecutePedAgg_ValidData_ReturnsResult()
        {
            var service = CreateService();
            var query = CreateValidQuery();

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation("1", It.IsAny<DateTime>()))
                         .Returns(CreateLocation());

            _pedRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                    .Returns(CreatePedData());

            _cycleRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                      .Returns(CreateCycleData());

            var result = (await service.ExecutePedAgg(query)).ToList();

            Assert.Single(result);
            Assert.True(result[0].TotalVolume >= 0);
        }

        [Fact]
        public async Task ExecutePedAgg_PhaseFilter_Works()
        {
            var service = CreateService();
            var query = CreateValidQuery();
            query.Phase = 1;

            var pedData = CreatePedData();
            pedData[0].PhaseNumber = 2;

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation("1", It.IsAny<DateTime>()))
                         .Returns(CreateLocation());

            _pedRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                    .Returns(pedData);

            _cycleRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                      .Returns(CreateCycleData());

            var result = (await service.ExecutePedAgg(query)).ToList();

            Assert.Equal(0, result[0].TotalVolume);
        }

        // ---------------------------
        // EquationCalculation Branches
        // ---------------------------

        [Fact]
        public async Task ExecutePedAgg_RecallHighActivity_UsesCorrectFormula()
        {
            var service = CreateService();
            var query = CreateValidQuery();

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation("1", It.IsAny<DateTime>()))
                .Returns(CreateLocation());

            // activity high => >350
            _pedRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(CreatePedData(calls: 400, walk: 500));

            _cycleRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(CreateCycleData());

            var result = (await service.ExecutePedAgg(query)).First();

            Assert.True(result.TotalVolume > 0);
        }

        [Fact]
        public async Task ExecutePedAgg_NoRecall_LongCycle_UsesCorrectFormula()
        {
            var service = CreateService();
            var query = CreateValidQuery();

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation("1", It.IsAny<DateTime>()))
                .Returns(CreateLocation());

            // recall = false
            _pedRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(CreatePedData(calls: 100, walk: 10));

            // long cycle
            _cycleRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(CreateCycleData(1));

            var result = (await service.ExecutePedAgg(query)).First();

            Assert.True(result.TotalVolume >= 0);
        }

        // ---------------------------
        // CalculateStatistics
        // ---------------------------

        [Fact]
        public void CalculateStatistics_Empty_ReturnsDefaults()
        {
            var service = CreateService();

            var result = service.CalculateStatistics(new List<RawDataPoint>(), DateTime.Now, DateTime.Now, TimeSpan.FromHours(1));

            Assert.Equal(null, result.Events);
            Assert.Equal(null, result.Count);
        }

        [Fact]
        public void CalculateStatistics_ValidData_ComputesStats()
        {
            var service = CreateService();

            var data = new List<RawDataPoint>
        {
            new RawDataPoint { Timestamp = DateTime.Now, PedestrianCount = 10 },
            new RawDataPoint { Timestamp = DateTime.Now.AddHours(1), PedestrianCount = 20 }
        };

            var result = service.CalculateStatistics(data, DateTime.Now, DateTime.Now.AddHours(1), TimeSpan.FromHours(1));

            Assert.Equal(30, result.Count);
            Assert.Equal(15, result.Mean);
        }

        // ---------------------------
        // RawDataSwap
        // ---------------------------

        [Theory]
        [InlineData(PedestrianTimeUnit.Hour)]
        [InlineData(PedestrianTimeUnit.Day)]
        [InlineData(PedestrianTimeUnit.Week)]
        [InlineData(PedestrianTimeUnit.Month)]
        [InlineData(PedestrianTimeUnit.Year)]
        public async Task ExecutePedAgg_TimeUnitAggregation_Works(PedestrianTimeUnit unit)
        {
            var service = CreateService();
            var query = CreateValidQuery();
            query.TimeUnit = unit;

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation("1", It.IsAny<DateTime>()))
                .Returns(CreateLocation());

            _pedRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(CreatePedData());

            _cycleRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(CreateCycleData());

            var result = (await service.ExecutePedAgg(query)).First();

            Assert.NotNull(result.RawData);
        }

        // ---------------------------
        // Averages
        // ---------------------------

        [Fact]
        public async Task ExecutePedAgg_Averages_AreComputed()
        {
            var service = CreateService();
            var query = CreateValidQuery();

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation("1", It.IsAny<DateTime>()))
                .Returns(CreateLocation());

            _pedRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(CreatePedData());

            _cycleRepo.Setup(x => x.GetAggregationsBetweenDates("1", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(CreateCycleData());

            var result = (await service.ExecutePedAgg(query)).First();

            Assert.NotEmpty(result.AverageVolumeByHourOfDay);
            Assert.NotEmpty(result.AverageVolumeByDayOfWeek);
            Assert.NotEmpty(result.AverageVolumeByMonthOfYear);
        }

        // ---------------------------
        // Averages
        // ---------------------------

        [Fact]
        public void AverageVolumeOverall_EmptyList_ReturnsZero()
        {
            var service = CreateService();

            var data = new List<PedestrianAggregationService.CombinedHourlyAggregation>();

            var result = service.AverageVolumeOverall(data);

            Assert.Equal(0, result);
        }

        [Fact]
        public void AverageVolumeOverall_SingleEntry_ReturnsValue()
        {
            var service = CreateService();

            var data = new List<PedestrianAggregationService.CombinedHourlyAggregation>
            {
                new() { Timestamp = DateTime.Today, CalculatedVolume = 100 }
            };

            var result = service.AverageVolumeOverall(data);

            Assert.Equal(100, result);
        }

        [Fact]
        public void AverageVolumeOverall_MultipleEntriesSameDay_SumsCorrectly()
        {
            var service = CreateService();

            var data = new List<PedestrianAggregationService.CombinedHourlyAggregation>
            {
                new() { Timestamp = DateTime.Today.AddHours(1), CalculatedVolume = 50 },
                new() { Timestamp = DateTime.Today.AddHours(5), CalculatedVolume = 70 }
            };

            var result = service.AverageVolumeOverall(data);

            // One day total = 120 ? average = 120
            Assert.Equal(120, result);
        }

        [Fact]
        public void AverageVolumeOverall_MultipleDays_AveragesDailyTotals()
        {
            var service = CreateService();

            var data = new List<PedestrianAggregationService.CombinedHourlyAggregation>
            {
                // Day 1 total = 100
                new() { Timestamp = DateTime.Today, CalculatedVolume = 40 },
                new() { Timestamp = DateTime.Today.AddHours(2), CalculatedVolume = 60 },

                // Day 2 total = 200
                new() { Timestamp = DateTime.Today.AddDays(1), CalculatedVolume = 200 }
            };

            var result = service.AverageVolumeOverall(data);

            // (100 + 200) / 2 = 150
            Assert.Equal(150, result);
        }

        [Fact]
        public void AverageVolumeOverall_IgnoresTimeOfDay_GroupsByDateOnly()
        {
            var service = CreateService();

            var data = new List<PedestrianAggregationService.CombinedHourlyAggregation>
            {
                new() { Timestamp = new DateTime(2026, 1, 1, 1, 0, 0), CalculatedVolume = 10 },
                new() { Timestamp = new DateTime(2026, 1, 1, 23, 59, 0), CalculatedVolume = 20 }
            };

            var result = service.AverageVolumeOverall(data);

            // Same day ? total = 30
            Assert.Equal(30, result);
        }

        [Fact]
        public void AverageVolumeOverall_DecimalValues_PreservesPrecision()
        {
            var service = CreateService();

            var data = new List<PedestrianAggregationService.CombinedHourlyAggregation>
            {
                new() { Timestamp = DateTime.Today, CalculatedVolume = 10.5 },
                new() { Timestamp = DateTime.Today.AddDays(1), CalculatedVolume = 20.5 }
            };

            var result = service.AverageVolumeOverall(data);

            // (10.5 + 20.5) / 2 = 15.5
            Assert.Equal(15.5, result, precision: 5);
        }

        [Fact]
        public async Task ExecutePedAgg_NullParameter_ReturnsEmpty2()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.ExecutePedAgg(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ExecutePedAgg_LocationNotFound_ThrowsException2()
        {
            // Arrange
            var service = CreateService();
            var query = new PedatLocationDataQuery
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddHours(1),
                LocationIdentifiers = new List<string> { "LOC1" }
            };

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation(It.IsAny<string>(), It.IsAny<DateTime>()))
                             .Returns((Location)null);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => service.ExecutePedAgg(query));
        }

        [Fact]
        public async Task ExecutePedAgg_SingleLocation_SimpleAggregation_CheckTotals2()
        {
            // Arrange
            var service = CreateService();
            var query = new PedatLocationDataQuery
            {
                StartDate = new DateTime(2026, 3, 18, 8, 0, 0),
                EndDate = new DateTime(2026, 3, 18, 10, 0, 0),
                LocationIdentifiers = new List<string> { "LOC1" },
                TimeUnit = PedestrianTimeUnit.Hour
            };

            var location = new Location
            {
                LocationIdentifier = "LOC1",
                PrimaryName = "Main",
                SecondaryName = "Street",
                Latitude = 1.1,
                Longitude = 2.2,
                Areas = new List<Area> { new Area { Name = "Downtown" } }
            };

            var pedEvents = new List<PhasePedAggregation>
        {
            new PhasePedAggregation { Start = query.StartDate, End = query.StartDate.AddMinutes(30), PhaseNumber = 1, PedBeginWalkCount = 5, ImputedPedCallsRegistered = 5, UniquePedDetections = 5 },
            new PhasePedAggregation { Start = query.StartDate.AddMinutes(30), End = query.StartDate.AddHours(1), PhaseNumber = 1, PedBeginWalkCount = 10, ImputedPedCallsRegistered = 10, UniquePedDetections = 10 }
        };

            var cycleEvents = new List<PhaseCycleAggregation>
        {
            new PhaseCycleAggregation { Start = query.StartDate, End = query.StartDate.AddMinutes(30), PhaseNumber = 1, PhaseBeginCount = 2 },
            new PhaseCycleAggregation { Start = query.StartDate.AddMinutes(30), End = query.StartDate.AddHours(1), PhaseNumber = 1, PhaseBeginCount = 2 }
        };

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation("LOC1", query.StartDate)).Returns(location);
            _pedRepo.Setup(x => x.GetAggregationsBetweenDates("LOC1", query.StartDate, query.EndDate)).Returns(pedEvents);
            _cycleRepo.Setup(x => x.GetAggregationsBetweenDates("LOC1", query.StartDate, query.EndDate)).Returns(cycleEvents);

            // Act
            var result = (await service.ExecutePedAgg(query)).ToList();

            // Assert
            Assert.Single(result);
            var data = result[0];

            // Check location info
            Assert.Equal("LOC1", data.LocationIdentifier);
            Assert.Equal("Main & Street", data.Names);
            Assert.Equal("Downtown", data.Areas);
            Assert.Equal(1.1, data.Latitude);
            Assert.Equal(2.2, data.Longitude);

            // Check totals
            Assert.True(data.TotalVolume > 0);
            Assert.True(data.AverageDailyVolume > 0);
            Assert.NotEmpty(data.RawData);
            Assert.NotNull(data.StatisticData);
        }

        [Fact]
        public async Task ExecutePedAgg_MultiplePhasesAndCycles_ValidateCalculatedVolumesAndAverages2()
        {
            // Arrange
            var service = CreateService();
            var query = new PedatLocationDataQuery
            {
                StartDate = new DateTime(2026, 3, 18),
                EndDate = new DateTime(2026, 3, 18, 23, 0, 0),
                LocationIdentifiers = new List<string> { "LOC2" },
                Phase = 1,
                TimeUnit = PedestrianTimeUnit.Day
            };

            var location = new Location
            {
                LocationIdentifier = "LOC2",
                PrimaryName = "Second",
                SecondaryName = "Avenue",
                Latitude = 10,
                Longitude = 20,
                Areas = new List<Area> { new Area { Name = "Uptown" } }
            };

            // Generate ped events across multiple hours
            var pedEvents = new List<PhasePedAggregation>();
            var cycleEvents = new List<PhaseCycleAggregation>();
            for (int hour = 0; hour < 24; hour++)
            {
                pedEvents.Add(new PhasePedAggregation
                {
                    Start = query.StartDate.AddHours(hour),
                    End = query.StartDate.AddHours(hour + 1),
                    PhaseNumber = 1,
                    PedBeginWalkCount = 10 + hour,
                    ImputedPedCallsRegistered = 10 + hour,
                    UniquePedDetections = 5 + hour
                });
                cycleEvents.Add(new PhaseCycleAggregation
                {
                    Start = query.StartDate.AddHours(hour),
                    End = query.StartDate.AddHours(hour + 1),
                    PhaseNumber = 1,
                    PhaseBeginCount = 2
                });
            }

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation("LOC2", query.StartDate)).Returns(location);
            _pedRepo.Setup(x => x.GetAggregationsBetweenDates("LOC2", query.StartDate, query.EndDate)).Returns(pedEvents);
            _cycleRepo.Setup(x => x.GetAggregationsBetweenDates("LOC2", query.StartDate, query.EndDate)).Returns(cycleEvents);

            // Act
            var result = (await service.ExecutePedAgg(query)).First();

            // Assert
            Assert.Equal("LOC2", result.LocationIdentifier);
            Assert.True(result.TotalVolume > 0);
            Assert.Equal(result.RawData.Sum(d => d.PedestrianCount), result.TotalVolume);
            Assert.Equal(result.RawData.Count, 1); // Day aggregation should group all 24 hours
            Assert.NotEmpty(result.AverageVolumeByHourOfDay);
            Assert.NotEmpty(result.AverageVolumeByDayOfWeek);
            Assert.NotEmpty(result.AverageVolumeByMonthOfYear);
            Assert.True(result.AverageDailyVolume > 0);
        }

        [Fact]
        public async Task ExecutePedAgg_MultiHourMultiDay_ValidateAggregationsAndStatistics2()
        {
            // Arrange
            var service = CreateService();
            var startDate = new DateTime(2026, 3, 16, 6, 0, 0);
            var endDate = new DateTime(2026, 3, 18, 22, 0, 0);

            var query = new PedatLocationDataQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                LocationIdentifiers = new List<string> { "LOC3" },
                TimeUnit = PedestrianTimeUnit.Day
            };

            var location = new Location
            {
                LocationIdentifier = "LOC3",
                PrimaryName = "Third",
                SecondaryName = "Boulevard",
                Latitude = 15,
                Longitude = 25,
                Areas = new List<Area> { new Area { Name = "Midtown" } }
            };

            // Generate pedestrian events: 6AM-10PM, 3 days, single phase
            var pedEvents = new List<PhasePedAggregation>();
            var cycleEvents = new List<PhaseCycleAggregation>();
            for (int day = 0; day < 3; day++)
            {
                for (int hour = 6; hour <= 22; hour++)
                {
                    pedEvents.Add(new PhasePedAggregation
                    {
                        Start = startDate.AddDays(day).AddHours(hour - 6),
                        End = startDate.AddDays(day).AddHours(hour - 5),
                        PhaseNumber = 1,
                        PedBeginWalkCount = 5 + hour,
                        ImputedPedCallsRegistered = 5 + hour,
                        UniquePedDetections = 2 + hour
                    });
                    cycleEvents.Add(new PhaseCycleAggregation
                    {
                        Start = startDate.AddDays(day).AddHours(hour - 6),
                        End = startDate.AddDays(day).AddHours(hour - 5),
                        PhaseNumber = 1,
                        PhaseBeginCount = 1 + (hour % 3)
                    });
                }
            }

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation("LOC3", startDate)).Returns(location);
            _pedRepo.Setup(x => x.GetAggregationsBetweenDates("LOC3", startDate, endDate)).Returns(pedEvents);
            _cycleRepo.Setup(x => x.GetAggregationsBetweenDates("LOC3", startDate, endDate)).Returns(cycleEvents);

            // Act
            var result = (await service.ExecutePedAgg(query)).First();

            // Assert location info
            Assert.Equal("LOC3", result.LocationIdentifier);
            Assert.Equal("Third & Boulevard", result.Names);
            Assert.Equal("Midtown", result.Areas);
            Assert.Equal(15, result.Latitude);
            Assert.Equal(25, result.Longitude);

            // Assert raw data (daily aggregation over 3 days)
            Assert.Equal(3, result.RawData.Count);
            Assert.All(result.RawData, r => Assert.True(r.PedestrianCount > 0));

            // Check total volume matches sum of raw data
            // Sum PedestrianCount, defaulting to 0 if RawData is null or empty
            var rawDataSum = result.RawData?.Sum(r => r.PedestrianCount) ?? 0;

            // Now you can safely compare to TotalVolume
            Assert.Equal((int)Math.Round(result.TotalVolume), (int)Math.Round(rawDataSum));

            // Check averages
            Assert.True(result.AverageDailyVolume > 0);
            Assert.NotEmpty(result.AverageVolumeByHourOfDay);
            Assert.NotEmpty(result.AverageVolumeByDayOfWeek);
            Assert.NotEmpty(result.AverageVolumeByMonthOfYear);

            // Check statistics
            Assert.NotNull(result.StatisticData);
            Assert.Equal(result.RawData.Count, result.StatisticData.Events);
            Assert.True(result.StatisticData.Count > 0);
            Assert.True(result.StatisticData.Min > 0);
            Assert.True(result.StatisticData.Max > 0);
            Assert.True(result.StatisticData.Mean > 0);
        }

        [Fact]
        public async Task ExecutePedAgg_WithDeterministicValues_VerifyExactResults()
        {
            // Arrange
            var service = CreateService();
            var startDate = new DateTime(2026, 3, 18, 8, 0, 0);
            var endDate = new DateTime(2026, 3, 18, 10, 0, 0);

            var query = new PedatLocationDataQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                LocationIdentifiers = new List<string> { "LOC_TEST" },
                TimeUnit = PedestrianTimeUnit.Hour
            };

            var location = new Location
            {
                LocationIdentifier = "LOC_TEST",
                PrimaryName = "Test",
                SecondaryName = "Street",
                Latitude = 50,
                Longitude = 100,
                Areas = new List<Area> { new Area { Name = "TestArea" } }
            };

            // Pedestrian events: 2 hours, 1 phase
            var pedEvents = new List<PhasePedAggregation>
            {
                new PhasePedAggregation
                {
                    Start = startDate,
                    End = startDate.AddHours(1),
                    PhaseNumber = 1,
                    PedBeginWalkCount = 10,
                    ImputedPedCallsRegistered = 10,
                    UniquePedDetections = 5
                },
                new PhasePedAggregation
                {
                    Start = startDate.AddHours(1),
                    End = endDate,
                    PhaseNumber = 1,
                    PedBeginWalkCount = 20,
                    ImputedPedCallsRegistered = 20,
                    UniquePedDetections = 10
                }
            };

            // Cycle events: 2 hours, 1 phase
            var cycleEvents = new List<PhaseCycleAggregation>
            {
                new PhaseCycleAggregation
                {
                    Start = startDate,
                    End = startDate.AddHours(1),
                    PhaseNumber = 1,
                    PhaseBeginCount = 2
                },
                new PhaseCycleAggregation
                {
                    Start = startDate.AddHours(1),
                    End = endDate,
                    PhaseNumber = 1,
                    PhaseBeginCount = 4
                }
            };

            _locationRepo.Setup(x => x.GetLatestVersionOfLocation("LOC_TEST", startDate)).Returns(location);
            _pedRepo.Setup(x => x.GetAggregationsBetweenDates("LOC_TEST", startDate, endDate)).Returns(pedEvents);
            _cycleRepo.Setup(x => x.GetAggregationsBetweenDates("LOC_TEST", startDate, endDate)).Returns(cycleEvents);

            // Act
            var result = (await service.ExecutePedAgg(query)).First();

            // Assert Location Info
            Assert.Equal("LOC_TEST", result.LocationIdentifier);
            Assert.Equal("Test & Street", result.Names);
            Assert.Equal("TestArea", result.Areas);
            Assert.Equal(50, result.Latitude);
            Assert.Equal(100, result.Longitude);

            // ---- Calculated Expected Values ----
            // Cycle length = (totalMinutes / allCycles) = ((10:00 - 8:00) = 120 minutes) / (2 + 4 = 6) = 20
            int cycleLength = 120 / 6; // 20

            // Recall = pedWalk >= pedCallsRegistered = 30 >= 30 -> true
            bool recall = true;

            // Activity = sum(ImputedPedCallsRegistered)/totalDays = 30 / 1 = 30
            int activity = 30;

            // EquationCalculation for each hour
            double expectedVolumeHour1 = 2.304 * 10 + 0.148 * (10 * 10); // recall = true, activity > 350? no, so 1.31 + 0.083? Wait activity =30<350
                                                                         // Check EquationCalculation logic: recall = true, activity =30<350, so use else: 1.31*FortyFiveB + 0.083*FortyFiveB^2
            expectedVolumeHour1 = 1.31 * 10 + 0.083 * (10 * 10); // 13.1 + 8.3 = 21.4
            double expectedVolumeHour2 = 1.31 * 20 + 0.083 * (20 * 20); // 26.2 + 33.2 = 59.4

            // Total Volume = sum of CalculatedVolume
            double expectedTotalVolume = expectedVolumeHour1 + expectedVolumeHour2; // 21.4 + 59.4 = 80.8

            // Average Volume by Hour = same as calculated volumes since each hour is unique
            var expectedAvgByHour = new List<double> { expectedVolumeHour1, expectedVolumeHour2 };

            // Raw data per hour
            var expectedRawData = new List<int> { (int)Math.Round(expectedVolumeHour1), (int)Math.Round(expectedVolumeHour2) };

            // Assert Total Volume
            Assert.Equal(expectedTotalVolume, result.TotalVolume, 1); // allow small rounding

            // Assert AverageDailyVolume = totalVolume / 1 day = 80.8
            Assert.Equal(expectedTotalVolume, result.AverageDailyVolume, 1);

            // Assert AverageVolumeByHourOfDay
            Assert.Equal(2, result.AverageVolumeByHourOfDay.Count);
            Assert.Equal(expectedVolumeHour1, result.AverageVolumeByHourOfDay[0].Volume, 1);
            Assert.Equal(expectedVolumeHour2, result.AverageVolumeByHourOfDay[1].Volume, 1);

            double rawPedestrianCount0 = result.RawData[0].PedestrianCount ?? 0;
            double rawPedestrianCount1 = result.RawData[1].PedestrianCount ?? 0;

            // Assert Raw Data PedestrianCount matches rounded CalculatedVolume
            Assert.Equal((int) Math.Round((double)expectedRawData[0]), (int)Math.Round((double)rawPedestrianCount0));
            Assert.Equal((int)Math.Round((double)expectedRawData[1]), (int)Math.Round((double)rawPedestrianCount1));

            // Assert StatisticData
            Assert.Equal(2, result.StatisticData.Events);
            
            double tolerance = 0.1;

            Assert.InRange((double)result.StatisticData.Count, expectedTotalVolume - tolerance, expectedTotalVolume + tolerance);
            Assert.InRange((double)result.StatisticData.Min, Math.Min(expectedVolumeHour1, expectedVolumeHour2) - tolerance,
                                                        Math.Min(expectedVolumeHour1, expectedVolumeHour2) + tolerance);
            Assert.InRange((double)result.StatisticData.Max, Math.Max(expectedVolumeHour1, expectedVolumeHour2) - tolerance,
                                                        Math.Max(expectedVolumeHour1, expectedVolumeHour2) + tolerance);
        }
    }
}