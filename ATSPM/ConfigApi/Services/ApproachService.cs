using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ConfigApi.Models;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.ConfigApi.Services
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

                        // Update detection types
                        var existingDetectionTypes = detector.DetectionTypes.ToList();
                        var dtoDetectionTypes = detectorDto.DetectionTypes.ToList();

                        // Remove detection types not in DTO
                        foreach (var existingDetectionType in existingDetectionTypes)
                        {
                            if (!dtoDetectionTypes.Contains(existingDetectionType))
                            {
                                detector.DetectionTypes.Remove(existingDetectionType);
                            }
                        }

                        // Add new detection types
                        foreach (var dtoDetectionType in dtoDetectionTypes)
                        {
                            if (!existingDetectionTypes.Contains(dtoDetectionType))
                            {
                                var detectionType = allDetectionTypes.FirstOrDefault(dt => dt.Id == dtoDetectionType.Id);
                                if (detectionType != null)
                                {
                                    detector.DetectionTypes.Add(detectionType);
                                }
                            }
                        }
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
                    Detectors = dto.Detectors.Select(d => new Detector
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
                        DetectionTypes = new List<DetectionType>()
                    }).ToList()
                };

                // Get all detection types
                var allDetectionTypes = _detectionTypeRepository.GetList().ToList();

                // Add detection types to the new detectors
                foreach (var detector in approach.Detectors)
                {
                    foreach (var dtoDetectionType in detector.DetectionTypes)
                    {
                        var detectionType = allDetectionTypes.FirstOrDefault(dt => dt.Id == dtoDetectionType.Id);
                        if (detectionType != null)
                        {
                            detector.DetectionTypes.Add(detectionType);
                        }
                    }
                }

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
                    DetectionTypes = d.DetectionTypes.Select(dt => dt).ToList()
                }).ToList()
            };
        }
    }
}
