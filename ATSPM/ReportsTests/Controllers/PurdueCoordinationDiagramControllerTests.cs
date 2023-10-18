using Xunit;
using ATSPM.Application.Reports.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Moq;
using Reports.Business.Common;
using System.Globalization;
using System.Net;
using Reports.Business.PurdueCoordinationDiagram;
using ATSPM.Application.Reports.Business.PerdueCoordinationDiagram;
using ATSPM.Application.Reports.Business.PedDelay;

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
            ILogger<SignalPhaseService> logger = loggerFactory.CreateLogger<SignalPhaseService>();

            SignalPhaseService signalPhaseService = new SignalPhaseService(planService, cycleService, logger);

            System.DateTime start = new System.DateTime(2020, 12, 01, 6, 0, 0);
            System.DateTime end = new System.DateTime(2020, 12, 01, 7, 0, 0);
            List<ControllerEventLog> events = LoadDetectorEventsFromCsv(@"PCDevents.csv"); // Sampleevents
            List<ControllerEventLog> cycleEvents = events.Where(e => new List<int> { 1, 8, 9 }.Contains(e.EventCode)).ToList(); // Sample cycle events
            List<ControllerEventLog> detectorEvents = events.Where(e => new List<int> { 82 }.Contains(e.EventCode)).ToList(); // Load detector events from CSV
            List<ControllerEventLog> planEvents = events.Where(e => new List<int> { 131 }.Contains(e.EventCode)).ToList(); // Load plan events from CSV

            // Create the mock Approach object
            var approach = new Mock<Approach>();

            // Set the properties of the mock Approach object
            approach.Object.Id = 3106; // Updated Id
            approach.Object.SignalId = 1933; // Updated SignalId
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
            mockSignal.Object.Id = 1933; // Updated Id
            mockSignal.Object.SignalIdentifier = "7191"; // Updated SignalId
            mockSignal.Object.Latitude = 40.69988569;
            mockSignal.Object.Longitude = -111.8713268;
            mockSignal.Object.PrimaryName = "700 East";
            mockSignal.Object.SecondaryName = "3300 South";
            mockSignal.Object.Ipaddress = IPAddress.Parse("10.202.6.75");
            mockSignal.Object.RegionId = 2;
            mockSignal.Object.ControllerTypeId = 4; // Updated ControllerTypeId
            mockSignal.Object.ChartEnabled = true;
            mockSignal.Object.LoggingEnabled = true;
            mockSignal.Object.VersionActionId = SignaVersionActions.Initial;
            mockSignal.Object.Note = "Initial";
            mockSignal.Object.Start = new System.DateTime(2011, 1, 1);
            mockSignal.Object.JurisdictionId = 35;
            mockSignal.Object.Pedsare1to1 = true;

            // Create the mock Approach object and set its Signal property to the mock Signal object
            approach.Setup(a => a.Signal).Returns(mockSignal.Object);
            var approaches = new List<Approach>() { approach.Object};
            mockSignal.Setup(a => a.Approaches).Returns(approaches);

            var phaseDetail = phaseService.GetPhases(mockSignal.Object);

            var options = new PurdueCoordinationDiagramOptions() { SignalIdentifier = "7191", SelectedBinSize = 15, Start = start, End = end, ShowVolumes = true };

            SignalPhase signalPhase = await signalPhaseService.GetSignalPhaseData(phaseDetail.FirstOrDefault(), start, end, true, 0, 15, cycleEvents, planEvents, detectorEvents);
            var result = purdueCoordinationDiagramService.GetChartData(options, approach.Object, signalPhase);

            // Assert
            //Assert.Equal(approach.Object.Id, result.ApproachId);
            //Assert.Equal(approach.Object.Signal.SignalIdentifier, result.SignalIdentifier);
            //Assert.Equal(2, result.PhaseNumber);
            //Assert.Equal("NBT Ph2", result.PhaseDescription);
            //Assert.Equal(start, result.Start);
            //Assert.Equal(end, result.End);
            //Assert.Equal(199, result.TotalOnGreenEvents);
            //Assert.Equal(382, result.TotalDetectorHits);
            //Assert.Equal(52, result.PercentArrivalOnGreen);
            


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