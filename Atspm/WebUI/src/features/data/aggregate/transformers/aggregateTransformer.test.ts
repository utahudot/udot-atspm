import { AggregateChartSettings } from '../handlers/aggregateDataHandler'
import { AggregateData } from '../types/aggregateData'
import { XAxisType } from '../types/aggregateOptionsData'
import { formatLocations, transformData } from './aggregateTransformer'

const makeSettings = (xAxisType: number): AggregateChartSettings => ({
  xAxisType,
  yAxisType: 0,
  metricType: 'Approach PCD-volume',
  averageOrSum: 0,
  visualChartType: 'bar',
  startDateTime: new Date(2026, 9, 1),
  endDateTime: new Date(2026, 9, 15),
})

describe('transformData', () => {
  it('charts named groups on a category axis, lined up across series', () => {
    const data: AggregateData = {
      identifier: '',
      locationIdentifiers: ['7115', '7116'],
      series: [
        {
          identifier: 'Main St',
          dataPoints: [
            {
              identifier: 'Northbound',
              start: '0001-01-01T00:00:00',
              value: 10,
            },
            {
              identifier: 'Southbound',
              start: '0001-01-01T00:00:00',
              value: 20,
            },
          ],
        },
        {
          identifier: 'State St',
          dataPoints: [
            { identifier: 'Eastbound', start: '0001-01-01T00:00:00', value: 5 },
            {
              identifier: 'Northbound',
              start: '0001-01-01T00:00:00',
              value: 7,
            },
          ],
        },
      ],
    }

    const chart = transformData(makeSettings(XAxisType.Direction), data)

    expect(chart.xAxis).toMatchObject({
      type: 'category',
      name: 'Direction',
      data: ['Northbound', 'Southbound', 'Eastbound'],
    })
    expect((chart.series as { data: unknown[] }[]).map((s) => s.data)).toEqual([
      ['10.00', '20.00', null],
      ['7.00', null, '5.00'],
    ])
  })

  it('keeps Time charts on a time axis', () => {
    const data: AggregateData = {
      identifier: '',
      locationIdentifiers: ['7116'],
      series: [
        {
          identifier: 'Main St',
          dataPoints: [
            { identifier: '', start: '2026-10-01T06:00:00', value: 3 },
            { identifier: '', start: '2026-10-01T07:00:00', value: 4 },
          ],
        },
      ],
    }

    const chart = transformData(makeSettings(XAxisType.Time), data)

    expect(chart.xAxis).toMatchObject({ type: 'time' })
    expect((chart.series as { data: unknown[] }[])[0].data).toEqual([
      ['2026-10-01T06:00:00', '3.00'],
      ['2026-10-01T07:00:00', '4.00'],
    ])
  })

  it("titles the chart with the chart's own locations", () => {
    const data: AggregateData = {
      identifier: '',
      locationIdentifiers: ['7116'],
      series: [],
    }

    const chart = transformData(makeSettings(XAxisType.Time), data)

    expect(JSON.stringify(chart.title)).toContain('Volume\\n7116')
  })

  it('leaves the location line out when the chart names no locations', () => {
    const data: AggregateData = {
      identifier: '',
      locationIdentifiers: [],
      series: [],
    }

    const chart = transformData(makeSettings(XAxisType.Time), data)

    expect(JSON.stringify(chart.title)).toContain('Volume"')
  })
})

describe('formatLocations', () => {
  it('names up to three locations and counts the rest', () => {
    expect(formatLocations(['7115'])).toBe('7115')
    expect(formatLocations(['7115', '7116', '7117'])).toBe('7115, 7116, 7117')
    expect(
      formatLocations(
        Array.from({ length: 20 }, (_, index) => String(7115 + index))
      )
    ).toBe('7115, 7116, 7117 and 17 more')
  })
})
