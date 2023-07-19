using Xunit;
using ATSPM.Application.Reports.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace ATSPM.Application.Reports.Business.Common.Tests
{
    public class CyclePcdTests
    {
        [Fact]
        public void CyclePcd_PropertiesShouldBeCalculatedCorrectly()
        {
            // Arrange
            DateTime startOfCycle = new DateTime(2023, 4, 17, 7, 45, 0);
            DateTime greenEvent = new DateTime(2023, 4, 17, 7, 50, 0);
            DateTime yellowEvent = new DateTime(2023, 4, 17, 7, 55, 0);
            DateTime endOfCycle = new DateTime(2023, 4, 17, 8, 0, 0);

            CyclePcd cyclePcd = new CyclePcd(startOfCycle, greenEvent, yellowEvent, endOfCycle);

            // Act
            DetectorDataPoint detectorDataPoint1 = new DetectorDataPoint(startOfCycle, startOfCycle.AddSeconds(5), greenEvent, yellowEvent);
            DetectorDataPoint detectorDataPoint2 = new DetectorDataPoint(startOfCycle, startOfCycle.AddSeconds(10), greenEvent, yellowEvent);
            cyclePcd.AddDetectorData(detectorDataPoint1);
            cyclePcd.AddDetectorData(detectorDataPoint2);

            // Assert
            Assert.Equal(startOfCycle, cyclePcd.StartTime);
            Assert.Equal(endOfCycle, cyclePcd.EndTime);
            Assert.Equal(greenEvent, cyclePcd.GreenEvent);
            Assert.Equal(yellowEvent, cyclePcd.YellowEvent);
            Assert.Equal(300, cyclePcd.TotalGreenTimeSeconds);
            Assert.Equal(300, cyclePcd.TotalYellowTimeSeconds);
            Assert.Equal(300, cyclePcd.TotalRedTimeSeconds);
            Assert.Equal(900, cyclePcd.TotalTimeSeconds);
            Assert.Equal(300000, cyclePcd.TotalGreenTimeMilliseconds);
            Assert.Equal(300000, cyclePcd.TotalYellowTimeMilliseconds);
            Assert.Equal(300000, cyclePcd.TotalRedTimeMilliseconds);
            Assert.Equal(900000, cyclePcd.TotalTimeMilliseconds);
            Assert.Equal(2, cyclePcd.DetectorEvents.Count);
            Assert.Equal(2, cyclePcd.TotalVolume);
            Assert.Equal(0, cyclePcd.TotalArrivalOnGreen);
            Assert.Equal(0, cyclePcd.TotalArrivalOnYellow);
            Assert.Equal(2, cyclePcd.TotalArrivalOnRed);
            Assert.Equal(585, cyclePcd.TotalDelaySeconds);
        }
    }

}
