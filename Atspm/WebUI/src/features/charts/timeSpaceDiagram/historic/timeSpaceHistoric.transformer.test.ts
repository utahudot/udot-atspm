import { ToolType } from '@/features/charts/common/types'
import type { EChartsOption, SeriesOption } from 'echarts'
import transformTimeSpaceHistoricData from './timeSpaceHistoric.transformer'
import type {
  RawTimeSpaceDiagramResponse,
  RawTimeSpaceHistoricData,
} from '../shared/types'

function buildHistoricLocation(
  phaseType: 'Primary' | 'Opposing',
  overrides: Partial<RawTimeSpaceHistoricData> = {}
): RawTimeSpaceHistoricData {
  return {
    start: '2026-04-07T08:00:00Z',
    end: '2026-04-07T09:00:00Z',
    locationIdentifier:
      phaseType === 'Primary' ? 'primary-location' : 'opposing-location',
    locationDescription: `${phaseType} location`,
    phaseNumber: phaseType === 'Primary' ? 2 : 6,
    phaseNumberSort: phaseType === 'Primary' ? '2' : '6',
    distanceToNextLocation: 100,
    distanceToPreviousLocation: 0,
    speed: 35,
    approachId: phaseType === 'Primary' ? 1 : 2,
    approachDescription: phaseType === 'Primary' ? 'NBT ph2' : 'SBT ph6',
    phaseType,
    calculatedDistanceToNext: 100,
    calculatedDistanceToPrevious: 0,
    isIgnoredLocation: false,
    greenTimeEvents: [],
    laneByLaneCountDetectors: [
      {
        distanceToStopBar: 40,
        detectorOn: new Date('2026-04-07T08:10:00Z'),
        detectorOff: new Date('2026-04-07T08:10:05Z'),
      },
    ],
    advanceCountDetectors: [
      {
        distanceToStopBar: 60,
        detectorOn: new Date('2026-04-07T08:15:00Z'),
        detectorOff: new Date('2026-04-07T08:15:04Z'),
      },
    ],
    stopBarPresenceDetectors: [
      {
        distanceToStopBar: 0,
        detectorOn: new Date('2026-04-07T08:20:00Z'),
        detectorOff: new Date('2026-04-07T08:20:06Z'),
      },
    ],
    cycleAllEvents: null,
    pedestrianIntervals: [],
    percentArrivalOnGreen: 50,
    tmcForPhase: {
      leftTurnEvents: [],
      rightTurnEvents: [],
    },
    order: phaseType === 'Primary' ? 1 : 2,
    cycleLength: null,
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

describe('transformTimeSpaceHistoricData detection series interaction', () => {
  it('marks detection series as non-interactable', () => {
    const response: RawTimeSpaceDiagramResponse = {
      type: ToolType.TimeSpaceHistoric,
      data: [
        {
          isSuccess: true,
          error: null,
          result: buildHistoricLocation('Primary'),
        },
        {
          isSuccess: true,
          error: null,
          result: buildHistoricLocation('Opposing'),
        },
      ],
    }

    const result = transformTimeSpaceHistoricData(response)
    const chart = result.data.chart as EChartsOption
    const series = Array.isArray(chart.series)
      ? (chart.series as SeriesOption[])
      : []

    const laneByLane = series.find((entry) =>
      String(entry.name).startsWith('Lane by Lane Count ')
    )
    const advanceCount = series.find((entry) =>
      String(entry.name).startsWith('Advance Count ')
    )
    const stopBarPresence = series.find((entry) =>
      String(entry.name).startsWith('Stop Bar Presence ')
    )

    expect(laneByLane?.silent).toBe(true)
    expect(laneByLane?.tooltip).toMatchObject({ show: false })
    expect(advanceCount?.silent).toBe(true)
    expect(advanceCount?.tooltip).toMatchObject({ show: false })
    expect(stopBarPresence?.silent).toBe(true)
    expect(stopBarPresence?.tooltip).toMatchObject({ show: false })
  })

  it('does not synthesize TSP series when no TSP events are present', () => {
    const response: RawTimeSpaceDiagramResponse = {
      type: ToolType.TimeSpaceHistoric,
      data: [
        {
          isSuccess: true,
          error: null,
          result: buildHistoricLocation('Primary'),
        },
        {
          isSuccess: true,
          error: null,
          result: buildHistoricLocation('Opposing'),
        },
      ],
    }

    const result = transformTimeSpaceHistoricData(response)
    const chart = result.data.chart as EChartsOption
    const series = Array.isArray(chart.series)
      ? (chart.series as SeriesOption[])
      : []

    expect(
      series.some((entry) => String(entry.name) === 'TSP Request (112-115)')
    ).toBe(false)
    expect(
      series.some((entry) => String(entry.name) === 'TSP Service (118-119)')
    ).toBe(false)
  })
})
