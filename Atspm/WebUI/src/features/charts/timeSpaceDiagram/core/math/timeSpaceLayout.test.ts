// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceLayout.test.ts
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
  getDistanceAtDisplayCoordinate,
  getHybridDistanceData,
  getSequenceDistanceData,
  TIME_SPACE_HYBRID_BIN_STEP,
  TIME_SPACE_MIN_SEGMENT,
} from './timeSpaceLayout'

describe('timeSpaceLayout distance spacing', () => {
  it('returns equal row spacing for sequence distance data', () => {
    expect(getSequenceDistanceData(4)).toEqual([
      0,
      TIME_SPACE_MIN_SEGMENT,
      TIME_SPACE_MIN_SEGMENT * 2,
      TIME_SPACE_MIN_SEGMENT * 3,
    ])
  })

  it('maps raw segment distances into hybrid short, medium, long, and extra-long buckets', () => {
    const shortest = 300
    const rawDistanceData = [
      0,
      shortest,
      shortest + shortest + TIME_SPACE_HYBRID_BIN_STEP,
      shortest + shortest + TIME_SPACE_HYBRID_BIN_STEP * 3,
      shortest + shortest + TIME_SPACE_HYBRID_BIN_STEP * 6,
      shortest + shortest + TIME_SPACE_HYBRID_BIN_STEP * 10,
    ]
    const displayDistanceData = getHybridDistanceData(rawDistanceData)
    const displaySegments = displayDistanceData.map((distance, index) =>
      index === 0 ? 0 : distance - displayDistanceData[index - 1]
    )
    const baseSegment = Math.max(shortest, TIME_SPACE_MIN_SEGMENT)

    expect(displaySegments).toEqual([
      0,
      baseSegment,
      baseSegment,
      baseSegment + TIME_SPACE_HYBRID_BIN_STEP,
      baseSegment + TIME_SPACE_HYBRID_BIN_STEP * 2.5,
      baseSegment + TIME_SPACE_HYBRID_BIN_STEP * 4,
    ])
  })

  it('falls back to sequence spacing when there are no positive hybrid segments', () => {
    expect(getHybridDistanceData([0, 0, 0])).toEqual(
      getSequenceDistanceData(3)
    )
  })

  it('interpolates and extrapolates display coordinates from raw distance coordinates', () => {
    const rawDistanceData = [0, 100, 300]
    const displayDistanceData = [0, 1000, 1500]

    expect(
      getDistanceAtDisplayCoordinate(
        rawDistanceData,
        displayDistanceData,
        50
      )
    ).toBe(500)
    expect(
      getDistanceAtDisplayCoordinate(
        rawDistanceData,
        displayDistanceData,
        200
      )
    ).toBe(1250)
    expect(
      getDistanceAtDisplayCoordinate(
        rawDistanceData,
        displayDistanceData,
        -50
      )
    ).toBe(-500)
  })
})
