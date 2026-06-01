# Plan Service New Plan Table Design

## Purpose

Move report plan-window retrieval to the `SignalTimingPlans` aggregation table through `PlanService`, while keeping existing report and frontend response shapes stable.

This document is design-only. It does not propose report payload changes, frontend changes, or an event-log fallback path.

## Terminology

Use these names consistently:

- `locationIdentifier`: the string controller/location identifier used by reports, event logs, aggregation rows, and `SignalTimingPlan.LocationIdentifier`.
- `locationId`: the integer configuration database key, exposed as `Location.Id` and as foreign keys such as `Device.LocationId`.

The plan table is keyed by `LocationIdentifier`; it must not be queried by `Location.Id`.

## Current State

Current report plan data is mostly derived from Indiana event code `131`:

- Report services load controller events over a padded time window, commonly `Start.AddHours(-12)` to `End.AddHours(12)`.
- Report services call `GetPlanEvents(...)` to reduce event code `131` rows into plan-change events.
- Business services call `PlanService` methods such as `GetBasicPlans`, `GetPcdPlans`, `GetSplitMonitorPlans`, `GetSpeedPlans`, and `GetSplitFailPlans`.
- The frontend receives report-specific `Plans` payloads and renders plan overlays through existing chart transformers.

That 12-hour pre/post range exists so event-derived plan logic can find the active plan before the chart starts and the next plan after the chart ends. The table-backed design should remove that padding from plan retrieval. Reports should request plans for the actual chart/report `[start, end)` range and let `PlanService` use overlapping table rows, clipping, open-ended rows, and Unknown gap filling to produce complete plan windows.

The new table path already exists:

- Model: `Atspm/Data/Models/AggregationModels/SignalTimingPlan.cs`
- DbSet and mapping: `Atspm/Data/AggregationContext.cs`
- Repository interface: `Atspm/Application/Repositories/AggregationRepositories/ISignalTimingPlanRepository.cs`
- EF repository: `Atspm/Infrastructure/Repositories/AggregationRepositories/SignalTimingPlanEFRepository.cs`
- DI registration: `AddAtspmEFAggregationRepositories`

`SignalTimingPlan` has:

- `LocationIdentifier`
- `PlanNumber`
- `Start`
- `End`
- computed `Valid`

`SignalTimingPlans` has primary key `{ LocationIdentifier, PlanNumber, Start }`.

## Proposed Service Contract

Add a reusable plan-window API to `PlanService` that can operate on supplied plan data:

```csharp
IReadOnlyList<Plan> GetPlans(
    string locationIdentifier,
    DateTime start,
    DateTime end,
    IEnumerable<SignalTimingPlan> signalTimingPlans);
```

Also add a repository-backed convenience API that retrieves `SignalTimingPlan` rows and delegates to the reusable method:

```csharp
Task<IReadOnlyList<Plan>> GetPlansAsync(
    string locationIdentifier,
    DateTime start,
    DateTime end,
    CancellationToken cancellationToken = default);
```

The implementation should:

- Validate `start < end`.
- Validate `locationIdentifier` is not null or whitespace.
- Keep clipping, ordering, overlap handling, and Unknown gap filling in the reusable method that accepts plan data.
- Let the repository-backed method query only `ISignalTimingPlanRepository`, then pass the retrieved rows to the reusable method.
- Filter by `SignalTimingPlan.LocationIdentifier == locationIdentifier` in both paths, so callers can safely pass a wider set of plan rows.
- Never join or filter by `Location.Id`.
- Use the requested report/chart `start` and `end` directly; do not expand plan retrieval by 12 hours before or after the requested window.
- Not fall back to event code `131`.

The repository query should retrieve overlapping table rows:

```csharp
plans.Where(p =>
    p.LocationIdentifier == locationIdentifier &&
    p.Start < end &&
    (p.End == DateTime.MinValue || p.End > start))
```

Treat `DateTime.MinValue` in `End` as open-ended for retrieval, then clip it to the requested `end`.

The reusable method should not depend on EF, repositories, or DI. This makes the plan-window logic usable by tests, report code that has already batched plan rows, future batch APIs, and workflow code without forcing every caller to perform one repository call per location/window.

## Window Contract

Reports should receive plan windows clipped to the exact requested range:

- The first returned segment starts at `start`.
- The last returned segment ends at `end`.
- Segment ranges are contiguous and ordered by `Start`.
- Returned segments use `[Start, End)` semantics.
- Table rows that start before `start` are clipped to `start`.
- Table rows that end after `end` are clipped to `end`.
- Open-ended table rows are clipped to `end`.

If table rows do not cover any portion of the requested range, fill that gap with a synthetic Unknown plan:

- `PlanNumber = "0"` for `Business.Common.Plan`.
- Description remains the existing `Unknown` behavior.
- Unknown segments should be clipped like real table-backed segments.

This keeps the source table-only while still guaranteeing every report timeframe starts and ends with a plan segment.

## Plan Timeline Requirements

The table-backed service depends on `SignalTimingPlans` representing active plan intervals for a location timeline. For a report window, the service needs to know which plan is active at each time, even when the report start and end do not line up with plan changes.

The existing population workflow should be reviewed before implementation because its reconciliation step groups by `(LocationIdentifier, PlanNumber)`. For active report timelines, end times must represent the next plan change for the same `LocationIdentifier`, not only the next occurrence of the same plan number.

Example desired active timeline:

| LocationIdentifier | PlanNumber | Start | End |
| --- | ---: | --- | --- |
| `1001` | 1 | 08:00 | 09:15 |
| `1001` | 2 | 09:15 | 11:30 |
| `1001` | 3 | 11:30 | open-ended |

For a request from 09:00 to 10:00, `PlanService` should return:

| PlanNumber | Start | End |
| ---: | --- | --- |
| 1 | 09:00 | 09:15 |
| 2 | 09:15 | 10:00 |

For a request from 07:30 to 08:30 with no row covering 07:30 to 08:00, `PlanService` should return:

| PlanNumber | Start | End |
| ---: | --- | --- |
| 0 | 07:30 | 08:00 |
| 1 | 08:00 | 08:30 |

## Consumer Unification

Use `PlanService` as the only source for base plan windows.

`PlanService` should be the only owner of plan-window normalization, but it should not always be the owner of data retrieval. Callers that already have relevant `SignalTimingPlan` rows may pass them directly to the reusable method. Callers that do not have rows should use the repository-backed convenience method.

Keep existing report-specific conversion responsibilities:

- PCD and arrival-on-red can convert base windows to `PurdueCoordinationPlan` and related plan DTOs.
- Split Monitor and Green Time Utilization can convert base windows to `PlanSplitMonitorData`.
- Split Failure can convert base windows to `PlanSplitFail`.
- Approach Speed can convert base windows to `SpeedPlan`.
- TSP can convert base windows to `TransitSignalPriorityBasicPlan` and continue building TSP-specific summary plans.
- Preempt, Turning Movement Counts, Wait Time, Yellow/Red Activations, and Pedestrian Delay can keep their existing response DTOs.

Report services should stop extracting plan events with `GetPlanEvents` once they use the table-backed `PlanService` path. They may still query event logs for cycle, detector, split, preempt, priority, pedestrian, and other non-plan events.

When the only reason for a report's `Start.AddHours(-12)` / `End.AddHours(12)` query is plan discovery, remove that padding from the plan path. If a report still needs padded event ranges for non-plan calculations, keep that event-query behavior separate from plan retrieval and pass the exact chart/report window to `PlanService`.

## Edge Cases

- No rows for `locationIdentifier`: return one Unknown plan covering `[start, end)`.
- Leading gap before first row: return Unknown from `start` to first covered row.
- Internal gap between rows: return Unknown for the gap.
- Trailing gap after last row: return Unknown to `end`.
- Duplicate or overlapping rows: sort by `Start`; implementation must define deterministic precedence before coding. Recommended default is to prefer the earlier-starting active row until its clipped end, then continue timeline construction.
- Invalid table row where `End <= Start` and `End != DateTime.MinValue`: ignore the row or log it; do not let it create negative or zero-length report windows.

## Test Plan

Unit tests for `PlanService`:

- Filters by string `LocationIdentifier`.
- Does not use `Location.Id` for `SignalTimingPlans`.
- Produces the same output from caller-supplied plan rows and repository-retrieved plan rows.
- Does not call the repository when caller-supplied plan rows are used.
- Clips rows starting before the requested window.
- Clips rows ending after the requested window.
- Clips `DateTime.MinValue` open-ended rows to the requested end.
- Produces Unknown for no rows.
- Produces Unknown for leading, internal, and trailing gaps.
- Returns ordered contiguous plan windows covering `[start, end)`.

Repository-level test:

- Confirms overlapping table rows are retrieved with `Start < end` and `(End == DateTime.MinValue || End > start)`.

Representative report compatibility tests:

- Purdue Coordination Diagram.
- Approach Speed.
- Split Monitor.
- Turning Movement Counts.
- Transit Signal Priority.

Acceptance criteria:

- Existing report payload shapes remain compatible.
- The frontend can continue rendering existing `Plans` payloads.
- No report plan retrieval uses event code `131`.
- No table-backed plan retrieval uses integer `Location.Id`.
- Reports and charts no longer use 12-hour pre/post expansion for plan retrieval.
