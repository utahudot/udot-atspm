#region license
// Copyright 2026 Utah Departement of Transportation
// for DataApiTests - Utah.Udot.ATSPM.DataApiTests.Services/AggregationImporterServiceTests.cs
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Moq;
using System.Text;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.AggregationRepositories;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.DataApi.Services;

namespace DataApiTests
{
    /// <summary>
    /// Unit tests for AggregationImporterService.
    /// Tests CSV import functionality for both pedestrian and cycle aggregations.
    /// </summary>
    public class AggregationImporterServiceTests
    {
        private readonly Mock<IPhasePedAggregationRepository> _mockPedRepository;
        private readonly Mock<IPhaseCycleAggregationRepository> _mockCycleRepository;
        private readonly Mock<ILocationRepository> _mockLocationRepository;
        private readonly AggregationImporterService _service;

        public AggregationImporterServiceTests()
        {
            _mockPedRepository = new Mock<IPhasePedAggregationRepository>();
            _mockCycleRepository = new Mock<IPhaseCycleAggregationRepository>();
            _mockLocationRepository = new Mock<ILocationRepository>();
            _service = new AggregationImporterService(
                _mockPedRepository.Object,
                _mockCycleRepository.Object,
                _mockLocationRepository.Object);
        }

        #region Helper Methods

        /// <summary>
        /// Creates a MemoryStream from CSV content string.
        /// </summary>
        private static Stream CreateCsvStream(string csvContent)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(csvContent);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Returns a valid pedestrian aggregation CSV with two data rows.
        /// </summary>
        private static string GetValidPedCsv()
        {
            return @"PhaseNumber,PedBeginWalkCount,PedCallsRegisteredCount,PedCycles,PedDelay,PedRequests,ImputedPedCallsRegistered,MaxPedDelay,MinPedDelay,UniquePedDetections,Start,End
1,10,5,20,2.5,15,0,5.0,0.0,8,2023-01-01T00:00:00,2023-01-01T01:00:00
2,12,6,22,3.0,18,1,6.0,1.0,9,2023-01-01T00:00:00,2023-01-01T01:00:00";
        }

        /// <summary>
        /// Returns a valid cycle aggregation CSV with two data rows.
        /// </summary>
        private static string GetValidCycleCsv()
        {
            return @"GreenTime,PhaseBeginCount,PhaseNumber,RedTime,TotalGreenToGreenCycles,TotalRedToRedCycles,YellowTime,ApproachId,Start,End
30,5,1,25,10,8,5,1,2023-01-01T00:00:00,2023-01-01T01:00:00
35,6,2,20,12,9,4,2,2023-01-01T00:00:00,2023-01-01T01:00:00";
        }

        /// <summary>
        /// Returns a pedestrian CSV with an invalid row (non-numeric phase number).
        /// </summary>
        private static string GetInvalidPedCsv()
        {
            return @"PhaseNumber,PedBeginWalkCount,PedCallsRegisteredCount,PedCycles,PedDelay,PedRequests,ImputedPedCallsRegistered,MaxPedDelay,MinPedDelay,UniquePedDetections,Start,End
invalid,5,20,2.5,15,0,5.0,0.0,8,2023-01-01T00:00:00,2023-01-01T01:00:00";
        }

        /// <summary>
        /// Returns an empty CSV with only headers.
        /// </summary>
        private static string GetEmptyPedCsv()
        {
            return @"PhaseNumber,PedBeginWalkCount,PedCallsRegisteredCount,PedCycles,PedDelay,PedRequests,ImputedPedCallsRegistered,MaxPedDelay,MinPedDelay,UniquePedDetections,Start,End";
        }

        #endregion

        #region ImportPedestrianAggregationFromCsvAsync Tests

        /// <summary>
        /// Test: Valid CSV data should import successfully with all records processed.
        /// </summary>
        [Fact]
        public async Task ImportPedestrianAggregationFromCsvAsync_ValidData_ReturnsSuccess()
        {
            // Arrange
            var locationIdentifier = "TestLocation";
            var location = new Location { LocationIdentifier = locationIdentifier };
            _mockLocationRepository.Setup(r => r.GetLatestVersionOfLocation(locationIdentifier)).Returns(location);
            _mockPedRepository.Setup(r => r.AddAsync(It.IsAny<CompressedAggregations<PhasePedAggregation>>())).Returns(Task.CompletedTask);
            var csvStream = CreateCsvStream(GetValidPedCsv());

            // Act
            var result = await _service.ImportPedestrianAggregationFromCsvAsync(locationIdentifier, csvStream);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.RecordsImported);
            Assert.Contains("Successfully imported 2", result.Message);
            Assert.Empty(result.Errors);
        }

        /// <summary>
        /// Test: Non-existent location should return failure.
        /// </summary>
        [Fact]
        public async Task ImportPedestrianAggregationFromCsvAsync_LocationNotFound_ReturnsFailure()
        {
            // Arrange
            var locationIdentifier = "NonExistentLocation";
            _mockLocationRepository.Setup(r => r.GetLatestVersionOfLocation(locationIdentifier)).Returns((Location)null);
            var csvStream = CreateCsvStream(GetValidPedCsv());

            // Act
            var result = await _service.ImportPedestrianAggregationFromCsvAsync(locationIdentifier, csvStream);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(0, result.RecordsImported);
            Assert.Contains("not found", result.Message);
        }

        /// <summary>
        /// Test: CSV with invalid rows should result in errors being captured.
        /// </summary>
        [Fact]
        public async Task ImportPedestrianAggregationFromCsvAsync_InvalidCsvData_CapturesErrors()
        {
            // Arrange
            var locationIdentifier = "TestLocation";
            var location = new Location { LocationIdentifier = locationIdentifier };
            _mockLocationRepository.Setup(r => r.GetLatestVersionOfLocation(locationIdentifier)).Returns(location);
            _mockPedRepository.Setup(r => r.AddAsync(It.IsAny<CompressedAggregations<PhasePedAggregation>>())).Returns(Task.CompletedTask);
            var csvStream = CreateCsvStream(GetInvalidPedCsv());

            // Act
            var result = await _service.ImportPedestrianAggregationFromCsvAsync(locationIdentifier, csvStream);

            // Assert
            Assert.NotEmpty(result.Errors);
        }

        /// <summary>
        /// Test: Empty CSV should return failure.
        /// </summary>
        [Fact]
        public async Task ImportPedestrianAggregationFromCsvAsync_EmptyCsv_ReturnsFailure()
        {
            // Arrange
            var locationIdentifier = "TestLocation";
            var location = new Location { LocationIdentifier = locationIdentifier };
            _mockLocationRepository.Setup(r => r.GetLatestVersionOfLocation(locationIdentifier)).Returns(location);
            var csvStream = CreateCsvStream(GetEmptyPedCsv());

            // Act
            var result = await _service.ImportPedestrianAggregationFromCsvAsync(locationIdentifier, csvStream);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(0, result.RecordsImported);
            Assert.Contains("No valid records found", result.Message);
        }

        /// <summary>
        /// Test: Database errors during insertion should be captured.
        /// </summary>
        [Fact]
        public async Task ImportPedestrianAggregationFromCsvAsync_DatabaseError_CapturesException()
        {
            // Arrange
            var locationIdentifier = "TestLocation";
            var location = new Location { LocationIdentifier = locationIdentifier };
            _mockLocationRepository.Setup(r => r.GetLatestVersionOfLocation(locationIdentifier)).Returns(location);
            _mockPedRepository.Setup(r => r.AddAsync(It.IsAny<CompressedAggregations<PhasePedAggregation>>()))
                .ThrowsAsync(new Exception("Database connection failed"));
            var csvStream = CreateCsvStream(GetValidPedCsv());

            // Act
            var result = await _service.ImportPedestrianAggregationFromCsvAsync(locationIdentifier, csvStream);

            // Assert
            Assert.NotEmpty(result.Errors);
        }

        #endregion

        #region ImportCycleAggregationFromCsvAsync Tests

        /// <summary>
        /// Test: Valid cycle CSV data should import successfully.
        /// </summary>
        [Fact]
        public async Task ImportCycleAggregationFromCsvAsync_ValidData_ReturnsSuccess()
        {
            // Arrange
            var locationIdentifier = "TestLocation";
            var location = new Location { LocationIdentifier = locationIdentifier };
            _mockLocationRepository.Setup(r => r.GetLatestVersionOfLocation(locationIdentifier)).Returns(location);
            _mockCycleRepository.Setup(r => r.AddAsync(It.IsAny<CompressedAggregations<PhaseCycleAggregation>>())).Returns(Task.CompletedTask);
            var csvStream = CreateCsvStream(GetValidCycleCsv());

            // Act
            var result = await _service.ImportCycleAggregationFromCsvAsync(locationIdentifier, csvStream);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.RecordsImported);
            Assert.Contains("Successfully imported 2", result.Message);
            Assert.Empty(result.Errors);
        }

        /// <summary>
        /// Test: Non-existent location should return failure for cycle import.
        /// </summary>
        [Fact]
        public async Task ImportCycleAggregationFromCsvAsync_LocationNotFound_ReturnsFailure()
        {
            // Arrange
            var locationIdentifier = "NonExistentLocation";
            _mockLocationRepository.Setup(r => r.GetLatestVersionOfLocation(locationIdentifier)).Returns((Location)null);
            var csvStream = CreateCsvStream(GetValidCycleCsv());

            // Act
            var result = await _service.ImportCycleAggregationFromCsvAsync(locationIdentifier, csvStream);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(0, result.RecordsImported);
            Assert.Contains("not found", result.Message);
        }

        /// <summary>
        /// Test: Empty cycle CSV should return failure.
        /// </summary>
        [Fact]
        public async Task ImportCycleAggregationFromCsvAsync_EmptyCsv_ReturnsFailure()
        {
            // Arrange
            var locationIdentifier = "TestLocation";
            var location = new Location { LocationIdentifier = locationIdentifier };
            _mockLocationRepository.Setup(r => r.GetLatestVersionOfLocation(locationIdentifier)).Returns(location);
            var emptyCycleCsv = @"GreenTime,PhaseBeginCount,PhaseNumber,RedTime,TotalGreenToGreenCycles,TotalRedToRedCycles,YellowTime,ApproachId,Start,End";
            var csvStream = CreateCsvStream(emptyCycleCsv);

            // Act
            var result = await _service.ImportCycleAggregationFromCsvAsync(locationIdentifier, csvStream);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(0, result.RecordsImported);
            Assert.Contains("No valid records found", result.Message);
        }

        #endregion
    }
}