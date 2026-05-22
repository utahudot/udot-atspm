import {
  dateToTimestamp,
  formatInstantAsLocalDate,
  formatInstantAsLocalDateTime,
  localDateTimeToUtcODataLiteral,
  parseWallClockDateTimeLiteral,
  toWallClockDateTimeLiteral,
} from './dateTime'

const pad2 = (n: number) => String(n).padStart(2, '0')

const localDateStamp = (date: Date) =>
  `${date.getFullYear()}-${pad2(date.getMonth() + 1)}-${pad2(date.getDate())}`

const localDateTimePrefix = (date: Date) =>
  `${localDateStamp(date)} ${pad2(date.getHours())}:${pad2(
    date.getMinutes()
  )}:${pad2(date.getSeconds())}`

const odataUtcLiteral = (date: Date) =>
  date.toISOString().replace(/\.\d{3}Z$/, 'Z')

describe('date/time policy helpers', () => {
  it('preserves wall-clock traffic time without converting offsets', () => {
    expect(toWallClockDateTimeLiteral('2026-05-21T08:00:00Z')).toBe(
      '2026-05-21T08:00:00'
    )
    expect(dateToTimestamp('2026-05-21T08:00:00-06:00')).toBe(
      '2026-05-21T08:00:00'
    )
  })

  it('formats equivalent system instants as the same local display value', () => {
    const instant = new Date('2026-05-21T02:30:00+00:00')

    expect(formatInstantAsLocalDate('2026-05-21T02:30:00+00:00')).toBe(
      localDateStamp(instant)
    )
    expect(formatInstantAsLocalDate('2026-05-20T20:30:00-06:00')).toBe(
      localDateStamp(instant)
    )
    expect(formatInstantAsLocalDateTime('2026-05-20T20:30:00-06:00')).toMatch(
      new RegExp(`^${localDateTimePrefix(instant)} `)
    )
  })

  it('builds UTC OData literals from selected local calendar dates', () => {
    expect(localDateTimeToUtcODataLiteral('2026-05-21T00:00:00')).toBe(
      odataUtcLiteral(new Date(2026, 4, 21, 0, 0, 0))
    )
    expect(
      localDateTimeToUtcODataLiteral(new Date(2026, 4, 21, 8, 0, 0))
    ).toBe(
      odataUtcLiteral(new Date(2026, 4, 21, 8, 0, 0))
    )
  })

  it('parses wall-clock values into matching local date parts', () => {
    const parsed = parseWallClockDateTimeLiteral('2026-05-21T08:00:00Z')

    expect(parsed?.getFullYear()).toBe(2026)
    expect(parsed?.getMonth()).toBe(4)
    expect(parsed?.getDate()).toBe(21)
    expect(parsed?.getHours()).toBe(8)
  })
})
