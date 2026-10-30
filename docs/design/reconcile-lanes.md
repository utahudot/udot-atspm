# Approach and lane reconciliation

Updated October 28, 2026. This document describes the implemented location-editor workflow. For count collection, report timezone handling, and source limitations, see [TMC source data flows](tmc-source-data-flows.md).

## Scope and feature flags

The location toolbar has one **Reconcile Approaches** action. Controller-event approach reconciliation and the shared approach/detector editors are available independently of the lane feature flag. This includes phase/overlap matching, FYA warnings, pedestrian checks, assignment removal, and whole-approach deletion.

Camera lane reconciliation requires two independent, default-off settings:

| Setting | Purpose |
|---|---|
| WebUI `ENABLE_LANE_RECONCILIATION=true` | `/api/feature-flags` exposes `laneReconciliation`; opens the combined Approaches/Lanes dialog. Only the exact environment value `true` enables it. |
| Report API `Features:LaneReconciliation=true` | Authorizes device zone-evidence requests. Environment form: `Features__LaneReconciliation=true`. |

These settings are separate from `Features:TmcDeviceSources` and `Features:DeviceTestDownload`. Lane evidence can be enabled independently of camera TMC charts. With the WebUI lane flag absent or false, the approach workflow remains available and the lane component is not mounted.

The combined dialog starts approach comparison on opening. Lane comparison starts only after selecting **Lanes** and clicking **Compare lanes**. Tabs retain their review results, selections, and edited channels. Closing the dialog discards its lane review; changes already applied to the editor remain. Closing through Escape, the backdrop, or Close is blocked while either comparison is busy. Changed configuration or lane inputs invalidate the previous lane review before it can be applied.

## Approach reconciliation

Config API zone retrieval requires location-configuration edit permission, device discovery requires device-edit permission, and Sync Location requires location-configuration edit permission. Editors can still retrieve zones using unsaved connection settings. Saved-device zone inventory uses the provider interface; the built-in provider supports Econolite Vision FIR/AI cameras with numeric identifiers. Other products need a compatible provider, and provider/connection failures are shown in the lane review.

`SignalTemplateService` reads Indiana events from yesterday's local midnight through the current server time. It compares the current location configuration; the comparison itself does not save changes. These event sets establish observed assignments:

| Evidence | Codes | Interpretation |
|---|---|---|
| Regular vehicle phase | 1, 8, 11 | Protected/permissive phase-number namespace |
| Vehicle overlap | 61-64 | Separate overlap-number namespace |
| Pedestrian phase | 21-23 | Regular pedestrian phase |
| Pedestrian overlap | 67-69 | Separate pedestrian overlap |
| Detector activity | 82 | Detector channel |
| FYA | 32, 33 | Manual left-turn configuration warning, with observed identifiers |

Off/Dark events 65, 66, and 70 do not establish an active overlap. Event 90 does not establish a pedestrian phase. Identical phase and overlap numbers do not imply the same output; each configured assignment uses its own overlap flag. FYA evidence never automatically converts a protected assignment into a permissive one or establishes the controller's mapping.

Each configured protected, permissive, and pedestrian assignment is checked independently. Another observed assignment or detector may retain an approach while one of its assignments is missing. Missing observations are evidence for review, not proof that a configured movement is absent in the field.

When `PedsAre1to1` is enabled, a null pedestrian phase inherits the protected phase for observation matching and unassigned-pedestrian warnings, provided neither the protected nor pedestrian assignment is flagged as an overlap. An explicitly different pedestrian phase or pedestrian-overlap setting is flagged for manual review against the one-to-one setting on a regular protected phase. The fallback does not write a pedestrian phase into configuration. Unconfigured pedestrian phases/overlaps produce crosswalk-review warnings rather than vehicle-phase additions.

### Review and editing

- **Found** separates Phases, Overlaps, and Detector Channels. New phases/overlaps use the shared approach editor. An overlap can be added to a new approach or an available assignment on an existing approach; existing assignments are not silently overwritten.
- A new detector must belong to an approach. The shared detector editor exposes its properties before adding the draft.
- **Not Found** shows the existing approach editor and missing assignments together. Removing an assignment retains the approach and detectors only if a protected or permissive vehicle assignment remains. Removing protected sets its number to `0`; removing permissive or pedestrian sets it to null. The corresponding overlap flag is cleared. Bulk removal enforces the same constraint.
- **Delete entire approach** is a separately confirmed action and also deletes its detectors. Removing the last vehicle assignment is not offered as a way to leave an invalid approach.
- Phase/overlap edits and additions are staged in the location editor and require saving. Existing detector and whole-approach deletion use the existing immediate API deletion paths; they are not merely draft removals. The confirmation distinguishes detector deletion from staged assignment changes. In the scenario tool all actions affect mock state only.

## Lane reconciliation

The current location already includes any template applied at creation. No separate template is selected, and no town-specific zone-name convention is used for matching. Decoder choices come from active devices' comma-separated `TmcDecoder` properties, using the chart's existing source grouping. Configured decoder groups are offered; the report backend requires lane-evidence capability. Config API independently selects a zone provider using the saved device hardware. `VisionCameraAPI` supplies evidence, and `VisionDeviceZoneProvider` supplies configuration. `VisionBinStatisticsStored` is a chart source, not a lane-evidence source.

1. Choose a supported decoder and observation range. Defaults are three completed calendar days, 15-minute bins, and a minimum count of **25** for unassigned movements. Report dates use the location timezone; end is exclusive.
2. Query the existing TMC report endpoint with `source=devices`, selected device IDs, decoder, and `reconcileLanes=true`. The decoder resolves physical-camera aliases; `sourceDeviceIds` identifies the devices whose inventories must be read, including devices returning no bins.
3. Read complete zone names through the existing **Get Zones** API for those devices. Compare exact zone names with active configured detectors above channel 64. No automatic change is inferred from an unavailable or ambiguous inventory.
4. Review independent recommendations and verification warnings. Nothing is preselected. Enter a unique unused channel above 64 for each new movement; defaults reserve the next available channels, including channels occupied by disabled entries.
5. **Apply selected to editor** stages all selected lane changes atomically. Use the existing editor save controls to persist them. This does not change camera zones or historical event data.

### Recommendations and safeguards

| Situation | Behavior |
|---|---|
| Through and a turn share a camera zone | Offer separate movement entries sharing the zone, physical lane number, and copied detector settings. Preview the original entry's movement change before applying. |
| Through exists; Left and Right are both missing | Offer two independent additions, each with its own channel. |
| Combined TR/TL/LTR entry | Offer an explicit split. Splitting preserves configured movements even if unobserved. |
| Configured movement has zero counts | Attach an optional removal to its verification warning. Removal narrows the existing entry without adding detectors; removing Right from LTR leaves TL. |
| Removing Through would leave combined Left/Right | No LR movement type exists. Warn to review/split first rather than create an invalid entry. |
| Known zone has no bin evidence | Offer optional removal for a named vehicle entry, with a warning. A zero-bin row is treated as missing evidence. |
| Configured zone is absent from inventory | Warn; absence alone does not offer deletion. |
| Missing turn has a unique existing entry on the same approach and physical lane, but its zone is absent | Offer a zone change instead of an extra detector, subject to the coverage guard below. Channel and other detector settings remain intact. |
| Multiple candidate detectors or destination zones | Warn for manual verification; do not guess a reassignment or add a duplicate turn entry. |
| Repeated movement on distinct camera zones | Keep entries separate; no overlap warning solely because both are Through, Left, or Right. |
| Overlapping movement assignments on the same zone | Warn about potential double counting and suppress movement recommendations for that zone. |

A zone repair is permitted only when every device returned for the location is covered by the selected decoder's device IDs. This check includes devices with missing or empty `TmcDecoder`. Detectors do not record device ownership, so another uncovered device could own the apparently missing zone. In that case the potential repair/addition becomes a manual ownership warning. This conservative check can also block a legitimate repair until ownership is verified. It does not guess from IP address or zone naming.

Channels 1-64 are excluded from lane matching and recommendations. Zones used only on those channels are not reported as unused. This exclusion belongs to camera lane reconciliation; controller-event approach reconciliation still checks detector activity.

Unused named inventory zones offer **Add Detector**, independently of count thresholds and warnings on other zones or source reads. The shared detector editor prefills the zone and next available channel above 64, requires an existing approach, and validates channel uniqueness. This manual action stages a detector; the user must save the approach. Cancel leaves configuration unchanged. After staging, compare again to refresh the review.

Source-read warnings suppress all lane recommendations for that run. An incomplete inventory blocks matching changes because another device might own the same name. Duplicate names across devices or multiple zone IDs with the same name require verification. Unnamed evidence remains separate by zone ID and is displayed as `Unnamed zone <ID>`; that label is never used as a configured zone name. Negative-count errors identify the camera/device.

Counts do not establish permitted turns, physical lane geometry, protected/permissive operation, or complete collection. Low-volume movements, zero counts, and missing bins require review. A matching total is not proof of correct camera alignment. Multiple physical lanes sharing a zone require additional verification before assigning an unrepresented movement.

## Decoder contract and reuse

`ITurningMovementCountDecoder.SupportsLaneReconciliation` defaults to false. Supporting decoders return `TmcZoneEvidence` containing device ID, exact zone name, optional source zone ID, T/L/R totals, and distinct bin count. `DeviceCountSource` preserves evidence by device and returns selected device IDs after alias resolution; it does not aggregate lane evidence into chart approaches.

Vision lane evidence is filtered to the requested time range before grouping, but bypasses chart layer selection, closest-setback filtering, and direction/zone normalization. Get Zones reads configuration through `IDeviceZoneProvider` in Config API; Report API returns only observed count evidence. Providers are discovered at startup and selected using `CanRead(Device)`. A future device family implements this provider and a report decoder with lane-evidence capability; no frontend adapter or vendor allowlist is required. Missing or ambiguous providers block inventory-based recommendations. The legacy Get Zones request remains available for unsaved editor settings. The lane feature reuses report, inventory, and configuration APIs without changing collection/import behavior.

## Scenarios, tests, and code references

The standalone scenario mock tool is preserved on branch `feature/scenario-mock-tool`. This branch retains automated mocks in `Atspm/WebUI/e2e/fixtures` for testing the regular location page. Approach fixture responses are checked against the real backend. Mocks do not validate real camera geometry or traffic.

Tests cover independent feature flags; phase/overlap namespaces; FYA and pedestrian warnings; inherited pedestrian assignments; assignment and approach deletion; shared editors; movement additions/removals; channel uniqueness; shared-zone repair; uncovered devices; aliases; unnamed zones; stale input; source failures; and preserving review state across tabs. Playwright tests exercise the regular location page with mocked HTTP responses. Test commands and fixture regeneration instructions are in [WebUI E2E README](../../Atspm/WebUI/e2e/README.md).

- [Approach comparison](../../Atspm/ConfigApi/Services/SignalTemplateService.cs)
- [Combined dialog](../../Atspm/WebUI/src/features/locations/components/ReconcileLanes/ReconcileConfigurationButton.tsx)
- [Lane request and review panel](../../Atspm/WebUI/src/features/locations/components/ReconcileLanes/LaneDialog.tsx)
- [Lane matching and staging](../../Atspm/WebUI/src/features/locations/components/ReconcileLanes/reconcileLanes.ts)
- [Approach review view](../../Atspm/WebUI/src/features/locations/components/ApproachesReconcilationReport/ApproachesReconciliationView.tsx)
- [Missing-assignment actions](../../Atspm/WebUI/src/features/locations/components/ApproachesReconcilationReport/NotFoundSection.tsx)

Manual warning actions also allow editing or removing existing detectors for overlapping movement assignments and missing configured zones. Missing lane numbers or unknown movement/lane types open the shared editor. Low-count unassigned movements and ambiguous physical-lane assignments allow an explicit addition with approach/lane selection. These are user-directed draft changes, not automatic recommendations; each action invalidates the old comparison. Removal preserves the saved baseline until Save, and Reset restores it. The LTR-without-Through case remains unchanged.
