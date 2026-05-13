import type { ECharts, EChartsOption } from 'echarts'
import { buildOffsetResetButtons } from './TimeSpaceEChart'

describe('buildOffsetResetButtons', () => {
  function buildChart() {
    return {
      convertToPixel: () => [50, 60],
      getModel: () => ({
        getComponent: () => ({
          coordinateSystem: {
            getRect: () => ({
              x: 100,
              width: 200,
            }),
          },
        }),
      }),
    } as unknown as ECharts
  }

  it('returns reset buttons only when the offset row is actually modified', () => {
    const option: EChartsOption = {
      series: [
        {
          id: 'Location axis',
          name: 'Location axis',
          type: 'custom',
          data: [
            ['2026-03-20T00:00:00Z', 10, '6192', 'Main', 132, 89, 88, 1],
            ['2026-03-20T00:00:00Z', 20, '7001', 'Main', 132, 93, 93, 0],
            ['2026-03-20T00:00:00Z', 30, '8123', 'Main', 120, 93, 93, 120],
          ],
        },
      ],
    }

    const buttons = buildOffsetResetButtons(buildChart(), option)

    expect(buttons).toHaveLength(3)
    expect(buttons.map((button) => [button.location, button.active])).toEqual([
      ['6192', true],
      ['7001', false],
      ['8123', true],
    ])
  })
})
