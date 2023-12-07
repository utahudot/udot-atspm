using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.TurningMovementCounts;
using CsvHelper;
using Moq;
using System.Globalization;
using System.Net;
using ATSPM.ReportApi.TempExtensions;
using ATSPM.Application.Extensions;

namespace ATSPM.Application.Reports.Controllers.Tests
{
    public class TurningMovementCountsControllerTests
    {
        [Fact()]
        public async void GetChartDataTest()
        {
            // Arrange
            PlanService planService = new PlanService();
            TurningMovementCountsService turningMovementCountsService = new TurningMovementCountsService(planService);

            System.DateTime start = new System.DateTime(2023, 5, 16, 8, 56, 0);
            System.DateTime end = new System.DateTime(2023, 5, 16, 12, 1, 0);
            List<ControllerEventLog> events = LoadDetectorEventsFromCsv(@"TMCEventcodes.csv"); // Sampleevents
            List<ControllerEventLog> planEvents = events.Where(e => new List<int> { 131 }.Contains(e.EventCode)).ToList(); // Load plan events from CSV

            // Create the mock DirectionType object
            var directionType = new Mock<DirectionType>();
            directionType.Object.Description = "Westbound";
            directionType.Object.Abbreviation = "WB";
            directionType.Object.Id = (DirectionTypes)4;
            directionType.Object.DisplayOrder = 2;

            // Create the mock Approach object
            var approach = new Mock<Approach>();

            // Set the properties of the mock Approach object
            approach.Object.Id = 14239; // Updated Id
            approach.Object.SignalId = 2840; // Updated SignalId
            approach.Object.DirectionTypeId = DirectionTypes.WB;
            approach.Object.Description = "WBT Ph2";
            approach.Object.Mph = 35;
            approach.Object.ProtectedPhaseNumber = 2;
            approach.Object.IsProtectedPhaseOverlap = false;
            approach.Object.PermissivePhaseNumber = null;
            approach.Object.IsPermissivePhaseOverlap = false;
            approach.Object.PedestrianPhaseNumber = null;
            approach.Object.IsPedestrianPhaseOverlap = false;
            approach.Object.PedestrianDetectors = null;
            approach.Object.DirectionType = directionType.Object;

            var mockSignal = new Mock<Signal>();

            // Set the properties of the mock Signal object
            mockSignal.Object.Id = 2840; // Updated Id
            mockSignal.Object.SignalIdentifier = "6387"; // Updated SignalId
            mockSignal.Object.Latitude = 40.326352;
            mockSignal.Object.Longitude = -111.724889;
            mockSignal.Object.PrimaryName = "1600 N (SR-241)";
            mockSignal.Object.SecondaryName = "1200 W";
            mockSignal.Object.Ipaddress = IPAddress.Parse("10.163.6.51");
            mockSignal.Object.RegionId = 3;
            mockSignal.Object.ControllerTypeId = 2; // Updated ControllerTypeId
            mockSignal.Object.ChartEnabled = true;
            mockSignal.Object.VersionAction = SignalVersionActions.Initial;
            mockSignal.Object.Note = "Initial - WAS #6500";
            mockSignal.Object.Start = new System.DateTime(1900, 1, 1);
            mockSignal.Object.JurisdictionId = 21;
            mockSignal.Object.Pedsare1to1 = true;

            // Create the mock Approach object and set its Signal property to the mock Signal object
            approach.Setup(a => a.Signal).Returns(mockSignal.Object);

            var options = new TurningMovementCountsOptions()
            {
                SignalIdentifier = "6387",
                SelectedBinSize = 15,
                Start = new System.DateTime(2023, 6, 14, 12, 0, 0),
                End = new System.DateTime(2023, 6, 14, 23, 59, 0)
            };

            var mockDetector1 = new Mock<Detector>();

            // Set the properties of the mock Detector object
            //mockDetector.Object.AllDetectionTypes = ?;
            //mockDetector.Object.AllHardwareTypes = ?;
            //mockDetector.Object.Approach = ?;
            mockDetector1.Object.ApproachId = 14239;
            mockDetector1.Object.DateAdded = new System.DateTime(2019, 12, 16);
            mockDetector1.Object.DateDisabled = null;
            mockDetector1.Object.DecisionPoint = null;
            mockDetector1.Object.DetectorChannel = 22;
            mockDetector1.Object.MovementType = MovementTypes.T;
            mockDetector1.Object.LaneType = LaneTypes.V;
            //mockDetector.Object.DetectionHardware = ?;
            mockDetector1.Object.DetectionHardware = DetectionHardwareTypes.WavetronixMatrix;
            //mockDetector.Object.DetectionIDs = ?;
            //mockDetector.Object.DetectionTypes = ?;
            //mockDetector.Object.DetectorComments = ?;
            mockDetector1.Object.DectectorIdentifier = "638722";
            mockDetector1.Object.DistanceFromStopBar = null;
            //mockDetector.Object.HasErrors = ?;
            mockDetector1.Object.Id = 47742;
            //mockDetector.Object.Index = ?;
            //mockDetector.Object.IsChanged = ?;
            mockDetector1.Object.LaneNumber = 1;
            //mockDetector.Object.LaneType = ?;
            mockDetector1.Object.LatencyCorrection = 0;
            mockDetector1.Object.MinSpeedFilter = null;
            mockDetector1.Object.MovementDelay = null;
            //mockDetector.Object.MovementType = ?;

            mockDetector1.Setup(a => a.Approach).Returns(approach.Object);

            var mockDetector2 = new Mock<Detector>();

            // Set the properties of the mock Detector object
            //mockDetector.Object.AllDetectionTypes = ?;
            //mockDetector.Object.AllHardwareTypes = ?;
            //mockDetector.Object.Approach = ?;
            mockDetector2.Object.ApproachId = 14239;
            mockDetector2.Object.DateAdded = new System.DateTime(2019, 12, 16);
            mockDetector2.Object.DateDisabled = null;
            mockDetector2.Object.DecisionPoint = null;
            mockDetector2.Object.DetectorChannel = 23;
            //mockDetector.Object.DetectionHardware = ?;
            mockDetector2.Object.DetectionHardware = DetectionHardwareTypes.WavetronixMatrix;
            mockDetector2.Object.MovementType = MovementTypes.TR;
            mockDetector2.Object.LaneType = LaneTypes.V;
            //mockDetector.Object.DetectionIDs = ?;
            //mockDetector.Object.DetectionTypes = ?;
            //mockDetector.Object.DetectorComments = ?;
            mockDetector2.Object.DectectorIdentifier = "638723";
            mockDetector2.Object.DistanceFromStopBar = null;
            //mockDetector.Object.HasErrors = ?;
            mockDetector2.Object.Id = 47743;
            //mockDetector.Object.Index = ?;
            //mockDetector.Object.IsChanged = ?;
            mockDetector2.Object.LaneNumber = 2;
            //mockDetector.Object.LaneType = ?;
            mockDetector2.Object.LatencyCorrection = 0;
            mockDetector2.Object.MinSpeedFilter = null;
            mockDetector2.Object.MovementDelay = null;
            //mockDetector.Object.MovementType = ?;

            mockDetector2.Setup(a => a.Approach).Returns(approach.Object);

            //var detectors = approach.GetDetectorsForMetricType(5);
            var detectors = new List<Detector> { mockDetector1.Object, mockDetector2.Object };

            //TurningMovementCountsResult viewModel = turningMovementCountsService.GetChartData(
            //    options,
            //    approach.Object,
            //    detectors,
            //    events,
            //    planEvents
            //    );
            var plans = planService.GetBasicPlans(options.Start, options.End, options.SignalIdentifier, planEvents);

            TurningMovementCountsResult viewModel = await turningMovementCountsService.GetChartData(
                detectors,
                LaneTypes.V,
                MovementTypes.T,
                DirectionTypes.WB,
                options,
                events,
                plans.ToList(),
                mockSignal.Object.SignalIdentifier,
                mockSignal.Object.SignalDescription()
                );

            // Assert
            //Assert.Equal(2190, events.Count);
            //Assert.Equal(13, planEvents.Count);
            Assert.Equal(0.8363514419852448, viewModel.LaneUtilizationFactor);
            Assert.Equal("2:30 PM - 3:30 PM", viewModel.PeakHour);
            Assert.Equal(0.89, viewModel.PeakHourFactor);
            Assert.Equal(341, viewModel.PeakHourVolume);
            Assert.Equal(2494, viewModel.TotalVolume);
            //Assert.Equal("no idea", viewModel.TotalVolumes);

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