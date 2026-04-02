import { Color } from '@/features/charts/utils'
import type { GridComponentOption } from 'echarts'
import type { RawTimeSpaceAverageData } from '../types'
import {
  formatSignedOffsetSeconds,
  generateCycleLabels,
  getLocationsLabelOption,
  getOffsetDeltaVisuals,
} from './timeSpaceTransformerBase'

let originalCanvasGetContext: typeof HTMLCanvasElement.prototype.getContext

function stripRichText(text: string): string {
  return text.replace(/\{[^|}]+\|([^}]+)\}/g, '$1')
}

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
    typeof candidate.style?.text === 'string'
      ? [stripRichText(candidate.style.text)]
      : []
  const childTexts = Array.isArray(candidate.children)
    ? candidate.children.flatMap((child) => collectRenderTexts(child))
    : []

  return [...ownText, ...childTexts]
}

function findTextStyle(
  node: unknown,
  text: string
): {
  fill?: unknown
  fontSize?: unknown
  fontWeight?: unknown
  rich?: unknown
} | null {
  if (!node || typeof node !== 'object') {
    return null
  }

  const candidate = node as {
    children?: unknown[]
    style?: {
      text?: unknown
      fill?: unknown
      fontSize?: unknown
      fontWeight?: unknown
      rich?: unknown
    }
  }

  if (candidate.style?.text === text) {
    return {
      fill: candidate.style.fill,
      fontSize: candidate.style.fontSize,
      fontWeight: candidate.style.fontWeight,
      rich: candidate.style.rich,
    }
  }

  if (!Array.isArray(candidate.children)) {
    return null
  }

  for (const child of candidate.children) {
    const match = findTextStyle(child, text)
    if (match) {
      return match
    }
  }

  return null
}

function renderLocationCardNode(location: RawTimeSpaceAverageData): unknown {
  const series = getLocationsLabelOption([location], [0], {
    left: 100,
  } as GridComponentOption) as {
    data?: unknown[]
    renderItem?: (
      params: {
        dataIndex: number
        dataIndexInside: number
        dataInsideLength: number
      },
      api: {
        coord: (value: unknown[]) => [number, number]
        value: (index: number, dataIndex?: number) => unknown
      }
    ) => unknown
  }

  const dataPoint = Array.isArray(series.data)
    ? (series.data[0] as unknown[])
    : []
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

  return renderResult
}

function renderLocationCardTexts(location: RawTimeSpaceAverageData): string[] {
  return collectRenderTexts(renderLocationCardNode(location))
}

function renderCycleLabelNode(
  isIgnored: boolean,
  {
    column = 'right',
    distanceData = [0],
    rowIndex = 0,
    headerTexts,
  }: {
    column?: 'left' | 'right'
    distanceData?: number[]
    rowIndex?: number
    headerTexts?: string[]
  } = {}
): unknown {
  const series = generateCycleLabels(
    distanceData,
    'NB',
    0,
    headerTexts ?? distanceData.map(() => 'NB'),
    distanceData.map(() => ['AOG: 55%']),
    column,
    distanceData.map(() => isIgnored)
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

  const dataPoint =
    Array.isArray(series.data) && series.data.length > rowIndex
      ? series.data[rowIndex]
      : 0
  const renderResult = series.renderItem?.(
    {
      dataIndex: rowIndex,
      coordSys: { x: 0, width: 100 },
    },
    {
      coord: (value) => [0, Number(value[1] ?? 0)],
      value: () => dataPoint,
    }
  )

  return renderResult
}

function renderCycleLabelTexts(
  isIgnored: boolean,
  options?: {
    column?: 'left' | 'right'
    distanceData?: number[]
    rowIndex?: number
  }
): string[] {
  return collectRenderTexts(renderCycleLabelNode(isIgnored, options))
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

  it('renders unknown when the cycle length is missing', () => {
    const texts = renderLocationCardTexts(
      buildLocation({
        cycleLength: null,
      })
    )

    expect(texts).toContain('Cycle')
    expect(texts).toContain('unknown')
  })

  it('omits phase-card detail lines for ignored locations', () => {
    const visibleTexts = renderCycleLabelTexts(false)
    const hiddenTexts = renderCycleLabelTexts(true)

    expect(visibleTexts).toContain('NB')
    expect(visibleTexts).toContain('AOG')
    expect(visibleTexts).toContain('55%')
    expect(hiddenTexts).toContain('NB')
    expect(hiddenTexts).not.toContain('AOG')
    expect(hiddenTexts).not.toContain('55%')
  })

  it('styles the AOG label like the location-card metric labels', () => {
    const cycleMetricStyle = findTextStyle(
      renderLocationCardNode(buildLocation()),
      'Cycle'
    )
    const aogStyle = findTextStyle(renderCycleLabelNode(false), 'AOG')

    expect(cycleMetricStyle).toMatchObject({
      fill: '#64748B',
      fontSize: 11,
      fontWeight: 500,
    })
    expect(aogStyle).toMatchObject({
      fill: '#64748B',
      fontSize: 10,
      fontWeight: 500,
    })
  })

  it('styles phase-card identifier/name headers like location-card titles', () => {
    const headerStyle = findTextStyle(
      renderCycleLabelNode(false, {
        headerTexts: ['6192 - 200 S & State St'],
      }),
      '{ident|6192}{name| - 200 S & State St}'
    )

    expect(headerStyle).toMatchObject({
      fontSize: 10,
      rich: {
        ident: {
          fontWeight: 700,
        },
        name: {
          fontWeight: 400,
        },
      },
    })
  })

  it('does not render a primary footer inside the phase card series', () => {
    const texts = renderCycleLabelTexts(false, {
      column: 'left',
      distanceData: [0, 100],
      rowIndex: 1,
    })

    expect(texts).not.toContain('primary')
  })

  it('does not render an opposing footer inside the phase card series', () => {
    const texts = renderCycleLabelTexts(false, {
      column: 'right',
      distanceData: [0, 100],
      rowIndex: 1,
    })

    expect(texts).not.toContain('opposing')
  })
})
