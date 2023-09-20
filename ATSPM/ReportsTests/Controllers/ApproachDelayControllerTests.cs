using ATSPM.Application.Reports.Business.AppoachDelay;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Moq;
using Reports.Business.Common;
using System.Globalization;
using System.Net;
using Xunit;


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
            ILogger<SignalPhaseService> logger = loggerFactory.CreateLogger<SignalPhaseService>();

            SignalPhaseService signalPhaseService = new SignalPhaseService(planService, cycleService, logger);

            System.DateTime start = new System.DateTime(2023, 4, 17, 8, 0, 0);
            System.DateTime end = new System.DateTime(2023, 4, 17, 9, 0, 0);
            List<ControllerEventLog> events = LoadDetectorEventsFromCsv(@"ControllerEvents-ApproachDelay.csv"); // Sampleevents
            List<ControllerEventLog> cycleEvents = events.Where(e => new List<int> { 1, 8, 9 }.Contains(e.EventCode)).ToList(); // Sample cycle events
            List<ControllerEventLog> detectorEvents = events.Where(e => new List<int> { 82 }.Contains(e.EventCode)).ToList(); // Load detector events from CSV
            List<ControllerEventLog> planEvents = events.Where(e => new List<int> { 131 }.Contains(e.EventCode)).ToList(); // Load plan events from CSV

            // Create the mock Approach object
            var approach = new Mock<Approach>();

            // Set the properties of the mock Approach object
            approach.Object.Id = 2880; // Updated Id
            approach.Object.SignalId = 1680; // Updated SignalId
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

            var mockSignal = new Mock<Signal>();

            // Set the properties of the mock Signal object
            mockSignal.Object.Id = 1680; // Updated Id
            mockSignal.Object.SignalIdentifier = "7115"; // Updated SignalId
            mockSignal.Object.Latitude = 40.62398502;
            mockSignal.Object.Longitude = -111.9387819;
            mockSignal.Object.PrimaryName = "Redwood Road";
            mockSignal.Object.SecondaryName = "7000 South";
            mockSignal.Object.Ipaddress = IPAddress.Parse("10.210.14.39");
            mockSignal.Object.RegionId = 2;
            mockSignal.Object.ControllerTypeId = 2; // Updated ControllerTypeId
            mockSignal.Object.ChartEnabled = true;
            mockSignal.Object.VersionActionId = SignaVersionActions.Initial;
            mockSignal.Object.Note = "10";
            mockSignal.Object.Start = new System.DateTime(2011, 1, 1);
            mockSignal.Object.JurisdictionId = 35;
            mockSignal.Object.Pedsare1to1 = true;

            // Create the mock Approach object and set its Signal property to the mock Signal object
            approach.Setup(a => a.Signal).Returns(mockSignal.Object);

            var phaseDetail = phaseService.GetPhases(mockSignal.Object);

            var options = new ApproachDelayOptions() { SignalIdentifier = "7115", BinSize = 15, Start = start, End = end, GetVolume = true };

            SignalPhase signalPhase = await signalPhaseService.GetSignalPhaseData(phaseDetail.FirstOrDefault(), start, end, true, 0, 15, cycleEvents, planEvents, detectorEvents);
            var result = approachDelayService.GetChartData(options, phaseDetail.FirstOrDefault(), signalPhase);

            // Assert
            Assert.Equal(approach.Object.Id, result.ApproachId);
            Assert.Equal(approach.Object.Signal.SignalIdentifier, result.SignalIdentifier);
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

        private List<ControllerEventLog> LoadDetectorEventsFromCsv(string fileName)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", fileName);
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //csv.Context.TypeConverterCache.AddConverter<DateTime>(new CustomDateTimeConverter());

                List<ControllerEventLog> detectorEvents = csv.GetRecords<ControllerEventLog>().ToList();
                return detectorEvents;
            }
        }
    }
}