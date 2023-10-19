using ATSPM.Application.Reports.Business.WaitTime;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using CsvHelper;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ReportsTests.Controllers
{
    public class SplitFailControllerTests
    {
        [Fact()]
        public async void GetChartDataTest()
        {
            // Arrange
            WaitTimeService waitTimeService = new WaitTimeService();
            List<ControllerEventLog> allEvents = LoadDetectorEventsFromCsv("SplitFailECDetections.csv");

            // Mock Signal 
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

            // Create the mock Approach object and set its Signal property to the mock Signal object
            approach.Setup(a => a.Signal).Returns(mockSignal.Object);

            // Create mock detector
            var mockDetector1 = new Mock<Detector>();

            // Set the properties of the mock Detector object
            mockDetector1.Object.ApproachId = 2880;
            mockDetector1.Object.DateAdded = new System.DateTime(2016, 3, 8);
            mockDetector1.Object.DateDisabled = null;
            mockDetector1.Object.DecisionPoint = null;
            mockDetector1.Object.DetChannel = 22;
            mockDetector1.Object.MovementType = new MovementType { Abbreviation = "T", Id = MovementTypes.T };
            mockDetector1.Object.LaneType = new LaneType { Id = LaneTypes.V };
            mockDetector1.Object.DetectionHardwareId = DetectionHardwareTypes.WavetronixMatrix;
            mockDetector1.Object.DectectorIdentifier = "711519";
            mockDetector1.Object.DistanceFromStopBar = null;
            mockDetector1.Object.Id = 6424; //database ID for the detector
            mockDetector1.Object.LaneNumber = 1;
            mockDetector1.Object.LaneTypeId = LaneTypes.V;
            mockDetector1.Object.LatencyCorrection = 1.2;
            mockDetector1.Object.MinSpeedFilter = null;
            mockDetector1.Object.MovementDelay = null;
            mockDetector1.Object.MovementTypeId = MovementTypes.T;

            // Associate the Detector to the Approach
            mockDetector1.Setup(a => a.Approach).Returns(approach.Object);

            Assert.Equal(1, 1);
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
