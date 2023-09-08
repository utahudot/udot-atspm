using Xunit;
using ATSPM.Application.Reports.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATSPM.Data.Models;
using Moq;
using ATSPM.Application.Reports.Business.PreemptService;
using ATSPM.Application.Reports.Business.Common;
using CsvHelper;
using System.Globalization;

namespace ATSPM.Application.Reports.Controllers.Tests
{
    public class PreemptServiceControllerTests
    {
        [Fact()]
        public void GetChartDataTest()
        {
            // Arrange
            PlanService planService = new PlanService();
            PreemptServiceService preemptServiceService = new PreemptServiceService(planService);
            
            //CycleService cycleService = new CycleService();
            //ILoggerFactory loggerFactory = new LoggerFactory();
            //ILogger<SignalPhaseService> logger = loggerFactory.CreateLogger<SignalPhaseService>();

            //SignalPhaseService signalPhaseService = new SignalPhaseService(planService, cycleService, logger);

            System.DateTime start = new System.DateTime(2023, 4, 17, 12, 0, 0);
            System.DateTime end = new System.DateTime(2023, 4, 17, 14, 0, 0);
            List<ControllerEventLog> events = LoadDetectorEventsFromCsv(@"ControllerEvents-Preempt.csv"); // Sampleevents
            List<ControllerEventLog> planEvents = events.Where(e => new List<int> { 131 }.Contains(e.EventCode)).ToList(); // Load plan events from CSV
            List<ControllerEventLog> preemptEvents = events.Where(e => new List<int> { 105 }.Contains(e.EventCode)).ToList(); // Load preempt events from CSV


            // Create the mock Approach object
            //var approach = new Mock<Approach>();

            //// Set the properties of the mock Approach object
            //approach.Object.Id = 2880; // Updated Id
            //approach.Object.SignalId = 1680; // Updated SignalId
            //approach.Object.DirectionTypeId = DirectionTypes.NB;
            //approach.Object.Description = "NBT Ph2";
            //approach.Object.Mph = 45;
            //approach.Object.ProtectedPhaseNumber = 2;
            //approach.Object.IsProtectedPhaseOverlap = false;
            //approach.Object.PermissivePhaseNumber = null;
            //approach.Object.IsPermissivePhaseOverlap = false;
            //approach.Object.PedestrianPhaseNumber = null;
            //approach.Object.IsPedestrianPhaseOverlap = false;
            //approach.Object.PedestrianDetectors = null;

            //var mockSignal = new Mock<Signal>();

            //// Set the properties of the mock Signal object
            //mockSignal.Object.Id = 1680; // Updated Id
            //mockSignal.Object.SignalId = "7115"; // Updated SignalId
            //mockSignal.Object.Latitude = "40.62398502";
            //mockSignal.Object.Longitude = "-111.9387819";
            //mockSignal.Object.PrimaryName = "Redwood Road";
            //mockSignal.Object.SecondaryName = "7000 South";
            //mockSignal.Object.Ipaddress = IPAddress.Parse("10.210.14.39");
            //mockSignal.Object.RegionId = 2;
            //mockSignal.Object.ControllerTypeId = 2; // Updated ControllerTypeId
            //mockSignal.Object.Enabled = true;
            //mockSignal.Object.VersionActionId = SignaVersionActions.Initial;
            //mockSignal.Object.Note = "10";
            //mockSignal.Object.Start = new System.DateTime(2011, 1, 1);
            //mockSignal.Object.JurisdictionId = 35;
            //mockSignal.Object.Pedsare1to1 = true;

            //// Create the mock Approach object and set its Signal property to the mock Signal object
            //approach.Setup(a => a.Signal).Returns(mockSignal.Object);

            var options = new PreemptServiceMetricOptions() { SignalId = "7573", Start = start, End = end};

            //SignalPhase signalPhase = signalPhaseService.GetSignalPhaseData(start, end, true, 0, 15, approach.Object, cycleEvents, planEvents, detectorEvents);
            var viewModel = preemptServiceService.GetChartData(options, planEvents, preemptEvents);

            // Assert
            Assert.Equal(15, viewModel.PreemptServiceEvents.Count);
            //Assert.NotEmpty(viewModel.ApproachDelayPerVehicleDataPoints);


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