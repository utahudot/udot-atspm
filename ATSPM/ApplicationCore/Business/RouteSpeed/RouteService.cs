using ATSPM.Application.Repositories.SpeedManagementAggregationRepositories;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using NetTopologySuite.Geometries;
using NetTopologySuite;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace ATSPM.Application.Business.RouteSpeed
{
    public class RouteService
    {
        private readonly IRouteRepository _routeRepository;
        private double[] centerUtah = new double[2] { 39.419220, -111.950684 };
        private int maxRadius = 300; // 300 miles radius
        private int maxLinkDistance = 5; // Each link no longer than 5 miles
        private int numRoutes = 14000;

        public RouteService(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public async Task AddRandomRoutes()
        {
            var routes = GenerateRandomRoutes(numRoutes, centerUtah, maxRadius, maxLinkDistance);
            await _routeRepository.AddRoutesAsync(routes);
        }

        private List<Route> GenerateRandomRoutes(int numRoutes, double[] center, int maxRadius, int maxLinkDistance)
        {
            var routes = new List<Route>();
            var rand = new Random();
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            for (int i = 0; i < numRoutes; i++)
            {
                Coordinate startCoord = GetRandomCoordinate(center, maxRadius, rand);
                Coordinate endCoord = GetRandomCoordinate(new double[] { startCoord.Y, startCoord.X }, maxLinkDistance, rand);

                var startPoint = geometryFactory.CreatePoint(startCoord);
                var endPoint = geometryFactory.CreatePoint(endCoord);
                var lineString = geometryFactory.CreateLineString(new[] { startCoord, endCoord });

                var route = new Route
                {
                    Id = i,
                    UdotRouteNumber = rand.Next(1000, 9999),
                    StartMilePoint = 0,
                    EndMilePoint = rand.NextDouble() * maxLinkDistance,
                    FunctionalType = GetRandomFunctionalType(rand),
                    Name = $"Route {i + 1}",
                    Direction = GetRandomDirection(rand),
                    SpeedLimit = rand.Next(25, 75),
                    Region = "Region " + rand.Next(1, 5),
                    City = "City " + rand.Next(1, 100),
                    County = "County " + rand.Next(1, 30),
                    Shape = lineString,
                    ShapeWKT = lineString.AsText(),
                    AlternateIdentifier = null,
                    RouteEntities = new List<RouteEntity>()
                };

                routes.Add(route);
            }

            return routes;
        }

        private Coordinate GetRandomCoordinate(double[] center, int maxDistance, Random rand)
        {
            double maxDistanceKm = maxDistance * 1.60934; // Convert miles to kilometers

            double angle = rand.NextDouble() * 2 * Math.PI;
            double distance = rand.NextDouble() * maxDistanceKm;

            return GetDestination(center[0], center[1], distance, angle);
        }

        private Coordinate GetDestination(double lat, double lon, double distance, double angle)
        {
            const double EarthRadiusKm = 6371.0;
            double latRad = ToRadians(lat);
            double lonRad = ToRadians(lon);

            double lat2 = Math.Asin(Math.Sin(latRad) * Math.Cos(distance / EarthRadiusKm) +
                                    Math.Cos(latRad) * Math.Sin(distance / EarthRadiusKm) * Math.Cos(angle));
            double lon2 = lonRad + Math.Atan2(Math.Sin(angle) * Math.Sin(distance / EarthRadiusKm) * Math.Cos(latRad),
                                              Math.Cos(distance / EarthRadiusKm) - Math.Sin(latRad) * Math.Sin(lat2));

            return new Coordinate(ToDegrees(lon2), ToDegrees(lat2));
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private double ToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }

        private string GetRandomFunctionalType(Random rand)
        {
            string[] types = { "Interstate", "Highway", "Local Road" };
            return types[rand.Next(types.Length)];
        }

        private string GetRandomDirection(Random rand)
        {
            string[] directions = { "N", "S", "E", "W" };
            return directions[rand.Next(directions.Length)];
        }
    }
}
