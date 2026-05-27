import type { UsageEntry } from '@/api/config'
import { formatInstantAsLocalDate } from '@/utils/dateTime'
import {
  buildByTime,
  formatUsageLocalDateRange,
} from './utils'

describe('ReportApiInsightsCard usage time helpers', () => {
  it('groups usage rows by local day', () => {
    const instant = '2026-05-21T02:30:00+00:00'
    const rows: UsageEntry[] = [
      {
        id: 1,
        apiName: 'ReportApi',
        timestamp: instant,
        success: true,
      },
      {
        id: 2,
        apiName: 'ReportApi',
        timestamp: '2026-05-20T20:30:00-06:00',
        success: true,
      },
    ]

    expect(
      buildByTime(rows, { groupBy: 'day', metric: 'ReportsGenerated' })
    ).toEqual([{ name: formatInstantAsLocalDate(instant), count: 2 }])
  })

  it('formats usage date ranges with local display semantics', () => {
    expect(
      formatUsageLocalDateRange(
        '2026-05-01T00:00:00',
        '2026-05-21T00:00:00'
      )
    ).toBe(
      `${formatInstantAsLocalDate(
        '2026-05-01T00:00:00'
      )} - ${formatInstantAsLocalDate('2026-05-21T00:00:00')}`
    )
  })
})
