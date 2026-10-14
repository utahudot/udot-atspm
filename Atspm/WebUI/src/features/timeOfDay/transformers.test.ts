import type { TimeOfDayResult } from '@/api/reports'
import type { SeriesOption } from 'echarts'

import {
  buildTimeOfDaySchedulesModel,
  getTimeOfDayPlanColorMap,
} from './components/schedules/timeOfDayScheduleModel'
import {
  buildPlanProfileSeries,
  buildScheduleRows,
  buildSplitPressureSeries,
  buildTimeOfDayAnalysisModel,
  buildTimeOfDayLocationNumberMap,
  getLocationNumber,
  getLocationPeakEvents,
  getMovementPressures,
  getTimeOfDayPresetSeriesSelection,
  getTimeOfDaySignalPeakDetailKey,
} from './transformers'

describe('time-of-day chart titles', () => {
  test('uses the corridor peak time that belongs to the displayed value', () => {
    const model = buildTimeOfDayAnalysisModel({
      recommendation: {
        amPeakTime: '08:15',
      },
      planProfile: {
        peaks: [
          {
            period: 'AM',
            series: 'Corridor',
            minutes: 510,
            value: 2685,
            valueUnits: 'vph',
          },
        ],
      },
    } as TimeOfDayResult)

    expect(
      model.header.summaryItems.find(
        (item) => item.label === 'AM Corridor Peak'
      )?.value
    ).toBe('08:30 - 2,685 vph')
  })

  test('keeps location badges stable across modes without identifier collisions', () => {
    const sharedResult = {
      locationIdentifiers: ['100-1', '1001'],
      planProfile: {
        peaks: [
          {
            period: 'AM',
            locationIdentifier: '1001',
            minutes: 480,
            value: 1200,
          },
          {
            period: 'AM',
            locationIdentifier: '100-1',
            minutes: 495,
            value: 1100,
          },
        ],
      },
      splitPressure: {
        movementPressures: [
          {
            period: 'AM',
            locationIdentifier: '100-1',
            movementLabel: 'Left',
            volume: 200,
          },
          {
            period: 'AM',
            locationIdentifier: '1001',
            movementLabel: 'Left',
            volume: 300,
          },
        ],
      },
    } as TimeOfDayResult
    const locationNumberMap = buildTimeOfDayLocationNumberMap(sharedResult)
    const signalPeaks = getLocationPeakEvents(
      sharedResult.planProfile?.peaks,
      'AM',
      locationNumberMap
    )

    expect(getLocationNumber(locationNumberMap, '100-1')).toBe(1)
    expect(getLocationNumber(locationNumberMap, '1001')).toBe(2)
    expect(getLocationNumber({}, 'constructor')).toBeUndefined()
    expect(
      Object.fromEntries(
        signalPeaks.map((peak) => [peak.locationIdentifier, peak.badgeNumber])
      )
    ).toEqual({ '100-1': 1, 1001: 2 })
    expect(
      getTimeOfDaySignalPeakDetailKey({
        period: 'AM',
        locationIdentifier: '100-1',
        minutes: 495,
        value: 1100,
      })
    ).not.toBe(
      getTimeOfDaySignalPeakDetailKey({
        period: 'AM',
        locationIdentifier: '1001',
        minutes: 495,
        value: 1100,
      })
    )
  })

  test('records the concrete series index for selectable chart details', () => {
    const peak = {
      period: 'AM',
      locationIdentifier: '100-1',
      minutes: 495,
      value: 1100,
    }
    const model = buildTimeOfDayAnalysisModel({
      locationIdentifiers: ['100-1'],
      planProfile: { peaks: [peak] },
    } as TimeOfDayResult)
    const target = model.detailTargets[getTimeOfDaySignalPeakDetailKey(peak)]
    const series = model.option.series as Array<{ name?: string }>

    expect(target.seriesIndex).toBe(
      series.findIndex(({ name }) => name === 'AM Signal Peaks')
    )
    expect(target.dataIndex).toBe(0)
  })

  test('uses red and purple for the AM and PM corridor peak stars', () => {
    const builtSeries = buildPlanProfileSeries({
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
    const series = builtSeries as Array<{
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

  test('pads the volume axis to cover plan and movement demand peaks', () => {
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
    const { option } = buildTimeOfDayAnalysisModel(sharedResult)

    expect((option.yAxis as Array<{ max?: number }>)[0].max).toBe(6000)
  })

  test('names representative series by period when period directions differ', () => {
    const getSeriesNames = (splitPressure: TimeOfDayResult['splitPressure']) =>
      buildSplitPressureSeries({ selectedDates: [], splitPressure }).map(
        (series) => series.name
      )
    const primaryProfile = { points: [{ minutes: 480, averageVolume: 900 }] }
    const crossStreetProfile = {
      points: [{ minutes: 480, averageVolume: 300 }],
    }

    expect(
      getSeriesNames({
        primaryDirections: ['Northbound', 'Southbound'],
        crossDirections: ['Eastbound', 'Westbound'],
        primaryDirectionsByPeriod: {
          AllDay: ['Northbound', 'Southbound'],
          AM: ['Eastbound', 'Westbound'],
          PM: ['Northbound', 'Southbound'],
        },
        crossDirectionsByPeriod: {
          AllDay: ['Eastbound', 'Westbound'],
          AM: ['Northbound', 'Southbound'],
          PM: ['Eastbound', 'Westbound'],
        },
        primaryProfile,
        crossStreetProfile,
      })
    ).toEqual(
      expect.arrayContaining([
        'Representative primary street (AM: Eastbound, Westbound · PM: Northbound, Southbound · Other hours: Northbound, Southbound)',
        'Representative cross street (AM: Northbound, Southbound · PM: Eastbound, Westbound · Other hours: Eastbound, Westbound)',
      ])
    )

    const sameDirections = ['Northbound', 'Southbound']
    expect(
      getSeriesNames({
        primaryDirections: sameDirections,
        primaryDirectionsByPeriod: {
          AllDay: sameDirections,
          AM: sameDirections,
          PM: sameDirections,
        },
        primaryProfile,
      })
    ).toContain('Representative Northbound, Southbound primary')
  })

  test('uses square chart markers for movement pressure', () => {
    const builtSeries = buildSplitPressureSeries({
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
    const series = builtSeries as Array<{
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
  })

  test('uses star chart markers for peaks without a location', () => {
    const planSeries = buildPlanProfileSeries({
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
    const pressureSeries = buildSplitPressureSeries({
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
      series: ReturnType<typeof buildPlanProfileSeries>,
      name: string
    ) =>
      (
        series as Array<{
          name?: string
          symbol?: string
          z?: number
          data?: Array<{
            itemStyle?: { color?: string }
          }>
        }>
      ).find((entry) => entry.name === name)

    expect(getPeakSeries(planSeries, 'AM Corridor Peak')).toMatchObject({
      symbol: expect.stringMatching(/^path:\/\//),
      z: 100,
    })
    const volumePeaks = getPeakSeries(pressureSeries, 'Volume Peaks')
    expect(volumePeaks).toMatchObject({
      symbol: expect.stringMatching(/^path:\/\//),
      z: 100,
    })
    expect(volumePeaks?.data).toEqual([
      expect.objectContaining({ itemStyle: { color: '#c62828' } }),
      expect.objectContaining({ itemStyle: { color: '#1b5e20' } }),
    ])
    const percentPeaks = getPeakSeries(
      pressureSeries,
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
      { 'location 1': 2, 'location 2': 1 }
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

  test('colors chart plans the same way as the Schedules tab', () => {
    const plan = (planNumber: string, start: string, end: string) => ({
      planNumber,
      start: `2026-07-15T${start}:00`,
      end: end === '24:00' ? '2026-07-16T00:00:00' : `2026-07-15T${end}:00`,
    })
    // A reported schedule that runs plan 7 twice, where the old peak-time
    // coloring painted both plan 7 windows orange. The existing schedule adds
    // a 30-minute special plan between plans 7 and 13.
    const scheduleResult = {
      selectedDates: [],
      recommendation: {
        amPeakTime: '08:15',
        middayValleyTime: '10:00',
        pmPeakTime: '17:15',
        recommendedSchedule: [
          plan('254', '00:00', '06:00'),
          plan('1', '06:00', '08:15'),
          plan('7', '08:15', '16:45'),
          plan('13', '16:45', '18:00'),
          plan('7', '18:00', '22:45'),
          plan('254', '22:45', '24:00'),
        ],
      },
      planComparison: {
        commonCurrentSchedule: [
          plan('254', '00:00', '06:00'),
          plan('1', '06:00', '09:00'),
          plan('7', '09:00', '15:00'),
          plan('5', '15:00', '15:30'),
          plan('13', '15:30', '18:00'),
          plan('7', '18:00', '22:00'),
          plan('254', '22:00', '24:00'),
        ],
      },
      planProfile: {
        corridorProfile: {
          points: [{ minutes: 495, averageVolume: 2685 }],
        },
      },
    } as TimeOfDayResult
    const model = buildTimeOfDayAnalysisModel(scheduleResult)
    const series = model.option.series as Array<{
      name?: string
      data?: unknown[][]
      markArea?: {
        data?: Array<[{ xAxis?: number; itemStyle?: { color?: string } }]>
      }
    }>
    const getSeries = (name: string) =>
      series.find((seriesOption) => seriesOption.name === name)
    const getWindowColors = (name: string) =>
      Object.fromEntries(
        getSeries(name)?.markArea?.data?.map(([start]) => [
          start.xAxis,
          start.itemStyle?.color,
        ]) ?? []
      )
    const getRailColors = (name: string) =>
      Object.fromEntries(
        getSeries(name)?.data?.map((datum) => [datum[3], datum[4]]) ?? []
      )
    const free = '#607d8b'
    const orange = '#ef6c00'
    const green = '#2e7d32'
    const blue = '#1565c0'
    const purple = '#6a1b9a'

    expect(
      Object.fromEntries(
        getTimeOfDayPlanColorMap(buildTimeOfDaySchedulesModel(scheduleResult))
      )
    ).toEqual({ FREE: free, 1: orange, 7: green, 13: blue, 5: purple })
    expect(getRailColors('Proposed schedule rail')).toEqual({
      FREE: free,
      1: orange,
      7: green,
      13: blue,
    })
    expect(getRailColors('Existing schedule rail')).toEqual({
      FREE: free,
      1: orange,
      7: green,
      5: purple,
      13: blue,
    })
    expect(getWindowColors('Proposed plan windows')).toEqual({
      0: '#ffffff',
      360: 'rgb(251, 217, 189)',
      495: 'rgb(201, 221, 202)',
      1005: 'rgb(194, 215, 239)',
      1080: 'rgb(201, 221, 202)',
      1365: '#ffffff',
    })
    expect(getWindowColors('Existing plan windows')[900]).toBe(
      'rgb(216, 196, 229)'
    )
    expect(
      model.layers
        .find((layer) => layer.id === 'existing-schedule')
        ?.legendItems?.map(({ label }) => label)
    ).toEqual(['Plan 1', 'Plan 7', 'Plan 5', 'Plan 13', 'FREE'])
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

    expect(tooltipHtml).toContain('08:30–08:45')
    expect(tooltipHtml).toContain('AM peak - 7621 - 9000 South and Monroe')
    expect(tooltipHtml).not.toContain('AM Signal Peaks')
    expect(tooltipHtml).toContain('2,824')
    expect(tooltipHtml).toContain('81.9%')
    expect(tooltipHtml).not.toContain('510.00')
    expect(
      formatter([
        {
          axisValue: 1440,
          seriesName: 'Cross-traffic percent',
          value: [1440, 35],
        },
      ])
    ).toContain('23:45–24:00')
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
          additionalColors: [],
          seriesNames: ['Proposed plan windows', 'Proposed schedule rail'],
          legendItems: [
            { label: 'Plan 1', color: '#ef6c00', preview: 'area' },
            { label: 'FREE', color: '#607d8b', preview: 'area' },
          ],
        }),
        expect.objectContaining({
          id: 'existing-schedule',
          label: 'Existing',
          color: '#2e7d32',
          additionalColors: [],
          seriesNames: ['Existing plan windows', 'Existing schedule rail'],
          legendItems: [
            { label: 'Plan 7', color: '#2e7d32', preview: 'area' },
            { label: 'FREE', color: '#607d8b', preview: 'area' },
          ],
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
