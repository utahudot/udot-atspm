using ATSPM.Application.Business.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using System.Net;

namespace ATSPM.Application.Reports.Business.Common.Tests
{
    public class Startup
    {

    }

    public class LocationPhaseServiceTests
    {
        [Fact()]
        public async void GetLocationPhaseDataTest()
        {
            // Arrange
            PlanService planService = new PlanService(); // Replace with your PlanService instance
            CycleService cycleService = new CycleService(); // Replace with your CycleService instance
            ILoggerFactory loggerFactory = new LoggerFactory(); // Create an instance of ILoggerFactory
            ILogger<LocationPhaseService> logger = loggerFactory.CreateLogger<LocationPhaseService>(); // Create the ILogger<LocationPhaseService> instance
            PhaseService phaseService = new PhaseService();

            LocationPhaseService locationPhaseService = new LocationPhaseService(planService, cycleService, logger);


            DateTime start = new DateTime(2020, 12, 1, 6, 0, 0);
            DateTime end = new DateTime(2020, 12, 1, 7, 0, 0);
            List<IndianaEvent> events = LoadDetectorEventsFromCsv(@"ControllerEventLogs-7119-Phase2-CycleDetectorPlanEvents-20201201-00-24.csv"); // Sampleevents
            List<IndianaEvent> cycleEvents = events.Where(e => new List<DataLoggerEnum> { DataLoggerEnum.PhaseBeginGreen, DataLoggerEnum.PhaseBeginYellowChange, DataLoggerEnum.PhaseEndYellowChange }.Contains(e.EventCode)).ToList(); // Sample cycle events
            List<IndianaEvent> detectorEvents = events.Where(e => new List<DataLoggerEnum> { DataLoggerEnum.DetectorOn }.Contains(e.EventCode)).ToList(); // Load detector events from CSV
            List<IndianaEvent> planEvents = events.Where(e => new List<DataLoggerEnum> { DataLoggerEnum.CoordPatternChange }.Contains(e.EventCode)).ToList(); // Load plan events from CSV

            // Create the mock Approach object
            var approach = new Mock<Approach>();

            // Set the properties of the mock Approach object
            approach.Object.Id = 3106;
            approach.Object.LocationId = 1933;
            approach.Object.DirectionTypeId = DirectionTypes.NB;
            approach.Object.Description = "NBT Ph2";
            approach.Object.Mph = 45;
            approach.Object.ProtectedPhaseNumber = 2;
            approach.Object.IsProtectedPhaseOverlap = false;
            approach.Object.PermissivePhaseNumber = null;
            approach.Object.IsPermissivePhaseOverlap = false;
            approach.Object.PedestrianPhaseNumber = null;
            approach.Object.IsPedestrianPhaseOverlap = false;
            approach.Object.PedestrianDetectors = null;

            var mockLocation = new Mock<Location>();

            // Set the properties of the mock Location object
            mockLocation.Object.Id = 1933;
            mockLocation.Object.LocationIdentifier = "7191";
            mockLocation.Object.Latitude = 40.69988569;
            mockLocation.Object.Longitude = -111.8713268;
            mockLocation.Object.PrimaryName = "700 East";
            mockLocation.Object.SecondaryName = "3300 South";
            //mockLocation.Object.Ipaddress = IPAddress.Parse("10.202.6.75");
            mockLocation.Object.RegionId = 2;
            mockLocation.Object.LocationTypeId = 4;
            mockLocation.Object.ChartEnabled = true;
            mockLocation.Object.VersionAction = LocationVersionActions.Initial;
            mockLocation.Object.Note = "10";
            mockLocation.Object.Start = new DateTime(2011, 1, 1);
            mockLocation.Object.JurisdictionId = 35;
            mockLocation.Object.PedsAre1to1 = true;
            //mockLocation.Object.Approaches = new List<Approach>() { approach };

            // Create the mock Approach object and set its Location property to the mock Location object
            mockLocation.Setup(mock => mock.Approaches).Returns(new List<Approach>() { approach.Object });
            approach.Setup(a => a.Location).Returns(mockLocation.Object);
            var phaseDetail = phaseService.GetPhases(mockLocation.Object);

            LocationPhase result = await locationPhaseService.GetLocationPhaseData(phaseDetail.FirstOrDefault(), start, end, true, 0, 15, cycleEvents, planEvents, detectorEvents);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Volume.Items);
            Assert.NotEmpty(result.Plans);
            Assert.NotEmpty(result.Cycles);
            Assert.Same(approach.Object, result.Approach);
            Assert.Equal(start, result.StartDate);
            Assert.Equal(end, result.EndDate);
            Assert.Equal(199, result.TotalArrivalOnGreen);
            Assert.Equal(177, result.TotalArrivalOnRed);
            Assert.Equal(6, result.TotalArrivalOnYellow);
            Assert.Equal(4574.7, result.TotalDelaySeconds);
            Assert.Equal(382, result.TotalVolume);
            Assert.Equal(1662.2999999999997, result.TotalGreenTimeSeconds);
            Assert.Equal(158.40000000000003, result.TotalYellowTimeSeconds);
            Assert.Equal(1908.1999999999998, result.TotalRedTimeSeconds);
            Assert.Equal(11.97565445026178, result.AvgDelaySeconds);
            Assert.Equal(52, result.PercentArrivalOnGreen);
            Assert.Equal(45, result.PercentGreen);
            Assert.Equal(1.16, result.PlatoonRatio);
            Assert.Equal(3728.9, result.TotalTime);

            // Resetting volume should set it to null
            result.ResetVolume();
            Assert.Null(result.Volume);
        }

        private List<IndianaEvent> LoadDetectorEventsFromCsv(string fileName)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", fileName);
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //csv.Context.TypeConverterCache.AddConverter<DateTime>(new CustomDateTimeConverter());

                List<IndianaEvent> detectorEvents = csv.GetRecords<IndianaEvent>().ToList();
                return detectorEvents;
            }
        }
    }
}