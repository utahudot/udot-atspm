// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceResultData.test.ts
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
import { ToolType } from '@/features/charts/common/types'
import type {
  RawTimeSpaceDiagramResponse,
  RawTimeSpaceHistoricData,
  TimeSpaceBaseData,
  TimeSpaceDiagramPhaseResult,
  TimeSpaceSrmPhaseOverlay,
} from '../../shared/types'
jest.mock('nanoid', () => ({
  nanoid: () => 'test-entry-id',
}))

import {
  addDefaultTimeSpaceValues,
  canFetchLinkPivotForTimeSpace,
  createEmptyTimeSpaceEntry,
  getPrimaryTimeSpaceLocations,
  mergeSrmOverlaysIntoWrappedData,
  recomputeTimeSpaceData,
  recomputeWrappedTimeSpaceData,
  supportsLinkPivotForTimeSpace,
} from './timeSpaceResultData'

type LaneDatum = TimeSpaceBaseData & {
  description?: string
}

function buildLaneDatum(
  locationIdentifier: string,
  phaseType: 'Primary' | 'Opposing',
  distanceToNextLocation: number,
  overrides: Partial<LaneDatum> = {}
): LaneDatum {
  return {
    start: '2026-04-23T08:00:00Z',
    end: '2026-04-23T09:00:00Z',
    locationIdentifier,
    locationDescription: `${locationIdentifier} description`,
    phaseType,
    distanceToNextLocation,
    distanceToPreviousLocation: 0,
    phaseNumber: 2,
    phaseNumberSort: '2',
    speed: 35,
    approachId: 1,
    approachDescription: `${phaseType} approach`,
    calculatedDistanceToNext: distanceToNextLocation,
    calculatedDistanceToPrevious: 0,
    isIgnoredLocation: false,
    ...overrides,
  }
}

function buildHistoricLocation(
  locationIdentifier: string,
  phaseType: 'Primary' | 'Opposing',
  order: number,
  overrides: Partial<RawTimeSpaceHistoricData> = {}
): RawTimeSpaceHistoricData {
  return {
    start: '2026-04-23T08:00:00Z',
    end: '2026-04-23T09:00:00Z',
    locationIdentifier,
    locationDescription: `${locationIdentifier} description`,
    phaseNumber: phaseType === 'Primary' ? 2 : 6,
    phaseNumberSort: phaseType === 'Primary' ? '2' : '6',
    distanceToNextLocation: 100,
    distanceToPreviousLocation: 0,
    speed: 35,
    approachId: 1,
    approachDescription: `${phaseType} approach`,
    phaseType,
    calculatedDistanceToNext: 100,
    calculatedDistanceToPrevious: 0,
    isIgnoredLocation: false,
    greenTimeEvents: [],
    laneByLaneCountDetectors: [],
    advanceCountDetectors: [],
    stopBarPresenceDetectors: [],
    cycleAllEvents: null,
    pedestrianIntervals: [],
    percentArrivalOnGreen: 50,
    tmcForPhase: {
      leftTurnEvents: [],
      rightTurnEvents: [],
    },
    order,
    cycleLength: 120,
    isPhaseOverLap: false,
    tspNumberCheckins: 0,
    tspNumberCheckouts: 0,
    tspNumberEarlyGreens: 0,
    tspNumberExtendedGreens: 0,
    tspEvents: [],
    priorityAndPreemptionEvents: [],
    srmEntityTracks: [],
    ...overrides,
  }
}

describe('timeSpaceResultData', () => {
  it('creates an empty GPX entry from the first and last locations', () => {
    const entry = createEmptyTimeSpaceEntry(['1000', '2000', '3000'], true)

    expect(entry).toMatchObject({
      startLocation: '1000',
      endLocation: '3000',
      primary: true,
      error: null,
    })
    expect(typeof entry.id).toBe('string')
    expect(entry.id.length).toBeGreaterThan(0)
  })

  it('only allows link-pivot fetches for historic runs', () => {
    expect(canFetchLinkPivotForTimeSpace(ToolType.TimeSpaceAverage)).toBe(false)
    expect(canFetchLinkPivotForTimeSpace(ToolType.TimeSpaceHistoric)).toBe(true)
  })

  it('only shows link-pivot UI for historic results', () => {
    expect(supportsLinkPivotForTimeSpace(ToolType.TimeSpaceAverage)).toBe(false)
    expect(supportsLinkPivotForTimeSpace(ToolType.TimeSpaceHistoric)).toBe(true)
  })

  it('recomputes distances around ignored locations separately for each direction lane', () => {
    const recomputed = recomputeTimeSpaceData<LaneDatum>(
      [
        buildLaneDatum('P1', 'Primary', 100),
        buildLaneDatum('P2', 'Primary', 200),
        buildLaneDatum('P3', 'Primary', 300),
        buildLaneDatum('O1', 'Opposing', 50),
        buildLaneDatum('O2', 'Opposing', 75),
      ],
      ['P2', 'O1']
    )

    expect(recomputed).toEqual([
      expect.objectContaining({
        locationIdentifier: 'P1',
        calculatedDistanceToNext: 300,
        calculatedDistanceToPrevious: 0,
        isIgnoredLocation: false,
      }),
      expect.objectContaining({
        locationIdentifier: 'P2',
        calculatedDistanceToNext: 0,
        calculatedDistanceToPrevious: 0,
        isIgnoredLocation: true,
      }),
      expect.objectContaining({
        locationIdentifier: 'P3',
        calculatedDistanceToNext: 0,
        calculatedDistanceToPrevious: 300,
        isIgnoredLocation: false,
      }),
      expect.objectContaining({
        locationIdentifier: 'O1',
        calculatedDistanceToNext: 0,
        calculatedDistanceToPrevious: 0,
        isIgnoredLocation: true,
      }),
      expect.objectContaining({
        locationIdentifier: 'O2',
        calculatedDistanceToNext: 0,
        calculatedDistanceToPrevious: 50,
        isIgnoredLocation: false,
      }),
    ])
  })

  it('recomputes wrapped data while preserving failed entries in place', () => {
    const wrappedData: RawTimeSpaceDiagramResponse['data'] = [
      {
        error: null,
        isSuccess: true,
        result: buildHistoricLocation('P1', 'Primary', 1, {
          distanceToNextLocation: 100,
        }),
      },
      {
        error: 'backend failed',
        isSuccess: false,
        result: null,
      },
      {
        error: null,
        isSuccess: true,
        result: buildHistoricLocation('P2', 'Primary', 2, {
          distanceToNextLocation: 200,
        }),
      },
      {
        error: null,
        isSuccess: true,
        result: buildHistoricLocation('P3', 'Primary', 3, {
          distanceToNextLocation: 300,
        }),
      },
    ]

    const recomputed = recomputeWrappedTimeSpaceData(wrappedData, ['P2'])

    expect(recomputed[0]).toMatchObject({
      isSuccess: true,
      result: expect.objectContaining({
        locationIdentifier: 'P1',
        calculatedDistanceToNext: 300,
        isIgnoredLocation: false,
      }),
    })
    expect(recomputed[1]).toEqual(wrappedData[1])
    expect(recomputed[2]).toMatchObject({
      isSuccess: true,
      result: expect.objectContaining({
        locationIdentifier: 'P2',
        calculatedDistanceToNext: 0,
        calculatedDistanceToPrevious: 0,
        isIgnoredLocation: true,
      }),
    })
    expect(recomputed[3]).toMatchObject({
      isSuccess: true,
      result: expect.objectContaining({
        locationIdentifier: 'P3',
        calculatedDistanceToNext: 0,
        calculatedDistanceToPrevious: 300,
        isIgnoredLocation: false,
      }),
    })
  })

  it('merges SRM overlays by location, phase type, and order only', () => {
    const wrappedData: TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>[] = [
      {
        error: null,
        isSuccess: true,
        result: buildHistoricLocation('P1', 'Primary', 1),
      },
      {
        error: null,
        isSuccess: true,
        result: buildHistoricLocation('P1', 'Primary', 2),
      },
      {
        error: 'failed',
        isSuccess: false,
        result: null,
      },
    ]
    const overlays: TimeSpaceSrmPhaseOverlay[] = [
      {
        locationIdentifier: 'P1',
        phaseType: 'Primary',
        order: 2,
        srmEntityTracks: [
          {
            entityId: 'bus-7',
            points: [],
          },
        ],
      },
    ]

    const merged = mergeSrmOverlaysIntoWrappedData(wrappedData, overlays)

    expect(merged[0]).toMatchObject({
      isSuccess: true,
      result: expect.objectContaining({
        srmEntityTracks: [],
      }),
    })
    expect(merged[1]).toMatchObject({
      isSuccess: true,
      result: expect.objectContaining({
        srmEntityTracks: [
          {
            entityId: 'bus-7',
            points: [],
          },
        ],
      }),
    })
    expect(merged[2]).toEqual(wrappedData[2])
  })

  it('adds default calculated distances and ignored flags to successful results only', () => {
    const response: RawTimeSpaceDiagramResponse = {
      type: ToolType.TimeSpaceHistoric,
      data: [
        {
          error: null,
          isSuccess: true,
          result: buildHistoricLocation('P1', 'Primary', 1, {
            calculatedDistanceToNext: 0,
            calculatedDistanceToPrevious: 0,
            isIgnoredLocation: true,
            distanceToNextLocation: 150,
            distanceToPreviousLocation: 25,
          }),
        },
        {
          error: 'failed',
          isSuccess: false,
          result: null,
        },
      ],
    }

    const nextResponse = addDefaultTimeSpaceValues(response)

    expect(nextResponse.data[0]).toMatchObject({
      isSuccess: true,
      result: expect.objectContaining({
        calculatedDistanceToNext: 150,
        calculatedDistanceToPrevious: 25,
        isIgnoredLocation: false,
      }),
    })
    expect(nextResponse.data[1]).toEqual(response.data[1])
  })

  it('returns only successful primary location identifiers', () => {
    const response: RawTimeSpaceDiagramResponse = {
      type: ToolType.TimeSpaceHistoric,
      data: [
        {
          error: null,
          isSuccess: true,
          result: buildHistoricLocation('P1', 'Primary', 1),
        },
        {
          error: null,
          isSuccess: true,
          result: buildHistoricLocation('O1', 'Opposing', 1),
        },
        {
          error: 'failed',
          isSuccess: false,
          result: null,
        },
      ],
    }

    expect(getPrimaryTimeSpaceLocations(response)).toEqual(['P1'])
  })
})
