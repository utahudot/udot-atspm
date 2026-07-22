# ATSPM v4 to v5 Configuration Migration

## Feature Summary
The `transferv4-config` command migrates configuration records from ATSPM v4 (MOE) to ATSPM v5:
- Signals -> Locations and Devices
- ControllerTypes -> DeviceConfigurations
- SignalApproaches -> Approaches
- SignalDetectors -> Detectors
- DetectorComments -> DetectorComments

`--locations` can be used to limit migration to specific signal identifiers.

Each migrated v4 signal creates one v5 Location and one v5 Device. The Device is attached to the Location and points to a DeviceConfiguration derived from the signal's v4 ControllerType.

The migration is safe to run more than once. Existing Locations, Devices, Approaches, and Detectors are checked by their natural keys and skipped when already present; controller-type-backed Products and DeviceConfigurations are also reused when available.

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

### Signals -> Devices
| v4 Source | v5 Target | Notes |
|---|---|---|
| SignalID | DeviceIdentifier | Same identifier used for the migrated Location |
| IPAddress | Ipaddress | Defaults to `0.0.0.0` when blank |
| Enabled | LoggingEnabled / DeviceStatus | Sets `LoggingEnabled` and active/inactive status |
| ControllerTypeID | DeviceConfigurationId / DeviceProperties | Resolved to the migrated DeviceConfiguration and stored in device properties |
| Note | Notes | Migrated note text |
| Location | LocationId | Resolved through the migrated Location |
| DeviceType | DeviceType | Defaults to `SignalController` |

### ControllerTypes -> DeviceConfigurations
| v4 Source | v5 Target | Notes |
|---|---|---|
| ControllerTypeID | (mapping only) | Used internally for v4->v5 key mapping |
| Description | Description / Product.Manufacturer | Truncated for the DeviceConfiguration description; also used as the Product manufacturer |
| SNMPPort | Port | Stored as the device configuration port |
| FTPDirectory | Path / ConnectionProperties | Stored in the device configuration path and connection properties |
| ActiveFTP | Protocol / ConnectionProperties | `true` maps to FTP, otherwise SNMP |
| UserName | UserName | Preserved |
| Password | Password | Preserved |
| Product | ProductId | Creates or reuses a Product with `Manufacturer = Description` and `Model = Controller` |

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
