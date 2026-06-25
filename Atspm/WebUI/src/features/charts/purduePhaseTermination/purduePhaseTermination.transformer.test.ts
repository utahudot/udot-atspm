// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - purduePhaseTermination.transformer.test.ts
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
import { ChartType } from '@/features/charts/common/types'
import type { DataZoomComponentOption } from 'echarts'
import transformPurduePhaseTerminationData from './purduePhaseTermination.transformer'
import type { RawPurduePhaseTerminationResponse } from './types'

describe('transformPurduePhaseTerminationData', () => {
  it('uses the shared horizontal data zoom defaults', () => {
    const response: RawPurduePhaseTerminationResponse = {
      type: ChartType.PurduePhaseTermination,
      data: {
        locationIdentifier: '1123',
        locationDescription: '#1123 - Wolcott St/ 1455 E & 100 South',
        start: '2026-04-08T00:00:00',
        end: '2026-04-09T00:00:00',
        consecutiveCount: 1,
        plans: [
          {
            planNumber: '1',
            planDescription: 'Plan 1',
            start: '2026-04-08T05:30:00',
            end: '2026-04-08T07:00:00',
          },
        ],
        phases: [
          {
            phaseNumber: 2,
            gapOuts: [],
            maxOuts: [],
            forceOffs: [],
            pedWalkBegins: [],
            unknownTerminations: [],
          },
          {
            phaseNumber: 4,
            gapOuts: [],
            maxOuts: [],
            forceOffs: [],
            pedWalkBegins: [],
            unknownTerminations: ['2026-04-08T11:40:00'],
          },
        ],
      },
    }

    const result = transformPurduePhaseTerminationData(response)
    const chart = result.data.charts[0].chart
    const dataZoom = chart.dataZoom as DataZoomComponentOption[]
    const horizontalSlider = dataZoom.find(
      (zoom) =>
        zoom.type === 'slider' && (zoom.orient ?? 'horizontal') === 'horizontal'
    )

    expect(horizontalSlider).toMatchObject({
      xAxisIndex: 0,
      bottom: 15,
      height: 30,
      showDataShadow: false,
    })
  })
})
