import type { GridComponentOption } from 'echarts'
import { Color } from '@/features/charts/utils'
import type { RawTimeSpaceAverageData } from '../types'
import {
  formatSignedOffsetSeconds,
  getOffsetDeltaVisuals,
  generateCycleLabels,
  getLocationsLabelOption,
} from './timeSpaceTransformerBase'

let originalCanvasGetContext: typeof HTMLCanvasElement.prototype.getContext

function buildLocation(
  overrides: Partial<RawTimeSpaceAverageData> = {}
): RawTimeSpaceAverageData {
  return {
    start: '2026-03-20T00:00:00Z',
    end: '2026-03-20T01:00:00Z',
    locationIdentifier: '6192',
    locationDescription: '200 S / 220 S & State St (US-89)',
    phaseNumber: 2,
    phaseNumberSort: '2',
    distanceToNextLocation: 100,
    distanceToPreviousLocation: 0,
    speed: 35,
    approachId: 1,
    approachDescription: 'NB',
    phaseType: 'Primary',
    calculatedDistanceToNext: 100,
    calculatedDistanceToPrevious: 0,
    isIgnoredLocation: false,
    offset: 5,
    cycleLength: 100,
    programmedSplit: 50,
    coordinatedPhases: true,
    greenTimeEvents: [],
    cycleAllEvents: null,
    ...overrides,
  }
}

function collectRenderTexts(node: unknown): string[] {
  if (!node || typeof node !== 'object') {
    return []
  }

  const candidate = node as {
    children?: unknown[]
    style?: { text?: unknown }
  }

  const ownText =
    typeof candidate.style?.text === 'string' ? [candidate.style.text] : []
  const childTexts = Array.isArray(candidate.children)
    ? candidate.children.flatMap((child) => collectRenderTexts(child))
    : []

  return [...ownText, ...childTexts]
}

function renderLocationCardTexts(location: RawTimeSpaceAverageData): string[] {
  const series = getLocationsLabelOption(
    [location],
    [0],
    { left: 100 } as GridComponentOption
  ) as {
    data?: unknown[]
    renderItem?: (
      params: { dataIndex: number; dataIndexInside: number; dataInsideLength: number },
      api: {
        coord: (value: unknown[]) => [number, number]
        value: (index: number, dataIndex?: number) => unknown
      }
    ) => unknown
  }

  const dataPoint = Array.isArray(series.data) ? (series.data[0] as unknown[]) : []
  const renderResult = series.renderItem?.(
    {
      dataIndex: 0,
      dataIndexInside: 0,
      dataInsideLength: 1,
    },
    {
      coord: (value) => [0, Number(value[1] ?? 0)],
      value: (index) => dataPoint[index],
    }
  )

  return collectRenderTexts(renderResult)
}

function renderCycleLabelTexts(isIgnored: boolean): string[] {
  const series = generateCycleLabels(
    [0],
    'NB',
    0,
    ['NB'],
    [['AOG: 55%']],
    'right',
    [isIgnored]
  ) as {
    data?: unknown[]
    renderItem?: (
      params: {
        dataIndex: number
        coordSys: { x: number; width: number }
      },
      api: {
        coord: (value: unknown[]) => [number, number]
        value: (index: number) => unknown
      }
    ) => unknown
  }

  const dataPoint = Array.isArray(series.data) ? series.data[0] : 0
  const renderResult = series.renderItem?.(
    {
      dataIndex: 0,
      coordSys: { x: 0, width: 100 },
    },
    {
      coord: (value) => [0, Number(value[1] ?? 0)],
      value: () => dataPoint,
    }
  )

  return collectRenderTexts(renderResult)
}

describe('timeSpaceTransformerBase offset formatting', () => {
  beforeAll(() => {
    originalCanvasGetContext = HTMLCanvasElement.prototype.getContext
    HTMLCanvasElement.prototype.getContext = jest.fn(() => null)
  })

  afterAll(() => {
    HTMLCanvasElement.prototype.getContext = originalCanvasGetContext
  })

  it('formats positive offsets with an explicit plus sign', () => {
    expect(formatSignedOffsetSeconds(5)).toBe('+5s')
    expect(formatSignedOffsetSeconds(0.5)).toBe('+0.5s')
  })

  it('preserves negative offsets and keeps zero neutral', () => {
    expect(formatSignedOffsetSeconds(-3)).toBe('-3s')
    expect(formatSignedOffsetSeconds(0)).toBe('0s')
    expect(formatSignedOffsetSeconds(-0.04)).toBe('0s')
  })

  it('returns sign-aware visuals for the location card', () => {
    expect(getOffsetDeltaVisuals(4, false)).toMatchObject({
      direction: 'positive',
      valueColor: Color.Green,
    })

    expect(getOffsetDeltaVisuals(-2, false)).toMatchObject({
      direction: 'negative',
      valueColor: Color.BrightRed,
    })

    expect(getOffsetDeltaVisuals(0, false)).toMatchObject({
      direction: 'neutral',
      valueColor: '#0F172A',
    })
  })

  it('omits the location card body for ignored locations', () => {
    const visibleTexts = renderLocationCardTexts(buildLocation())
    const hiddenTexts = renderLocationCardTexts(
      buildLocation({
        isIgnoredLocation: true,
        cycleLength: Number.NaN,
        offset: Number.NaN,
      })
    )

    expect(visibleTexts).toContain('Cycle')
    expect(visibleTexts).toContain('Offset')
    expect(hiddenTexts).not.toContain('Cycle')
    expect(hiddenTexts).not.toContain('Offset')
    expect(hiddenTexts.join(' ')).not.toContain('NaN')
  })

  it('omits phase-card detail lines for ignored locations', () => {
    const visibleTexts = renderCycleLabelTexts(false)
    const hiddenTexts = renderCycleLabelTexts(true)

    expect(visibleTexts).toContain('NB')
    expect(visibleTexts).toContain('AOG: 55%')
    expect(hiddenTexts).toContain('NB')
    expect(hiddenTexts).not.toContain('AOG: 55%')
  })
})
