import { ToolType } from '@/features/charts/common/types'
import type { EChartsOption, SeriesOption } from 'echarts'
import type {
  RawTimeSpaceAverageData,
  RawTimeSpaceDiagramResponse,
} from '../shared/types'
import transformTimeSpaceAverageData from './timeSpaceAverage.transformer'

function buildAverageLocation(
  phaseType: 'Primary' | 'Opposing',
  overrides: Partial<RawTimeSpaceAverageData> = {}
): RawTimeSpaceAverageData {
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
    approachDescription: phaseType === 'Primary' ? 'NB Through' : 'SB Through',
    phaseType,
    order: phaseType === 'Primary' ? 0 : 0,
    calculatedDistanceToNext: 100,
    calculatedDistanceToPrevious: 0,
    isIgnoredLocation: false,
    offset: 0,
    cycleLength: 120,
    programmedSplit: 60,
    coordinatedPhases: true,
    greenTimeEvents: [],
    cycleAllEvents: [
      { start: '2026-04-07T08:00:00Z', value: 1 },
      { start: '2026-04-07T08:00:30Z', value: 3 },
      { start: '2026-04-07T08:01:00Z', value: 8 },
      { start: '2026-04-07T08:01:30Z', value: 9 },
      { start: '2026-04-07T08:02:00Z', value: 11 },
    ],
    ...overrides,
  }
}

describe('transformTimeSpaceAverageData', () => {
  it('returns transformed chart data when wrapped results are successful', () => {
    const response: RawTimeSpaceDiagramResponse = {
      type: ToolType.TimeSpaceAverage,
      data: [
        {
          isSuccess: true,
          error: null,
          result: buildAverageLocation('Primary'),
        },
        {
          isSuccess: true,
          error: null,
          result: buildAverageLocation('Opposing'),
        },
      ],
    }

    const result = transformTimeSpaceAverageData(response)

    expect(result.type).toBe(ToolType.TimeSpaceAverage)
    expect(result.data.chart).toBeDefined()
  })

  it('returns errors and empty chart when all wrapped results fail', () => {
    const response: RawTimeSpaceDiagramResponse = {
      type: ToolType.TimeSpaceAverage,
      data: [
        {
          isSuccess: false,
          error: 'Primary failed',
          result: null,
        },
        {
          isSuccess: false,
          error: 'Opposing failed',
          result: null,
        },
      ],
    }

    const result = transformTimeSpaceAverageData(response)

    expect(result.type).toBe(ToolType.TimeSpaceAverage)
    expect(result.errors).toEqual(['Primary failed', 'Opposing failed'])
    expect(result.data.chart).toEqual({})
  })

  it('creates 3 cycle-related series per non-ignored location (rail, cycles, duration labels)', () => {
    const response: RawTimeSpaceDiagramResponse = {
      type: ToolType.TimeSpaceAverage,
      data: [
        {
          isSuccess: true,
          error: null,
          result: buildAverageLocation('Primary'),
        },
        {
          isSuccess: true,
          error: null,
          result: buildAverageLocation('Opposing'),
        },
      ],
    }

    const result = transformTimeSpaceAverageData(response)
    const chart = result.data.chart as EChartsOption
    const series = Array.isArray(chart.series)
      ? (chart.series as SeriesOption[])
      : []

    const primaryCycleSeries = series.filter((entry) =>
      String(entry.id).includes('primary')
    )
    const opposingCycleSeries = series.filter((entry) =>
      String(entry.id).includes('opposing')
    )

    expect(
      primaryCycleSeries.some((entry) =>
        String(entry.id).startsWith('Cycle Rail ')
      )
    ).toBe(true)
    expect(
      primaryCycleSeries.some((entry) => String(entry.id).startsWith('Cycles '))
    ).toBe(true)
    expect(
      primaryCycleSeries.some((entry) =>
        String(entry.id).startsWith('Cycle Duration Labels ')
      )
    ).toBe(true)

    expect(
      opposingCycleSeries.some((entry) =>
        String(entry.id).startsWith('Cycle Rail ')
      )
    ).toBe(true)
    expect(
      opposingCycleSeries.some((entry) =>
        String(entry.id).startsWith('Cycles ')
      )
    ).toBe(true)
    expect(
      opposingCycleSeries.some((entry) =>
        String(entry.id).startsWith('Cycle Duration Labels ')
      )
    ).toBe(true)
  })
})
