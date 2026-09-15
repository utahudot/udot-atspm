import type { TimeOfDayResult } from '@/api/reports'
import type { SeriesOption } from 'echarts'

import {
  buildPlanProfileOption,
  buildScheduleRows,
  buildSplitPressureOption,
  buildTimeOfDayAnalysisModel,
  getLocationPeakEvents,
  getMovementPressures,
  getTimeOfDayPresetSeriesSelection,
} from './transformers'

const result = {
  selectedDates: [],
} as TimeOfDayResult

describe('time-of-day chart titles', () => {
  test('includes the plan recommendation title', () => {
    const option = buildPlanProfileOption(result)

    expect(Array.isArray(option.title)).toBe(true)
    expect(option.title).toEqual(
      expect.arrayContaining([
        expect.objectContaining({
          text: 'Corridor Plan Recommendation',
          left: 0,
          textAlign: 'left',
        }),
      ])
    )
  })

  test('includes the split pressure title', () => {
    const option = buildSplitPressureOption(result)

    expect(Array.isArray(option.title)).toBe(true)
    expect(option.title).toEqual(
      expect.arrayContaining([
        expect.objectContaining({
          text: 'Corridor Movement Demand',
          left: 0,
          textAlign: 'left',
        }),
      ])
    )
  })

  test('uses red and purple for the AM and PM corridor peak stars', () => {
    const option = buildPlanProfileOption({
      planProfile: {
        peaks: [
          {
            period: 'AM',
            series: 'Corridor',
            minutes: 480,
            value: 3000,
          },
          {
            period: 'PM',
            series: 'Corridor',
            minutes: 1020,
            value: 3200,
          },
        ],
      },
    } as TimeOfDayResult)
    const series = option.series as Array<{
      name?: string
      symbol?: string
      z?: number
      itemStyle?: { color?: string }
    }>

    expect(
      series.find((seriesOption) => seriesOption.name === 'AM Corridor Peak')
    ).toMatchObject({
      symbol: expect.stringContaining('path://'),
      z: 100,
      itemStyle: { color: '#c62828' },
    })
    expect(
      series.find((seriesOption) => seriesOption.name === 'PM Corridor Peak')
    ).toMatchObject({
      symbol: expect.stringContaining('path://'),
      z: 100,
      itemStyle: { color: '#6a1b9a' },
    })
  })

  test('uses compact spacing for the title, date, and info rows', () => {
    const option = buildPlanProfileOption({
      selectedDates: ['2026-04-14', '2026-04-16'],
      recommendation: { amPeakTime: '08:30' },
    } as TimeOfDayResult)
    const titles = option.title as Array<{
      top?: number | string
    }>

    expect(titles.map((title) => title.top)).toEqual([0, 32, 58])
  })

  test('uses one padded volume-axis maximum for both analysis charts', () => {
    const sharedResult = {
      selectedDates: [],
      planProfile: {
        corridorProfile: {
          points: [{ minutes: 480, averageVolume: 3300, smoothedVolume: 3200 }],
        },
        peaks: [
          {
            period: 'PM',
            series: 'Location',
            locationIdentifier: '7192',
            minutes: 1020,
            value: 5200,
            valueUnits: 'vph',
          },
        ],
      },
      splitPressure: {
        primaryProfile: {
          points: [{ minutes: 1020, averageVolume: 4700 }],
        },
        movementPressures: [
          {
            period: 'PM',
            locationIdentifier: '7191',
            peakTime: '17:00',
            volume: 5100,
          },
        ],
        periodPeaks: [
          {
            period: 'PM',
            series: 'CrossTrafficPercent',
            minutes: 1020,
            value: 10000,
            valueUnits: 'percent',
          },
        ],
      },
    } as TimeOfDayResult
    const getVolumeAxisMax = (
      option: ReturnType<typeof buildPlanProfileOption>
    ) => (option.yAxis as Array<{ max?: number }>)[0].max

    expect(getVolumeAxisMax(buildPlanProfileOption(sharedResult))).toBe(6000)
    expect(getVolumeAxisMax(buildSplitPressureOption(sharedResult))).toBe(6000)
  })

  test('uses square chart markers for movement pressure', () => {
    const option = buildSplitPressureOption({
      selectedDates: [],
      splitPressure: {
        crossTrafficLocations: [
          {
            period: 'AM',
            locationIdentifier: 'cross-traffic',
            minutes: 480,
            totalVehiclesPerHour: 100,
          },
        ],
        movementPressures: [
          {
            period: 'AM',
            locationIdentifier: 'movement-pressure',
            movementLabel: 'Left',
            peakTime: '09:00',
            volume: 200,
          },
        ],
      },
    } as TimeOfDayResult)
    const series = option.series as Array<{
      name?: string
      z?: number
      data?: Array<{ name?: string; symbol?: string; value?: unknown[] }>
    }>
    const amCrossTraffic = series.find(
      (seriesOption) => seriesOption.name === 'AM Cross Traffic Locations'
    )
    const amMovementPressure = series.find(
      (seriesOption) => seriesOption.name === 'AM Movement Demand'
    )
    const legend = option.legend as {
      data?: Array<{ name?: string }>
      selected?: Record<string, boolean>
    }

    expect(amCrossTraffic?.data).toEqual([
      expect.objectContaining({ name: '1', symbol: 'circle' }),
    ])
    expect(amCrossTraffic?.z).toBe(50)
    expect(amMovementPressure?.data).toEqual([
      expect.objectContaining({ name: '2', symbol: 'rect' }),
    ])
    expect(amMovementPressure?.data?.[0]?.value?.[2]).toBe(
      'movement-pressure · Left'
    )
    expect(amMovementPressure?.z).toBe(50)
    expect(legend.data).toEqual(
      expect.arrayContaining([
        expect.objectContaining({ name: 'AM Cross Traffic Locations' }),
        expect.objectContaining({ name: 'AM Movement Demand' }),
      ])
    )
    expect(legend.selected).toMatchObject({
      'AM Movement Demand': false,
      'PM Movement Demand': false,
    })
  })

  test('uses star chart markers for peaks without a location', () => {
    const planOption = buildPlanProfileOption({
      selectedDates: [],
      planProfile: {
        peaks: [
          {
            period: 'AM',
            series: 'Corridor',
            minutes: 480,
            value: 3000,
          },
        ],
      },
    } as TimeOfDayResult)
    const pressureOption = buildSplitPressureOption({
      selectedDates: [],
      splitPressure: {
        periodPeaks: [
          {
            period: 'AM',
            series: 'PrimaryVolume',
            minutes: 480,
            value: 2500,
          },
          {
            period: 'Midday',
            series: 'PrimaryVolume',
            minutes: 720,
            value: 2200,
          },
          {
            period: 'PM',
            series: 'CrossTrafficPercent',
            minutes: 1020,
            value: 42,
            valueUnits: 'percent',
          },
        ],
      },
    } as TimeOfDayResult)
    const getPeakSeries = (
      option: ReturnType<typeof buildPlanProfileOption>,
      name: string
    ) =>
      (
        option.series as Array<{
          name?: string
          symbol?: string
          z?: number
          data?: Array<{
            itemStyle?: { color?: string }
          }>
        }>
      ).find((series) => series.name === name)

    expect(getPeakSeries(planOption, 'AM Corridor Peak')).toMatchObject({
      symbol: expect.stringMatching(/^path:\/\//),
      z: 100,
    })
    const volumePeaks = getPeakSeries(pressureOption, 'Volume Peaks')
    expect(volumePeaks).toMatchObject({
      symbol: expect.stringMatching(/^path:\/\//),
      z: 100,
    })
    expect(volumePeaks?.data).toEqual([
      expect.objectContaining({ itemStyle: { color: '#c62828' } }),
      expect.objectContaining({ itemStyle: { color: '#1b5e20' } }),
    ])
    const percentPeaks = getPeakSeries(
      pressureOption,
      'Cross Traffic Percent Peaks'
    )
    expect(percentPeaks).toMatchObject({
      symbol: expect.stringMatching(/^path:\/\//),
      z: 100,
    })
    expect(percentPeaks?.data).toEqual([
      expect.objectContaining({ itemStyle: { color: '#6a1b9a' } }),
    ])
  })

  test('groups movement pressure by numbered location, then direction', () => {
    const movements = getMovementPressures(
      [
        {
          period: 'AM',
          locationIdentifier: 'Location 2',
          movementLabel: 'Right',
          volume: 500,
        },
        {
          period: 'AM',
          locationIdentifier: 'Location 1',
          movementLabel: 'Thru',
          volume: 400,
        },
        {
          period: 'AM',
          locationIdentifier: 'Location 2',
          movementLabel: 'Left',
          volume: 100,
        },
        {
          period: 'AM',
          locationIdentifier: 'Location 1',
          movementLabel: 'Right',
          volume: 300,
        },
        {
          period: 'AM',
          locationIdentifier: 'Location 1',
          movementLabel: 'Left',
          volume: 200,
        },
      ],
      'AM',
      { location1: 2, location2: 1 }
    )

    expect(
      movements.map((movement) => [
        movement.locationIdentifier,
        movement.movementLabel,
      ])
    ).toEqual([
      ['Location 2', 'Left'],
      ['Location 2', 'Right'],
      ['Location 1', 'Left'],
      ['Location 1', 'Thru'],
      ['Location 1', 'Right'],
    ])
  })

  test.each([
    ['plan recommendation', buildPlanProfileOption],
    ['split pressure', buildSplitPressureOption],
  ])(
    'overlays synchronized existing and proposed schedule rails on the %s chart',
    (_, buildOption) => {
      const option = buildOption({
        locationIdentifiers: ['7192', '7191', '7190'],
        recommendation: {
          recommendedSchedule: [
            {
              planNumber: 'Free',
              planDescription: 'Free',
              start: '2026-01-01T00:00:00',
              end: '2026-01-01T07:00:00',
            },
            {
              planNumber: '1',
              planDescription: 'Plan 1',
              start: '2026-01-01T07:00:00',
              end: '2026-01-01T09:00:00',
            },
          ],
        },
        planComparison: {
          commonCurrentSchedule: [
            {
              planNumber: 'Free',
              planDescription: 'Free',
              start: '2026-01-01T00:00:00',
              end: '2026-01-01T06:00:00',
            },
            {
              planNumber: '7',
              planDescription: 'Plan 7',
              start: '2026-01-01T06:00:00',
              end: '2026-01-01T09:00:00',
            },
          ],
          exceptionLocationIdentifiers: ['7191', '7190'],
        },
      } as TimeOfDayResult)
      const series = option.series as Array<{
        name?: string
        data?: unknown[][]
        markArea?: {
          data?: Array<[{ name?: string; xAxis?: number }, { xAxis?: number }]>
        }
      }>
      const existingRail = series.find(
        (seriesOption) => seriesOption.name === 'Existing schedule rail'
      )
      const proposedRail = series.find(
        (seriesOption) => seriesOption.name === 'Proposed schedule rail'
      )
      const scheduleContextSeries = series.find(
        (seriesOption) => seriesOption.markArea?.data?.length
      )
      const dataZoom = option.dataZoom as Array<{
        xAxisIndex?: number | number[]
      }>

      expect(Array.isArray(option.grid)).toBe(true)
      expect(option.grid).toHaveLength(2)
      expect(dataZoom.map((zoom) => zoom.xAxisIndex)).toEqual([
        [0, 1],
        [0, 1],
      ])
      expect(existingRail?.data?.map((datum) => datum.slice(0, 4))).toEqual([
        [0, 360, 0, 'FREE'],
        [360, 540, 0, '7'],
      ])
      expect(proposedRail?.data?.map((datum) => datum.slice(0, 4))).toEqual([
        [0, 420, 1, 'FREE'],
        [420, 540, 1, '1'],
      ])
      expect(scheduleContextSeries?.markArea?.data).toEqual(
        expect.arrayContaining([
          [
            expect.objectContaining({
              name: 'Existing and proposed plans differ',
              xAxis: 360,
            }),
            { xAxis: 420 },
          ],
          [
            expect.objectContaining({
              name: 'Existing and proposed plans differ',
              xAxis: 420,
            }),
            { xAxis: 540 },
          ],
        ])
      )
      expect(option.graphic).toBeUndefined()
    }
  )

  test('paints plan windows opaque so overlapping schedules do not darken', () => {
    const option = buildPlanProfileOption({
      recommendation: {
        amPeakTime: '08:00',
        middayValleyTime: '12:00',
        pmPeakTime: '17:00',
        recommendedSchedule: [
          {
            planNumber: 'Free',
            start: '2026-01-01T00:00:00',
            end: '2026-01-01T07:30:00',
          },
          {
            planNumber: '1',
            start: '2026-01-01T07:30:00',
            end: '2026-01-01T09:45:00',
          },
          {
            planNumber: '7',
            start: '2026-01-01T09:45:00',
            end: '2026-01-01T15:00:00',
          },
          {
            planNumber: '13',
            start: '2026-01-01T15:00:00',
            end: '2026-01-01T18:45:00',
          },
          {
            planNumber: '7',
            start: '2026-01-01T18:45:00',
            end: '2026-01-01T22:30:00',
          },
          {
            planNumber: 'Free',
            start: '2026-01-01T22:30:00',
            end: '2026-01-02T00:00:00',
          },
        ],
      },
      planProfile: {
        corridorProfile: {
          points: [{ minutes: 480, averageVolume: 3000 }],
        },
      },
    } as TimeOfDayResult)
    const series = option.series as Array<{
      markArea?: {
        data?: Array<
          [
            {
              xAxis?: number
              itemStyle?: { color?: string; opacity?: number }
            },
            { xAxis?: number },
          ]
        >
      }
    }>
    const planWindows = series.find(
      (seriesOption) => seriesOption.markArea?.data?.length === 6
    )?.markArea?.data
    const styleByStart = new Map(
      planWindows?.map(([start]) => [start.xAxis, start.itemStyle])
    )

    expect(styleByStart.get(0)).toEqual({ color: '#ffffff', opacity: 0 })
    expect(styleByStart.get(450)).toEqual({
      color: 'rgb(251, 217, 189)',
      opacity: 1,
    })
    expect(styleByStart.get(585)).toEqual({
      color: 'rgb(201, 221, 202)',
      opacity: 1,
    })
    expect(styleByStart.get(900)).toEqual({
      color: 'rgb(194, 215, 239)',
      opacity: 1,
    })
    expect(styleByStart.get(1125)?.color).toBe('rgb(201, 221, 202)')
    expect(styleByStart.get(1350)).toEqual({ color: '#ffffff', opacity: 0 })
  })

  test('aligns recommended and current plans by shared time windows', () => {
    const rows = buildScheduleRows({
      recommendation: {
        recommendedSchedule: [
          {
            planNumber: 'Free',
            planDescription: 'Free',
            start: '2026-01-01T00:00:00',
            end: '2026-01-01T07:00:00',
          },
          {
            planNumber: '1',
            planDescription: 'Plan 1',
            start: '2026-01-01T07:00:00',
            end: '2026-01-01T09:00:00',
          },
        ],
      },
      planComparison: {
        commonCurrentSchedule: [
          {
            planNumber: 'Free',
            planDescription: 'Free',
            start: '2026-01-01T00:00:00',
            end: '2026-01-01T06:00:00',
          },
          {
            planNumber: '7',
            planDescription: 'Plan 7',
            start: '2026-01-01T06:00:00',
            end: '2026-01-01T09:00:00',
          },
        ],
      },
    } as TimeOfDayResult)

    expect(rows).toEqual([
      expect.objectContaining({
        start: '00:00',
        end: '06:00',
        durationMinutes: 360,
        recommended: { plan: 'FREE', description: 'Free' },
        current: { plan: 'FREE', description: 'Free' },
        comparison: 'Same',
      }),
      expect.objectContaining({
        start: '06:00',
        end: '07:00',
        durationMinutes: 60,
        recommended: { plan: 'FREE', description: 'Free' },
        current: { plan: '7', description: 'Plan 7' },
        comparison: 'Different',
      }),
      expect.objectContaining({
        start: '07:00',
        end: '09:00',
        durationMinutes: 120,
        recommended: { plan: '1', description: 'Plan 1' },
        current: { plan: '7', description: 'Plan 7' },
        comparison: 'Different',
      }),
    ])
  })

  test('names the location and the time of day in the chart tooltip', () => {
    const model = buildTimeOfDayAnalysisModel({
      planProfile: {
        corridorProfile: {
          points: [{ minutes: 510, averageVolume: 3000 }],
        },
        peaks: [
          {
            period: 'AM',
            series: 'Location',
            label: 'AM peak',
            locationIdentifier: '7621',
            locationDescription: '9000 South and Monroe',
            minutes: 510,
            value: 2824,
          },
        ],
      },
      splitPressure: {
        crossTrafficShare: [{ minutes: 510, crossTrafficPercent: 81.94 }],
      },
    } as TimeOfDayResult)
    const tooltip = model.option.tooltip as { formatter?: unknown }
    const formatter = tooltip.formatter as (params: unknown) => string
    const peakSeries = (model.option.series as SeriesOption[]).find(
      (seriesOption) => seriesOption.name === 'AM Signal Peaks'
    )
    const [peakPoint] = (peakSeries?.data ?? []) as Array<{ value: unknown }>

    const tooltipHtml = formatter([
      {
        axisValue: 510,
        marker: '<i></i>',
        seriesName: 'AM Signal Peaks',
        data: peakPoint,
        value: peakPoint.value,
      },
      {
        axisValue: 510,
        marker: '<i></i>',
        seriesName: 'Cross-traffic percent',
        value: [510, 81.94],
      },
    ])

    expect(tooltipHtml).toContain('08:30')
    expect(tooltipHtml).toContain('AM peak - 7621 - 9000 South and Monroe')
    expect(tooltipHtml).not.toContain('AM Signal Peaks')
    expect(tooltipHtml).toContain('2,824')
    expect(tooltipHtml).toContain('81.9%')
    expect(tooltipHtml).not.toContain('510.00')
  })

  test('names the profile a volume peak belongs to and marks it with a star', () => {
    const model = buildTimeOfDayAnalysisModel({
      splitPressure: {
        primaryProfile: {
          points: [{ minutes: 1035, averageVolume: 1246 }],
        },
        crossStreetProfile: {
          points: [{ minutes: 1035, averageVolume: 2918 }],
        },
        periodPeaks: [
          {
            period: 'PM',
            series: 'Primary',
            label: 'PM primary peak',
            minutes: 1035,
            value: 1246,
          },
        ],
      },
    } as TimeOfDayResult)
    const tooltip = model.option.tooltip as { formatter?: unknown }
    const formatter = tooltip.formatter as (params: unknown) => string
    const volumePeaks = (model.option.series as SeriesOption[]).find(
      (seriesOption) => seriesOption.name === 'Volume Peaks'
    )
    const [volumePeakPoint] = (volumePeaks?.data ?? []) as Array<{
      value: unknown
    }>

    const tooltipHtml = formatter([
      {
        axisValue: 1035,
        color: '#c62828',
        marker: '<i>dot</i>',
        seriesName: 'Volume Peaks',
        data: volumePeakPoint,
        value: volumePeakPoint.value,
      },
    ])

    expect(tooltipHtml).toContain('PM primary peak')
    expect(tooltipHtml).not.toContain('Volume Peaks')
    expect(tooltipHtml).toContain('★')
    expect(tooltipHtml).not.toContain('<i>dot</i>')
  })

  test('builds one layered chart with presets, schedule context, and detail targets', () => {
    const model = buildTimeOfDayAnalysisModel({
      planProfile: {
        corridorProfile: {
          points: [{ minutes: 480, averageVolume: 3000, smoothedVolume: 2900 }],
        },
        directionalProfiles: [
          {
            label: 'Northbound total profile',
            points: [{ minutes: 480, averageVolume: 1600 }],
          },
          {
            label: 'Southbound total profile',
            points: [{ minutes: 480, averageVolume: 1400 }],
          },
        ],
        peaks: [
          {
            period: 'AM',
            series: 'Location',
            locationIdentifier: '7190',
            minutes: 480,
            value: 2500,
          },
        ],
      },
      splitPressure: {
        primaryProfile: {
          points: [{ minutes: 480, averageVolume: 2400 }],
        },
        crossStreetProfile: {
          points: [{ minutes: 480, averageVolume: 900 }],
        },
        crossTrafficShare: [{ minutes: 480, crossTrafficPercent: 27.3 }],
        crossTrafficLocations: [
          {
            period: 'AM',
            locationIdentifier: '7191',
            minutes: 480,
            totalVehiclesPerHour: 900,
          },
        ],
        movementPressures: [
          {
            period: 'AM',
            locationIdentifier: '7192',
            movementLabel: 'Left',
            peakTime: '08:00',
            volume: 500,
          },
        ],
      },
      recommendation: {
        amPeakTime: '08:00',
        recommendedSchedule: [
          {
            planNumber: 'Free',
            planDescription: 'Free',
            start: '2026-01-01T00:00:00',
            end: '2026-01-01T07:00:00',
          },
          {
            planNumber: '1',
            planDescription: 'Plan 1',
            start: '2026-01-01T07:00:00',
            end: '2026-01-01T09:00:00',
          },
        ],
      },
      planComparison: {
        commonCurrentSchedule: [
          {
            planNumber: 'Free',
            planDescription: 'Free',
            start: '2026-01-01T00:00:00',
            end: '2026-01-01T06:00:00',
          },
          {
            planNumber: '7',
            planDescription: 'Plan 7',
            start: '2026-01-01T06:00:00',
            end: '2026-01-01T09:00:00',
          },
        ],
      },
    } as TimeOfDayResult)
    expect(model.header.title).toBe('Corridor Time-of-Day Analysis')
    expect(model.header.summaryItems.map((item) => item.label)).toContain(
      'AM Corridor Peak'
    )
    expect(model.layers).toEqual(
      expect.arrayContaining([
        expect.objectContaining({
          id: 'proposed-schedule',
          label: 'Proposed',
          color: '#ef6c00',
          additionalColors: ['#2e7d32', '#1565c0'],
          seriesNames: ['Proposed plan windows', 'Proposed schedule rail'],
          legendItems: [
            {
              label: 'AM peak plan',
              color: '#ef6c00',
              preview: 'area',
            },
            {
              label: 'Midday plan',
              color: '#2e7d32',
              preview: 'area',
            },
            {
              label: 'PM peak plan',
              color: '#1565c0',
              preview: 'area',
            },
            {
              label: 'FREE operation',
              color: '#607d8b',
              preview: 'area',
            },
          ],
        }),
        expect.objectContaining({
          id: 'existing-schedule',
          label: 'Existing',
          seriesNames: ['Existing plan windows', 'Existing schedule rail'],
        }),
        expect.objectContaining({
          id: 'schedule-differences',
          label: 'Schedule differences',
          preview: 'hatch',
          color: '#f59e0b',
          seriesNames: ['Plan difference windows'],
          legendItems: [
            {
              label: 'Proposed and existing schedules differ',
              color: '#f59e0b',
              preview: 'hatch',
            },
          ],
        }),
        expect.objectContaining({
          id: 'directional-profiles',
          label: 'Directional profiles',
          color: '#00897b',
          additionalColors: ['#7b1fa2'],
          seriesNames: ['Northbound total profile', 'Southbound total profile'],
          seriesControls: [
            {
              seriesName: 'Northbound total profile',
              label: 'Northbound total profile',
              color: '#00897b',
              available: true,
            },
            {
              seriesName: 'Southbound total profile',
              label: 'Southbound total profile',
              color: '#7b1fa2',
              available: true,
            },
          ],
        }),
        expect.objectContaining({
          id: 'split-review-threshold',
          label: '35% split review',
          color: '#f9a825',
          seriesNames: ['35% split review'],
        }),
        expect.objectContaining({
          id: 'shoulder-review-threshold',
          label: '45% shoulder review',
          color: '#c62828',
          seriesNames: ['45% shoulder review'],
        }),
        expect.objectContaining({
          id: 'corridor-peaks',
          color: '#c62828',
          additionalColors: ['#6a1b9a'],
          seriesNames: ['AM Corridor Peak', 'PM Corridor Peak'],
        }),
        expect.objectContaining({
          id: 'pressure-peaks',
          color: '#c62828',
          additionalColors: ['#1b5e20', '#6a1b9a'],
          seriesNames: ['Volume Peaks', 'Cross Traffic Percent Peaks'],
        }),
        expect.objectContaining({
          id: 'signal-peaks',
          color: '#ef6c00',
          additionalColors: ['#1565c0'],
          previewLabel: '1',
          seriesNames: ['AM Signal Peaks', 'PM Signal Peaks'],
        }),
        expect.objectContaining({
          id: 'cross-traffic-locations',
          color: '#ef6c00',
          additionalColors: ['#1b5e20', '#1565c0'],
          previewLabel: '1',
          seriesNames: [
            'AM Cross Traffic Locations',
            'Midday Cross Traffic Locations',
            'PM Cross Traffic Locations',
          ],
        }),
        expect.objectContaining({
          id: 'movement-pressure',
          color: '#ef6c00',
          additionalColors: ['#1565c0'],
          previewLabel: '1',
          seriesNames: ['AM Movement Demand', 'PM Movement Demand'],
        }),
      ])
    )
    expect(model.option.title).toEqual([])
    const series = model.option.series as Array<{
      type?: string
      clip?: boolean
      z?: number
      name?: string
      data?: unknown[][]
      dimensions?: string[]
      encode?: { tooltip?: number[] }
      tooltip?: {
        show?: boolean
        trigger?: string
        formatter?: (params: unknown) => string
      }
      silent?: boolean
      cursor?: string
      renderItem?: (
        params: {
          dataIndex?: number
          coordSys: { x: number; y: number; width: number; height: number }
        },
        api: {
          value: (dimension: number) => number
          coord: (values: number[]) => number[]
          size?: (values: number[]) => number[]
        }
      ) =>
        | {
            children?: Array<{
              type?: string
              silent?: boolean
              shape?: {
                x?: number
                width?: number
                height?: number
                r?: number
              }
              style?: {
                fill?: string
                text?: string
                y?: number
                stroke?: string
                lineWidth?: number
                lineDash?: number[]
              }
              emphasis?: {
                style?: {
                  fill?: string
                }
              }
            }>
          }
        | undefined
      markArea?: {
        data?: Array<
          [
            {
              name?: string
              xAxis?: number
              itemStyle?: {
                decal?: { rotation?: number; dashArrayY?: number[] }
              }
            },
            { xAxis?: number },
          ]
        >
      }
    }>
    const existingSchedule = series.find(
      (seriesOption) => seriesOption.name === 'Existing schedule rail'
    )
    const proposedSchedule = series.find(
      (seriesOption) => seriesOption.name === 'Proposed schedule rail'
    )
    const differenceWindows = series.find(
      (seriesOption) => seriesOption.name === 'Plan difference windows'
    )
    const existingPlanWindows = series.find(
      (seriesOption) => seriesOption.name === 'Existing plan windows'
    )
    const chartGrids = model.option.grid as Array<{
      bottom?: number
      top?: number
      height?: number
    }>
    expect(chartGrids[0]).toMatchObject({ top: 112, bottom: 116 })
    expect(chartGrids[1]).toMatchObject({ top: 24, height: 76 })

    const scheduleXAxis = (
      model.option.xAxis as Array<{
        axisPointer?: { show?: boolean }
      }>
    )[1]
    const scheduleYAxes = model.option.yAxis as Array<{
      axisPointer?: { show?: boolean }
      triggerEvent?: boolean
    }>
    const scheduleYAxis = scheduleYAxes[scheduleYAxes.length - 1]
    expect(scheduleXAxis.axisPointer?.show).toBe(false)
    expect(scheduleYAxis.axisPointer?.show).toBe(false)
    expect(scheduleYAxis.triggerEvent).toBe(true)
    expect(proposedSchedule).toMatchObject({
      clip: false,
      tooltip: {
        show: true,
        trigger: 'item',
        formatter: expect.any(Function),
      },
    })
    expect(proposedSchedule?.dimensions).not.toContain('Hovered')
    expect(proposedSchedule?.encode).not.toHaveProperty('tooltip')

    const percentAxis = (
      model.option.yAxis as Array<{
        show?: boolean
        axisLabel?: { formatter?: string | ((value: number) => string) }
      }>
    )[1]

    expect(existingSchedule?.data?.map((datum) => datum.slice(0, 4))).toEqual([
      [0, 360, 0, 'FREE'],
      [360, 540, 0, '7'],
    ])
    expect(proposedSchedule?.data?.map((datum) => datum.slice(0, 4))).toEqual([
      [0, 420, 1, 'FREE'],
      [420, 540, 1, '1'],
    ])
    expect(existingSchedule?.data?.map((datum) => datum.slice(3, 5))).toEqual([
      ['FREE', '#607d8b'],
      ['7', '#2e7d32'],
    ])
    expect(proposedSchedule?.data?.map((datum) => datum.slice(3, 5))).toEqual([
      ['FREE', '#607d8b'],
      ['1', '#ef6c00'],
    ])

    const proposedPlanDatum = proposedSchedule?.data?.[1]
    const proposedTooltipFormatter = proposedSchedule?.tooltip?.formatter
    expect(typeof proposedTooltipFormatter).toBe('function')
    expect(proposedTooltipFormatter?.({ value: proposedPlanDatum })).toBe(
      '<strong>Proposed schedule</strong><br/>07:00–09:00<br/>Plan 1'
    )
    const renderedProposedPlan = proposedSchedule?.renderItem?.(
      { coordSys: { x: 0, y: 0, width: 1440, height: 60 } },
      {
        value: (dimension) => proposedPlanDatum?.[dimension] as number,
        coord: ([minutes, lane]) => [minutes, lane * 20],
        size: () => [0, 40],
      }
    )
    const proposedPlanStyle = renderedProposedPlan?.children?.[0]?.style
    expect(proposedPlanStyle).toMatchObject({
      fill: 'rgba(239, 108, 0, 0.2)',
    })
    expect(renderedProposedPlan?.children?.[0]?.shape?.height).toBe(34)
    expect(renderedProposedPlan?.children?.[0]).not.toHaveProperty('emphasis')
    expect(renderedProposedPlan?.children?.[1]?.style).toMatchObject({
      text: '07:00\u201309:00',
      y: -5,
    })
    expect(renderedProposedPlan?.children?.[2]?.style).toMatchObject({
      text: '1',
      y: 20,
    })

    const existingPlanDatum = existingSchedule?.data?.[1]
    const renderedExistingPlan = existingSchedule?.renderItem?.(
      { coordSys: { x: 0, y: 0, width: 1440, height: 60 } },
      {
        value: (dimension) => existingPlanDatum?.[dimension] as number,
        coord: ([minutes, lane]) => [minutes, lane * 20],
        size: () => [0, 40],
      }
    )
    expect(renderedExistingPlan?.children?.[1]?.style?.text).toBe('7')
    expect(renderedExistingPlan?.children?.[2]).toBeUndefined()

    const compactDatum = [420, 540, 1, '1', '#ef6c00', 'Plan 1']
    const renderedCompactPlan = proposedSchedule?.renderItem?.(
      { coordSys: { x: 0, y: 0, width: 720, height: 60 } },
      {
        value: (dimension) => compactDatum[dimension] as number,
        coord: ([minutes, lane]) => [minutes / 2, lane * 20],
        size: () => [0, 40],
      }
    )
    expect(renderedCompactPlan?.children?.[1]?.style?.text).toBe('07:00')
    expect(renderedCompactPlan?.children?.[2]?.style?.text).toBe('1')

    const renderedPlanOnly = proposedSchedule?.renderItem?.(
      { coordSys: { x: 0, y: 0, width: 480, height: 60 } },
      {
        value: (dimension) => compactDatum[dimension] as number,
        coord: ([minutes, lane]) => [minutes / 3, lane * 20],
        size: () => [0, 40],
      }
    )
    expect(renderedPlanOnly?.children?.[1]?.style?.text).toBe('1')

    const renderedUnlabeledPlan = proposedSchedule?.renderItem?.(
      { coordSys: { x: 0, y: 0, width: 288, height: 60 } },
      {
        value: (dimension) => compactDatum[dimension] as number,
        coord: ([minutes, lane]) => [minutes / 5, lane * 20],
        size: () => [0, 40],
      }
    )
    expect(renderedUnlabeledPlan?.children).toBeUndefined()

    const endOfDayDatum = [1320, 1440, 1, 'FREE', '#607d8b', 'Free']
    const renderedEndOfDayPlan = proposedSchedule?.renderItem?.(
      { coordSys: { x: 0, y: 0, width: 1440, height: 60 } },
      {
        value: (dimension) => endOfDayDatum[dimension] as number,
        coord: ([minutes, lane]) => [minutes, lane * 20],
        size: () => [0, 40],
      }
    )
    expect(renderedEndOfDayPlan?.children?.[1]?.style?.text).toBe(
      '22:00\u201324:00'
    )
    expect(renderedEndOfDayPlan?.children?.[2]?.style?.text).toBe('FREE')

    const proposedRowDatum = proposedSchedule?.data?.[0]
    const renderedProposedRow = proposedSchedule?.renderItem?.(
      {
        dataIndex: 0,
        coordSys: { x: 0, y: 0, width: 1440, height: 60 },
      },
      {
        value: (dimension) => proposedRowDatum?.[dimension] as number,
        coord: ([minutes, lane]) => [minutes, lane * 20],
        size: () => [0, 40],
      }
    )
    expect(renderedProposedRow?.children?.[0]).toMatchObject({
      type: 'rect',
      shape: {
        x: -72,
        width: 1584,
        height: 40,
        r: 4,
      },
      style: {
        fill: 'rgba(255, 255, 255, 0)',
      },
      emphasis: {
        style: {
          fill: 'rgba(71, 84, 103, 0.08)',
        },
      },
    })
    expect(renderedProposedRow?.children?.[1]?.silent).toBe(false)
    expect(renderedProposedRow?.children?.[2]?.silent).toBe(true)
    expect(renderedProposedRow?.children?.[0]?.style).not.toHaveProperty(
      'stroke'
    )
    expect(renderedProposedRow?.children?.[0]?.style).not.toHaveProperty(
      'lineWidth'
    )
    expect(proposedPlanStyle).not.toHaveProperty('stroke')
    expect(proposedPlanStyle).not.toHaveProperty('lineWidth')
    expect(differenceWindows).toMatchObject({
      type: 'custom',
      data: [
        [360, 420, 'FREE', 'Free', '7', 'Plan 7'],
        [420, 540, '1', 'Plan 1', '7', 'Plan 7'],
      ],
      silent: false,
      cursor: 'help',
      tooltip: {
        show: true,
        trigger: 'item',
        formatter: expect.any(Function),
      },
      z: -1,
    })
    const differenceTooltipFormatter = differenceWindows?.tooltip?.formatter
    expect(typeof differenceTooltipFormatter).toBe('function')
    expect(
      differenceTooltipFormatter?.({ value: differenceWindows?.data?.[0] })
    ).toBe(
      '<strong>Schedules differ</strong><br/>06:00–07:00<br/>Proposed: FREE<br/>Existing: Plan 7'
    )
    expect(existingPlanWindows).toMatchObject({
      z: -2,
      markArea: { z: -2 },
    })
    const renderedDifference = differenceWindows?.renderItem?.(
      { coordSys: { x: 0, y: 0, width: 100, height: 60 } },
      {
        value: (dimension) => (dimension === 0 ? 20 : 80),
        coord: ([value]) => [value, 0],
      }
    )
    expect(
      renderedDifference?.children?.some((child) => child.type === 'line')
    ).toBe(true)
    expect(renderedDifference?.children?.[0]?.style?.fill).toBe(
      'rgba(226, 232, 240, 0.78)'
    )
    expect(
      renderedDifference?.children?.filter(
        (child) => child.type === 'line' && !child.style?.lineDash
      )
    ).toHaveLength(7)
    expect(
      renderedDifference?.children?.filter(
        (child) => child.style?.lineDash?.join(',') === '5,4'
      )
    ).toHaveLength(2)
    expect(existingPlanWindows?.markArea?.data).toHaveLength(2)
    expect(model.defaultSelectedSeries).toMatchObject({
      'Median Raw Volume': true,
      'Northbound total profile': true,
      'Southbound total profile': true,
      '35% split review': false,
      '45% shoulder review': false,
      'Existing plan windows': true,
      'Existing schedule rail': true,
      'Proposed plan windows': true,
      'Proposed schedule rail': true,
      'Plan difference windows': true,
      'AM Movement Demand': false,
    })
    expect(percentAxis.show).toBe(false)
    const percentFormatter = percentAxis.axisLabel?.formatter
    expect(typeof percentFormatter).toBe('function')
    if (typeof percentFormatter === 'function') {
      expect(percentFormatter(83.33333333333334)).toBe('83.3%')
      expect(percentFormatter(100)).toBe('100%')
    }

    expect(model.percentSeriesNames).toEqual(
      expect.arrayContaining([
        'Cross-traffic percent',
        '35% split review',
        '45% shoulder review',
      ])
    )
    expect(Object.values(model.detailTargets)).toEqual(
      expect.arrayContaining([
        expect.objectContaining({ layerId: 'signal-peaks' }),
        expect.objectContaining({ layerId: 'cross-traffic-locations' }),
        expect.objectContaining({ layerId: 'movement-pressure' }),
      ])
    )

    const pressureSelection = getTimeOfDayPresetSeriesSelection(
      model.layers,
      'pressure',
      {
        ...model.defaultSelectedSeries,
        'Proposed plan windows': false,
        'Proposed schedule rail': true,
        'Existing plan windows': true,
        'Existing schedule rail': true,
      }
    )

    expect(pressureSelection).toMatchObject({
      'Median Raw Volume': false,
      'Northbound total profile': false,
      'Southbound total profile': false,
      'Cross-traffic percent': true,
      '35% split review': true,
      '45% shoulder review': true,
      'AM Cross Traffic Locations': true,
      'AM Movement Demand': false,
      'Existing schedule rail': true,
      'Existing plan windows': true,
      'Proposed schedule rail': false,
      'Proposed plan windows': false,
      'Plan difference windows': true,
    })
  })
})

describe('time-of-day signal peak numbering', () => {
  test('uses the same sequential badge number for each location in AM and PM', () => {
    const peaks = [
      {
        period: 'AM',
        series: 'Location',
        locationIdentifier: '7522',
        minutes: 420,
        value: 2295,
      },
      {
        period: 'PM',
        series: 'Location',
        locationIdentifier: '7522',
        minutes: 975,
        value: 2808,
      },
      {
        period: 'AM',
        series: 'Location',
        locationIdentifier: '7521',
        minutes: 465,
        value: 2480,
      },
      {
        period: 'PM',
        series: 'Location',
        locationIdentifier: '7521',
        minutes: 1035,
        value: 2735,
      },
    ]

    expect(
      getLocationPeakEvents(peaks, 'AM').map((peak) => [
        peak.locationIdentifier,
        peak.badgeNumber,
      ])
    ).toEqual([
      ['7522', 1],
      ['7521', 2],
    ])
    expect(
      getLocationPeakEvents(peaks, 'PM').map((peak) => [
        peak.locationIdentifier,
        peak.badgeNumber,
      ])
    ).toEqual([
      ['7522', 1],
      ['7521', 2],
    ])
  })
})
