# Plan Service Design

## Purpose

This document describes the current `PlanService` design for report and chart plan windows, including the `SignalTimingPlans` table path, supplied-event fallback behavior, current backend consumers, and frontend plan overlay consumers.

The design goal is to make `PlanService` the owner of base plan-window normalization while keeping existing report response shapes and frontend chart rendering stable.

## Identity Rules

Use these names consistently:

- `locationIdentifier`: string controller/location identifier used by reports, controller events, aggregation rows, and `SignalTimingPlan.LocationIdentifier`.
- `locationId`: integer configuration database key, exposed as `Location.Id` and foreign keys such as `Device.LocationId`.

`SignalTimingPlans` is keyed by `LocationIdentifier`. Plan table queries must not join to or filter by integer `Location.Id`.

## Plan Table Model

The current table-backed path uses:

- Model: `Atspm/Data/Models/AggregationModels/SignalTimingPlan.cs`
- DbSet and mapping: `Atspm/Data/AggregationContext.cs`
- Repository interface: `Atspm/Application/Repositories/AggregationRepositories/ISignalTimingPlanRepository.cs`
- EF repository: `Atspm/Infrastructure/Repositories/AggregationRepositories/SignalTimingPlanEFRepository.cs`
- DI registration: `AddAtspmEFAggregationRepositories`

`SignalTimingPlan` contains:

- `LocationIdentifier`
- `PlanNumber`
- `Start`
- `End`
- computed `Valid`

`SignalTimingPlans` has primary key `{ LocationIdentifier, PlanNumber, Start }`.

## Service Contract

`PlanService` supports three plan-window entry points.

Repository-backed table retrieval:

```csharp
Task<IReadOnlyList<Plan>> GetPlansAsync(
    string locationIdentifier,
    DateTime start,
    DateTime end,
    CancellationToken cancellationToken = default);
```

Repository-backed table retrieval with supplied controller-event fallback:

```csharp
Task<IReadOnlyList<Plan>> GetPlansAsync(
    string locationIdentifier,
    DateTime start,
    DateTime end,
    IReadOnlyList<IndianaEvent> fallbackControllerEvents,
    CancellationToken cancellationToken = default);
```

Reusable supplied-plan-data normalization:

```csharp
IReadOnlyList<Plan> GetPlans(
    string locationIdentifier,
    DateTime start,
    DateTime end,
    IEnumerable<SignalTimingPlan> signalTimingPlans);
```

The reusable method does not depend on EF, repositories, or DI. This keeps the same clipping, filtering, ordering, and Unknown gap-filling behavior available to tests, batch workflows, and future callers that already loaded plan rows.

## Table Retrieval Rules

Repository-backed retrieval validates:

- `start < end`
- `locationIdentifier` is not null or whitespace
- `ISignalTimingPlanRepository` is available

The repository query retrieves only usable overlapping table rows:

```csharp
plans.Where(p =>
    p.LocationIdentifier == locationIdentifier &&
    p.Valid &&
    p.Start < end &&
    (p.End == DateTime.MinValue || p.End > start))
```

`DateTime.MinValue` in `End` is treated as open-ended, but only when the row is otherwise valid. Rows where `Valid == false` are not eligible for report plan windows, stale-row fallback checks, or gap filling.

The caller-supplied table-row path applies the same `LocationIdentifier`, `Valid`, overlap, and valid-range filters before producing plan windows.

## Window Normalization

Returned plan windows use `[Start, End)` semantics and are clipped to the exact requested report/chart window.

Required output shape:

- The first segment starts at `start`.
- The last segment ends at `end`.
- Segments are ordered and contiguous.
- Table rows that start before `start` are clipped to `start`.
- Table rows that end after `end` are clipped to `end`.
- Open-ended table rows are clipped to `end`.
- Missing coverage is filled with synthetic Unknown plan `0`.
- Back-to-back segments with the same plan number are merged into one segment.

Unknown means:

- `PlanNumber = "0"` in `Business.Common.Plan`
- Existing report/frontend description behavior should label it as `Unknown`

Example with no table row until 05:00:

| PlanNumber | Start | End |
| ---: | --- | --- |
| 0 | 00:00 | 05:00 |
| 1 | 05:00 | requested end |

Example with a row active before the request:

| Table row | Requested window | Returned segment |
| --- | --- | --- |
| Plan 7, 22:00 to 06:30 | 00:00 to 09:00 | Plan 7, 00:00 to 06:30 |

## Table Data Quality Requirement

`PlanService` normalizes plan windows, but it is not a full table reconciliation engine. `SignalTimingPlans` should represent active, non-overlapping plan intervals for a location timeline.

For a valid active timeline, each row's `End` should be the next plan change for the same `LocationIdentifier`, or `DateTime.MinValue` only for the latest known open-ended row.

If contradictory overlapping rows exist, the implementation applies deterministic ordering by `Start`, then longer `End`, then `PlanNumber`. Earlier active rows can consume the cursor before later overlapping rows are evaluated. The population/backfill workflow should prevent stale open-ended rows from overlapping newer rows.

## Supplied Event Fallback

Fallback uses only controller events already supplied by the caller. `PlanService` does not query the event-log repository and does not perform a 72-hour lookback.

Fallback input rules:

- Only event code `131` (`CoordPatternChange`) is used.
- Events must match the requested `locationIdentifier`.
- Events are cloned before fallback normalization so caller-supplied event instances are not mutated.
- Report services should not fetch extra pre/post controller events only to seed plan fallback.

Fallback is used in two cases:

- No usable valid table rows overlap the requested window.
- Exactly one usable table row overlaps the requested window, it started before `start`, it has `End == DateTime.MinValue`, and supplied in-window `131` events report a different plan number.

Fallback is not used to patch leading, internal, or trailing gaps when normal usable table rows exist. Those gaps remain Unknown.

## Fallback Starting Plan Rules

When fallback is active, event-derived plan windows are normalized through the same `[start, end)` window contract.

The starting plan is determined as follows:

- If a supplied `131` event exists exactly at `start`, that event is the first plan.
- Otherwise, if a supplied pre-window `131` event exists, the latest pre-window event establishes the active plan at `start`.
- Otherwise, Unknown plan `0` is inserted from `start` until the first supplied in-window `131`.
- If no usable supplied `131` event exists, Unknown plan `0` covers the full requested window.

Important observed behavior: if most controllers log `131/254` at midnight and a chart starts at midnight, fallback starts with plan `254` (`Free`) rather than Unknown. That is current behavior because the event exists at the requested start.

## Time Window Policy

Plan retrieval uses the exact report/chart `start` and `end`. The plan path should not expand the requested window by 12 hours before or after.

Some reports still query padded controller events for non-plan calculations such as cycle length, offset, programmed splits, TMC data, priority data, or legacy chart logic. That is separate from plan retrieval. When those reports call `PlanService`, they should pass the exact plan window and only the intended fallback event subset.

## Current Backend Plan Consumers

The following report services currently route base plan windows through `PlanService.GetPlansAsync` and keep their existing report-specific plan DTOs or response shapes:

| Report service | Current plan behavior |
| --- | --- |
| `ApproachDelayReportService` | Uses `PlanService` base windows, then existing approach delay conversion. |
| `ApproachSpeedReportService` | Uses `PlanService` base windows, then `SpeedPlan` conversion. |
| `ArrivalOnRedReportService` | Uses `PlanService` base windows before PCD-style phase data. |
| `GreenTimeUtilizationReportService` | Uses `PlanService` base windows before split-monitor-style plan data. |
| `PedDelayReportService` | Uses `PlanService` base windows before pedestrian delay conversion. |
| `PreemptRequestReportService` | Uses `PlanService` base windows before preempt request grouping. |
| `PreemptServiceReportService` | Uses `PlanService` base windows before preempt service grouping. |
| `PurdueCoordinationDiagramReportService` | Uses `PlanService` base windows before PCD plan conversion. |
| `PurduePhaseTerminationReportService` | Uses `PlanService` base windows before phase termination statistics. |
| `SplitFailReportService` | Uses `PlanService` base windows before split fail cycle assignment. |
| `SplitMonitorReportService` | Uses `PlanService` base windows before split monitor statistics. |
| `TimeSpaceDiagramReportService` | Uses `PlanService` base windows per processed route location; still uses padded controller logs for non-plan events. |
| `TimeSpaceDiagramAverageReportService` | Uses `PlanService` to determine the active plan per period; still uses padded controller logs for non-plan events. |
| `TurningMovementCountReportService` | Uses `PlanService` base windows before lane/movement aggregation. |
| `WaitTimeReportService` | Uses `PlanService` base windows before wait-time plan conversion. |
| `YellowRedActivationsReportService` | Uses `PlanService` base windows before yellow/red activation statistics. |

Application/business services that now consume base `Plan` windows include:

- `AnalysisPhaseCollectionService`
- `LocationPhaseService`
- `ApproachSpeedService`
- `GreenTimeUtilizationService`
- `SplitFailPhaseService`
- `SplitMonitorService`
- `YellowRedActivationsService`
- `PreemptServiceService`
- `PreemptServiceRequestService`
- `PedPhaseService` and `PedDelayService`
- `TransitSignalPriorityService`
- `LinkPivotPcdService`
- `LinkPivotPairService`

Legacy event-derived helper overloads remain for compatibility, including `GetBasicPlans(..., IReadOnlyList<IndianaEvent>)`, `GetPlanEvents(...)`, and the event-list overload on `AnalysisPhaseCollectionService`. New report plan paths should prefer base `Plan` windows from `PlanService`.

## Consumers Not Currently Identified As Plan-Overlay Users

These report/chart areas query events or aggregations but were not identified as current plan-window overlay consumers in the searched code paths:

- Approach Volume
- Left Turn Gap Analysis and Left Turn Gap report services
- Priority Summary and Priority Details
- Preemption Details
- Ramp Metering
- Timing and Actuation
- Watchdog reports

If future work adds plan overlays to these reports, they should consume base windows from `PlanService`.

## Frontend Plan Consumers

Frontend chart overlays continue to consume report `Plans` payloads. The backend response shape should remain stable.

Shared behavior:

- `features/charts/common/transformers.ts` exposes `createPlans`.
- `createPlans` renders the ECharts `Plans` series.
- `createPlans` uses `planDescription` for the displayed label.
- Split Monitor also synchronizes plan display props in `PhaseTable.tsx`.

Current chart transformers using `createPlans`:

| Frontend chart | Plan overlay path |
| --- | --- |
| Approach Delay | `approachDelay.transformer.ts` |
| Approach Speed | `approachSpeed.transformer.ts` |
| Arrivals on Red | `arrivalsOnRed.transformer.ts` |
| Green Time Utilization | `greenTimeUtilization.transformer.ts` |
| Pedestrian Delay | `pedestrianDelay.transformer.ts` |
| Purdue Coordination Diagram | `purdueCoordinationDiagram.transformer.ts` |
| Purdue Phase Termination | `purduePhaseTermination.transformer.ts` |
| Purdue Split Failure | `purdueSplitFailure.transformer.ts` |
| Split Monitor | `splitMonitor.tranformer.ts` and `PhaseTable.tsx` |
| Turning Movement Counts | `turningMovementCounts.transformer.ts` |
| Wait Time | `waitTime.transformer.ts` |
| Yellow and Red Actuations | `yellowAndRedActuations.transformer.ts` |

Unknown plan display is supported when the backend returns `PlanNumber = "0"` with the existing Unknown description behavior.

## Validation Checklist

- All table-backed plan queries use `locationIdentifier`.
- No `SignalTimingPlans` query uses integer `Location.Id`.
- `SignalTimingPlan.Valid == false` rows are excluded.
- Every returned report window is contiguous from requested `start` to requested `end`.
- Missing table coverage is represented as Unknown plan `0`.
- Open-ended rows use `End == DateTime.MinValue` and are clipped to requested `end`.
- Adjacent same-number plan windows are merged.
- Supplied-event fallback uses only caller-supplied controller events.
- Supplied-event fallback uses event code `131` only.
- Supplied-event fallback does not perform a separate event-log query or lookback.
- Supplied-event fallback can use pre-window context only when the caller intentionally supplies it.
- Supplied-event fallback does not patch gaps in otherwise usable table data.
- Reports do not fetch 12-hour pre/post event data only for plan retrieval.
- Existing report payload shapes remain compatible.
- Existing frontend `Plans` rendering remains compatible.

## Test Coverage Expectations

`PlanService` tests should cover:

- Filtering by string `LocationIdentifier`.
- Excluding invalid table rows.
- Never using `Location.Id`.
- Producing identical output from supplied plan rows and repository-retrieved rows.
- Not calling the repository for supplied plan-row normalization.
- Clipping rows before and after the requested window.
- Clipping open-ended rows.
- Unknown output for no rows.
- Unknown plus fallback behavior when only invalid table rows exist.
- Leading, internal, and trailing Unknown gaps.
- Supplied `131` fallback when no usable table rows exist.
- Stale single open-ended-row fallback when in-window supplied `131` events contradict the table row.
- Exact-start fallback events becoming the starting plan.
- Pre-window fallback events becoming the starting plan only when supplied.
- No mutation of caller-supplied fallback events.
- Ordered contiguous windows covering `[start, end)`.
