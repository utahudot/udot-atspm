#region license
// Copyright 2026 Utah Departement of Transportation
// for ConfigApi - Utah.Udot.Atspm.ConfigApi.Services/SignalTemplateService.cs
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
    public class SignalTemplateService : ISignalTemplateService
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

        //Sync signal: From the data in the database return a list of phases and detector channels that are in the logs and compare the newly created signal.
        //Remove any phases or channels that dont exist on the signal, provide a list of non configured event phases. .
        public async Task<TemplateLocationModifiedDto> SyncNewLocationDetectorsAndApproaches(int locationId)
        {
            DateTime now = DateTime.Now;
            DateTime yesterday = DateTime.Today.AddDays(-1);
            var sourceLocation = _locationRepository.GetVersionByIdDetached(locationId);
            if (sourceLocation != null)
            {
                var compressedLocationsEvents = await _eventLogRepository
                    .GetData<IndianaEvent>(sourceLocation.LocationIdentifier, yesterday, now)
                    .ToListAsync();
                var indianaEvents = compressedLocationsEvents.SelectMany(s => s.Data).ToList();
                return ModifyLocationWithEvents(sourceLocation, indianaEvents);
            }
            else
            {
                throw new ArgumentException($"{locationId} is not a valid Location");
            }
        }

        public static TemplateLocationModifiedDto ModifyLocationWithEvents(Location sourceLocation, IEnumerable<IndianaEvent> indianaEvents)
        {
            // Phase and overlap identifiers are separate namespaces. A matching number
            // in the other namespace must never validate a configured assignment.
            var events = indianaEvents.ToList();
            var protectedOrPermissivePhasesInUse = events.Where(e => e.EventCode is 1 or 8 or 11)
                .Select(e => e.EventParam).ToHashSet();
            var overlapPhasesInUse = events.Where(e => e.EventCode >= 61 && e.EventCode <= 64)
                .Select(e => e.EventParam).ToHashSet();
            var pedestrianPhasesInUse = events.Where(e => e.EventCode is 21 or 22 or 23)
                .Select(e => e.EventParam).ToHashSet();
            var pedestrianOverlapsInUse = events.Where(e => e.EventCode >= 67 && e.EventCode <= 69)
                .Select(e => e.EventParam).ToHashSet();
            var detectorChannelsInUse = events.Where(e => e.EventCode == 82)
                .Select(e => e.EventParam).ToHashSet();
            var warnings = new List<string>();
            var fyaIdentifiers = events.Where(e => e.EventCode is 32 or 33)
                .Select(e => e.EventParam).Distinct().OrderBy(p => p).ToList();
            if (fyaIdentifiers.Count > 0)
            {
                // FYA indicates permissive operation, not whether protected service also
                // exists. Keep the reported identifiers without assuming a phase mapping.
                warnings.Add($"Flashing yellow arrow (FYA) events were observed for identifiers {string.Join(", ", fyaIdentifiers)}. Review the left-turn configuration manually to ensure the protected/permissive assignments and overlap settings are correct.");
            }

            // A null pedestrian assignment inherits the regular protected phase in
            // one-to-one mode. Use the same effective value for retention and warnings.
            int? PedestrianPhase(Approach approach)
            {
                if (approach.PedestrianPhaseNumber.HasValue)
                    return approach.PedestrianPhaseNumber;
                if (sourceLocation.PedsAre1to1 && !approach.IsProtectedPhaseOverlap && !approach.IsPedestrianPhaseOverlap)
                    return approach.ProtectedPhaseNumber;
                return null;
            }

            bool Observed(int? number, bool overlap, bool pedestrian = false)
            {
                if (!number.HasValue) return false;
                if (pedestrian)
                    return overlap
                        ? pedestrianOverlapsInUse.Contains((short)number.Value)
                        : pedestrianPhasesInUse.Contains((short)number.Value);
                return overlap
                    ? overlapPhasesInUse.Contains((short)number.Value)
                    : protectedOrPermissivePhasesInUse.Contains((short)number.Value);
            }

            void CheckAssignment(int approachId, string role, int? number, bool overlap,
                ICollection<MissingPhaseAssignmentDto> missingAssignments, bool pedestrian = false)
            {
                // Report each field independently even when another phase or detector
                // keeps the approach active. Zero means unassigned, not a missing phase.
                if (number > 0 && !Observed(number, overlap, pedestrian))
                    missingAssignments.Add(new MissingPhaseAssignmentDto
                    {
                        ApproachId = approachId, Role = role,
                        PhaseNumber = number.Value, IsOverlap = overlap
                    });
            }

            var missingAssignments = new List<MissingPhaseAssignmentDto>();
            foreach (var approach in sourceLocation.Approaches)
            {
                // One-to-one pedestrian settings must be checked against regular vehicle
                // phases, never inferred from an overlap with the same numeric identifier.
                if (sourceLocation.PedsAre1to1 && approach.ProtectedPhaseNumber > 0 && !approach.IsProtectedPhaseOverlap &&
                    ((approach.PedestrianPhaseNumber ?? approach.ProtectedPhaseNumber) != approach.ProtectedPhaseNumber || approach.IsPedestrianPhaseOverlap))
                    warnings.Add($"Approach {approach.Id} ({approach.Description}): pedestrian configuration does not match the one-to-one setting. Verify pedestrian phase {approach.ProtectedPhaseNumber} and its detector mapping.");
                CheckAssignment(approach.Id, "protected", approach.ProtectedPhaseNumber, approach.IsProtectedPhaseOverlap, missingAssignments);
                CheckAssignment(approach.Id, "permissive", approach.PermissivePhaseNumber, approach.IsPermissivePhaseOverlap, missingAssignments);
                CheckAssignment(approach.Id, "pedestrian", approach.PedestrianPhaseNumber, approach.IsPedestrianPhaseOverlap, missingAssignments, pedestrian: true);
            }

            var newVersion = (Location)sourceLocation.Clone();
            bool ApproachObserved(Approach approach) =>
                Observed(approach.ProtectedPhaseNumber, approach.IsProtectedPhaseOverlap) ||
                Observed(approach.PermissivePhaseNumber, approach.IsPermissivePhaseOverlap) ||
                Observed(PedestrianPhase(approach), approach.IsPedestrianPhaseOverlap, pedestrian: true) ||
                // Preserve approaches supported by detector activity even when no phase
                // transition occurred in the observation window.
                approach.Detectors.Any(d => detectorChannelsInUse.Contains((short)d.DetectorChannel));
            var removedApproaches = newVersion.Approaches.Where(a => !ApproachObserved(a)).ToList();
            newVersion.Approaches = newVersion.Approaches.Where(ApproachObserved).ToList();
            var removedDetectors = new List<string>();

            foreach (var approach in newVersion.Approaches)
            {
                removedDetectors.AddRange(approach.Detectors
                    .Where(det => !detectorChannelsInUse.Contains((short)det.DetectorChannel))
                    .Select(det => det.DetectorChannel.ToString())
                    .ToList());

                approach.Detectors = approach.Detectors
                    .Where(det => detectorChannelsInUse.Contains((short)det.DetectorChannel))
                    .ToList();
            }

            // Step 2: Collect all detectorChannels attached to any approach
            var usedDetectorChannels = newVersion.Approaches
                .SelectMany(approach => approach.Detectors.Select(det => (short)det.DetectorChannel))
                .Distinct()
                .ToList();

            // Step 3: Find all detectorChannels from the input array that are not attached to any approach
            var unattachedDetectorChannels = detectorChannelsInUse.Except(usedDetectorChannels).ToList();

            var usedProtectedOrPermissivePhases = newVersion.Approaches.SelectMany(a => new int?[]
                {
                    a.IsProtectedPhaseOverlap ? null : a.ProtectedPhaseNumber,
                    a.IsPermissivePhaseOverlap ? null : a.PermissivePhaseNumber
                }).Where(p => p.HasValue).Select(p => (short)p.Value).ToHashSet();
            var unattachedProtectedOrPermissivePhases = protectedOrPermissivePhasesInUse.Except(usedProtectedOrPermissivePhases).OrderBy(p => p).ToList();

            // Each field has its own overlap flag; one flagged field must not consume
            // the regular phase number from another field on the same approach.
            var usedOverlapPhases = newVersion.Approaches.SelectMany(a => new int?[]
                {
                    a.IsProtectedPhaseOverlap ? a.ProtectedPhaseNumber : null,
                    a.IsPermissivePhaseOverlap ? a.PermissivePhaseNumber : null
                }).Where(p => p.HasValue).Select(p => (short)p.Value).ToHashSet();
            var unattachedOverlapPhases = overlapPhasesInUse.Except(usedOverlapPhases).OrderBy(p => p).ToList();
            var usedPedestrianPhases = newVersion.Approaches.Where(a => !a.IsPedestrianPhaseOverlap)
                .Select(PedestrianPhase).Where(p => p > 0).Select(p => (short)p.Value);
            var unattachedPedestrianPhases = pedestrianPhasesInUse.Except(usedPedestrianPhases).OrderBy(p => p).ToList();
            foreach (var phase in unattachedPedestrianPhases)
                warnings.Add($"Pedestrian phase {phase} was observed without a configured pedestrian phase assignment. Review the crosswalk mapping and one-to-one pedestrian setting manually; do not add it as a vehicle phase.");
            var usedPedestrianOverlaps = newVersion.Approaches.Where(a => a.IsPedestrianPhaseOverlap)
                .Select(PedestrianPhase).Where(p => p > 0).Select(p => (short)p.Value);
            foreach (var overlap in pedestrianOverlapsInUse.Except(usedPedestrianOverlaps).OrderBy(p => p))
                warnings.Add($"Pedestrian overlap {overlap} was observed without a configured pedestrian overlap assignment. Review the crosswalk mapping manually; do not add it as a vehicle phase.");

            return new TemplateLocationModifiedDto
            {
                Id = sourceLocation.Id.ToString(),
                LocationId = newVersion.Id,
                ReconciliationWarnings = warnings,
                MissingPhaseAssignments = missingAssignments,
                LoggedButUnusedProtectedOrPermissivePhases = unattachedProtectedOrPermissivePhases,
                LoggedButUnusedOverlapPhases = unattachedOverlapPhases,
                LoggedButUnusedPedestrianPhases = unattachedPedestrianPhases,
                LoggedButUnusedDetectorChannels = unattachedDetectorChannels,
                RemovedDetectors = removedDetectors,
                RemovedApproachIds = removedApproaches.Select(i => i.Id).ToList(),
            };
        }
    }
}
