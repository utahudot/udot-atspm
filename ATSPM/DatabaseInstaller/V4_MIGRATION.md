# ATSPM v4 to v5 Configuration Migration

## Feature Summary
The `transferv4-config` command migrates configuration records from ATSPM v4 (MOE) to ATSPM v5:
- Signals -> Locations
- SignalApproaches -> Approaches
- SignalDetectors -> Detectors
- DetectorComments -> DetectorComments

`--locations` can be used to limit migration to specific signal identifiers.

## Field Mapping

### Signals -> Locations
| v4 Source | v5 Target | Notes |
|---|---|---|
| SignalID | (mapping only) | Used internally for v4->v5 key mapping |
| SignalNumber | LocationIdentifier | Primary identity in v5 |
| SignalName | PrimaryName | |
| Latitude | Latitude | Defaults to 0.0 when null |
| Longitude | Longitude | Defaults to 0.0 when null |
| Comments | Note | |
| JurisdictionID | JurisdictionId | Mapped through synced jurisdiction keys |
| RegionID | RegionId | Mapped through synced region keys |

### SignalApproaches -> Approaches
| v4 Source | v5 Target | Notes |
|---|---|---|
| ApproachID | (mapping only) | Used internally for v4->v5 key mapping |
| SignalID | LocationId | Resolved via LocationIdentifier mapping |
| ApproachDirection | DirectionTypeId | Direction string mapped to v5 enum |
| ApproachDescription | Description | |
| ProtectedPhaseNumber | ProtectedPhaseNumber | |
| IsProtectedPhaseOverlap | IsProtectedPhaseOverlap | |
| PermissivePhaseNumber | PermissivePhaseNumber | |
| IsPermissivePhaseOverlap | IsPermissivePhaseOverlap | |
| PedestrianPhaseNumber | PedestrianPhaseNumber | |
| IsPedestrianPhaseOverlap | IsPedestrianPhaseOverlap | |
| PedestrianDetectors | PedestrianDetectors | |
| TransitSignalPriorityPhase | Not persisted | v5 table does not contain TransitSignalPriorityNumber |

### SignalDetectors -> Detectors
| v4 Source | v5 Target | Notes |
|---|---|---|
| DetectorID | (mapping only) | Used internally for v4->v5 key mapping |
| ApproachID | ApproachId | Resolved via approach mapping |
| DetectorNumber | DectectorIdentifier | v5 field name is `DectectorIdentifier` |
| DetectorChannel | DetectorChannel | |
| DistanceFromStopBar | DistanceFromStopBar | |
| SpeedFilter | MinSpeedFilter | |
| DateAdded | DateAdded | |
| DateDisabled | DateDisabled | |
| Lane | LaneNumber | |
| MovementType | MovementType | Enum value mapping |
| LaneType | LaneType | Enum value mapping |
| DetectionHardwareType | DetectionHardware | Enum value mapping |
| DecisionPoint | DecisionPoint | |
| MovementDelay | MovementDelay | |
| Latency | LatencyCorrection | |

### DetectorComments -> DetectorComments
| v4 Source | v5 Target | Notes |
|---|---|---|
| CommentID | (mapping only) | Used internally for v4->v5 key mapping |
| DetectorID | DetectorId | Resolved via detector mapping |
| CommentDate | TimeStamp | |
| CommentText | Comment | |

## Direction Mapping
| v4 Direction | v5 DirectionTypes |
|---|---|
| N, NORTH, NB, NORTHBOUND | NB |
| S, SOUTH, SB, SOUTHBOUND | SB |
| E, EAST, EB, EASTBOUND | EB |
| W, WEST, WB, WESTBOUND | WB |
| NE, NORTHEAST | NE |
| NW, NORTHWEST | NW |
| SE, SOUTHEAST | SE |
| SW, SOUTHWEST | SW |
| Other/unknown | NA |
