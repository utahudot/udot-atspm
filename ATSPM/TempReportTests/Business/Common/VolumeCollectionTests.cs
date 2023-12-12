using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using Xunit;

namespace ATSPM.Application.Reports.Business.Common.Tests
{
    public class VolumeCollectionTests
    {
        [Fact]
        public void VolumeCollection_Constructor_WithPrimaryAndOpposingDirection_ShouldCombineVolumes()
        {
            // Arrange
            var binSize = 15;
            VolumeCollection primaryDirectionVolume = GetVolumeCollection(binSize);

            VolumeCollection opposingDirectionVolume = GetVolumeCollection(binSize);

            // Act
            VolumeCollection volumeCollection = new VolumeCollection(primaryDirectionVolume, opposingDirectionVolume, binSize);

            // Assert
            Assert.Equal(4, volumeCollection.Items.Count);
            Assert.Equal(10, volumeCollection.Items[0].DetectorCount);
            Assert.Equal(10, volumeCollection.Items[1].DetectorCount);
            Assert.Equal(10, volumeCollection.Items[2].DetectorCount);
            Assert.Equal(10, volumeCollection.Items[3].DetectorCount);
            Assert.Equal(10 * 60 / binSize, volumeCollection.Items[0].HourlyVolume);
            Assert.Equal(10 * 60 / binSize, volumeCollection.Items[1].HourlyVolume);
            Assert.Equal(10 * 60 / binSize, volumeCollection.Items[2].HourlyVolume);
            Assert.Equal(10 * 60 / binSize, volumeCollection.Items[3].HourlyVolume);
        }

        [Fact]
        public void VolumeCollection_Constructor_WithStartTimeEndTimeAndDetectorEvents_ShouldCreateVolumes()
        {
            // Arrange
            int binSize = 15;
            VolumeCollection volumeCollection = GetVolumeCollection(binSize);

            // Assert
            Assert.Equal(4, volumeCollection.Items.Count);
            Assert.Equal(5, volumeCollection.Items[0].DetectorCount);
            Assert.Equal(5, volumeCollection.Items[1].DetectorCount);
            Assert.Equal(5, volumeCollection.Items[2].DetectorCount);
            Assert.Equal(5, volumeCollection.Items[3].DetectorCount);
            Assert.Equal(5 * 60 / binSize, volumeCollection.Items[0].HourlyVolume);
            Assert.Equal(5 * 60 / binSize, volumeCollection.Items[1].HourlyVolume);
            Assert.Equal(5 * 60 / binSize, volumeCollection.Items[2].HourlyVolume);
            Assert.Equal(5 * 60 / binSize, volumeCollection.Items[3].HourlyVolume);
        }

        private static VolumeCollection GetVolumeCollection(int binSize)
        {
            DateTime startTime = new DateTime(2023, 1, 1, 0, 0, 0);
            DateTime endTime = new DateTime(2023, 1, 1, 1, 0, 0);

            List<ControllerEventLog> detectorEvents = new List<ControllerEventLog>();
            for (DateTime time = startTime; time < endTime; time = time.AddMinutes(15))
            {
                for (int i = 0; i < 5; i++)
                {
                    detectorEvents.Add(new ControllerEventLog { Timestamp = time.AddMinutes(i) });
                }
            }

            binSize = 15;

            // Act
            return new VolumeCollection(startTime, endTime, detectorEvents, binSize);
        }
    }

    public class VolumeTests
    {
        [Fact]
        public void Volume_HourlyVolume_ShouldCalculateCorrectly()
        {
            // Arrange
            DateTime startTime = new DateTime(2023, 1, 1, 0, 0, 0);
            DateTime endTime = new DateTime(2023, 1, 1, 1, 0, 0);
            int binSize = 15;

            Volume volume = new Volume(startTime, endTime, binSize);
            volume.DetectorCount = 5;

            // Act
            int hourlyVolume = volume.HourlyVolume;

            // Assert
            Assert.Equal(20, hourlyVolume);
        }
    }
}



