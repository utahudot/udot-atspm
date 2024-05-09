using ATSPM.Application.Business.AppoachDelay;
using ATSPM.Application.Business.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Channels;


namespace ATSPM.Application.Reports.Controllers.Tests
{
    public class ApproachDelayControllerTests
    {
        [Fact()]
        public async void GetChartDataTest()
        {
            // Arrange
            ApproachDelayService approachDelayService = new ApproachDelayService();
            PlanService planService = new PlanService();
            CycleService cycleService = new CycleService();
            ILoggerFactory loggerFactory = new LoggerFactory();
            PhaseService phaseService = new PhaseService();
            ILogger<LocationPhaseService> logger = loggerFactory.CreateLogger<LocationPhaseService>();

            LocationPhaseService locationPhaseService = new LocationPhaseService(planService, cycleService, logger);

            System.DateTime start = new System.DateTime(2023, 4, 17, 8, 0, 0);
            System.DateTime end = new System.DateTime(2023, 4, 17, 9, 0, 0);
            List<IndianaEvent> events = LoadDetectorEventsFromCsv(@"ControllerEvents-ApproachDelay.csv"); // Sampleevents
            List<IndianaEvent> cycleEvents = events.Where(e => new List<short> { (short)IndianaEnumerations.PhaseBeginGreen, (short)IndianaEnumerations.PhaseBeginYellowChange, (short)IndianaEnumerations.PhaseEndYellowChange }.Contains(e.EventCode)).ToList(); // Sample cycle events
            List<IndianaEvent> detectorEvents = events.Where(e => new List<short> { (short)IndianaEnumerations.VehicleDetectorOn }.Contains(e.EventCode)).ToList(); // Load detector events from CSV
            List<IndianaEvent> planEvents = events.Where(e => new List<short> { (short)IndianaEnumerations.CoordPatternChange }.Contains(e.EventCode)).ToList(); // Load plan events from CSV

            // Create the mock Approach object
            var approach = new Mock<Approach>();

            // Set the properties of the mock Approach object
            approach.Object.Id = 2880; // Updated Id
            approach.Object.LocationId = 1680; // Updated LocationId
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
            mockLocation.Object.Id = 1680; // Updated Id
            mockLocation.Object.LocationIdentifier = "7115"; // Updated LocationId
            mockLocation.Object.Latitude = 40.62398502;
            mockLocation.Object.Longitude = -111.9387819;
            mockLocation.Object.PrimaryName = "Redwood Road";
            mockLocation.Object.SecondaryName = "7000 South";
            //mockLocation.Object.Ipaddress = IPAddress.Parse("10.210.14.39");
            mockLocation.Object.RegionId = 2;
            mockLocation.Object.LocationTypeId = 2; // Updated ControllerTypeId
            mockLocation.Object.ChartEnabled = true;
            mockLocation.Object.VersionAction = LocationVersionActions.Initial;
            mockLocation.Object.Note = "10";
            mockLocation.Object.Start = new System.DateTime(2011, 1, 1);
            mockLocation.Object.JurisdictionId = 35;
            mockLocation.Object.PedsAre1to1 = true;

            // Create the mock Approach object and set its Location property to the mock Location object
            mockLocation.Setup(mock => mock.Approaches).Returns(new List<Approach>() { approach.Object });
            approach.Setup(a => a.Location).Returns(mockLocation.Object);

            var phaseDetail = phaseService.GetPhases(mockLocation.Object);

            var options = new ApproachDelayOptions() { LocationIdentifier = "7115", BinSize = 15, Start = start, End = end, GetVolume = true };

            LocationPhase locationPhase = await locationPhaseService.GetLocationPhaseData(phaseDetail.FirstOrDefault(), start, end, true, 0, 15, cycleEvents, planEvents, detectorEvents);
            var result = approachDelayService.GetChartData(options, phaseDetail.FirstOrDefault(), locationPhase);

            // Assert
            Assert.Equal(approach.Object.Id, result.ApproachId);
            Assert.Equal(approach.Object.Location.LocationIdentifier, result.locationIdentifier);
            Assert.Equal(2, result.PhaseNumber);
            Assert.Equal("NBT Ph2", result.PhaseDescription);
            Assert.Equal(start, result.Start);
            Assert.Equal(end, result.End);
            Assert.Equal(23.15599626691554, result.AverageDelayPerVehicle);
            Assert.Equal(49623.3, result.TotalDelay);
            Assert.NotEmpty(result.Plans);
            Assert.NotEmpty(result.ApproachDelayDataPoints);
            Assert.NotEmpty(result.ApproachDelayPerVehicleDataPoints);

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