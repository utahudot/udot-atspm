import type { PrioritySummaryResult } from '@/api/reports'
import transformPrioritySummaryData from './prioritySummary.transformer'

describe('transformPrioritySummaryData', () => {
  it('creates one chart per TSP number and places unassigned events on the baseline', () => {
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
        earlyGreen: ['2026-04-07T08:03:00Z'],
        extendGreen: ['2026-04-07T08:04:00Z'],
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

    expect(transformed.data.charts).toHaveLength(2)

    const titles = transformed.data.charts.map((chartWrapper) => {
      const title = chartWrapper.chart.title
      return Array.isArray(title) ? title[0]?.text : title?.text
    })

    expect(titles).toEqual([
      'Priority Summary - TSP 1',
      'Priority Summary - TSP 2',
    ])

    for (const chartWrapper of transformed.data.charts) {
      const series = chartWrapper.chart.series
      expect(Array.isArray(series)).toBe(true)

      const earlyGreen = series?.find(
        (entry) => entry.name === 'Unassociated Early Green (113)'
      )
      const extendGreen = series?.find(
        (entry) => entry.name === 'Unassociated Extend Green (114)'
      )

      expect(earlyGreen?.data).toEqual([['2026-04-07T08:03:00Z', 0]])
      expect(extendGreen?.data).toEqual([['2026-04-07T08:04:00Z', 0]])
      expect(earlyGreen?.z).toBe(100000)
      expect(extendGreen?.z).toBe(100000)
      expect(earlyGreen?.zlevel).toBe(1)
      expect(extendGreen?.zlevel).toBe(1)
      expect(earlyGreen?.itemStyle).toMatchObject({
        borderColor: '#FFFFFF',
        borderWidth: 2,
        opacity: 1,
      })
      expect(extendGreen?.itemStyle).toMatchObject({
        borderColor: '#FFFFFF',
        borderWidth: 2,
        opacity: 1,
      })
    }
  })
})
