// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - utils.test.ts
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
