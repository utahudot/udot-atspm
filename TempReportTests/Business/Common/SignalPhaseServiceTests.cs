using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using System.Net;
using Xunit;

namespace ATSPM.Application.Reports.Business.Common.Tests
{
    public class Startup
    {

    }

    public class SignalPhaseServiceTests
    {
        [Fact()]
        public async void GetSignalPhaseDataTest()
        {
            // Arrange
            PlanService planService = new PlanService(); // Replace with your PlanService instance
            CycleService cycleService = new CycleService(); // Replace with your CycleService instance
            ILoggerFactory loggerFactory = new LoggerFactory(); // Create an instance of ILoggerFactory
            ILogger<SignalPhaseService> logger = loggerFactory.CreateLogger<SignalPhaseService>(); // Create the ILogger<SignalPhaseService> instance
            PhaseService phaseService = new PhaseService();

            SignalPhaseService signalPhaseService = new SignalPhaseService(planService, cycleService, logger);


            DateTime start = new DateTime(2020, 12, 1, 6, 0, 0);
            DateTime end = new DateTime(2020, 12, 1, 7, 0, 0);
            List<ControllerEventLog> events = LoadDetectorEventsFromCsv(@"ControllerEventLogs-7119-Phase2-CycleDetectorPlanEvents-20201201-00-24.csv"); // Sampleevents
            List<ControllerEventLog> cycleEvents = events.Where(e => new List<int> { 1, 8, 9 }.Contains(e.EventCode)).ToList(); // Sample cycle events
            List<ControllerEventLog> detectorEvents = events.Where(e => new List<int> { 82 }.Contains(e.EventCode)).ToList(); // Load detector events from CSV
            List<ControllerEventLog> planEvents = events.Where(e => new List<int> { 131 }.Contains(e.EventCode)).ToList(); // Load plan events from CSV

            // Create the mock Approach object
            var approach = new Mock<Approach>();

            // Set the properties of the mock Approach object
            approach.Object.Id = 3106;
            approach.Object.SignalId = 1933;
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
            mockSignal.Object.Id = 1933;
            mockSignal.Object.SignalIdentifier = "7191";
            mockSignal.Object.Latitude = 40.69988569;
            mockSignal.Object.Longitude = -111.8713268;
            mockSignal.Object.PrimaryName = "700 East";
            mockSignal.Object.SecondaryName = "3300 South";
            mockSignal.Object.Ipaddress = IPAddress.Parse("10.202.6.75");
            mockSignal.Object.RegionId = 2;
            mockSignal.Object.ControllerTypeId = 4;
            mockSignal.Object.ChartEnabled = true;
            mockSignal.Object.VersionAction = SignalVersionActions.Initial;
            mockSignal.Object.Note = "10";
            mockSignal.Object.Start = new DateTime(2011, 1, 1);
            mockSignal.Object.JurisdictionId = 35;
            mockSignal.Object.Pedsare1to1 = true;

            // Create the mock Approach object and set its Signal property to the mock Signal object
            approach.Setup(a => a.Signal).Returns(mockSignal.Object);
            var phaseDetail = phaseService.GetPhases(mockSignal.Object);

            SignalPhase result = await signalPhaseService.GetSignalPhaseData(phaseDetail.FirstOrDefault(), start, end, true, 0, 15, cycleEvents, planEvents, detectorEvents);

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