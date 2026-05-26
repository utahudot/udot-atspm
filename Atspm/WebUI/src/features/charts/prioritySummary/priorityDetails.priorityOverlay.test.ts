// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - priorityDetails.priorityOverlay.test.ts
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
import type { PriorityDetailsResult } from '@/api/reports'
import {
  buildPriorityOverlay,
  type RectDatum,
} from './priorityDetails.priorityOverlay'

describe('buildPriorityOverlay', () => {
  it('uses 112 as the request range start instead of rendering a separate marker', () => {
    const rows = [
      {
        tspEvents: [
          {
            eventCode: 112,
            eventParam: 1,
            timestamp: '2026-04-07T08:00:00Z',
          },
          {
            eventCode: 113,
            eventParam: 1,
            timestamp: '2026-04-07T08:00:05Z',
          },
          {
            eventCode: 114,
            eventParam: 1,
            timestamp: '2026-04-07T08:00:10Z',
          },
          {
            eventCode: 115,
            eventParam: 1,
            timestamp: '2026-04-07T08:01:00Z',
          },
        ],
      },
    ] as PriorityDetailsResult[]

    const { series } = buildPriorityOverlay(rows)
    const requestSeries = series.find((entry) =>
      String(entry.name).startsWith('TSP Request')
    )
    const requestData = (requestSeries?.data ?? []) as RectDatum[]

    expect(series.some((entry) => entry.name === 'Check In (112)')).toBe(false)
    expect(requestData).toHaveLength(1)
    expect(requestData[0].value[0]).toBe(0)
    expect(requestData[0].value[3]).toBe(60_000)
    expect(
      series.some((entry) => entry.name === 'Early Green (113)')
    ).toBe(true)
    expect(
      series.some((entry) => entry.name === 'Extend Green (114)')
    ).toBe(true)
  })
})
