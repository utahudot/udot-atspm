using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.SplitFail;
using ATSPM.Application.Reports.Business.WaitTime;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using CsvHelper;
using IdentityServer4.Extensions;
using Moq;
using Reports.Business.Common;
using System.Globalization;
using System.Net;
using Xunit;

namespace ReportsTests.Controllers
{
    public class SplitFailControllerTests
    {
        WaitTimeService waitTimeService = new WaitTimeService();
        SplitFailPhaseService splitFailPhaseService = new SplitFailPhaseService(new CycleService(), new PlanService());
        PhaseService phaseService = new PhaseService();

        [Fact()]
        public async void GetChartDataTest()
        {
            // first look for get chart data

            // Arrange
            List<ControllerEventLog> allEvents = LoadDetectorEventsFromCsv("Split_Failure_NL.csv");

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

            // Set up relationship between signal and approach
            mockSignal.Setup(a => a.Approaches).Returns(new List<Approach> { approach.Object });

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

            var detectors = new List<Detector> { mockDetector1.Object };
            approach.Setup(a => a.Detectors).Returns(detectors);
            

            //set up detection type
            // Create mock Detection type
            var detectionTypeAC = new Mock<DetectionType>();
            detectionTypeAC.Object.Id = DetectionTypes.SBP;
            detectionTypeAC.Object.Abbreviation = "SBP";
            // Associate detectors with detection types
            mockDetector1.Setup(a => a.DetectionTypes).Returns(new List<DetectionType>() { detectionTypeAC.Object });
            
            //associate detection type with metric types
            //detectionTypeAC.Setup(a => a.MetricTypeMetrics).Returns(new List<MetricType>() { });

            // Create mock movement type
            var movementType = new Mock<MovementType>();
            movementType.Object.Id = MovementTypes.T;
            movementType.Object.Abbreviation = "T";
            // Associate movements with movement types
            mockDetector1.Setup(a => a.MovementType).Returns(movementType.Object);


            // set up plan events. Getting plan events from csv
            // PlanService planService = new PlanService();
            List<ControllerEventLog> planEvents = allEvents.Where(e => new List<int> { 131 }.Contains(e.EventCode)).ToList(); // Load plan events from CSV


            // set up phase detials
            // PhaseService phaseService = new PhaseService();
            var phaseDetail = phaseService.GetPhases(mockSignal.Object); // is this what gets phase 2??
            // // SignalPhaseService signalPhaseService = new SignalPhaseService(planService, cycleService, logger);
            var phase = phaseDetail.Where(p => p.PhaseNumber == 2).FirstOrDefault();


            // 
            var options = new SplitFailOptions
            {
                Start = new DateTime(2023, 8, 9, 15, 0, 0),
                End = new DateTime(2023, 8, 9, 15, 8, 0),
                SignalIdentifier = "7115",
                FirstSecondsOfRed = 5,
                UsePermissivePhase = false,

            };

            // instead of looping through phases, I just want phase 2
            var result = await GetChartDataForApproach(options, phase, allEvents, planEvents, detectors, false);

            // cycle events


            // termination events


            // detectors for the approach 


            // Detector events

            Assert.NotEmpty(result);
            //Assert.Equal(0.33, result.First().PercentFails);

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

        // I copied this entire function for the tasks under phase detials
        private async Task<IEnumerable<SplitFailsResult>> GetChartDataForApproach(
            SplitFailOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            List<Detector> detectors,
            bool usePermissivePhase)
        {
            //var cycleEventCodes = approach.GetCycleEventCodes(options.UsePermissivePhase);
            var cycleEvents = controllerEventLogs.GetCycleEventsWithTimeExtension(
                phaseDetail.PhaseNumber,
                options.UsePermissivePhase,
                options.Start,
                options.End);
            if (cycleEvents.IsNullOrEmpty())
                return null;
            var terminationEvents = controllerEventLogs.GetEventsByEventCodes(
                 options.Start,
                 options.End,
                 new List<int> { 4, 5, 6 },
                 phaseDetail.PhaseNumber);
            // var detectors = phaseDetail.Approach.GetDetectorsForMetricType(options.MetricTypeId);
            var tasks = new List<Task<SplitFailsResult>>();
            foreach (var detectionType in detectors.SelectMany(d => d.DetectionTypes).Distinct())
            {
                tasks.Add(GetChartDataByDetectionType(options, phaseDetail, controllerEventLogs, planEvents, cycleEvents, terminationEvents, detectors, detectionType));
            }
            var results = await Task.WhenAll(tasks);
            return results.Where(result => result != null);
        }


        // I copied this entire function for the tasks under detectors
        private async Task<SplitFailsResult> GetChartDataByDetectionType(
            SplitFailOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            IReadOnlyList<ControllerEventLog> cycleEvents,
            IReadOnlyList<ControllerEventLog> terminationEvents,
            List<Detector> detectors,
            DetectionType detectionType)
        {
            var tempDetectorEvents = controllerEventLogs.GetDetectorEvents(
               options.MetricTypeId,
               phaseDetail.Approach,
               options.Start,
               options.End,
               true,
               true,
               detectionType);
            if (tempDetectorEvents == null)
            {
                return null;
            }
            var detectorEvents = tempDetectorEvents.ToList();
            AddBeginEndEventsByDetector(options, detectors, detectionType, detectorEvents);
            var splitFailData = splitFailPhaseService.GetSplitFailPhaseData(
                options,
                cycleEvents,
                planEvents,
                terminationEvents,
                detectorEvents,
                phaseDetail.Approach);
            var result = new SplitFailsResult(
                options.SignalIdentifier,
                phaseDetail.Approach.Id,
                phaseDetail.PhaseNumber,
                options.Start,
                options.End,
                splitFailData.TotalFails,
                splitFailData.Plans,
                splitFailData.Cycles.Where(c => c.IsSplitFail).Select(c => new DataPointBase(c.StartTime)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.GapOut)
                    .Select(b => new DataPointForDouble(b.StartTime, b.GreenOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.GapOut)
                    .Select(b => new DataPointForDouble(b.StartTime, b.RedOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.ForceOff)
                    .Select(b => new DataPointForDouble(b.StartTime, b.GreenOccupancyPercent)).ToList(),
                splitFailData.Cycles
                    .Where(c => c.TerminationEvent == CycleSplitFail.TerminationType.ForceOff)
                    .Select(b => new DataPointForDouble(b.StartTime, b.RedOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new DataPointForDouble(b.StartTime, b.AverageGreenOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new DataPointForDouble(b.StartTime, b.AverageRedOccupancyPercent)).ToList(),
                splitFailData.Bins.Select(b => new DataPointForDouble(b.StartTime, b.PercentSplitfails)).ToList()
                );
            result.ApproachDescription = phaseDetail.Approach.Description;
            result.SignalDescription = phaseDetail.Approach.Signal.SignalDescription();
            return result;
        }

        // I copied this entire function for detector events
        private static void AddBeginEndEventsByDetector(SplitFailOptions options, List<Detector> detectors, DetectionType detectionType, List<ControllerEventLog> detectorEvents)
        {
            foreach (Detector channel in detectors.Where(d => d.DetectionTypes.Contains(detectionType)))
            {
                //add an EC 82 at the beginning if the first EC code is 81
                var firstEvent = detectorEvents.Where(d => d.EventParam == channel.DetChannel).FirstOrDefault();
                var lastEvent = detectorEvents.Where(d => d.EventParam == channel.DetChannel).LastOrDefault();

                if (firstEvent != null && firstEvent.EventCode == 81)
                {
                    var newDetectorOn = new ControllerEventLog();
                    newDetectorOn.SignalIdentifier = options.SignalIdentifier;
                    newDetectorOn.Timestamp = options.Start;
                    newDetectorOn.EventCode = 82;
                    newDetectorOn.EventParam = channel.DetChannel;
                    detectorEvents.Add(newDetectorOn);
                }

                //add an EC 81 at the end if the last EC code is 82
                if (lastEvent != null && lastEvent.EventCode == 82)
                {
                    var newDetectorOn = new ControllerEventLog();
                    newDetectorOn.SignalIdentifier = options.SignalIdentifier;
                    newDetectorOn.Timestamp = options.End;
                    newDetectorOn.EventCode = 81;
                    newDetectorOn.EventParam = channel.DetChannel;
                    detectorEvents.Add(newDetectorOn);
                }
            }
        }
    }
}
