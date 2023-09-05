using Xunit;
using ATSPM.Application.Reports.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATSPM.Application.Reports.Business.LeftTurnGapAnalysis;
using ATSPM.Data.Models;
using Moq;
using ATSPM.Data.Enums;
using ATSPM.Application.Reports.Business.Common;
using System.Net;
using CsvHelper;
using System.Globalization;

namespace ATSPM.Application.Reports.Controllers.Tests
{
    public class LeftTurnGapAnalysisControllerTests
    {
        [Fact()]
        public void GetChartDataTest()
        {
            // Arrange
            //PlanService planService = new PlanService();
            LeftTurnGapAnalysisService leftTurnGapAnalysisService = new LeftTurnGapAnalysisService(); //might need to change the service to have inputs

            List<ControllerEventLog> allEvents = LoadAllEventsFromCsv(@"LTGap_EventLogs.csv"); // Sampleevents
            //List<ControllerEventLog> planEvents = events.Where(e => new List<int> { 131 }.Contains(e.EventCode)).ToList(); // Load plan events from CSV

            // Create a mock DetectionType object
            var detectionTypeSBP = new Mock<DetectionType>();
            detectionTypeSBP.Object.Id = (DetectionTypes)6;
            detectionTypeSBP.Object.Abbreviation = "SBP";

            // Create a mock DetectionType object
            var detectionTypeLLC = new Mock<DetectionType>();
            detectionTypeLLC.Object.Id = (DetectionTypes)4;
            detectionTypeLLC.Object.Abbreviation = "LLC";

            var mockDetectorN1 = new Mock<Detector>();
            // Set the properties of the mock Detector object
            mockDetectorN1.Object.ApproachId = 15255;
            mockDetectorN1.Object.DetChannel = 19;
            mockDetectorN1.Object.DetectorId = "711519";
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
            mockDetectorN2.Object.DetChannel = 25;
            mockDetectorN2.Object.DetectorId = "711525";
            mockDetectorN2.Object.Id = 50750;
            mockDetectorN2.Object.LaneNumber = 1;
            mockDetectorN2.Object.LaneTypeId = LaneTypes.V;
            mockDetectorN2.Object.LatencyCorrection = 0;
            mockDetectorN2.Object.MovementTypeId = MovementTypes.T;
            mockDetectorN2.Object.DetectionTypes = new List<DetectionType>() { detectionTypeLLC.Object };
            mockDetectorN2.Setup(t => t.DetectionTypes).Returns(new List<DetectionType>() { detectionTypeLLC.Object });

            var mockDetectorN3 = new Mock<Detector>();
            // Set the properties of the mock Detector object
            mockDetectorN3.Object.ApproachId = 15255;
            mockDetectorN3.Object.DetChannel = 26;
            mockDetectorN3.Object.DetectorId = "711526";
            mockDetectorN3.Object.Id = 50751;
            mockDetectorN3.Object.LaneNumber = 2;
            mockDetectorN3.Object.LaneTypeId = LaneTypes.V;
            mockDetectorN3.Object.LatencyCorrection = 0;
            mockDetectorN3.Object.MovementTypeId = MovementTypes.T;
            mockDetectorN3.Object.DetectionTypes = new List<DetectionType>() { detectionTypeLLC.Object };
            mockDetectorN3.Setup(t => t.DetectionTypes).Returns(new List<DetectionType>() { detectionTypeLLC.Object });

            var mockDetectorN4 = new Mock<Detector>();
            // Set the properties of the mock Detector object
            mockDetectorN4.Object.ApproachId = 15255;
            mockDetectorN4.Object.DetChannel = 27;
            mockDetectorN4.Object.DetectorId = "711527";
            mockDetectorN4.Object.Id = 50752;
            mockDetectorN4.Object.LaneNumber = 3;
            mockDetectorN4.Object.LaneTypeId = LaneTypes.V;
            mockDetectorN4.Object.LatencyCorrection = 0;
            mockDetectorN4.Object.MovementTypeId = MovementTypes.T;
            mockDetectorN4.Object.DetectionTypes = new List<DetectionType>() { detectionTypeLLC.Object };
            mockDetectorN4.Setup(t => t.DetectionTypes).Returns(new List<DetectionType>() { detectionTypeLLC.Object });

            var mockDetectorN5 = new Mock<Detector>();
            // Set the properties of the mock Detector object
            mockDetectorN5.Object.ApproachId = 15255;
            mockDetectorN5.Object.DetChannel = 28;
            mockDetectorN5.Object.DetectorId = "711528";
            mockDetectorN5.Object.Id = 50753;
            mockDetectorN5.Object.LaneNumber = 1;
            mockDetectorN5.Object.LaneTypeId = LaneTypes.V;
            mockDetectorN5.Object.LatencyCorrection = 0;
            mockDetectorN5.Object.MovementTypeId = MovementTypes.R;
            mockDetectorN5.Object.DetectionTypes = new List<DetectionType>() { detectionTypeLLC.Object };
            mockDetectorN5.Setup(t => t.DetectionTypes).Returns(new List<DetectionType>() { detectionTypeLLC.Object });

            var detectorsN = new List<Detector>() { mockDetectorN1.Object, mockDetectorN2.Object, mockDetectorN3.Object, mockDetectorN4.Object, mockDetectorN5.Object };

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

            // Create the mock Approach object
            var approach6 = new Mock<Approach>();
            // Set the properties of the mock Approach object
            approach6.Object.Id = 15257; // Updated Id
            approach6.Object.SignalId = 2934; // Updated SignalId
            approach6.Object.DirectionTypeId = DirectionTypes.SB;
            approach6.Object.Description = "SBT Ph6";
            approach6.Object.Mph = null;
            approach6.Object.ProtectedPhaseNumber = 6;
            approach6.Object.IsProtectedPhaseOverlap = false;
            approach6.Object.PermissivePhaseNumber = null;
            approach6.Object.IsPermissivePhaseOverlap = false;
            approach6.Object.PedestrianPhaseNumber = null;
            approach6.Object.IsPedestrianPhaseOverlap = false;
            approach6.Object.PedestrianDetectors = null;
            //approach6.Object.DirectionType = directionType.Object;

            var approaches = new List<Approach>() { approach2.Object, approach6.Object };

            var mockSignal = new Mock<Signal>();

            // Set the properties of the mock Signal object
            mockSignal.Object.Id = 2840; // Updated Id
            mockSignal.Object.SignalId = "6387"; // Updated SignalId
            mockSignal.Object.Latitude = "40.62398502";
            mockSignal.Object.Longitude = "-111.9387819";
            mockSignal.Object.PrimaryName = "Redwood Road";
            mockSignal.Object.SecondaryName = "7000 South";
            mockSignal.Object.Ipaddress = IPAddress.Parse("10.210.14.39");
            mockSignal.Object.RegionId = 2;
            mockSignal.Object.ControllerTypeId = 2; // Updated ControllerTypeId
            mockSignal.Object.Enabled = true;
            mockSignal.Object.VersionActionId = SignaVersionActions.Delete;
            mockSignal.Object.Note = "Updated missing zones";
            mockSignal.Object.Start = new System.DateTime(1900, 1, 1);
            mockSignal.Object.JurisdictionId = 35;
            mockSignal.Object.Pedsare1to1 = true;
            mockSignal.Object.Approaches = approaches;
            // Create the mock Approach object and set its Signal property to the mock Signal object
            approach2.Setup(a => a.Signal).Returns(mockSignal.Object);
            approach6.Setup(a => a.Signal).Returns(mockSignal.Object);
            mockSignal.Setup(a => a.Approaches).Returns(approaches);
            

            var options = new LeftTurnGapAnalysisOptions() { 
                ApproachId = 15255, 
                BinSize = 15,
                SignalId = "7115", //2934 is Id of this signal
                StartDate = new System.DateTime(2023, 6, 13, 6, 0, 0),
                EndDate = new System.DateTime(2023, 6, 13, 6, 59, 0),
                Gap1Min = 1,
                Gap1Max = 3.3,
                Gap2Min = 3.3,
                Gap2Max = 3.7,
                Gap3Min = 3.7,
                Gap3Max = 7.4,
                Gap4Min = 7.4,
                //Gap4Max = ?,
                //Gap5Min = ?,
                //Gap5Max = ?,
                //Gap6Min = ?,
                //Gap6Max = ?,
                //Gap7Min = ?,
                //Gap7Max = ?,
                //Gap8Min = ?,
                //Gap8Max = ?,
                //Gap9Min = ?,
                //Gap9Max = ?,
                //Gap10Min = ?,
                //Gap10Max = ?,
                //SumDurationGap1 = ?,
                //SumDurationGap2 = ?,
                //SumDurationGap3 = ?,
                TrendLineGapThreshold = 7.4
            };

            
            //var detectors = approach.GetDetectorsForMetricType(5);
            //var detectors = new List<Detector> { mockDetector1.Object, mockDetector2.Object };

            List<LeftTurnGapAnalysisResult> viewModel = leftTurnGapAnalysisService.GetChartData(
                options,
                mockSignal.Object,
                allEvents
                );


            //var myGapCounts0 = new Mock<GapCount>(new System.DateTime(2023, 6, 13, 6, 15, 0), 14);
            var myGapCounts0 = new GapCount();
            myGapCounts0.StartTime = new System.DateTime(2023, 6,13,6,15,0);
            myGapCounts0.Gaps = 15;

            //var myGapCounts1 = new Mock<GapCount>(new System.DateTime(2023, 6, 13, 6, 30, 0), 19);
            var myGapCounts1 = new GapCount();
            myGapCounts1.StartTime = new System.DateTime(2023, 6, 13, 6, 30, 0);
            myGapCounts1.Gaps = 20;

            //var myGapCounts2 = new Mock<GapCount>(new System.DateTime(2023, 6, 13, 6, 45, 0), 4);
            var myGapCounts2 = new GapCount();
            myGapCounts2.StartTime = new System.DateTime(2023, 6, 13, 6, 45, 0);
            myGapCounts2.Gaps = 19;

            //var myGapCounts3 = new Mock<GapCount>(new System.DateTime(2023, 6, 13, 7, 00, 0), 14);
            var myGapCounts3 = new GapCount();
            myGapCounts3.StartTime = new System.DateTime(2023, 6, 13, 7, 00, 0);
            myGapCounts3.Gaps = 16;

            var group1 = new List<GapCount>() { myGapCounts0, myGapCounts1, myGapCounts2, myGapCounts3 };

            // Assert
            //Assert.Equal(2190, events.Count);
            //Assert.Equal(13, planEvents.Count);
            //var actualList = new List<int>() { viewModel[1].Gap1Count[0].GapCount, viewModel[1].Gap1Count[1].GapCount, viewModel[1].Gap1Count[0].GapCount, viewModel[1].Gap1Count[0].GapCount };
            //    )
            Assert.Equal(group1, viewModel[1].Gap1Count);
            //Assert.Equal(myGapCounts1, viewModel[1].Gap2Count);
            //Assert.Equal(myGapCounts2, viewModel[1].Gap3Count);
            //Assert.Equal("2:30 PM - 3:30 PM", viewModel.PeakHour);
            //Assert.Equal(0.89, viewModel.PeakHourFactor);
            //Assert.Equal(341, viewModel.PeakHourVolume);
            //Assert.Equal(2494, viewModel.TotalVolume);
            //Assert.Equal("no idea", viewModel.TotalVolumes);

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