using ATSPM.Application.Reports.Business.TimingAndActuation;
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
using ATSPM.Application.Extensions;

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

            mockDetector1.Object.ApproachId = 2880;
            mockDetector1.Object.DateAdded = new System.DateTime(2016, 3, 8);
            mockDetector1.Object.DateDisabled = null;
            mockDetector1.Object.DecisionPoint = null;
            mockDetector1.Object.DetChannel = 19;
            mockDetector1.Object.MovementType = new MovementType { Abbreviation = "T", Id = MovementTypes.T };
            mockDetector1.Object.LaneType = new LaneType { Id = LaneTypes.V };
            mockDetector1.Object.DetectionHardwareId = DetectionHardwareTypes.WavetronixMatrix;
            mockDetector1.Object.DectectorIdentifier = "711519";
            mockDetector1.Object.DistanceFromStopBar = null;
            mockDetector1.Object.Id = 6424;
            mockDetector1.Object.LaneNumber = 1;
            mockDetector1.Object.LaneTypeId = LaneTypes.V;
            mockDetector1.Object.LatencyCorrection = 1.2;
            mockDetector1.Object.MinSpeedFilter = null;
            mockDetector1.Object.MovementDelay = null;
            mockDetector1.Object.MovementTypeId = MovementTypes.T;

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
            var movementType = new Mock<MovementType>();
            movementType.Object.Id = MovementTypes.T;
            movementType.Object.Abbreviation = "T";
            // Associate movements with movement types
            mockDetector1.Setup(a => a.MovementType).Returns(movementType.Object);

            TimingAndActuationsOptions options = new TimingAndActuationsOptions
            {
                SignalIdentifier = "7115",
                Start = Convert.ToDateTime("6/14/2023 8:00:00.0"),
                End = Convert.ToDateTime("6/14/2023 8:05:00.0"),
                ShowPermissivePhases = true,
                ExtendVsdSearch = 5,
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
                ShowVehicleSignalDisplay = true,
            };
            // Test one approach
            var eventCodes = new List<int> { };
            
            if (options.ShowAdvancedCount || options.ShowAdvancedDilemmaZone || options.ShowLaneByLaneCount || options.ShowStopBarPresence)
                eventCodes.AddRange(new List<int> { 81, 82 });
            if (options.ShowPedestrianActuation)
                eventCodes.AddRange(new List<int> { 89, 90 });
            if (options.ShowPedestrianIntervals)
                eventCodes.AddRange(GetPedestrianIntervalEventCodes(false));
            if (options.PhaseEventCodesList != null)
                eventCodes.AddRange(options.PhaseEventCodesList);
                var result = GetChartDataForPhase(options, allEvents, approach.Object, eventCodes, false);

            Assert.NotEmpty(result.StopBarEvents);
            Assert.Equal( 9, result.StopBarEvents );
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
            Approach approach,
            List<int> eventCodes,
            bool usePermissivePhase)
        {
            var timingAndActuationsForPhaseService = new TimingAndActuationsForPhaseService();

            eventCodes.AddRange(timingAndActuationsForPhaseService.GetCycleCodes(
                (usePermissivePhase && approach.IsPermissivePhaseOverlap) ||
                (usePermissivePhase && approach.IsPermissivePhaseOverlap))
                );
            var approachevents = controllerEventLogs.GetEventsByEventCodes(
                options.Start,
                options.End,
                eventCodes,
                usePermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber).ToList();
            var viewModel = timingAndActuationsForPhaseService.GetChartData(options, approach, approachevents, usePermissivePhase);
            viewModel.SignalDescription = approach.Signal.SignalDescription();
            viewModel.ApproachDescription = approach.Description;
            return viewModel;
        }
    }
}
