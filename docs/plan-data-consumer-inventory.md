# Plan Data Consumer Inventory

## Purpose

Identify current report, business-service, and frontend chart consumers of plan data so they can be unified behind the table-backed `PlanService`.

This inventory distinguishes the location string `locationIdentifier` from the integer `locationId`. Plan data consumers should use `locationIdentifier` for event-log, report, aggregation, and `SignalTimingPlans` lookup paths.

## Backend Report Consumers

These report services currently derive or pass plan data from Indiana plan events and should move to table-backed `PlanService` base windows:

| Report service | Current plan-data pattern | Future direction |
| --- | --- | --- |
| `ApproachDelayReportService` | Gets plan events from controller logs and passes them into location phase data. | Use `PlanService` base windows for the location/time range. |
| `ApproachSpeedReportService` | Gets plan events, then `ApproachSpeedService` calls `GetSpeedPlans`. | Use table-backed base windows and keep `SpeedPlan` conversion in business logic. |
| `ArrivalOnRedReportService` | Gets plan events and passes them through PCD-style phase data. | Use table-backed base windows before building arrival-on-red plans. |
| `GreenTimeUtilizationReportService` | Gets plan events, then `GreenTimeUtilizationService` calls `GetSplitMonitorPlans`. | Use table-backed base windows and keep GTU statistics unchanged. |
| `PedDelayReportService` | Gets plan events and passes them into `PedPhaseService`. | Use table-backed base windows before building pedestrian delay plans. |
| `PreemptServiceReportService` | Gets plan events, then business service calls `GetBasicPlans`. | Use table-backed base windows and keep preempt plan DTO conversion. |
| `PreemptRequestReportService` | Gets plan events, then business service calls `GetBasicPlans`. | Use table-backed base windows and keep request-event grouping. |
| `PurdueCoordinationDiagramReportService` | Gets plan events and passes them into `LocationPhaseService`. | Use table-backed base windows and keep PCD-specific plan conversion. |
| `PurduePhaseTerminationReportService` | Gets plan events, then converts phase collection plans to common plan DTOs. | Use table-backed base windows and keep phase termination statistics. |
| `SplitFailReportService` | Gets plan events, then `SplitFailPhaseService` calls `GetSplitFailPlans`. | Use table-backed base windows and keep split fail cycle assignment. |
| `SplitMonitorReportService` | Gets plan events, then `AnalysisPhaseCollectionService` calls `GetSplitMonitorPlans`. | Use table-backed base windows and keep split monitor statistics. |
| `TimeSpaceDiagramReportService` | Gets plan events for each processed location/phase and passes them into `LocationPhaseService`. | Use table-backed base windows for each location/time range. |
| `TimeSpaceDiagramAverageReportService` | Gets plan events for each route location and calls `GetBasicPlans(...).FirstOrDefault()`. | Use table-backed base windows to determine active plan consistency per period. |
| `TurningMovementCountReportService` | Gets plan events and directly calls `GetBasicPlans`. | Use table-backed base windows and keep lane/movement aggregation unchanged. |
| `WaitTimeReportService` | Gets plan events, then uses analysis phase collection plans. | Use table-backed base windows before wait-time plan conversion. |
| `YellowRedActivationsReportService` | Gets plan events, then business service calls `GetYellowRedActivationPlans`. | Use table-backed base windows and keep violation statistics unchanged. |

Many of these report services currently load controller logs with `Start.AddHours(-12)` and `End.AddHours(12)` before calling `GetPlanEvents`. That padding should be removed from the plan path. The future plan path should pass the actual chart/report `start` and `end` to `PlanService`; `PlanService` is responsible for finding overlapping rows and clipping them to the requested window.

## Backend Business Consumers

These business services or helper services currently depend on plan windows or plan event lists:

| Business service | Current dependency | Future direction |
| --- | --- | --- |
| `PlanService` | Builds report plans from `IndianaEvent` plan events. | Own table-backed retrieval, clipping, gap filling, and base plan DTO creation. |
| `LocationPhaseService` | Calls `GetPcdPlans` using event-derived plan data. | Accept or retrieve base plan windows from `PlanService`. |
| `AnalysisPhaseCollectionService` | Calls `GetSplitMonitorPlans` using event-derived plan data. | Use table-backed base windows for phase collections. |
| `ApproachSpeedService` | Calls `GetSpeedPlans`. | Convert table-backed base windows to `SpeedPlan`. |
| `GreenTimeUtilizationService` | Calls `GetSplitMonitorPlans`. | Convert table-backed base windows to GTU plan data. |
| `SplitFailPhaseService` | Calls `GetSplitFailPlans`. | Convert table-backed base windows to `PlanSplitFail`. |
| `SplitMonitorService` | Builds plan statistics from phase collection plans. | Continue using phase collection plans after their source changes. |
| `YellowRedActivationsService` | Calls `GetYellowRedActivationPlans`. | Convert table-backed base windows to yellow/red activation plans. |
| `PreemptServiceService` | Calls `GetBasicPlans`. | Convert table-backed base windows to `PreemptPlan`. |
| `PreemptServiceRequestService` | Calls `GetBasicPlans`. | Convert table-backed base windows to request plans. |
| `PedPhaseService` and `PedDelayService` | Build pedestrian plans from supplied plan data. | Keep conversion/statistics behavior, change base plan source. |
| `TransitSignalPriorityService` | Builds TSP plans from event-derived plan events. | Use table-backed base windows for the plan timeline; continue using split/cycle events for TSP metrics. |
| `LinkPivotPcdService` and `LinkPivotPairService` | Use `GetPlanEvents` before building PCD data. | Use table-backed base windows for link pivot PCD flows. |

## Frontend Chart Consumers

These frontend chart transformers render a plan overlay from report `Plans` data, usually through `createPlans` in `features/charts/common/transformers.ts`:

| Frontend chart | Plan overlay path |
| --- | --- |
| Approach Delay | `approachDelay.transformer.ts` calls `createPlans`. |
| Approach Speed | `approachSpeed.transformer.ts` calls `createPlans`. |
| Arrivals on Red | `arrivalsOnRed.transformer.ts` calls `createPlans`. |
| Green Time Utilization | `greenTimeUtilization.transformer.ts` calls `createPlans`. |
| Pedestrian Delay | `pedestrianDelay.transformer.ts` calls `createPlans`. |
| Purdue Coordination Diagram | `purdueCoordinationDiagram.transformer.ts` calls `createPlans`. |
| Purdue Phase Termination | `purduePhaseTermination.transformer.ts` wraps `createPlans`. |
| Purdue Split Failure | `purdueSplitFailure.transformer.ts` calls `createPlans`. |
| Split Monitor | `splitMonitor.tranformer.ts` calls `createPlans`; `PhaseTable.tsx` also synchronizes plan display props. |
| Turning Movement Counts | `turningMovementCounts.transformer.ts` calls `createPlans`. |
| Wait Time | `waitTime.transformer.ts` calls `createPlans`. |
| Yellow and Red Actuations | `yellowAndRedActuations.transformer.ts` calls `createPlans`. |

Shared frontend behavior:

- `createPlans` creates the ECharts `Plans` series.
- Existing chart response shapes should remain stable.
- No frontend changes are required if backend report `Plans` payloads keep their current shape.

## Consumers Not Identified As Plan-Overlay Users

These report/chart areas query events or aggregations but were not identified as current plan-window consumers in the searched code paths:

- Approach Volume.
- Left Turn Gap Analysis and Left Turn Gap Report services.
- Priority Summary and Priority Details.
- Preemption Details.
- Ramp Metering.
- Timing and Actuation.
- Watchdog reports.

If future work adds plan overlays to these reports, they should consume base windows from `PlanService` instead of event code `131`.

## Unification Notes

The unification should happen at the backend plan-window source, not in frontend chart transformers.

Recommended flow:

1. Report service requests base plan windows from `PlanService` by `locationIdentifier`, `start`, and `end`.
2. Business service converts base windows into the report-specific plan DTO required by existing response models.
3. Frontend continues to render the existing `Plans` response data.

Implementation must remove direct plan-window dependence on `GetPlanEvents` for unified reports. Event logs remain valid for non-plan data such as cycles, splits, detections, preempt calls, pedestrian events, and priority events.

Implementation must also remove `Start.AddHours(-12)` / `End.AddHours(12)` from plan retrieval. If a report still needs padded event ranges for non-plan calculations, that query should remain separate from the exact-range `PlanService` call.

`PlanService` should support both retrieval modes:

- A repository-backed method for callers that only have `locationIdentifier`, `start`, and `end`.
- A reusable method that accepts caller-supplied `SignalTimingPlan` rows and performs the same filtering, clipping, ordering, and Unknown gap filling without hitting the repository.

This keeps the service reusable for tests, batched report workflows, multi-location scenarios, and code paths that already loaded plan rows for other reasons.

## Validation Checklist For Future Implementation

- All table-backed plan queries use `locationIdentifier`.
- No `SignalTimingPlans` query uses integer `Location.Id`.
- Every report window receives a contiguous clipped set of plans from requested `start` to requested `end`.
- Missing table coverage is represented as Unknown plan `0`.
- There is no event code `131` fallback.
- Plan retrieval does not use 12-hour pre/post expansion.
- Caller-supplied plan data can be passed to `PlanService` without forcing repository access.
- Existing frontend `Plans` rendering remains compatible.
