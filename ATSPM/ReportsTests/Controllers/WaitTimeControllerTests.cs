using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.WaitTime;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using CsvHelper;
using Moq;
using System.Globalization;
using Xunit;

namespace ATSPM.Application.Reports.Controllers.Tests
{
    public class WaitTimeControllerTests
    {
        [Fact()]
        public void GetChartDataTest()
        {
            // Arrange
            //PlanService planService = new PlanService();
            WaitTimeService waitTimeService = new WaitTimeService(); //might need to change the service to have inputs

            List<ControllerEventLog> allEvents = LoadAllEventsFromCsv(@"WaitTime_EventLogs.csv"); // Sampleevents
            List<ControllerEventLog> planEvents = allEvents.Where(e => new List<int> { 131 }.Contains(e.EventCode)).ToList(); // Load plan events from CSV


            // Create a mock DetectionType object
            var detectionTypeSBP = new Mock<DetectionType>();
            detectionTypeSBP.Object.Id = (DetectionTypes)6;
            detectionTypeSBP.Object.Abbreviation = "SBP";

            // Create a mock DetectionType object
            var detectionTypeAC = new Mock<DetectionType>();
            detectionTypeAC.Object.Id = (DetectionTypes)2;
            detectionTypeAC.Object.Abbreviation = "AC";

            var mockDetectorN1 = new Mock<Detector>();
            // Set the properties of the mock Detector object
            mockDetectorN1.Object.ApproachId = 15255;
            mockDetectorN1.Object.DetChannel = 19;
            mockDetectorN1.Object.DectectorIdentifier = "711519";
            mockDetectorN1.Object.Id = 50747;
            mockDetectorN1.Object.LaneNumber = 1;
            mockDetectorN1.Object.LaneTypeId = LaneTypes.V;
            mockDetectorN1.Object.LatencyCorrection = 1.2;
            mockDetectorN1.Object.MovementTypeId = MovementTypes.T;
            mockDetectorN1.Object.DetectionTypes = new List<DetectionType>() { detectionTypeSBP.Object };
            mockDetectorN1.Setup(t => t.DetectionTypes).Returns(new List<DetectionType>() { detectionTypeSBP.Object });

            var mockDetectorN2 = new Mock<Detector>();
            // Set the properties of the mock Detector object
            mockDetectorN2.Object.ApproachId = 15255;
            mockDetectorN2.Object.DetChannel = 20;
            mockDetectorN2.Object.DectectorIdentifier = "711520";
            mockDetectorN2.Object.Id = 50748;
            mockDetectorN2.Object.LaneNumber = 2;
            mockDetectorN2.Object.LaneTypeId = LaneTypes.V;
            mockDetectorN2.Object.LatencyCorrection = 1.2;
            mockDetectorN2.Object.MovementTypeId = MovementTypes.T;
            mockDetectorN2.Object.DetectionTypes = new List<DetectionType>() { detectionTypeSBP.Object };
            mockDetectorN2.Setup(t => t.DetectionTypes).Returns(new List<DetectionType>() { detectionTypeSBP.Object });

            var mockDetectorN3 = new Mock<Detector>();
            // Set the properties of the mock Detector object
            mockDetectorN3.Object.ApproachId = 15255;
            mockDetectorN3.Object.DetChannel = 21;
            mockDetectorN3.Object.DectectorIdentifier = "711521";
            mockDetectorN3.Object.Id = 50749;
            mockDetectorN3.Object.LaneNumber = 3;
            mockDetectorN3.Object.LaneTypeId = LaneTypes.V;
            mockDetectorN3.Object.LatencyCorrection = 1.2;
            mockDetectorN3.Object.MovementTypeId = MovementTypes.T;
            mockDetectorN3.Object.DetectionTypes = new List<DetectionType>() { detectionTypeSBP.Object };
            mockDetectorN3.Setup(t => t.DetectionTypes).Returns(new List<DetectionType>() { detectionTypeSBP.Object });

            var mockDetectorN4 = new Mock<Detector>();
            // Set the properties of the mock Detector object
            mockDetectorN4.Object.ApproachId = 15255;
            mockDetectorN4.Object.DetChannel = 02;
            mockDetectorN4.Object.DectectorIdentifier = "711502";
            //mockDetectorN4.Object.Id = null;
            mockDetectorN4.Object.LaneNumber = 1;
            mockDetectorN4.Object.LaneTypeId = LaneTypes.V;
            mockDetectorN4.Object.LatencyCorrection = 0;
            //mockDetectorN4.Object.MovementTypeId = null;
            mockDetectorN4.Object.DetectionTypes = new List<DetectionType>() { detectionTypeAC.Object };
            mockDetectorN4.Setup(t => t.DetectionTypes).Returns(new List<DetectionType>() { detectionTypeAC.Object });


            var detectorsN = new List<Detector>() { mockDetectorN1.Object, mockDetectorN2.Object, mockDetectorN3.Object, mockDetectorN4.Object };

            // Create the mock DirectionType object
            var directionType = new Mock<DirectionType>();
            directionType.Object.Description = "Northbound";
            directionType.Object.Abbreviation = "NB";
            directionType.Object.Id = (DirectionTypes)1;
            directionType.Object.DisplayOrder = 3;

            // Create the mock Approach object
            var approach2 = new Mock<Approach>();
            // Set the properties of the mock Approach object
            approach2.Object.Id = 15255; // Updated Id
            approach2.Object.SignalId = 2934; // Updated SignalId
            approach2.Object.DirectionTypeId = DirectionTypes.NB;
            approach2.Object.Description = "NBT Ph2";
            approach2.Object.Mph = null;
            approach2.Object.ProtectedPhaseNumber = 2;
            approach2.Object.IsProtectedPhaseOverlap = false;
            approach2.Object.PermissivePhaseNumber = null;
            approach2.Object.IsPermissivePhaseOverlap = false;
            approach2.Object.PedestrianPhaseNumber = null;
            approach2.Object.IsPedestrianPhaseOverlap = false;
            approach2.Object.PedestrianDetectors = null;
            approach2.Object.Detectors = detectorsN;
            approach2.Setup(d => d.Detectors).Returns(detectorsN);

            //var mockSignal = new Mock<Signal>();

            //// Set the properties of the mock Signal object
            //mockSignal.Object.Id = 2840; // Updated Id
            //mockSignal.Object.SignalId = "6387"; // Updated SignalId
            //mockSignal.Object.Latitude = "40.62398502";
            //mockSignal.Object.Longitude = "-111.9387819";
            //mockSignal.Object.PrimaryName = "Redwood Road";
            //mockSignal.Object.SecondaryName = "7000 South";
            //mockSignal.Object.Ipaddress = IPAddress.Parse("10.210.14.39");
            //mockSignal.Object.RegionId = 2;
            //mockSignal.Object.ControllerTypeId = 2; // Updated ControllerTypeId
            //mockSignal.Object.Enabled = true;
            //mockSignal.Object.VersionActionId = SignaVersionActions.Delete;
            //mockSignal.Object.Note = "Updated missing zones";
            //mockSignal.Object.Start = new System.DateTime(1900, 1, 1);
            //mockSignal.Object.JurisdictionId = 35;
            //mockSignal.Object.Pedsare1to1 = true;
            //mockSignal.Object.Approaches = approaches;
            //// Create the mock Approach object and set its Signal property to the mock Signal object
            //approach2.Setup(a => a.Signal).Returns(mockSignal.Object);
            //approach6.Setup(a => a.Signal).Returns(mockSignal.Object);
            //mockSignal.Setup(a => a.Approaches).Returns(approaches);
            AnalysisPhaseData partialAnalysisPhaseData = new AnalysisPhaseData();
            //partialAnalysisPhaseData.TerminationEvents = terminationEvents;

            var options = new WaitTimeOptions()
            {
                //ApproachId = 15287,
                BinSize = 15,
                Start = new System.DateTime(2023, 6, 13, 6, 0, 0),
                End = new System.DateTime(2023, 6, 13, 9, 0, 0)
            };


            //var detectors = approach.GetDetectorsForMetricType(5);
            //var detectors = new List<Detector> { mockDetector1.Object, mockDetector2.Object };

            //List<WaitTimeResult> viewModel = waitTimeService.GetChartData(
            //    options,
            //    approach2,
            //    allEvents,
            //    partialAnalysisPhaseData,
            //    planEvents//,
            //    volumeCollection
            //    );


            //Assert.Equal(86.46, viewModel[1].PercentTurnableSeries.ToList()[0].Seconds);
            //Assert.Equal(75.44, viewModel[1].PercentTurnableSeries.ToList()[1].Seconds);
            //Assert.Equal(72.79, viewModel[1].PercentTurnableSeries.ToList()[2].Seconds);
            //Assert.Equal(79.44, viewModel[1].PercentTurnableSeries.ToList()[3].Seconds); 

            //Assert.NotEmpty(result.ApproachDelayPerVehicleDataPoints);


        }

        private List<ControllerEventLog> LoadAllEventsFromCsv(string fileName)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", fileName);
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                //csv.Context.TypeConverterCache.AddConverter<DateTime>(new CustomDateTimeConverter());

                List<ControllerEventLog> allevents = csv.GetRecords<ControllerEventLog>().ToList();
                return allevents;
            }
        }
    }
}