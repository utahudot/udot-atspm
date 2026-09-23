// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - transformers.test.ts
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
import { createDataZoom } from './transformers'

describe('createDataZoom', () => {
  it('disables data shadows for horizontal sliders by default', () => {
    const dataZoom = createDataZoom()
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

  it('keeps horizontal slider defaults when a vertical slider is added', () => {
    const dataZoom = createDataZoom([
      {
        type: 'slider',
        orient: 'vertical',
        right: 220,
      },
    ])

    expect(dataZoom[0]).toMatchObject({
      type: 'slider',
      xAxisIndex: 0,
      bottom: 15,
      height: 30,
      showDataShadow: false,
    })
    expect(dataZoom[2]).toMatchObject({
      type: 'slider',
      orient: 'vertical',
      right: 220,
    })
  })
})
