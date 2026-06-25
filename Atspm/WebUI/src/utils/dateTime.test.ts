// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - dateTime.test.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
