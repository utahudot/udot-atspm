using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.PedDelay;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using CsvHelper;
using Moq;
using System.Globalization;
using System.Net;

namespace ATSPM.Application.Reports.Controllers.Tests
{
    public class PedDelayControllerTests
    {
        [Fact()]
        public void GetChartDataTest()
        {
            // Arrange
            PedDelayService pedDelayService = new PedDelayService();
            PedPhaseService pedPhaseService = new PedPhaseService();
            CycleService cycleService = new CycleService();


            System.DateTime start = new(2023, 5, 16, 8, 59, 0);
            System.DateTime end = new(2023, 5, 16, 12, 0, 5);

            List<IndianaEvent> events = LoadDetectorEventsFromCsv(@"PedDelayEventcodes.csv"); // Sampleevents

            List<IndianaEvent> cycleEvents = events.Where(e => new List<DataLoggerEnum> { DataLoggerEnum.PhaseBeginGreen, DataLoggerEnum.PhaseBeginYellowChange, DataLoggerEnum.PhaseEndYellowChange }.Contains(e.EventCode) && e.EventParam == 2).ToList(); // Sample cycle events
            List<IndianaEvent> pedEvents = events.Where(e => new List<DataLoggerEnum> { DataLoggerEnum.PedestrianBeginWalk, DataLoggerEnum.PedestrianBeginChangeInterval, DataLoggerEnum.PedestrianCallRegistered, DataLoggerEnum.PedDetectorOn }.Contains(e.EventCode) && e.EventParam == 2).ToList(); // Load detector events from CSV
            List<IndianaEvent> planEvents = events.Where(e => new List<DataLoggerEnum> { DataLoggerEnum.CoordPatternChange }.Contains(e.EventCode)).ToList(); // Load plan events from CSV

            // Create the mock Approach object
            var approach = new Mock<Approach>();

            // Set the properties of the mock Approach object
            approach.Object.Id = 1120; // Updated Id
            approach.Object.LocationId = 4080; // Updated LocationId
            approach.Object.DirectionTypeId = DirectionTypes.NB;
            approach.Object.Description = "NBT Ph2";
            approach.Object.Mph = 35;
            approach.Object.ProtectedPhaseNumber = 2;
            approach.Object.IsProtectedPhaseOverlap = false;
            approach.Object.PermissivePhaseNumber = null;
            approach.Object.IsPermissivePhaseOverlap = false;
            approach.Object.PedestrianPhaseNumber = null;
            approach.Object.IsPedestrianPhaseOverlap = false;
            approach.Object.PedestrianDetectors = null;

            var mockLocation = new Mock<Location>();

            // Set the properties of the mock Location object
            mockLocation.Object.Id = 4080; // Updated Id
            mockLocation.Object.LocationIdentifier = "5306"; // Updated LocationId
            mockLocation.Object.Latitude = 41.73907982;
            mockLocation.Object.Longitude = -111.8347528;
            mockLocation.Object.PrimaryName = "Main St.";
            mockLocation.Object.SecondaryName = "400 North";
            //mockLocation.Object.Ipaddress = IPAddress.Parse("10.239.5.15");
            mockLocation.Object.RegionId = 1;
            mockLocation.Object.LocationTypeId = 9; // Updated ControllerTypeId
            mockLocation.Object.ChartEnabled = true;
            mockLocation.Object.VersionAction = LocationVersionActions.Initial;
            mockLocation.Object.Note = "10";
            mockLocation.Object.Start = new System.DateTime(2011, 1, 1);
            mockLocation.Object.JurisdictionId = 35;
            mockLocation.Object.PedsAre1to1 = true;

            // Create the mock Approach object and set its Location property to the mock Location object
            approach.Setup(a => a.Location).Returns(mockLocation.Object);

            var options = new PedDelayOptions()
            {
                LocationIdentifier = "7115",
                PedRecallThreshold = 75,
                ShowCycleLength = true,
                ShowPedBeginWalk = false,
                ShowPedRecall = false,
                ShowPercentDelay = false,
                TimeBuffer = 15,
                Start = start,
                End = end
            };

            var pedPhaseData = pedPhaseService.GetPedPhaseData(
                options,
                approach.Object,
                planEvents.ToList(),
                pedEvents.ToList());

            var cycles = cycleService.GetRedToRedCycles(
                options.Start,
                options.End,
                cycleEvents.ToList());

            PedDelayResult viewModel = pedDelayService.GetChartData(
                options,
                pedPhaseData,
                cycles
                );

            // Assert
            //Assert.Equal(2190, events.Count);
            //Assert.Equal(22, cycleEvents.Count);
            //Assert.Equal(1031, pedEvents.Count);
            //Assert.Equal(13, planEvents.Count);

            Assert.Equal(0, pedPhaseData.MinDelay);
            Assert.Equal(57.2, pedPhaseData.MaxDelay);
            Assert.Equal(26.671428571428571, viewModel.AverageDelay);
            Assert.Equal(83, pedPhaseData.PedBeginWalkCount);
            Assert.Equal(21, pedPhaseData.PedPresses);
            Assert.Equal(15, pedPhaseData.PedCallsRegisteredCount);
            Assert.Equal(373.4, pedPhaseData.TotalDelay);
            Assert.Equal(17, pedPhaseData.UniquePedDetections);
            Assert.Equal(16, pedPhaseData.PedRequests);

            //Assert.Equal(2, result.PhaseNumber);
            //Assert.Equal("NBT Ph2", result.PhaseDescription);
            //Assert.Equal(start, result.Start);
            //Assert.Equal(end, result.End);
            //Assert.Equal(0, result.AverageDelayPerVehicle);
            //Assert.Equal(0, result.TotalDelay);
            //Assert.NotEmpty(result.Plans);
            //Assert.NotEmpty(result.ApproachDelayDataPoints);
            //Assert.NotEmpty(result.ApproachDelayPerVehicleDataPoints);


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