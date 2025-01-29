﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Services/RouteServices.cs
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

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Utah.Udot.Atspm.ConfigApi.Models;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.ConfigApi.Services
{
    public class RouteService : IRouteService
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IRouteLocationsRepository _routeLocationRepository;
        private readonly IRouteDistanceRepository _routeDistanceRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IDirectionTypeRepository _directionTypeRepository;
        private readonly List<Location> _locations;

        public RouteService(
            IRouteRepository routeRepository,
            IRouteLocationsRepository routeLocationRepository,
            IRouteDistanceRepository routeDistanceRepository,
            ILocationRepository locationRepository)
        {
            _routeRepository = routeRepository;
            _routeLocationRepository = routeLocationRepository;
            _routeDistanceRepository = routeDistanceRepository;
            _locationRepository = locationRepository;
        }

        public RouteDto UpsertRoute(RouteDto routeDto)
        {
            // Find existing route or create a new one
            var route = _routeRepository.GetList()
                .Where(r => r.Id == routeDto.Id)
                .Include(r => r.RouteLocations)
                .ThenInclude(rl => rl.PreviousLocationDistance)
                .Include(r => r.RouteLocations)
                .ThenInclude(rl => rl.NextLocationDistance)
                .FirstOrDefault() ?? new Data.Models.Route();

            // Update route properties
            route.Name = routeDto.Name;

            // Create a list of routeLocation identifiers from the DTO
            var routeLocationIds = routeDto.RouteLocations.Select(rl => rl.LocationIdentifier).ToList();

            // Delete RouteLocations that are not in the DTO
            var routeLocationsToDelete = route.RouteLocations
                .Where(rl => !routeLocationIds.Contains(rl.LocationIdentifier))
                .ToList();

            foreach (var routeLocation in routeLocationsToDelete)
            {
                route.RouteLocations.Remove(routeLocation);
            }

            // Create an ordered list of RouteLocationDto
            var orderedRouteLocations = routeDto.RouteLocations.OrderBy(rl => rl.Order).ToList();

            // Set previous distances based on the next item in the list
            for (int i = 1; i < orderedRouteLocations.Count; i++)
            {
                orderedRouteLocations[i].PreviousLocationDistanceId = orderedRouteLocations[i - 1].NextLocationDistanceId;
                orderedRouteLocations[i].PreviousLocationDistance = orderedRouteLocations[i - 1].NextLocationDistance;
            }

            // Update or create RouteLocations
            foreach (var routeLocationDto in orderedRouteLocations)
            {
                var existingLocation = route.RouteLocations
                    .FirstOrDefault(rl => rl.LocationIdentifier == routeLocationDto.LocationIdentifier);

                if (existingLocation == null)
                {
                    // Create new RouteLocation
                    var newLocation = new RouteLocation
                    {
                        Order = routeLocationDto.Order,
                        PrimaryPhase = routeLocationDto.PrimaryPhase,
                        OpposingPhase = routeLocationDto.OpposingPhase,
                        PrimaryDirectionId = (DirectionTypes)routeLocationDto.PrimaryDirectionId,
                        OpposingDirectionId = (DirectionTypes)routeLocationDto.OpposingDirectionId,
                        IsPrimaryOverlap = routeLocationDto.IsPrimaryOverlap,
                        IsOpposingOverlap = routeLocationDto.IsOpposingOverlap,
                        PreviousLocationDistanceId = routeLocationDto.PreviousLocationDistanceId,
                        NextLocationDistanceId = routeLocationDto.NextLocationDistanceId,
                        LocationIdentifier = routeLocationDto.LocationIdentifier,
                        RouteId = routeLocationDto.RouteId
                    };

                    //HandleDistances(routeLocationDto, newLocation);

                    route.RouteLocations.Add(newLocation);
                }
                else
                {
                    // Update existing RouteLocation
                    existingLocation.Order = routeLocationDto.Order;
                    existingLocation.PrimaryPhase = routeLocationDto.PrimaryPhase;
                    existingLocation.OpposingPhase = routeLocationDto.OpposingPhase;
                    existingLocation.PrimaryDirectionId = (DirectionTypes)routeLocationDto.PrimaryDirectionId;
                    existingLocation.OpposingDirectionId = (DirectionTypes)routeLocationDto.OpposingDirectionId;
                    existingLocation.IsPrimaryOverlap = routeLocationDto.IsPrimaryOverlap;
                    existingLocation.IsOpposingOverlap = routeLocationDto.IsOpposingOverlap;
                    existingLocation.PreviousLocationDistanceId = routeLocationDto.PreviousLocationDistanceId;
                    existingLocation.NextLocationDistanceId = routeLocationDto.NextLocationDistanceId;
                    existingLocation.LocationIdentifier = routeLocationDto.LocationIdentifier;

                    //HandleDistances(routeLocationDto, existingLocation);
                }
            }

            HandleDistances(orderedRouteLocations, route.RouteLocations.ToList());


            // Save changes
            if (route.Id == 0)
            {
                _routeRepository.Add(route);
            }
            else
            {
                _routeRepository.Update(route);
            }
            return ConvertRouteToDTO(route, false);
        }



        public RouteDto GetRouteWithExpandedLocations(int routeId, bool includeLocationDetail)
        {
            // Find existing routeDto or create a new one
            var route = _routeRepository.GetList()
                .Where(r => r.Id == routeId)
                .Include(r => r.RouteLocations)
                .ThenInclude(rl => rl.PreviousLocationDistance)
                .Include(r => r.RouteLocations)
                .ThenInclude(rl => rl.NextLocationDistance)
                .Include(r => r.RouteLocations)
                .ThenInclude(rl => rl.PrimaryDirection)
                .Include(r => r.RouteLocations)
                .ThenInclude(rl => rl.OpposingDirection)
                .FirstOrDefault();

            if (route == null)
            {
                throw new KeyNotFoundException($"Route with id {routeId} not found.");
            }
            return ConvertRouteToDTO(route, includeLocationDetail);
        }

        public RouteDto ConvertRouteToDTO(Data.Models.Route route, bool includeLocationDetail)
        {
            var routeDto = CreateRouteDto(route);
            HandleRouteLocations(routeDto, includeLocationDetail);
            return routeDto;
        }

        private void HandleRouteLocations(RouteDto routeDto, bool includeLocationDetail)
        {
            foreach (var routeLocationDto in routeDto.RouteLocations)
            {
                var location = _locationRepository.GetLatestVersionOfLocation(routeLocationDto.LocationIdentifier);
                if (location == null)
                {
                    continue;
                }
                routeLocationDto.Latitude = location.Latitude;
                routeLocationDto.Longitude = location.Longitude;
                routeLocationDto.PrimaryName = location.PrimaryName;
                routeLocationDto.SecondaryName = location.SecondaryName;
                routeLocationDto.LocationId = location.Id;

                if (includeLocationDetail && routeLocationDto.Approaches != null)
                {
                    foreach (var approach in location.Approaches)
                    {
                        routeLocationDto.Approaches.Add(CreateRouteApproachDto(approach));
                    }
                }
            }
        }

        private void HandleDistances(List<RouteLocationDto> routeLocationDtos, List<RouteLocation> locations)
        {
            var distancesToUpsert = new Dictionary<(string LocationIdentifierA, string LocationIdentifierB), (RouteDistanceDto DistanceDto, Action<RouteDistance> AssignId)>();

            for (int i = 0; i < routeLocationDtos.Count; i++)
            {
                var routeLocationDto = routeLocationDtos[i];
                var location = locations[i];

                if (routeLocationDto.NextLocationDistance != null)
                {
                    var key = (routeLocationDto.NextLocationDistance.LocationIdentifierA, routeLocationDto.NextLocationDistance.LocationIdentifierB);
                    if (!distancesToUpsert.ContainsKey(key))
                    {
                        distancesToUpsert[key] = (routeLocationDto.NextLocationDistance, rd =>
                        {
                            location.NextLocationDistanceId = rd.Id;
                            location.NextLocationDistance = rd;
                        }
                        );
                    }

                    // Set the PreviousLocationDistance for the next location
                    if (i + 1 < routeLocationDtos.Count)
                    {
                        var nextLocation = locations[i + 1];
                        nextLocation.PreviousLocationDistanceId = location.NextLocationDistanceId;
                        nextLocation.PreviousLocationDistance = location.NextLocationDistance;
                    }
                }
                else
                {
                    location.NextLocationDistanceId = null;
                    location.NextLocationDistance = null;
                }
            }

            foreach (var (key, value) in distancesToUpsert)
            {
                var (distanceDto, assignId) = value;
                var distance = _routeDistanceRepository.GetList()
                    .FirstOrDefault(rd => rd.LocationIdentifierA == key.LocationIdentifierA &&
                                          rd.LocationIdentifierB == key.LocationIdentifierB);

                if (distance == null)
                {
                    distance = new RouteDistance
                    {
                        Distance = distanceDto.Distance,
                        LocationIdentifierA = distanceDto.LocationIdentifierA,
                        LocationIdentifierB = distanceDto.LocationIdentifierB
                    };
                    _routeDistanceRepository.Add(distance);
                }
                else if (distance.Distance != distanceDto.Distance)
                {
                    distance.Distance = distanceDto.Distance;
                    _routeDistanceRepository.Update(distance);
                }
                assignId(distance);

                // Update the PreviousLocationDistance for the next location if a new distance was created
                if (distanceDto == routeLocationDtos.FirstOrDefault(rl => rl.NextLocationDistance == distanceDto)?.NextLocationDistance)
                {
                    var currentIndex = routeLocationDtos.FindIndex(rl => rl.NextLocationDistance == distanceDto);
                    if (currentIndex + 1 < routeLocationDtos.Count)
                    {
                        var nextLocation = locations[currentIndex + 1];
                        nextLocation.PreviousLocationDistanceId = distance.Id;
                        nextLocation.PreviousLocationDistance = distance;
                    }
                }
            }
        }

        //private void HandleDistances(RouteLocationDto routeLocationDto, RouteLocation location)
        //{
        //    var distancesToUpsert = new Dictionary<(string LocationIdentifierA, string LocationIdentifierB), (RouteDistanceDto DistanceDto, Action<RouteDistance> AssignId)>();

        //    if (routeLocationDto.NextLocationDistance != null)
        //    {
        //        var key = (routeLocationDto.NextLocationDistance.LocationIdentifierA, routeLocationDto.NextLocationDistance.LocationIdentifierB);
        //        if (!distancesToUpsert.ContainsKey(key))
        //        {
        //            distancesToUpsert[key] = (routeLocationDto.NextLocationDistance, rd =>
        //            {
        //                location.NextLocationDistanceId = rd.Id;
        //                location.NextLocationDistance = rd;
        //            }
        //            );
        //        }
        //    }
        //    else
        //    {
        //        location.NextLocationDistanceId = null;
        //        location.NextLocationDistance = null;
        //    }

        //    foreach (var (key, value) in distancesToUpsert)
        //    {
        //        var (distanceDto, assignId) = value;
        //        var distance = _routeDistanceRepository.GetList()
        //            .FirstOrDefault(rd => rd.LocationIdentifierA == key.LocationIdentifierA &&
        //                                  rd.LocationIdentifierB == key.LocationIdentifierB);

        //        if (distance == null)
        //        {
        //            distance = new RouteDistance
        //            {
        //                Distance = distanceDto.Distance,
        //                LocationIdentifierA = distanceDto.LocationIdentifierA,
        //                LocationIdentifierB = distanceDto.LocationIdentifierB
        //            };
        //            _routeDistanceRepository.Add(distance);
        //        }
        //        else if (distance.Distance.AreNotEqual(distanceDto.Distance))
        //        {
        //            distance.Distance = distanceDto.Distance;
        //            _routeDistanceRepository.Update(distance);
        //        }
        //        assignId(distance);
        //    }
        //}


        public RouteDto CreateRouteDto(Data.Models.Route route)
        {
            var routeDto = new RouteDto();
            routeDto.Id = route.Id;
            routeDto.Name = route.Name;
            routeDto.RouteLocations = new List<RouteLocationDto>();
            foreach (var routeLocation in route.RouteLocations)
            {
                routeDto.RouteLocations.Add(CreateRouteLocationDto(routeLocation));
            }
            return routeDto;
        }

        public RouteLocationDto CreateRouteLocationDto(RouteLocation routeLocation)
        {
            var routeLocationDto = new RouteLocationDto();
            routeLocationDto.Id = routeLocation.Id;
            routeLocationDto.Order = routeLocation.Order;
            routeLocationDto.PrimaryPhase = routeLocation.PrimaryPhase;
            routeLocationDto.OpposingPhase = routeLocation.OpposingPhase;
            routeLocationDto.PrimaryDirectionDescription = routeLocation.PrimaryDirectionId.GetAttributeOfType<DisplayAttribute>().Name;
            routeLocationDto.PrimaryDirectionId = (int)routeLocation.PrimaryDirectionId;
            routeLocationDto.OpposingDirectionDescription = routeLocation.OpposingDirectionId.GetAttributeOfType<DisplayAttribute>().Name;
            routeLocationDto.OpposingDirectionId = (int)routeLocation.OpposingDirectionId;
            routeLocationDto.IsPrimaryOverlap = routeLocation.IsPrimaryOverlap;

            routeLocationDto.IsOpposingOverlap = routeLocation.IsOpposingOverlap;
            if (routeLocation.PreviousLocationDistance != null)
            {
                routeLocationDto.PreviousLocationDistanceId = routeLocation.PreviousLocationDistance.Id;
                routeLocationDto.PreviousLocationDistance = CreateRouteDistanceDto(routeLocation.PreviousLocationDistance);
            }
            if (routeLocation.NextLocationDistance != null)
            {
                routeLocationDto.NextLocationDistanceId = routeLocation.NextLocationDistance.Id;
                routeLocationDto.NextLocationDistance = CreateRouteDistanceDto(routeLocation.NextLocationDistance);
            }
            routeLocationDto.LocationIdentifier = routeLocation.LocationIdentifier;
            routeLocationDto.RouteId = routeLocation.RouteId;
            return routeLocationDto;
        }


        public RouteDistanceDto CreateRouteDistanceDto(RouteDistance previousLocationDistance)
        {
            var routeDistanceDto = new RouteDistanceDto();
            routeDistanceDto.Id = previousLocationDistance.Id;
            routeDistanceDto.Distance = previousLocationDistance.Distance;
            routeDistanceDto.LocationIdentifierA = previousLocationDistance.LocationIdentifierA;
            routeDistanceDto.LocationIdentifierB = previousLocationDistance.LocationIdentifierB;
            return routeDistanceDto;
        }

        public RouteApproachDto CreateRouteApproachDto(Approach approach)
        {
            var routeApproachDto = new RouteApproachDto();
            routeApproachDto.Id = approach.Id;
            routeApproachDto.Description = approach.Description;
            routeApproachDto.Mph = approach.Mph;
            routeApproachDto.ProtectedPhaseNumber = approach.ProtectedPhaseNumber;
            routeApproachDto.IsProtectedPhaseOverlap = approach.IsProtectedPhaseOverlap;
            routeApproachDto.PermissivePhaseNumber = approach.PermissivePhaseNumber;
            routeApproachDto.IsPermissivePhaseOverlap = approach.IsPermissivePhaseOverlap;
            routeApproachDto.PedestrianPhaseNumber = approach.PedestrianPhaseNumber;
            routeApproachDto.IsPedestrianPhaseOverlap = approach.IsPedestrianPhaseOverlap;
            routeApproachDto.PedestrianDetectors = approach.PedestrianDetectors;
            routeApproachDto.LocationId = approach.LocationId;
            foreach (var detector in approach.Detectors)
            {
                routeApproachDto.Detectors.Add(CreateRouteDetectorsDto(detector));
            }
            return routeApproachDto;
        }


        public RouteDetectorDto CreateRouteDetectorsDto(Detector detector)
        {
            var routeDetectorDto = new RouteDetectorDto();
            routeDetectorDto.Id = detector.Id;
            routeDetectorDto.DectectorIdentifier = detector.DectectorIdentifier;
            routeDetectorDto.DetectorChannel = detector.DetectorChannel;
            routeDetectorDto.DistanceFromStopBar = detector.DistanceFromStopBar;
            routeDetectorDto.MinSpeedFilter = detector.MinSpeedFilter;
            routeDetectorDto.DateAdded = detector.DateAdded;
            routeDetectorDto.DateDisabled = detector.DateDisabled;
            routeDetectorDto.LaneNumber = detector.LaneNumber;
            routeDetectorDto.MovementType = detector.MovementType;
            routeDetectorDto.LaneType = detector.LaneType;
            routeDetectorDto.DetectionHardware = detector.DetectionHardware;
            routeDetectorDto.DecisionPoint = detector.DecisionPoint;
            routeDetectorDto.MovementDelay = detector.MovementDelay;
            routeDetectorDto.LatencyCorrection = detector.LatencyCorrection;
            routeDetectorDto.ApproachId = detector.ApproachId;
            return routeDetectorDto;
        }
    }
}
