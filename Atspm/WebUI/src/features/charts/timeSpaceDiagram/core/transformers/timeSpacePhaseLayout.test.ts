// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpacePhaseLayout.test.ts
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
  TIME_SPACE_HYBRID_BIN_STEP,
  TIME_SPACE_MIN_SEGMENT,
} from '../math/timeSpaceLayout'
import type { TimeSpaceCoreRow } from '../types/timeSpaceCore.types'
import { buildTimeSpacePhaseLayout } from './timeSpacePhaseLayout'

function buildRow(
  index: number,
  phaseType: TimeSpaceCoreRow['phaseType'],
  distanceToNextLocation: number
): TimeSpaceCoreRow {
  return {
    start: '2026-03-20T00:00:00Z',
    end: '2026-03-20T01:00:00Z',
    locationIdentifier: `${phaseType}-${index}`,
    locationDescription: `Location ${index}`,
    approachDescription: phaseType === 'Primary' ? 'NB' : 'SB',
    phaseType,
    distanceToNextLocation,
    calculatedDistanceToNext: distanceToNextLocation + 25,
    calculatedDistanceToPrevious: index === 0 ? 0 : distanceToNextLocation,
    isIgnoredLocation: false,
    speed: 35,
    cycleLength: 120,
    order: index,
  }
}

function buildRows() {
  const distances = [300, 1100, 0]

  return [
    ...distances.map((distance, index) => buildRow(index, 'Primary', distance)),
    ...distances.map((distance, index) =>
      buildRow(index, 'Opposing', distance)
    ),
  ]
}

function buildDirectionalRows() {
  const row = (
    phaseType: TimeSpaceCoreRow['phaseType'],
    locationIdentifier: string,
    order: number,
    distanceToNextLocation: number
  ): TimeSpaceCoreRow => ({
    ...buildRow(order, phaseType, distanceToNextLocation),
    locationIdentifier,
    approachDescription: phaseType === 'Primary' ? 'NB' : 'SB',
  })

  return [
    row('Primary', 'A', 0, 300),
    row('Primary', 'B', 1, 1100),
    row('Primary', 'C', 2, 0),
    row('Opposing', 'C', 0, 1100),
    row('Opposing', 'B', 1, 300),
    row('Opposing', 'A', 2, 0),
  ]
}

describe('buildTimeSpacePhaseLayout distance spacing modes', () => {
  it('uses actual scaled route distances in distance-based mode', () => {
    const layout = buildTimeSpacePhaseLayout(buildRows(), {
      distanceSpacingMode: 'distance',
    })

    expect(layout.rawDistanceData).toEqual([0, 300, 1400])
    expect(layout.locationCenterDistanceData).toEqual([
      0,
      TIME_SPACE_MIN_SEGMENT,
      (1400 * TIME_SPACE_MIN_SEGMENT) / 300,
    ])
  })

  it('uses equal row spacing in sequence mode', () => {
    const layout = buildTimeSpacePhaseLayout(buildRows(), {
      distanceSpacingMode: 'sequence',
    })

    expect(layout.locationCenterDistanceData).toEqual([
      0,
      TIME_SPACE_MIN_SEGMENT,
      TIME_SPACE_MIN_SEGMENT * 2,
    ])
    expect(layout.getDisplayDistanceOffset(1, -300)).toBe(
      -TIME_SPACE_MIN_SEGMENT
    )
    expect(layout.getDisplayDistanceOffset(1, 1100)).toBe(
      TIME_SPACE_MIN_SEGMENT
    )
  })

  it('uses hybrid buckets while preserving signed display offsets', () => {
    const layout = buildTimeSpacePhaseLayout(buildRows(), {
      distanceSpacingMode: 'hybrid',
    })

    expect(layout.locationCenterDistanceData).toEqual([
      0,
      TIME_SPACE_MIN_SEGMENT,
      TIME_SPACE_MIN_SEGMENT * 2 + TIME_SPACE_HYBRID_BIN_STEP,
    ])
    expect(layout.getDisplayDistanceOffset(1, -300)).toBe(
      -TIME_SPACE_MIN_SEGMENT
    )
    expect(layout.getDisplayDistanceOffset(1, 1100)).toBe(
      TIME_SPACE_MIN_SEGMENT + TIME_SPACE_HYBRID_BIN_STEP
    )
  })

  it('uses the opposing travel lane when the route is reversed', () => {
    const rows = buildDirectionalRows()
    const layout = buildTimeSpacePhaseLayout(rows, {
      routeOrientation: 'reversed',
      sortByOrder: true,
    })

    expect(
      layout.primaryPhaseData.map((location) => location.locationIdentifier)
    ).toEqual(['C', 'B', 'A'])
    expect(
      layout.opposingPhaseData.map((location) => location.locationIdentifier)
    ).toEqual(['A', 'B', 'C'])
    expect(layout.rawDistanceData).toEqual([0, 1100, 1400])
    expect(layout.primaryDirection).toBe('SB')
    expect(layout.opposingDirection).toBe('NB')
    expect(
      layout.primaryPhaseData.every(
        (location) => location.phaseType === 'Primary'
      )
    ).toBe(true)
    expect(rows.map((location) => location.phaseType)).toEqual([
      'Primary',
      'Primary',
      'Primary',
      'Opposing',
      'Opposing',
      'Opposing',
    ])
  })
})
