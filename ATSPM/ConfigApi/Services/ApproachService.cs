using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.ConfigApi.Models;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Services
{
    public class ApproachService : IApproachService
    {
        private readonly IApproachRepository _approachRepository;

        public ApproachService(IApproachRepository approachRepository)
        {
            _approachRepository = approachRepository;
        }

        public async Task<ApproachDto> UpsertApproachAsync(ApproachDto dto)
        {
            Approach approach;
            if (dto.Id > 0)
            {
                // Update existing approach
                approach = _approachRepository.GetList()
                    .FirstOrDefault(a => a.Id == dto.Id);

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
                        detector.ApproachId = detectorDto.ApproachId;
                    }
                    else
                    {
                        approach.Detectors.Add(new Detector
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
                            ApproachId = detectorDto.ApproachId
                        });
                    }
                }

                // Remove Detectors that are not in DTO
                var detectorIds = dto.Detectors.Select(d => d.Id).ToList();
                foreach (var detector in approach.Detectors.Where(d => !detectorIds.Contains(d.Id)).ToList())
                {
                    approach.Detectors.Remove(detector);
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
                        ApproachId = d.ApproachId
                    }).ToList()
                };

                await _approachRepository.AddAsync(approach);
            }


            dto.Id = approach.Id;
            return dto;
        }
    }

}
