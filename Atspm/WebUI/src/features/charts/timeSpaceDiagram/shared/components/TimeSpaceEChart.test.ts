// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - TimeSpaceEChart.test.ts
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
import type { ECharts, EChartsOption } from 'echarts'
import { buildOffsetResetButtons } from './TimeSpaceEChart'

describe('buildOffsetResetButtons', () => {
  function buildChart() {
    return {
      convertToPixel: () => [50, 60],
      getModel: () => ({
        getComponent: () => ({
          coordinateSystem: {
            getRect: () => ({
              x: 100,
              width: 200,
            }),
          },
        }),
      }),
    } as unknown as ECharts
  }

  it('returns reset buttons only when the offset row is actually modified', () => {
    const option: EChartsOption = {
      series: [
        {
          id: 'Location axis',
          name: 'Location axis',
          type: 'custom',
          data: [
            ['2026-03-20T00:00:00Z', 10, '6192', 'Main', 132, 89, 88, 1],
            ['2026-03-20T00:00:00Z', 20, '7001', 'Main', 132, 93, 93, 0],
            ['2026-03-20T00:00:00Z', 30, '8123', 'Main', 120, 93, 93, 120],
          ],
        },
      ],
    }

    const buttons = buildOffsetResetButtons(buildChart(), option)

    expect(buttons).toHaveLength(3)
    expect(buttons.map((button) => [button.location, button.active])).toEqual([
      ['6192', true],
      ['7001', false],
      ['8123', true],
    ])
  })
})
