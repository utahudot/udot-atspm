import type { EChartsOption, SeriesOption } from 'echarts'
import {
  applyTimeSpaceAppearanceToOption,
  createDefaultTimeSpaceAppearanceSettings,
} from './timeSpaceAppearance'

type GraphicNode = {
  style?: Record<string, unknown>
  children?: GraphicNode[]
}

describe('applyTimeSpaceAppearanceToOption', () => {
  it('applies overrides to supported line and custom series', () => {
    const appearance = createDefaultTimeSpaceAppearanceSettings()
    appearance.cycles.indicationColors.beginGreen = '#123456'
    appearance.cycles.opacity = 0.55
    appearance.greenBands.primary.color = '#234567'
    appearance.greenBands.primary.opacity = 0.4
    appearance.detection.laneByLaneCount.primary.color = '#345678'
    appearance.detection.laneByLaneCount.primary.opacity = 0.65
    appearance.detection.stopBarPresence.primary.color = '#456789'
    appearance.detection.stopBarPresence.primary.opacity = 0.7
    appearance.tspRequest.color = '#56789a'
    appearance.tspRequest.opacity = 0.5

    const option: EChartsOption = {
      series: [
        {
          name: 'Lane by Lane Count EB',
          type: 'line',
          lineStyle: {
            color: '#00008B',
            opacity: 0.7,
          },
          data: [[0, 0]],
        },
        {
          name: 'TSP Request (112-115)',
          type: 'line',
          lineStyle: {
            color: '#D55E00',
            opacity: 0.95,
          },
          data: [[0, 0]],
        },
        {
          name: 'Green Bands EB',
          type: 'custom',
          renderItem: jest.fn(() => ({
            type: 'polygon',
            style: {
              fill: '#4F9BAC',
              opacity: 0.3,
            },
          })),
          data: [[0, 0]],
        },
        {
          name: 'Stop Bar Presence EB',
          type: 'custom',
          renderItem: jest.fn(() => ({
            type: 'polygon',
            style: {
              fill: '#56B4E9',
              fillOpacity: 1,
              opacity: 1,
            },
          })),
          data: [[0, 0]],
        },
        {
          name: 'Cycles EB',
          type: 'custom',
          renderItem: jest.fn(() => ({
            type: 'group',
            children: [
              {
                type: 'rect',
                style: {
                  fill: '#0CC078',
                  opacity: 1,
                },
              },
              {
                type: 'rect',
                style: {
                  fill: '#000',
                  opacity: 1,
                },
              },
            ],
          })),
          data: [[0, 0]],
        },
      ],
    }

    const directionRoleBySeriesName = new Map([
      ['Lane by Lane Count EB', 'primary' as const],
      ['Green Bands EB', 'primary' as const],
      ['Stop Bar Presence EB', 'primary' as const],
      ['Cycles EB', 'primary' as const],
    ])

    const nextOption = applyTimeSpaceAppearanceToOption(
      option,
      appearance,
      directionRoleBySeriesName
    )
    const series = nextOption.series as SeriesOption[]

    expect((series[0].lineStyle as { color?: string; opacity?: number }).color).toBe(
      '#345678'
    )
    expect(
      (series[0].lineStyle as { color?: string; opacity?: number }).opacity
    ).toBe(0.65)
    expect((series[1].lineStyle as { color?: string; opacity?: number }).color).toBe(
      '#56789a'
    )
    expect(
      (series[1].lineStyle as { color?: string; opacity?: number }).opacity
    ).toBe(0.5)

    const greenBandGraphic = (
      series[2] as SeriesOption & { renderItem: () => GraphicNode }
    ).renderItem()
    expect(greenBandGraphic.style.fill).toBe('#234567')
    expect(greenBandGraphic.style.opacity).toBe(0.4)

    const stopBarGraphic = (
      series[3] as SeriesOption & { renderItem: () => GraphicNode }
    ).renderItem()
    expect(stopBarGraphic.style.fill).toBe('#456789')
    expect(stopBarGraphic.style.fillOpacity).toBe(0.7)
    expect(stopBarGraphic.style.opacity).toBe(1)

    const cycleGraphic = (
      series[4] as SeriesOption & { renderItem: () => GraphicNode }
    ).renderItem()
    expect(cycleGraphic.children[0].style.fill).toBe('#123456')
    expect(cycleGraphic.children[0].style.opacity).toBe(0.55)
    expect(cycleGraphic.children[1].style.fill).toBe('#000')
    expect(cycleGraphic.children[1].style.opacity).toBe(1)
  })
})
