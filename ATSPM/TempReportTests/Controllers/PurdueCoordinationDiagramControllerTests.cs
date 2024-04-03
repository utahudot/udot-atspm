using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.PurdueCoordinationDiagram;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using System.Net;

namespace ATSPM.Application.Reports.Controllers.Tests
{
    public class PurdueCoordinationDiagramControllerTests
    {
        [Fact()]
        public async void GetChartDataTest()
        {

            // Arrange
            PurdueCoordinationDiagramService purdueCoordinationDiagramService = new PurdueCoordinationDiagramService();
            PlanService planService = new PlanService();
            CycleService cycleService = new CycleService();
            ILoggerFactory loggerFactory = new LoggerFactory();
            PhaseService phaseService = new PhaseService();
            ILogger<LocationPhaseService> logger = loggerFactory.CreateLogger<LocationPhaseService>();

            LocationPhaseService locationPhaseService = new LocationPhaseService(planService, cycleService, logger);

            System.DateTime start = new System.DateTime(2020, 12, 01, 6, 0, 0);
            System.DateTime end = new System.DateTime(2020, 12, 01, 7, 0, 0);
            List<IndianaEvent> events = LoadDetectorEventsFromCsv(@"PCDevents.csv"); // Sampleevents
            List<IndianaEvent> cycleEvents = events.Where(e => new List<DataLoggerEnum> { DataLoggerEnum.PhaseBeginGreen, DataLoggerEnum.PhaseBeginYellowChange,DataLoggerEnum.PhaseEndYellowChange }.Contains(e.EventCode)).ToList(); // Sample cycle events
            List<IndianaEvent> detectorEvents = events.Where(e => new List<DataLoggerEnum> { DataLoggerEnum.DetectorOn }.Contains(e.EventCode)).ToList(); // Load detector events from CSV
            List<IndianaEvent> planEvents = events.Where(e => new List<DataLoggerEnum> { DataLoggerEnum.CoordPatternChange }.Contains(e.EventCode)).ToList(); // Load plan events from CSV

            // Create the mock Approach object
            var approach = new Mock<Approach>();

            // Set the properties of the mock Approach object
            approach.Object.Id = 3106; // Updated Id
            approach.Object.LocationId = 1933; // Updated LocationId
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
            mockLocation.Object.Id = 1933; // Updated Id
            mockLocation.Object.LocationIdentifier = "7191"; // Updated LocationId
            mockLocation.Object.Latitude = 40.69988569;
            mockLocation.Object.Longitude = -111.8713268;
            mockLocation.Object.PrimaryName = "700 East";
            mockLocation.Object.SecondaryName = "3300 South";
            //mockLocation.Object.Ipaddress = IPAddress.Parse("10.202.6.75");
            mockLocation.Object.RegionId = 2;
            mockLocation.Object.LocationTypeId = 4; // Updated ControllerTypeId
            mockLocation.Object.ChartEnabled = true;
            //mockLocation.Object.LoggingEnabled = true;
            mockLocation.Object.VersionAction = LocationVersionActions.Initial;
            mockLocation.Object.Note = "Initial";
            mockLocation.Object.Start = new System.DateTime(2011, 1, 1);
            mockLocation.Object.JurisdictionId = 35;
            mockLocation.Object.PedsAre1to1 = true;

            // Create the mock Approach object and set its Location property to the mock Location object
            approach.Setup(a => a.Location).Returns(mockLocation.Object);
            var approaches = new List<Approach>() { approach.Object};
            mockLocation.Setup(a => a.Approaches).Returns(approaches);

            var phaseDetail = phaseService.GetPhases(mockLocation.Object);

            var options = new PurdueCoordinationDiagramOptions() { LocationIdentifier = "7191", BinSize = 15, Start = start, End = end, ShowPlanStatistics = true };

            LocationPhase locationPhase = await locationPhaseService.GetLocationPhaseData(phaseDetail.FirstOrDefault(), start, end, true, 0, 15, cycleEvents, planEvents, detectorEvents);
            var result = purdueCoordinationDiagramService.GetChartData(options, approach.Object, locationPhase);

            // Assert
            //Assert.Equal(approach.Object.Id, result.ApproachId);
            //Assert.Equal(approach.Object.Location.LocationIdentifier, result.LocationIdentifier);
            //Assert.Equal(2, result.PhaseNumber);
            //Assert.Equal("NBT Ph2", result.PhaseDescription);
            //Assert.Equal(start, result.Start);
            //Assert.Equal(end, result.End);
            //Assert.Equal(199, result.TotalOnGreenEvents);
            //Assert.Equal(382, result.TotalDetectorHits);
            //Assert.Equal(52, result.PercentArrivalOnGreen);
            


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