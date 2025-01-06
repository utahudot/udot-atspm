#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Services/ApproachService.cs
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

using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.ValueObjects;

namespace Utah.Udot.Atspm.ConfigApi.Services
{
    public class SignalTemplateService
    {
        private readonly IApproachRepository _approachRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IDetectionTypeRepository _detectionTypeRepository;
        private readonly IEventLogRepository _eventLogRepository;

        public SignalTemplateService(ILocationRepository locationRepository, IApproachRepository approachRepository, IDetectionTypeRepository detectionTypeRepository, IEventLogRepository eventLogRepository)
        {
            _approachRepository = approachRepository;
            _detectionTypeRepository = detectionTypeRepository;
            _locationRepository = locationRepository;
            _eventLogRepository = eventLogRepository;
        }

        //public ApproachDto SyncNewLocationEvents(int locationId)
        //{
        //    //Sync that puts data into the database (trigger the event logging workflow for the signal) successful or failure message.
        //    //var workflow = new DetectorHourlySpeedAggregationWorkflow();
        //    //await workflow.Input.SendAsync(locationId);
        //}

        //Sync signal: From the data in the database return a list of phases and detector channels that are in the logs and compare the newly created signal.
        //Remove any phases or channels that dont exist on the signal, provide a list of non configured event phases. 
        public TemplateLocationModifiedDto SyncNewLocationDetectorsAndApproaches(int locationId)
        {
            DateTime now = DateTime.Now;
            var yesterday = now.Date.AddDays(-1).ToString("yyyy-MM-dd");
            var sourceLocation = _locationRepository.GetVersionByIdDetached(locationId);
            if (sourceLocation != null)
            {

                var compressedLocationsEvents = _eventLogRepository.GetArchivedEvents(locationId.ToString(), DateOnly.Parse(yesterday), DateOnly.Parse(yesterday));
                var indianaEvents = compressedLocationsEvents.Where(l => l.DataType == typeof(IndianaEvent)).SelectMany(s => s.Data).Cast<IndianaEvent>();
                return ModifyLocationWithEvents(sourceLocation, indianaEvents);
            }
            else
            {
                throw new ArgumentException($"{locationId} is not a valid Location");
            }
        }

        public static TemplateLocationModifiedDto ModifyLocationWithEvents(Location sourceLocation, IEnumerable<IndianaEvent> indianaEvents)
        {
            var cycleEvents = indianaEvents.Where(e =>
                new List<short>
                {
                    1,
                    8,
                    11,
                    61, //Overlap
                    63, //Overlap
                    64, //Overlap
                    90, //Ped
                }.Contains(e.EventCode)).ToList();

            var protectedOrPermissivePhasesInUse = cycleEvents.Where(d => ((d.EventCode == 1) || (d.EventCode == 8) || (d.EventCode == 11))).Select(d => d.EventParam).Distinct();
            var overlapPhasesInUse = cycleEvents.Where(d => ((d.EventCode == 61) || (d.EventCode == 63) || (d.EventCode == 64))).Select(d => d.EventParam).Distinct();
            var pedestrianPhasesInUse = cycleEvents.Where(d => (d.EventCode == 90)).Select(d => d.EventParam).Distinct();

            var detectorChannel = indianaEvents.Where(d => d.EventCode == 82).Select(d => d.EventParam).Distinct();

            var newVersion = (Location)sourceLocation.Clone();
            // Detach the original entity
            var ogApproaches = newVersion.Approaches;


            // Step 1: Partition the approaches into kept and removed
            var removedApproaches = newVersion.Approaches
                .Where(approach =>
                    !((approach.PermissivePhaseNumber == null ? false : protectedOrPermissivePhasesInUse.Contains((short)approach.PermissivePhaseNumber)) ||
                      (approach.ProtectedPhaseNumber == null ? false : protectedOrPermissivePhasesInUse.Contains((short)approach.ProtectedPhaseNumber)) ||
                      (approach.PedestrianPhaseNumber == null ? false : pedestrianPhasesInUse.Contains((short)approach.PedestrianPhaseNumber)) ||
                      (approach.PermissivePhaseNumber == null ? false : (overlapPhasesInUse.Contains((short)approach.PermissivePhaseNumber) && approach.IsPermissivePhaseOverlap == true)) ||
                      (approach.ProtectedPhaseNumber == null ? false : (overlapPhasesInUse.Contains((short)approach.ProtectedPhaseNumber) && approach.IsProtectedPhaseOverlap == true)) ||
                      (approach.PedestrianPhaseNumber == null ? false : (overlapPhasesInUse.Contains((short)approach.PedestrianPhaseNumber) && approach.IsPedestrianPhaseOverlap == true))))
                .ToList();

            newVersion.Approaches = newVersion.Approaches
                .Where(approach =>
                    ((approach.PermissivePhaseNumber == null ? false : protectedOrPermissivePhasesInUse.Contains((short)approach.PermissivePhaseNumber)) ||
                      (approach.ProtectedPhaseNumber == null ? false : protectedOrPermissivePhasesInUse.Contains((short)approach.ProtectedPhaseNumber)) ||
                      (approach.PedestrianPhaseNumber == null ? false : pedestrianPhasesInUse.Contains((short)approach.PedestrianPhaseNumber)) ||
                      (approach.PermissivePhaseNumber == null ? false : (overlapPhasesInUse.Contains((short)approach.PermissivePhaseNumber) && approach.IsPermissivePhaseOverlap == true)) ||
                      (approach.ProtectedPhaseNumber == null ? false : (overlapPhasesInUse.Contains((short)approach.ProtectedPhaseNumber) && approach.IsProtectedPhaseOverlap == true)) ||
                      (approach.PedestrianPhaseNumber == null ? false : (overlapPhasesInUse.Contains((short)approach.PedestrianPhaseNumber) && approach.IsPedestrianPhaseOverlap == true))))
                .ToList();

            var removedDetectors = new List<string>();
            // Step 1: Filter and save the Detectors for each Approach
            foreach (var approach in newVersion.Approaches)
            {
                removedDetectors.AddRange(approach.Detectors
                    .Where(det => !detectorChannel.Contains((short)det.DetectorChannel))
                    .Select(det => det.DectectorIdentifier)
                    .ToList());

                approach.Detectors = approach.Detectors
                    .Where(det => detectorChannel.Contains((short)det.DetectorChannel))
                    .ToList();
            }

            // Step 2: Collect all detectorChannels attached to any approach
            var usedDetectorChannels = newVersion.Approaches
                .SelectMany(approach => approach.Detectors.Select(det => (short)det.DetectorChannel))
                .Distinct()
                .ToList();

            // Step 3: Find all detectorChannels from the input array that are not attached to any approach
            var unattachedDetectorChannels = detectorChannel.Except(usedDetectorChannels).ToList();

            var usedProtectedOrPermissivePhases = newVersion.Approaches
                .SelectMany(approach => new List<int?>
                {
                    approach.PermissivePhaseNumber,
                    approach.ProtectedPhaseNumber
                })
                .Where(phase => phase.HasValue) // Filter out null values
                .Select(phase => (short)phase.Value) // Cast non-null values to short
                .Where(phase => protectedOrPermissivePhasesInUse.Contains(phase))
                .Distinct()
                .ToList();

            var unattachedProtectedOrPermissivePhases = protectedOrPermissivePhasesInUse.Except(usedProtectedOrPermissivePhases).ToList();

            // Step 2: Find shorts in overlapPhasesInUse that are not attached to any remaining approach
            var usedOverlapPhases = newVersion.Approaches
                .Where(approach => (approach.IsPermissivePhaseOverlap || approach.IsProtectedPhaseOverlap || approach.IsPedestrianPhaseOverlap))
                .SelectMany(approach => new List<int?>
                {
                    approach.PermissivePhaseNumber,
                    approach.ProtectedPhaseNumber,
                    approach.PedestrianPhaseNumber
                })
                .Where(phase => phase.HasValue) // Exclude nulls
                .Select(phase => (short)phase.Value) // Cast non-null values to short
                .Where(phase => overlapPhasesInUse.Contains(phase))
                .Distinct()
                .ToList();

            var unattachedOverlapPhases = overlapPhasesInUse.Except(usedOverlapPhases).ToList();


            // Step 3: Find shorts in pedestrianPhasesInUse that are not attached to any remaining approach
            var usedPedestrianPhases = newVersion.Approaches
                .SelectMany(approach => new List<int?> { approach.PedestrianPhaseNumber })
                .Where(phase => phase.HasValue) // Exclude nulls
                .Select(phase => (short)phase.Value) // Cast non-null values to short
                .Where(phase => pedestrianPhasesInUse.Contains(phase))
                .Distinct()
                .ToList();

            var unattachedPedestrianPhases = pedestrianPhasesInUse.Except(usedPedestrianPhases).ToList();

            return new TemplateLocationModifiedDto
            {
                Id = sourceLocation.Id.ToString(),
                LocationId = newVersion.Id,
                Location = new LocationDto
                {
                    Id = newVersion.Id,
                    Latitude = newVersion.Latitude,
                    Longitude = newVersion.Longitude,
                    PrimaryName = newVersion.PrimaryName,
                    SecondaryName = newVersion.SecondaryName,
                    ChartEnabled = newVersion.ChartEnabled,
                    VersionAction = newVersion.VersionAction,
                    Note = newVersion.Note,
                    Start = newVersion.Start,
                    PedsAre1to1 = newVersion.PedsAre1to1,
                    LocationIdentifier = newVersion.LocationIdentifier,
                    JurisdictionId = newVersion.JurisdictionId,
                    Jurisdiction = newVersion.Jurisdiction == null ? null : new JurisdictionDto
                    {
                        Id = newVersion.Jurisdiction.Id,
                        Name = newVersion.Jurisdiction.Name,
                        Mpo = newVersion.Jurisdiction.Mpo,
                        CountyParish = newVersion.Jurisdiction.CountyParish,
                        OtherPartners = newVersion.Jurisdiction.OtherPartners,
                        UserJurisdictions = newVersion.Jurisdiction.UserJurisdictions.Select(userJurisdiction => new UserJurisdictionDto
                        {
                            UserId = userJurisdiction.UserId,
                            JurisdictionId = userJurisdiction.JurisdictionId
                        }).ToList()
                    },
                    LocationTypeId = newVersion.LocationTypeId,
                    LocationType = newVersion.LocationType == null ? null : new LocationTypeDto
                    {
                        Id = newVersion.LocationType.Id,
                        Name = newVersion.LocationType.Name,
                        Icon = newVersion.LocationType.Icon
                    },
                    RegionId = newVersion.RegionId,
                    Region = newVersion.Region == null ? null : new RegionDto
                    {
                        Id = newVersion.Region.Id,
                        Description = newVersion.Region.Description,
                        UserRegions = newVersion.Region.UserRegions.Select(userRegion => new UserRegionDto
                        {
                            UserId = userRegion.UserId,
                            RegionId = userRegion.RegionId
                        }).ToList()
                    },
                    Approaches = newVersion.Approaches.Select(approach => new ApproachDto
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
                        Detectors = approach.Detectors.Select(detector => new DetectorDto
                        {
                            Id = detector.Id,
                            DectectorIdentifier = detector.DectectorIdentifier,
                            DetectorChannel = detector.DetectorChannel,
                            DistanceFromStopBar = detector.DistanceFromStopBar,
                            MinSpeedFilter = detector.MinSpeedFilter,
                            DateAdded = detector.DateAdded,
                            DateDisabled = detector.DateDisabled,
                            LaneNumber = detector.LaneNumber,
                            MovementType = detector.MovementType,
                            LaneType = detector.LaneType,
                            DetectionHardware = detector.DetectionHardware,
                            DecisionPoint = detector.DecisionPoint,
                            MovementDelay = detector.MovementDelay,
                            LatencyCorrection = detector.LatencyCorrection,
                            ApproachId = detector.ApproachId,
                            DetectionTypes = detector.DetectionTypes.Select(detectionType => new DetectionTypeDto
                            {
                                Id = detectionType.Id,
                                Description = detectionType.Description,
                                Abbreviation = detectionType.Abbreviation,
                                DisplayOrder = detectionType.DisplayOrder,
                                MeasureTypes = detectionType.MeasureTypes.Select(measureType => new MeasureTypeDto
                                {
                                    Id = measureType.Id,
                                    Name = measureType.Name,
                                    Abbreviation = measureType.Abbreviation,
                                    ShowOnWebsite = measureType.ShowOnWebsite,
                                    ShowOnAggregationSite = measureType.ShowOnAggregationSite,
                                    DisplayOrder = measureType.DisplayOrder,
                                    MeasureComments = measureType.MeasureComments.Select(comment => new MeasureCommentsDto
                                    {
                                        Id = comment.Id,
                                        TimeStamp = comment.TimeStamp,
                                        Comment = comment.Comment,
                                        LocationIdentifier = comment.LocationIdentifier
                                    }).ToList(),
                                    MeasureOptions = measureType.MeasureOptions.Select(option => new MeasureOptionDto
                                    {
                                        Id = option.Id,
                                        Option = option.Option,
                                        Value = option.Value,
                                        MeasureTypeId = option.MeasureTypeId
                                    }).ToList()
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList(),
                    Areas = newVersion.Areas.Select(area => new AreaDto
                    {
                        Id = area.Id,
                        Name = area.Name,
                        UserAreas = area.UserAreas.Select(userArea => new UserAreaDto
                        {
                            UserId = userArea.UserId,
                            AreaId = userArea.AreaId
                        }).ToList()
                    }).ToList(),
                    Devices = newVersion.Devices.Select(device => new DeviceDto
                    {
                        Id = device.Id,
                        LoggingEnabled = device.LoggingEnabled,
                        Ipaddress = device.Ipaddress,
                        DeviceStatus = device.DeviceStatus,
                        DeviceType = device.DeviceType,
                        Notes = device.Notes,
                        LocationId = device.LocationId,
                        DeviceConfigurationId = device.DeviceConfigurationId,
                        DeviceConfiguration = device.DeviceConfiguration == null ? null : new DeviceConfigurationDto
                        {
                            Id = device.DeviceConfiguration.Id,
                            Firmware = device.DeviceConfiguration.Firmware,
                            Notes = device.DeviceConfiguration.Notes,
                            Protocol = device.DeviceConfiguration.Protocol,
                            Port = device.DeviceConfiguration.Port,
                            Directory = device.DeviceConfiguration.Directory,
                            SearchTerms = device.DeviceConfiguration.SearchTerms,
                            ConnectionTimeout = device.DeviceConfiguration.ConnectionTimeout,
                            OperationTimeout = device.DeviceConfiguration.OperationTimeout,
                            Decoders = device.DeviceConfiguration.Decoders,
                            UserName = device.DeviceConfiguration.UserName,
                            Password = device.DeviceConfiguration.Password,
                            ProductId = device.DeviceConfiguration.ProductId,
                            Product = device.DeviceConfiguration.Product // Assuming Product has a mapping as well
                        }
                    }).ToList()
                },
                LoggedButUnusedProtectedOrPermissivePhases = unattachedProtectedOrPermissivePhases,
                LoggedButUnusedOverlapPhases = unattachedOverlapPhases,
                LoggedButUnusedPedestrianPhases = unattachedPedestrianPhases,
                LoggedButUnusedDetectorChannels = unattachedDetectorChannels,
                RemovedDetectors = removedDetectors,
                RemovedApproachIds = removedApproaches.Select(i => i.Id).ToList(),
                RemovedApproaches = removedApproaches.Select(approach => new ApproachDto
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
                    Detectors = approach.Detectors.Select(detector => new DetectorDto
                    {
                        Id = detector.Id,
                        DectectorIdentifier = detector.DectectorIdentifier,
                        DetectorChannel = detector.DetectorChannel,
                        DistanceFromStopBar = detector.DistanceFromStopBar,
                        MinSpeedFilter = detector.MinSpeedFilter,
                        DateAdded = detector.DateAdded,
                        DateDisabled = detector.DateDisabled,
                        LaneNumber = detector.LaneNumber,
                        MovementType = detector.MovementType,
                        LaneType = detector.LaneType,
                        DetectionHardware = detector.DetectionHardware,
                        DecisionPoint = detector.DecisionPoint,
                        MovementDelay = detector.MovementDelay,
                        LatencyCorrection = detector.LatencyCorrection,
                        ApproachId = detector.ApproachId,
                        DetectionTypes = detector.DetectionTypes.Select(detectionType => new DetectionTypeDto
                        {
                            Id = detectionType.Id,
                            Description = detectionType.Description,
                            Abbreviation = detectionType.Abbreviation,
                            DisplayOrder = detectionType.DisplayOrder,
                            MeasureTypes = detectionType.MeasureTypes.Select(measureType => new MeasureTypeDto
                            {
                                Id = measureType.Id,
                                Name = measureType.Name,
                                Abbreviation = measureType.Abbreviation,
                                ShowOnWebsite = measureType.ShowOnWebsite,
                                ShowOnAggregationSite = measureType.ShowOnAggregationSite,
                                DisplayOrder = measureType.DisplayOrder,
                                MeasureComments = measureType.MeasureComments.Select(comment => new MeasureCommentsDto
                                {
                                    Id = comment.Id,
                                    TimeStamp = comment.TimeStamp,
                                    Comment = comment.Comment,
                                    LocationIdentifier = comment.LocationIdentifier
                                }).ToList(),
                                MeasureOptions = measureType.MeasureOptions.Select(option => new MeasureOptionDto
                                {
                                    Id = option.Id,
                                    Option = option.Option,
                                    Value = option.Value,
                                    MeasureTypeId = option.MeasureTypeId
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList()
                }).ToList()

            };
        }
    }
}
