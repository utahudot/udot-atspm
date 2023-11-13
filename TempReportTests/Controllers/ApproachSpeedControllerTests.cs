using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.ApproachSpeed;
using ATSPM.ReportApi.Business.Common;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using System.Net;

namespace ATSPM.Application.Reports.Controllers.Tests
{
    public class ApproachSpeedControllerTests
    {
        [Fact()]
        public void GetChartDataTest()
        {
            // Arrange
            PlanService planService = new PlanService();
            CycleService cycleService = new CycleService();
            ApproachSpeedService approachSpeedService = new ApproachSpeedService(cycleService,planService);
            ILoggerFactory loggerFactory = new LoggerFactory();
            ILogger<SignalPhaseService> logger = loggerFactory.CreateLogger<SignalPhaseService>();
            SignalPhaseService signalPhaseService = new SignalPhaseService(planService, cycleService, logger); 
            
            System.DateTime start = new System.DateTime(2023, 6, 14, 12, 0, 0);
            System.DateTime end = new System.DateTime(2023, 6, 14, 13, 0, 0);
            List<ControllerEventLog> events = LoadDetectorEventsFromCsv(@"ECsForApproachSpeedTest.csv"); // Sampleevents
            List<SpeedEvent> speedEvents = LoadSpeedEventsFromCsv(@"SpeedEvents5000-20230614.csv");
            List<ControllerEventLog> cycleEvents = events.Where(e => new List<int> { 1, 8, 9 }.Contains(e.EventCode)).ToList(); // Sample cycle events
            List<ControllerEventLog> planEvents = events.Where(e => new List<int> { 131 }.Contains(e.EventCode)).ToList(); // Load plan events from CSV


            // Create the mock Approach object
            var approach = new Mock<Approach>();

            // Set the properties of the mock Approach object
            approach.Object.Id = 29724; // Updated Id
            approach.Object.SignalId = 4938; // Updated SignalId
            approach.Object.DirectionTypeId = DirectionTypes.WB;
            approach.Object.Description = "WBT Ph2";
            approach.Object.ProtectedPhaseNumber = 2;
            approach.Object.PermissivePhaseNumber = null;
            approach.Object.PedestrianPhaseNumber = null;
            approach.Object.IsProtectedPhaseOverlap = false;
            approach.Object.IsPermissivePhaseOverlap = false;
            approach.Object.IsPedestrianPhaseOverlap = false;
            approach.Object.PedestrianDetectors = null;
            approach.Object.Mph = 45;

            // Create the mock Detector object
            var detector = new Mock<Detector>();
            var detectionType = new Mock<DetectionType>();

            // Set the properties of the mock Detector object
            detector.Object.Id = 101540; // Updated Id
            detector.Object.ApproachId = 29724; //int
            detector.Object.DateAdded = new System.DateTime(2023, 04, 18); //DateTime
            detector.Object.DateDisabled = null; //DateTime
            detector.Object.DectectorIdentifier = "500006"; // Updated SignalId
            detector.Object.DetectorChannel = 6; //int
            detector.Object.DetectionHardware = DetectionHardwareTypes.WavetronixAdvance;
            detector.Object.MovementType = MovementTypes.T;
            detector.Object.LaneNumber = 1; //int
            detector.Object.LaneType = LaneTypes.V;
            detector.Object.DistanceFromStopBar = 255; //int
            detector.Object.DecisionPoint = 0; //int
            detector.Object.MovementDelay = null; //int
            detector.Object.MinSpeedFilter = 5; //int
            //DetectionType myDetectionType = new DetectionType
            //{
            //    Id = DetectionTypes.AS,
            //    Description = "Advanced Speed"
            //};
            detectionType.Object.Abbreviation = "AS";
            detectionType.Object.Description = "Advanced Speed";
            detectionType.Object.Description = "Advanced Speed";
            detectionType.Object.Id = DetectionTypes.AS;

            //myDetectionType.Detectors = new List<Detector> { };
            //myDetectionType.MetricTypeMetrics = new List<MetricType> { };
            //detector.Object.DetectionTypes = new List<DetectionType> { myDetectionType };
            //detector.Object.DetectionTypes = new List<DetectionType> { myDetectionType };  // no matter what I have tried, this detection type keeps returning as null, causing issues down the road

            //Need this
            var mockSignal = new Mock<Signal>();

            // Set the properties of the mock Signal object
            mockSignal.Object.Id = 4938; // Updated Id
            mockSignal.Object.Ipaddress = IPAddress.Parse("10.235.5.15");
            mockSignal.Object.ChartEnabled = true;
            mockSignal.Object.LoggingEnabled = true;
            mockSignal.Object.Note = "Copy of Now a Cobalt (32.67.20)";
            mockSignal.Object.JurisdictionId = 4;
            mockSignal.Object.SignalIdentifier = "5000"; // Updated SignalId
            mockSignal.Object.VersionAction = SignalVersionActions.NewVersion;
            mockSignal.Object.Start = new System.DateTime(2023, 4, 18);
            mockSignal.Object.ControllerTypeId = 9; // Updated ControllerTypeId
            mockSignal.Object.RegionId = 1;
            mockSignal.Object.PrimaryName = "Riverdale Road";
            mockSignal.Object.SecondaryName = "700 West";
            mockSignal.Object.Latitude = 41.18003983;
            mockSignal.Object.Longitude = -111.9956664;
            mockSignal.Object.Pedsare1to1 = true;

            // Create the mock Approach object and set its Signal property to the mock Signal object
            approach.Setup(a => a.Signal).Returns(mockSignal.Object);
            detector.Setup(a => a.Approach).Returns(approach.Object);
            detector.Setup(a => a.DetectionTypes).Returns(new List<DetectionType> { detectionType.Object });

            ApproachSpeedOptions options = new ApproachSpeedOptions()
            {
                //ApproachId = 2770, //CA - looks like this changed to SignalIdentifier??
                SignalIdentifier = "5000",
                End = end,
                SelectedBinSize = 15,
                Start = start
            };

            speedEvents = speedEvents.Where(s => s.DetectorId == detector.Object.DectectorIdentifier).ToList();

            ApproachSpeedResult viewModel = approachSpeedService.GetChartData(
                options, cycleEvents, planEvents, speedEvents, detector.Object); // Not sure if we need planEvents as an input - see PlanService.cs line 216

            //HACK: I couldn't find these
            //List<AverageSpeeds> expectedAvgSpeeds = new List<AverageSpeeds>() { 
            //    //{6/14/2023 12:00:00 PM}
            //    new AverageSpeeds(DateTime.Parse("6/14/2023 12:00:00 PM"), 43),
            //    //{6/14/2023 12:15:00 PM}
            //    new AverageSpeeds(DateTime.Parse("6/14/2023 12:15:00 PM"), 39),
            //    //{6/14/2023 12:30:00 PM}
            //    new AverageSpeeds(DateTime.Parse("6/14/2023 12:30:00 PM"), 39),
            //    //{6/14/2023 12:45:00 PM}
            //    new AverageSpeeds(DateTime.Parse("6/14/2023 12:45:00 PM"), 38),
            //};           
            //List<EightyFifthSpeeds> expectedEightyFifthSpeeds = new List<EightyFifthSpeeds>() { 
            //    //{6/14/2023 12:00:00 PM}
            //    new EightyFifthSpeeds(DateTime.Parse("6/14/2023 12:00:00 PM"), 51),
            //    //{6/14/2023 12:15:00 PM}
            //    new EightyFifthSpeeds(DateTime.Parse("6/14/2023 12:15:00 PM"), 47),
            //    //{6/14/2023 12:30:00 PM}
            //    new EightyFifthSpeeds(DateTime.Parse("6/14/2023 12:30:00 PM"), 48),
            //    //{6/14/2023 12:45:00 PM}
            //    new EightyFifthSpeeds(DateTime.Parse("6/14/2023 12:45:00 PM"), 47),
            //};
            //List<FifteenthSpeeds> expectedFifteenthSpeeds = new List<FifteenthSpeeds>() { 
            //    //{6/14/2023 12:00:00 PM}
            //    new FifteenthSpeeds(DateTime.Parse("6/14/2023 12:00:00 PM"), 35),
            //    //{6/14/2023 12:15:00 PM}
            //    new FifteenthSpeeds(DateTime.Parse("6/14/2023 12:15:00 PM"), 32),
            //    //{6/14/2023 12:30:00 PM}
            //    new FifteenthSpeeds(DateTime.Parse("6/14/2023 12:30:00 PM"), 30),
            //    //{6/14/2023 12:45:00 PM}
            //    new FifteenthSpeeds(DateTime.Parse("6/14/2023 12:45:00 PM"), 26),
            //};
            //var mockPlan = new SpeedPlan(new System.DateTime(2023, 6, 14, 12, 0, 0), new System.DateTime(2023, 6, 14, 1, 0, 0), "0", 40, 10, 48, 32);
            

            // Assert
            Assert.Equal(approach.Object.Id, viewModel.ApproachId);
            Assert.Equal(approach.Object.Signal.SignalIdentifier, viewModel.SignalIdentifier);
            //Assert.Equal("Wavetronix Advance: Speed Accuracy +/- 2mph", viewModel.DetectionType);
            Assert.Equal(255, viewModel.DistanceFromStopBar);
            Assert.Equal(start, viewModel.Start);
            Assert.Equal(end, viewModel.End);
            Assert.Equal(45, viewModel.PostedSpeed);
            Assert.Equal("WBT Ph2", viewModel.PhaseDescription);
            Assert.Equal(2, viewModel.PhaseNumber);

            //HACK: I couldn't find these
            //Assert.Equal(mockPlan.ToString(), viewModel.Plans.ToList()[0].ToString());
            //Assert.Equal(expectedAvgSpeeds.ToString(),viewModel.AverageSpeeds.ToString());
            //Assert.Equal(expectedEightyFifthSpeeds.ToString(),viewModel.EightyFifthSpeeds.ToString());
            //Assert.Equal(expectedFifteenthSpeeds.ToString(),viewModel.FifteenthSpeeds.ToString());
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
        private List<SpeedEvent> LoadSpeedEventsFromCsv(string fileName)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", fileName);
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //csv.Context.TypeConverterCache.AddConverter<DateTime>(new CustomDateTimeConverter());

                List<SpeedEvent> speedEvents = csv.GetRecords<SpeedEvent>().ToList();
                return speedEvents;
            }
        }
    }
}