import { toAggregateCharts } from './aggregateResponse'

describe('toAggregateCharts', () => {
  it('keeps the locations each chart came with', () => {
    const charts = toAggregateCharts([
      { identifier: '', locationIdentifiers: ['7115', '7116'], series: [] },
    ])

    expect(charts[0].locationIdentifiers).toEqual(['7115', '7116'])
  })

  it('gives a chart from an older report server an empty location list', () => {
    const charts = toAggregateCharts([{ identifier: '', series: [] }])

    expect(charts[0].locationIdentifiers).toEqual([])
  })

  it('reads an empty response as no charts', () => {
    expect(toAggregateCharts(undefined)).toEqual([])
  })
})
