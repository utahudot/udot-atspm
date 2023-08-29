using ATSPM.Data.Models;
using CsvHelper;
using System.Globalization;
using Xunit;

namespace ATSPM.Application.Reports.Business.Common.Tests
{
    public class CycleServiceTests
    {
        CycleService cycleService = new CycleService();
        [Fact]
        public void GetPcdCyclesWithEmptyListsReturnsEmptyList()
        {
            // Arrange
            DateTime startDate = new DateTime(2023, 1, 1);
            DateTime endDate = new DateTime(2023, 1, 31);
            List<ControllerEventLog> detectorEvents = new List<ControllerEventLog>();
            List<ControllerEventLog> cycleEvents = new List<ControllerEventLog>();
            int? pcdCycleTime = 10;
            var cycleService = new CycleService();

            // Act
            var result = cycleService.GetPcdCycles(startDate, endDate, detectorEvents, cycleEvents, pcdCycleTime);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetPcdCycles_WithLargeDetectorEvents_ReturnsExpectedResult()
        {
            // Arrange
            DateTime startDate = new DateTime(2023, 4, 17, 8, 0, 0);
            DateTime endDate = new DateTime(2023, 4, 17, 9, 1, 0);
            List<ControllerEventLog> events = LoadDetectorEventsFromCsv(@"ControllerEventLogs-7115-Phase2-DetectorCycle-20230417-800-901.csv"); // Sampleevents
            List<ControllerEventLog> cycleEvents = events.Where(e => new List<int> { 1, 8, 9 }.Contains(e.EventCode)).ToList(); // Sample cycle events
            List<ControllerEventLog> detectorEvents = events.Where(e => new List<int> { 82 }.Contains(e.EventCode)).ToList(); // Load detector events from CSV
            int? pcdCycleTime = 10;

            // Act
            var result = cycleService.GetPcdCycles(startDate, endDate, detectorEvents, cycleEvents, pcdCycleTime);

            // Assert
            Assert.True(result != null);
            Assert.Equal(23, result.Count);

            for (int i = 0; i < result.Count; i++)
            {
                var cyclePcd = result[i];
                var startOfCycle = cycleEvents.Where(c => c.EventCode == 9).ToList()[i].TimeStamp;
                var endOfCycle = cycleEvents.Where(c => c.EventCode == 9).ToList()[i + 1].TimeStamp;
                var greenEvents = cycleEvents.Where(c => c.EventCode == 1).ToList();
                greenEvents.RemoveAt(0);
                var greenEvent = greenEvents[i].TimeStamp;
                var yellowEvents = cycleEvents.Where(c => c.EventCode == 8).ToList();
                yellowEvents.RemoveAt(0);
                var yellowEvent = yellowEvents[i].TimeStamp;
                Assert.True(cyclePcd != null);
                Assert.True(cyclePcd.DetectorEvents != null);
                Assert.True(cyclePcd.DetectorEvents.Count > 0);
                Assert.Equal(startOfCycle, cyclePcd.StartTime);
                Assert.Equal(endOfCycle, cyclePcd.EndTime);
                Assert.Equal(greenEvent, cyclePcd.GreenEvent);
                Assert.Equal(yellowEvent, cyclePcd.YellowEvent);
            }
            //Sample the first cycle values
            Assert.Equal(62, result[0].TotalGreenTimeSeconds);
            Assert.Equal(5.1, result[0].TotalYellowTimeSeconds);
            Assert.Equal(82.9, result[0].TotalRedTimeSeconds);
            Assert.Equal(150, result[0].TotalTimeSeconds);
            Assert.Equal(62000, result[0].TotalGreenTimeMilliseconds);
            Assert.Equal(5100, result[0].TotalYellowTimeMilliseconds);
            Assert.Equal(82900, result[0].TotalRedTimeMilliseconds);
            Assert.Equal(150000, result[0].TotalTimeMilliseconds);
            Assert.Equal(48, result[0].TotalArrivalOnGreen);
            Assert.Equal(9, result[0].TotalArrivalOnYellow);
            Assert.Equal(73, result[0].TotalArrivalOnRed);
            Assert.Equal(3047.1, result[0].TotalDelaySeconds);
            Assert.Equal(120, result[0].TotalVolume);

            //Sample the 3 cycle values
            Assert.Equal(61.7, result[2].TotalGreenTimeSeconds);
            Assert.Equal(5.1, result[2].TotalYellowTimeSeconds);
            Assert.Equal(83.2, result[2].TotalRedTimeSeconds);
            Assert.Equal(150, result[2].TotalTimeSeconds);
            Assert.Equal(61700, result[2].TotalGreenTimeMilliseconds);
            Assert.Equal(5100, result[2].TotalYellowTimeMilliseconds);
            Assert.Equal(83200, result[2].TotalRedTimeMilliseconds);
            Assert.Equal(150000, result[2].TotalTimeMilliseconds);
            Assert.Equal(43, result[2].TotalArrivalOnGreen);
            Assert.Equal(6, result[2].TotalArrivalOnYellow);
            Assert.Equal(54, result[2].TotalArrivalOnRed);
            Assert.Equal(2498.4999999999995, result[2].TotalDelaySeconds);
            Assert.Equal(93, result[2].TotalVolume);

            //Sample the last cycle values
            Assert.Equal(57.7, result[22].TotalGreenTimeSeconds);
            Assert.Equal(5.1, result[22].TotalYellowTimeSeconds);
            Assert.Equal(87.2, result[22].TotalRedTimeSeconds);
            Assert.Equal(150, result[22].TotalTimeSeconds);
            Assert.Equal(57700, result[22].TotalGreenTimeMilliseconds);
            Assert.Equal(5100, result[22].TotalYellowTimeMilliseconds);
            Assert.Equal(87200, result[22].TotalRedTimeMilliseconds);
            Assert.Equal(150000, result[22].TotalTimeMilliseconds);
            Assert.Equal(36, result[22].TotalArrivalOnGreen);
            Assert.Equal(8, result[22].TotalArrivalOnYellow);
            Assert.Equal(37, result[22].TotalArrivalOnRed);
            Assert.Equal(2113.6000000000004, result[22].TotalDelaySeconds);
            Assert.Equal(71, result[22].TotalVolume);

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