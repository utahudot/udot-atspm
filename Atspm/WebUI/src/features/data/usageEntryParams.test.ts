import type { UsageEntryFiltersState } from '@/features/data/components/UsageEntryFilters'
import { buildUsageEntryParams } from '@/pages/data/usage'

const baseFilters: UsageEntryFiltersState = {
  fromUtc: undefined,
  toUtc: undefined,
  apiName: '',
  method: '',
  success: 'all',
  statusClass: 'all',
  userId: '',
}

const odataUtcLiteral = (date: Date) =>
  date.toISOString().replace(/\.\d{3}Z$/, 'Z')

describe('buildUsageEntryParams', () => {
  it('builds UTC OData bounds from selected local usage calendar dates', () => {
    const result = buildUsageEntryParams({
      ...baseFilters,
      fromUtc: '2026-05-01T00:00:00',
      toUtc: '2026-05-21T00:00:00',
    })

    expect(result.filter).toBe(
      `Timestamp ge ${odataUtcLiteral(
        new Date(2026, 4, 1, 0, 0, 0)
      )} and Timestamp lt ${odataUtcLiteral(new Date(2026, 4, 22, 0, 0, 0))}`
    )
    expect(result.orderby).toBe('Timestamp desc')
    expect(result.count).toBe(true)
  })

  it('escapes user ids without changing timestamp bounds', () => {
    const result = buildUsageEntryParams({
      ...baseFilters,
      fromUtc: '2026-05-01T00:00:00',
      userId: "o'hara",
    })

    expect(result.filter).toBe(
      `Timestamp ge ${odataUtcLiteral(
        new Date(2026, 4, 1, 0, 0, 0)
      )} and UserId eq 'o''hara'`
    )
  })
})
