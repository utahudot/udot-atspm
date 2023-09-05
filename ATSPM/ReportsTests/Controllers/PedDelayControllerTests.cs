using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PedDelay;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using CsvHelper;
using Moq;
using System.Globalization;
using System.Net;
using Xunit;

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


            System.DateTime start = new System.DateTime(2023, 5, 16, 8, 56, 0);
            System.DateTime end = new System.DateTime(2023, 5, 16, 12, 1, 0);
            List<ControllerEventLog> events = LoadDetectorEventsFromCsv(@"PedDelayEventcodes.csv"); // Sampleevents
            List<ControllerEventLog> cycleEvents = events.Where(e => new List<int> { 1, 8, 9 }.Contains(e.EventCode)).ToList(); // Sample cycle events
            List<ControllerEventLog> pedEvents = events.Where(e => new List<int> { 21, 22, 45, 90 }.Contains(e.EventCode)).ToList(); // Load detector events from CSV
            List<ControllerEventLog> planEvents = events.Where(e => new List<int> { 131 }.Contains(e.EventCode)).ToList(); // Load plan events from CSV

            // Create the mock Approach object
            var approach = new Mock<Approach>();

            // Set the properties of the mock Approach object
            approach.Object.Id = 1120; // Updated Id
            approach.Object.SignalId = 4080; // Updated SignalId
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

            var mockSignal = new Mock<Signal>();

            // Set the properties of the mock Signal object
            mockSignal.Object.Id = 4080; // Updated Id
            mockSignal.Object.SignalId = "5306"; // Updated SignalId
            mockSignal.Object.Latitude = "41.73907982";
            mockSignal.Object.Longitude = "-111.8347528";
            mockSignal.Object.PrimaryName = "Main St.";
            mockSignal.Object.SecondaryName = "400 North";
            mockSignal.Object.Ipaddress = IPAddress.Parse("10.239.5.15");
            mockSignal.Object.RegionId = 1;
            mockSignal.Object.ControllerTypeId = 9; // Updated ControllerTypeId
            mockSignal.Object.Enabled = true;
            mockSignal.Object.VersionActionId = SignaVersionActions.Initial;
            mockSignal.Object.Note = "10";
            mockSignal.Object.Start = new System.DateTime(2011, 1, 1);
            mockSignal.Object.JurisdictionId = 35;
            mockSignal.Object.Pedsare1to1 = true;

            // Create the mock Approach object and set its Signal property to the mock Signal object
            approach.Setup(a => a.Signal).Returns(mockSignal.Object);

            var options = new PedDelayOptions()
            {
                ApproachId = 1120,
                PedRecallThreshold = 75,
                ShowCycleLength = true,
                ShowPedBeginWalk = false,
                ShowPedRecall = false,
                ShowPercentDelay = false,
                TimeBuffer = 15,
                UsePermissivePhase = false,
                Start = new System.DateTime(2023, 5, 16, 8, 56, 0),
                End = new System.DateTime(2023, 5, 16, 12, 1, 0)
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
            Assert.Equal(2190, events.Count);
            Assert.Equal(1558, cycleEvents.Count);
            Assert.Equal(619, pedEvents.Count);
            Assert.Equal(13, planEvents.Count);

            Assert.Equal(0, pedPhaseData.MinDelay);
            Assert.Equal(57.19999967, pedPhaseData.MaxDelay);
            Assert.Equal(26.6714284, viewModel.AverageDelay);


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