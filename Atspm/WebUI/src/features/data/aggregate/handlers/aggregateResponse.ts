import { AggregateData } from '../types/aggregateData'

// The one place the page reads the report response, so a change in its shape
// lands here. A report server older than the page sends no location list, so
// those charts are titled without locations instead of failing.
export const toAggregateCharts = (response: unknown): AggregateData[] =>
  ((response ?? []) as AggregateData[]).map((chart) => ({
    ...chart,
    locationIdentifiers: chart.locationIdentifiers ?? [],
  }))
