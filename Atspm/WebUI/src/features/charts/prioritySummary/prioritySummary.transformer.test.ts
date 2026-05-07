import type { PrioritySummaryResult } from '@/api/reports'
import transformPrioritySummaryData from './prioritySummary.transformer'

describe('transformPrioritySummaryData', () => {
  it('creates a combined chart before one chart per TSP number and filters unassigned events by TSP', () => {
    const response: PrioritySummaryResult = {
      start: '2026-04-07T08:00:00Z',
      end: '2026-04-07T08:10:00Z',
      locationIdentifier: '1001',
      locationDescription: 'Main St',
      averageDuration: '00:00:30',
      numberCheckins: 2,
      numberCheckouts: 2,
      numberEarlyGreens: 1,
      numberExtendedGreens: 1,
      unassigned: {
        earlyGreen: [{ timestamp: '2026-04-07T08:03:00Z', value: 2 }],
        extendGreen: [{ timestamp: '2026-04-07T08:04:00Z', value: 3 }],
      },
      cycles: [
        {
          locationIdentifier: '1001',
          tspNumber: 2,
          checkIn: '2026-04-07T08:00:00Z',
          checkOut: '2026-04-07T08:01:00Z',
          requestEndOffsetSec: 60,
        },
        {
          locationIdentifier: '1001',
          tspNumber: 1,
          checkIn: '2026-04-07T08:05:00Z',
          checkOut: '2026-04-07T08:06:00Z',
          requestEndOffsetSec: 60,
        },
      ],
    }

    const transformed = transformPrioritySummaryData(response)

    expect(transformed.data.charts).toHaveLength(4)

    const titles = transformed.data.charts.map((chartWrapper) => {
      const title = chartWrapper.chart.title
      return Array.isArray(title) ? title[0]?.text : title?.text
    })

    expect(titles).toEqual([
      'Priority Summary',
      'Priority Summary - TSP 1',
      'Priority Summary - TSP 2',
      'Priority Summary - TSP 3',
    ])

    const combinedSeries = transformed.data.charts[0].chart.series
    expect(Array.isArray(combinedSeries)).toBe(true)

    const combinedRequestBar = combinedSeries?.find(
      (entry) => entry.type === 'bar' && entry.stack == null
    )

    expect(combinedRequestBar?.data).toHaveLength(2)

    const getSeries = (chartIndex: number, name: string) => {
      const series = transformed.data.charts[chartIndex].chart.series
      expect(Array.isArray(series)).toBe(true)
      return series?.find((entry) => entry.name === name)
    }

    const combinedEarlyGreen = getSeries(
      0,
      'Unassociated Early Green (113)'
    )
    const combinedExtendGreen = getSeries(
      0,
      'Unassociated Extend Green (114)'
    )

    expect(combinedEarlyGreen?.data).toEqual([
      ['2026-04-07T08:03:00Z', 0, 2],
    ])
    expect(combinedExtendGreen?.data).toEqual([
      ['2026-04-07T08:04:00Z', 0, 3],
    ])
    expect(combinedEarlyGreen?.z).toBe(100000)
    expect(combinedExtendGreen?.z).toBe(100000)
    expect(combinedEarlyGreen?.zlevel).toBe(1)
    expect(combinedExtendGreen?.zlevel).toBe(1)
    expect(combinedEarlyGreen?.itemStyle).toMatchObject({
      borderColor: '#000000',
      borderWidth: 1,
      opacity: 1,
    })
    expect(combinedExtendGreen?.itemStyle).toMatchObject({
      borderColor: '#000000',
      borderWidth: 1,
      opacity: 1,
    })

    expect(getSeries(1, 'Unassociated Early Green (113)')).toBeUndefined()
    expect(getSeries(1, 'Unassociated Extend Green (114)')).toBeUndefined()
    expect(getSeries(2, 'Unassociated Early Green (113)')?.data).toEqual([
      ['2026-04-07T08:03:00Z', 0, 2],
    ])
    expect(getSeries(2, 'Unassociated Extend Green (114)')).toBeUndefined()
    expect(getSeries(3, 'Unassociated Early Green (113)')).toBeUndefined()
    expect(getSeries(3, 'Unassociated Extend Green (114)')?.data).toEqual([
      ['2026-04-07T08:04:00Z', 0, 3],
    ])
  })
})
