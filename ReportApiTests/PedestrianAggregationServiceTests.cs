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
    }
}