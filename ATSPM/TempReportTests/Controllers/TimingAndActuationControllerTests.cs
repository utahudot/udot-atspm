using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using CsvHelper;
using Moq;
using System.Globalization;
using System.Net;
using ATSPM.ReportApi.TempExtensions;
using ATSPM.Application.Extensions;
using ATSPM.Application.Business.WaitTime;

namespace ReportsATSPM.Application.Reports.Controllers.Tests
{
    public class TimingAndActuationControllerTests
    {
        [Fact()]
        public async void GetChartDataTest()
        {
            
            // Arrange
            WaitTimeService waitTimeService = new WaitTimeService();
            List<ControllerEventLog> allEvents = LoadDetectorEventsFromCsv("ControllerEventLogs (89).csv");
            
            // Mock signal
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
            mockSignal.Object.VersionAction = SignalVersionActions.Initial;
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
            approach.Object.PermissivePhaseNumber = 19;
            approach.Object.IsProtectedPhaseOverlap = false;
            //approach.Object.PermissivePhaseNumber = null;
            approach.Object.IsPermissivePhaseOverlap = false;
            approach.Object.PedestrianPhaseNumber = 2;
            approach.Object.IsPedestrianPhaseOverlap = false;
            approach.Object.PedestrianDetectors = null;

            // Create the mock Approach object and set its Signal property to the mock Signal object
            approach.Setup(a => a.Signal).Returns(mockSignal.Object);

            // Create mock detector
            var mockDetector1 = new Mock<Detector>();

            mockDetector1.Object.ApproachId = 2880;
            mockDetector1.Object.DateAdded = new System.DateTime(2016, 3, 8);
            mockDetector1.Object.DateDisabled = null;
            mockDetector1.Object.DecisionPoint = null;
            mockDetector1.Object.DetectorChannel = 19;
            mockDetector1.Object.MovementType = MovementTypes.T;
            mockDetector1.Object.LaneType = LaneTypes.V;
            mockDetector1.Object.DetectionHardware = DetectionHardwareTypes.WavetronixMatrix;
            mockDetector1.Object.DectectorIdentifier = "711519";
            mockDetector1.Object.DistanceFromStopBar = null;
            mockDetector1.Object.Id = 6424;
            mockDetector1.Object.LaneNumber = 1;
            mockDetector1.Object.LatencyCorrection = 1.2;
            mockDetector1.Object.MinSpeedFilter = null;
            mockDetector1.Object.MovementDelay = null;
            mockDetector1.Object.MovementType = MovementTypes.T;

            // Associate detector to the Approach
            mockDetector1.Setup(a => a.Approach).Returns(approach.Object);
            // Associate approach to detectors
            approach.Setup(a => a.Detectors).Returns(new List<Detector> { mockDetector1.Object });
            
            // Create mock Detection type
            var detectionTypeAC = new Mock<DetectionType>();
            detectionTypeAC.Object.Id = DetectionTypes.SBP;
            detectionTypeAC.Object.Abbreviation = "SBP";
            // Associate detectors with detection types
            mockDetector1.Setup(a => a.DetectionTypes).Returns(new List<DetectionType>() { detectionTypeAC.Object });

            // Create mock movement type
            //var 
            //var movementType = new Mock<MovementTypes>();
            //movementType.Object.Id = MovementTypes.T;
            //movementType.Object.Abbreviation = "T";
            // Associate movements with movement types
            //mockDetector1.Setup(a => a.MovementType).Returns(MovementTypes.T);

            TimingAndActuationsOptions options = new TimingAndActuationsOptions
            {
                SignalIdentifier = "7115",
                Start = Convert.ToDateTime("6/14/2023 8:00:00.0"),
                End = Convert.ToDateTime("6/14/2023 8:05:00.0"),
                ExtendStartStopSearch = 2,
                GlobalEventCodesList = new List<int>(), 
                GlobalEventCounter = 1,
                GlobalEventParamsList = new List<int>(),
                PhaseEventCodesList = new List<int>(),
                ShowAdvancedCount = true,
                ShowAdvancedDilemmaZone = true,
                ShowAllLanesInfo = true, 
                ShowLaneByLaneCount = true, 
                ShowPedestrianActuation = true, 
                ShowPedestrianIntervals = true, 
                ShowStopBarPresence = true,
            };
            // Test one approach
            var eventCodes = new List<int> { };

            var phaseDetails = new PhaseDetail { PhaseNumber = 19, Approach = approach.Object, UseOverlap = false };

            if (options.ShowAdvancedCount || options.ShowAdvancedDilemmaZone || options.ShowLaneByLaneCount || options.ShowStopBarPresence)
                eventCodes.AddRange(new List<int> { 81, 82 });
            if (options.ShowPedestrianActuation)
                eventCodes.AddRange(new List<int> { 89, 90 });
            if (options.ShowPedestrianIntervals)
                eventCodes.AddRange(GetPedestrianIntervalEventCodes(false));
            if (options.PhaseEventCodesList != null)
                eventCodes.AddRange(options.PhaseEventCodesList);
            
            var result = GetChartDataForPhase(options, allEvents, phaseDetails, eventCodes, false);

            Assert.NotEmpty(result.StopBarDetectors);
            Assert.Equal(9, result.StopBarDetectors.Where(s => s.Name == "Stop Bar Presence , T 1, ch 19").FirstOrDefault().Events.Count(s => s.DetectorOff != null));
            Assert.Equal(9, result.StopBarDetectors.Where(s => s.Name == "Stop Bar Presence , T 1, ch 19").FirstOrDefault().Events.Count(s => s.DetectorOn != null));
            Assert.NotEmpty(result.PedestrianIntervals);
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

        public List<int> GetPedestrianIntervalEventCodes(bool isPhaseOrOverlap)
        {
            var overlapCodes = new List<int> { 21, 22, 23 };
            if (isPhaseOrOverlap)
            {
                overlapCodes = new List<int> { 67, 68, 69 };
            }

            return overlapCodes;
        }

        private TimingAndActuationsForPhaseResult GetChartDataForPhase(
            TimingAndActuationsOptions options,
            List<ControllerEventLog> controllerEventLogs,
            PhaseDetail phaseDetail,
            List<int> eventCodes,
            bool usePermissivePhase)
        {
            var timingAndActuationsForPhaseService = new TimingAndActuationsForPhaseService();

            eventCodes.AddRange(timingAndActuationsForPhaseService.GetCycleCodes(phaseDetail.UseOverlap));
            var approachevents = controllerEventLogs.GetEventsByEventCodes(
                options.Start,
                options.End,
                eventCodes,
                phaseDetail.PhaseNumber).ToList();
            var viewModel = timingAndActuationsForPhaseService.GetChartData(options, phaseDetail, approachevents, usePermissivePhase);
            viewModel.SignalDescription = phaseDetail.Approach.Signal.SignalDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
