using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ConfigApi.Models;
using ATSPM.ConfigApi.Services;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Moq;

namespace ConfigApiTests
{
    public class RouteServiceTests
    {
        private readonly Mock<IRouteRepository> _mockRouteRepository;
        private readonly Mock<IRouteLocationsRepository> _mockRouteLocationRepository;
        private readonly Mock<IRouteDistanceRepository> _mockRouteDistanceRepository;
        private readonly RouteService _routeService;

        public RouteServiceTests()
        {
            _mockRouteRepository = new Mock<IRouteRepository>();
            _mockRouteLocationRepository = new Mock<IRouteLocationsRepository>();
            _mockRouteDistanceRepository = new Mock<IRouteDistanceRepository>();

            _routeService = new RouteService(
                _mockRouteRepository.Object,
                _mockRouteLocationRepository.Object,
                _mockRouteDistanceRepository.Object
            );
        }

        [Fact]
        public void CreateOrUpdateRoute_ShouldAddNewRoute_WhenRouteDoesNotExist()
        {
            // Arrange
            var routeDto = new RouteDto
            {
                Id = 1,
                Name = "Test Route",
                RouteLocations = new List<RouteLocationDto>
        {
            new RouteLocationDto
            {
                Order = 1,
                PrimaryPhase = 1,
                OpposingPhase = 2,
                PrimaryDirectionId = DirectionTypes.NB,
                OpposingDirectionId = DirectionTypes.SB,
                LocationIdentifier = "Loc1",
                PreviousLocationDistance = new RouteDistanceDto
                {
                    Distance = 100,
                    LocationIdentifierA = "Loc0",
                    LocationIdentifierB = "Loc1"
                },
                NextLocationDistance = new RouteDistanceDto
                {
                    Distance = 200,
                    LocationIdentifierA = "Loc1",
                    LocationIdentifierB = "Loc2"
                }
            }
        }
            };

            _mockRouteRepository.Setup(r => r.GetList())
                .Returns(new List<Route>().AsQueryable());

            // Act
            _routeService.UpsertRoute(routeDto);

            // Assert
            _mockRouteRepository.Verify(r => r.Add(It.IsAny<Route>()), Times.Once);
            //_mockRouteRepository.Verify(r => r.SaveChanges(), Times.Once);
            _mockRouteDistanceRepository.Verify(rd => rd.Add(It.Is<RouteDistance>(d => d.Distance == 100 && d.LocationIdentifierA == "Loc0" && d.LocationIdentifierB == "Loc1")), Times.Once);
            _mockRouteDistanceRepository.Verify(rd => rd.Add(It.Is<RouteDistance>(d => d.Distance == 200 && d.LocationIdentifierA == "Loc1" && d.LocationIdentifierB == "Loc2")), Times.Once);
        }


        [Fact]
        public void CreateOrUpdateRoute_ShouldUpdateExistingRoute_WhenRouteExists()
        {
            // Arrange
            var routeDto = new RouteDto
            {
                Id = 1,
                Name = "Updated Route",
                RouteLocations = new List<RouteLocationDto>
                {
                    new RouteLocationDto
                    {
                        Order = 1,
                        PrimaryPhase = 1,
                        OpposingPhase = 2,
                        PrimaryDirectionId = DirectionTypes.NB,
                        OpposingDirectionId = DirectionTypes.SB,
                        LocationIdentifier = "Loc1",
                        PreviousLocationDistanceId = 1,
                        PreviousLocationDistance = new RouteDistanceDto
                        {
                            Id = 1,
                            Distance = 150,
                            LocationIdentifierA = "Loc0",
                            LocationIdentifierB = "Loc1"
                        },
                        NextLocationDistanceId = 2,
                        NextLocationDistance = new RouteDistanceDto
                        {
                            Id = 2,
                            Distance = 250,
                            LocationIdentifierA = "Loc1",
                            LocationIdentifierB = "Loc2"
                        }
                    }
                }
            };

            var existingRoute = new Route
            {
                Id = 1,
                Name = "Existing Route",
                RouteLocations = new List<RouteLocation>
                {
                    new RouteLocation
                    {
                        Order = 1,
                        PrimaryPhase = 1,
                        OpposingPhase = 2,
                        PrimaryDirectionId = DirectionTypes.NB,
                        OpposingDirectionId = DirectionTypes.SB,
                        LocationIdentifier = "Loc1",
                        PreviousLocationDistance = new RouteDistance
                        {
                            Id = 1,
                            Distance = 100,
                            LocationIdentifierA = "Loc0",
                            LocationIdentifierB = "Loc1"
                        },
                        NextLocationDistance = new RouteDistance
                        {
                            Id = 2,
                            Distance = 200,
                            LocationIdentifierA = "Loc1",
                            LocationIdentifierB = "Loc2"
                        }
                    }
                }
            };

            _mockRouteRepository.Setup(r => r.GetList())
                .Returns(new List<Route> { existingRoute }.AsQueryable());

            // Act
            _routeService.UpsertRoute(routeDto);

            // Assert
            _mockRouteRepository.Verify(r => r.Update(It.IsAny<Route>()), Times.Once);
            //_mockRouteRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void CreateOrUpdateRoute_ShouldUpdateRouteDistance_WhenDistanceIsDifferent()
        {
            // Arrange
            var routeDto = new RouteDto
            {
                Id = 1,
                Name = "Updated Route",
                RouteLocations = new List<RouteLocationDto>
                {
                    new RouteLocationDto
                    {
                        Order = 1,
                        PrimaryPhase = 1,
                        OpposingPhase = 2,
                        PrimaryDirectionId = DirectionTypes.NB,
                        OpposingDirectionId = DirectionTypes.SB,
                        LocationIdentifier = "Loc1",
                        PreviousLocationDistanceId = 1,
                        PreviousLocationDistance = new RouteDistanceDto
                        {
                            Id = 1,
                            Distance = 150,
                            LocationIdentifierA = "Loc0",
                            LocationIdentifierB = "Loc1"
                        },
                        NextLocationDistanceId = 2,
                        NextLocationDistance = new RouteDistanceDto
                        {
                            Id = 2,
                            Distance = 250,
                            LocationIdentifierA = "Loc1",
                            LocationIdentifierB = "Loc2"
                        }
                    }
                }
            };

            var existingRoute = new Route
            {
                Id = 1,
                Name = "Existing Route",
                RouteLocations = new List<RouteLocation>
                {
                    new RouteLocation
                    {
                        Order = 1,
                        PrimaryPhase = 1,
                        OpposingPhase = 2,
                        PrimaryDirectionId = DirectionTypes.NB,
                        OpposingDirectionId = DirectionTypes.SB,
                        LocationIdentifier = "Loc1",
                        PreviousLocationDistance = new RouteDistance
                        {
                            Id = 1,
                            Distance = 100,
                            LocationIdentifierA = "Loc0",
                            LocationIdentifierB = "Loc1"
                        },
                        NextLocationDistance = new RouteDistance
                        {
                            Id = 2,
                            Distance = 200,
                            LocationIdentifierA = "Loc1",
                            LocationIdentifierB = "Loc2"
                        }
                    }
                }
            };

            _mockRouteRepository.Setup(r => r.GetList())
                .Returns(new List<Route> { existingRoute }.AsQueryable());

            _mockRouteDistanceRepository.Setup(rd => rd.Lookup(It.IsAny<int>()))
                .Returns<int>(id => existingRoute.RouteLocations.SelectMany(rl => new[] { rl.PreviousLocationDistance, rl.NextLocationDistance })
                                                                 .FirstOrDefault(d => d.Id == id));

            // Act
            _routeService.UpsertRoute(routeDto);

            // Assert
            _mockRouteDistanceRepository.Verify(rd => rd.Update(It.Is<RouteDistance>(d => d.Id == 1 && d.Distance == 150)), Times.Once);
            _mockRouteDistanceRepository.Verify(rd => rd.Update(It.Is<RouteDistance>(d => d.Id == 2 && d.Distance == 250)), Times.Once);
        }

        // Additional test cases can be added to cover other scenarios
    }
}
