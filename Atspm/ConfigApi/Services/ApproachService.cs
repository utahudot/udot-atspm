#region license
// Copyright 2025 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Services/ApproachService.cs
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
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.ATSPM.ConfigApi.DTO;

namespace Utah.Udot.Atspm.ConfigApi.Services
{
    public class ApproachService : IApproachService
    {
        private readonly IApproachRepository _approachRepository;
        private readonly IDetectionTypeRepository _detectionTypeRepository;

        public ApproachService(IApproachRepository approachRepository, IDetectionTypeRepository detectionTypeRepository)
        {
            _approachRepository = approachRepository;
            _detectionTypeRepository = detectionTypeRepository;
        }

        public async Task<ApproachDto> UpsertApproachAsync(ApproachDto dto)
        {
            Approach approach;
            if (dto.Id.HasValue && dto.Id > 0)
            {
                // Update existing approach
                approach = _approachRepository.GetList()
                    .Include(a => a.Detectors)
                    .ThenInclude(d => d.DetectionTypes)
                    .FirstOrDefault(a => a.Id == dto.Id.Value);

                if (approach == null)
                {
                    throw new KeyNotFoundException("Approach not found.");
                }

                // Update properties
                approach.Description = dto.Description;
                approach.Mph = dto.Mph;
                approach.ProtectedPhaseNumber = dto.ProtectedPhaseNumber;
                approach.IsProtectedPhaseOverlap = dto.IsProtectedPhaseOverlap;
                approach.PermissivePhaseNumber = dto.PermissivePhaseNumber;
                approach.IsPermissivePhaseOverlap = dto.IsPermissivePhaseOverlap;
                approach.PedestrianPhaseNumber = dto.PedestrianPhaseNumber;
                approach.IsPedestrianPhaseOverlap = dto.IsPedestrianPhaseOverlap;
                approach.PedestrianDetectors = dto.PedestrianDetectors;
                approach.LocationId = dto.LocationId;
                approach.DirectionTypeId = dto.DirectionTypeId;

                // Remove Detectors that are not in DTO
                var detectorIds = dto.Detectors.Select(d => d.Id).ToList();
                foreach (var detector in approach.Detectors.Where(d => !detectorIds.Contains(d.Id)).ToList())
                {
                    approach.Detectors.Remove(detector);
                }


                // Get all detection types
                var allDetectionTypes = _detectionTypeRepository.GetList().ToList();

                // Update Detectors
                foreach (var detectorDto in dto.Detectors)
                {
                    var detector = approach.Detectors.FirstOrDefault(d => d.Id == detectorDto.Id);
                    if (detector != null)
                    {
                        detector.DectectorIdentifier = detectorDto.DectectorIdentifier;
                        detector.DetectorChannel = detectorDto.DetectorChannel;
                        detector.DistanceFromStopBar = detectorDto.DistanceFromStopBar;
                        detector.MinSpeedFilter = detectorDto.MinSpeedFilter;
                        detector.DateAdded = detectorDto.DateAdded;
                        detector.DateDisabled = detectorDto.DateDisabled;
                        detector.LaneNumber = detectorDto.LaneNumber;
                        detector.MovementType = detectorDto.MovementType;
                        detector.LaneType = detectorDto.LaneType;
                        detector.DetectionHardware = detectorDto.DetectionHardware;
                        detector.DecisionPoint = detectorDto.DecisionPoint;
                        detector.MovementDelay = detectorDto.MovementDelay;
                        detector.LatencyCorrection = detectorDto.LatencyCorrection;
                        detector.ApproachId = dto.Id ?? 0; // Assuming the approach is already created and has an ID
                        detector.DetectionTypes = detectorDto.DetectionTypes
                            .Select(dto => allDetectionTypes.FirstOrDefault(dt => dt.Id == dto.Id))
                            .Where(dt => dt != null).ToList();
                    }
                    else
                    {
                        var newDetector = new Detector
                        {
                            DectectorIdentifier = detectorDto.DectectorIdentifier,
                            DetectorChannel = detectorDto.DetectorChannel,
                            DistanceFromStopBar = detectorDto.DistanceFromStopBar,
                            MinSpeedFilter = detectorDto.MinSpeedFilter,
                            DateAdded = detectorDto.DateAdded,
                            DateDisabled = detectorDto.DateDisabled,
                            LaneNumber = detectorDto.LaneNumber,
                            MovementType = detectorDto.MovementType,
                            LaneType = detectorDto.LaneType,
                            DetectionHardware = detectorDto.DetectionHardware,
                            DecisionPoint = detectorDto.DecisionPoint,
                            MovementDelay = detectorDto.MovementDelay,
                            LatencyCorrection = detectorDto.LatencyCorrection,
                            ApproachId = dto.Id ?? 0, // Assuming the approach is already created and has an ID,
                            DetectionTypes = new List<DetectionType>()
                        };

                        // Add detection types to the new detector
                        foreach (var dtoDetectionType in detectorDto.DetectionTypes)
                        {
                            var detectionType = allDetectionTypes.FirstOrDefault(dt => dt.Id == dtoDetectionType.Id);
                            if (detectionType != null)
                            {
                                newDetector.DetectionTypes.Add(detectionType);
                            }
                        }

                        approach.Detectors.Add(newDetector);
                    }
                }

                await _approachRepository.UpdateAsync(approach);
            }
            else
            {
                // Create new approach
                approach = new Approach
                {
                    Description = dto.Description,
                    Mph = dto.Mph,
                    ProtectedPhaseNumber = dto.ProtectedPhaseNumber,
                    IsProtectedPhaseOverlap = dto.IsProtectedPhaseOverlap,
                    PermissivePhaseNumber = dto.PermissivePhaseNumber,
                    IsPermissivePhaseOverlap = dto.IsPermissivePhaseOverlap,
                    PedestrianPhaseNumber = dto.PedestrianPhaseNumber,
                    IsPedestrianPhaseOverlap = dto.IsPedestrianPhaseOverlap,
                    PedestrianDetectors = dto.PedestrianDetectors,
                    LocationId = dto.LocationId,
                    DirectionTypeId = dto.DirectionTypeId,
                    Detectors = dto.Detectors?.Select(d => new Detector
                    {
                        DectectorIdentifier = d.DectectorIdentifier,
                        DetectorChannel = d.DetectorChannel,
                        DistanceFromStopBar = d.DistanceFromStopBar,
                        MinSpeedFilter = d.MinSpeedFilter,
                        DateAdded = d.DateAdded,
                        DateDisabled = d.DateDisabled,
                        LaneNumber = d.LaneNumber,
                        MovementType = d.MovementType,
                        LaneType = d.LaneType,
                        DetectionHardware = d.DetectionHardware,
                        DecisionPoint = d.DecisionPoint,
                        MovementDelay = d.MovementDelay,
                        LatencyCorrection = d.LatencyCorrection,
                        DetectionTypes = DtoToDetectionType(d.DetectionTypes.ToList())
                    }).ToList()
                };

                await _approachRepository.AddAsync(approach);
            }

            // Return the updated DTO with IDs
            return ConvertToDto(approach);
        }

        public async Task<ApproachDto> GetApproachDtoByIdAsync(int id)
        {
            var approach = _approachRepository.GetList()
                .Include(a => a.Detectors)
                .ThenInclude(d => d.DetectionTypes)
                .FirstOrDefault(a => a.Id == id);
            if (approach == null)
            {
                throw new KeyNotFoundException("Approach not found.");
            }

            return ConvertToDto(approach);
        }

        private ApproachDto ConvertToDto(Approach approach)
        {
            return new ApproachDto
            {
                Id = approach.Id,
                Description = approach.Description,
                Mph = approach.Mph,
                ProtectedPhaseNumber = approach.ProtectedPhaseNumber,
                IsProtectedPhaseOverlap = approach.IsProtectedPhaseOverlap,
                PermissivePhaseNumber = approach.PermissivePhaseNumber,
                IsPermissivePhaseOverlap = approach.IsPermissivePhaseOverlap,
                PedestrianPhaseNumber = approach.PedestrianPhaseNumber,
                IsPedestrianPhaseOverlap = approach.IsPedestrianPhaseOverlap,
                PedestrianDetectors = approach.PedestrianDetectors,
                LocationId = approach.LocationId,
                DirectionTypeId = approach.DirectionTypeId,
                Detectors = approach.Detectors.Select(d => new DetectorDto
                {
                    Id = d.Id,
                    DectectorIdentifier = d.DectectorIdentifier,
                    DetectorChannel = d.DetectorChannel,
                    DistanceFromStopBar = d.DistanceFromStopBar,
                    MinSpeedFilter = d.MinSpeedFilter,
                    DateAdded = d.DateAdded,
                    DateDisabled = d.DateDisabled,
                    LaneNumber = d.LaneNumber,
                    MovementType = d.MovementType,
                    LaneType = d.LaneType,
                    DetectionHardware = d.DetectionHardware,
                    DecisionPoint = d.DecisionPoint,
                    MovementDelay = d.MovementDelay,
                    LatencyCorrection = d.LatencyCorrection,
                    ApproachId = d.ApproachId,
                    DetectionTypes = ExtractDetectionTypesToDto(d.DetectionTypes.ToList())
                }).ToList()
            };
        }

        private static List<DetectionTypeDto> ExtractDetectionTypesToDto(List<DetectionType> d)
        {
            return d.Select(dt => new DetectionTypeDto
            {
                Id = dt.Id,
                Description = dt.Description,
                Abbreviation = dt.Abbreviation,
                DisplayOrder = dt.DisplayOrder,
                MeasureTypes = dt.MeasureTypes.Select(mt => new MeasureTypeDto
                {
                    Id = mt.Id,
                    Name = mt.Name,
                    Abbreviation = mt.Abbreviation,
                    ShowOnWebsite = mt.ShowOnWebsite,
                    ShowOnAggregationSite = mt.ShowOnAggregationSite,
                    DisplayOrder = mt.DisplayOrder,
                    MeasureComments = mt.MeasureComments.Select(mc => new MeasureCommentsDto
                    {
                        Id = mc.Id,
                        TimeStamp = mc.TimeStamp,
                        Comment = mc.Comment,
                        LocationIdentifier = mc.LocationIdentifier
                    }).ToList(),
                    MeasureOptions = mt.MeasureOptions.Select(mo => new MeasureOptionDto
                    {
                        Id = mo.Id,
                        Option = mo.Option,
                        Value = mo.Value,
                        MeasureTypeId = mo.MeasureTypeId
                    }).ToList()
                }).ToList()
            }).ToList();
        }

        private List<DetectionType> DtoToDetectionType(List<DetectionTypeDto> d)
        {
            List<DetectionType> detectionTypes = new List<DetectionType>();
            // Get all detection types
            var allDetectionTypes = _detectionTypeRepository.GetList().ToList();

            foreach (var dtoDetectionType in d)
            {
                var detectionType = allDetectionTypes.FirstOrDefault(dt => dt.Id == dtoDetectionType.Id);
                if (detectionType != null)
                {
                    detectionTypes.Add(detectionType);
                }
            }
            return detectionTypes;
        }


    }
}
