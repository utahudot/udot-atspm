using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Moq;
using CsvHelper;
using System.Globalization;
using Microsoft.Extensions.Logging;
using System.Net;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.ApproachSpeed;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Application.TempExtensions;

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
            ILogger<LocationPhaseService> logger = loggerFactory.CreateLogger<LocationPhaseService>();
            LocationPhaseService LocationPhaseService = new LocationPhaseService(planService, cycleService, logger); 
            
            System.DateTime start = new System.DateTime(2023, 6, 14, 12, 0, 0);
            System.DateTime end = new System.DateTime(2023, 6, 14, 13, 0, 0);
            List<IndianaEvent> events = LoadDetectorEventsFromCsv(@"ECsForApproachSpeedTest.csv"); // Sampleevents
            List<SpeedEvent> speedEvents = LoadSpeedEventsFromCsv(@"SpeedEvents5000-20230614.csv");
            List<IndianaEvent> cycleEvents = events.Where(e => new List<DataLoggerEnum> { DataLoggerEnum.PhaseBeginGreen, DataLoggerEnum.PhaseBeginYellowChange, DataLoggerEnum.PhaseEndYellowChange }.Contains(e.EventCode)).ToList(); // Sample cycle events
            List<IndianaEvent> planEvents = events.Where(e => new List<DataLoggerEnum> { DataLoggerEnum.CoordPatternChange }.Contains(e.EventCode)).ToList(); // Load plan events from CSV


            // Create the mock Approach object
            var approach = new Mock<Approach>();

            // Set the properties of the mock Approach object
            approach.Object.Id = 29724; // Updated Id
            approach.Object.LocationId = 4938; // Updated LocationId
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
            var measureType = new Mock<MeasureType>();

            // Set the properties of the mock Detector object
            detector.Object.Id = 101540; // Updated Id
            detector.Object.ApproachId = 29724; //int
            detector.Object.DateAdded = new System.DateTime(2023, 04, 18); //DateTime
            detector.Object.DateDisabled = null; //DateTime
            detector.Object.DectectorIdentifier = "500006"; // Updated LocationId
            detector.Object.DetectorChannel = 6; //int
            detector.Object.DetectionHardware = DetectionHardwareTypes.WavetronixAdvance;
            detector.Object.MovementType = MovementTypes.T;
            detector.Object.LaneNumber = 1; //int
            detector.Object.LaneType = LaneTypes.V;
            detector.Object.DistanceFromStopBar = 255; //int
            detector.Object.DecisionPoint = 0; //int
            detector.Object.MovementDelay = null; //int
            detector.Object.MinSpeedFilter = 5; //int
            //detectionType.Object.Description = "Advanced Speed";
            detectionType.Object.Abbreviation = "AS";
            detectionType.Object.Description = "Advanced Speed";
            detectionType.Object.Id = DetectionTypes.AS;
            measureType.Object.Id = 10;

            //myDetectionType.Detectors = new List<Detector> { };
            //myDetectionType.MetricTypeMetrics = new List<MetricType> { };
            //detector.Object.DetectionTypes = new List<DetectionType> { myDetectionType };
            //detector.Object.DetectionTypes = new List<DetectionType> { myDetectionType };  // no matter what I have tried, this detection type keeps returning as null, causing issues down the road
            detectionType.Setup(d => d.MeasureTypes).Returns(new List<MeasureType>() { measureType.Object });
            detector.Setup(d => d.DetectionTypes).Returns(new  List<DetectionType>() { detectionType.Object });

            //Need this
            var mockLocation = new Mock<Location>();

            // Set the properties of the mock Location object
            mockLocation.Object.Id = 4938; // Updated Id
            //mockLocation.Object.Ipaddress = IPAddress.Parse("10.235.5.15");
            mockLocation.Object.ChartEnabled = true;
            //mockLocation.Object.LoggingEnabled = true;
            mockLocation.Object.Note = "Copy of Now a Cobalt (32.67.20)";
            mockLocation.Object.JurisdictionId = 4;
            mockLocation.Object.LocationIdentifier = "5000"; // Updated LocationId
            mockLocation.Object.VersionAction = LocationVersionActions.NewVersion;
            mockLocation.Object.Start = new System.DateTime(2023, 4, 18);
            mockLocation.Object.LocationTypeId = 9; // Updated ControllerTypeId
            mockLocation.Object.RegionId = 1;
            mockLocation.Object.PrimaryName = "Riverdale Road";
            mockLocation.Object.SecondaryName = "700 West";
            mockLocation.Object.Latitude = 41.18003983;
            mockLocation.Object.Longitude = -111.9956664;
            mockLocation.Object.PedsAre1to1 = true;

            // Create the mock Approach object and set its Location property to the mock Location object
            approach.Setup(a => a.Location).Returns(mockLocation.Object);
            approach.Setup(a => a.Detectors).Returns(new List<Detector>() { detector.Object });
            detector.Setup(a => a.Approach).Returns(approach.Object);
            detector.Setup(a => a.DetectionTypes).Returns(new List<DetectionType> { detectionType.Object });
            mockLocation.Setup(mock => mock.Approaches).Returns(new List<Approach>() { approach.Object });

            ApproachSpeedOptions options = new ApproachSpeedOptions()
            {
                //ApproachId = 2770, //CA - looks like this changed to LocationIdentifier??
                LocationIdentifier = "5000",
                End = end,
                BinSize = 15,
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
            Assert.Equal(approach.Object.Location.LocationIdentifier, viewModel.locationIdentifier);
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