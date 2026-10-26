import {
  BinSize,
  MetricTypeOptionsList,
  XAxisType,
  YAxisType,
  directionFilterOptions,
  getValidAxes,
  getXAxisOptions,
  getYAxisOptions,
  movementFilterOptions,
  xAxisOptions,
} from './aggregateOptionsData'

const ids = (options: { id: number }[]) => options.map((option) => option.id)

describe('aggregate axis choices', () => {
  it('offers every X-Axis before a Metric Type is chosen', () => {
    expect(getXAxisOptions('', BinSize.Hour)).toEqual(xAxisOptions)
  })

  it('offers only the X-Axis choices the server accepts for location metrics', () => {
    expect(
      ids(getXAxisOptions('Signal Preemption-preemptNumber', BinSize.Hour))
    ).toEqual([XAxisType.Time, XAxisType.TimeOfDay, XAxisType.Location])
  })

  it('offers Detector only for detector metrics', () => {
    expect(
      ids(
        getXAxisOptions(
          'Detector Activation Count-detectorActivationCount',
          BinSize.Hour
        )
      )
    ).toContain(XAxisType.Detector)
    expect(
      ids(getXAxisOptions('Approach PCD-volume', BinSize.Hour))
    ).not.toContain(XAxisType.Detector)
  })

  it('offers Time Of Day only for bins shorter than a day', () => {
    expect(
      ids(getXAxisOptions('Approach PCD-volume', BinSize.FifteenMinute))
    ).toContain(XAxisType.TimeOfDay)
    for (const binSize of [BinSize.Day, BinSize.Month]) {
      expect(ids(getXAxisOptions('', binSize))).not.toContain(
        XAxisType.TimeOfDay
      )
      expect(
        ids(getXAxisOptions('Approach PCD-volume', binSize))
      ).not.toContain(XAxisType.TimeOfDay)
    }
  })

  it("numbers the filter checkboxes with the server's direction and movement numbers", () => {
    const byLabel = (options: { id: number; label: string }[]) =>
      Object.fromEntries(options.map((option) => [option.label, option.id]))
    expect(byLabel(directionFilterOptions)).toEqual({
      Northbound: 1,
      Southbound: 2,
      Eastbound: 3,
      Westbound: 4,
      Northeast: 5,
      Northwest: 6,
      Southeast: 7,
      Southwest: 8,
    })
    expect(byLabel(movementFilterOptions)).toEqual({
      None: 0,
      Thru: 1,
      Right: 2,
      Left: 3,
      'Thru-Right': 4,
      'Thru-Left': 5,
      'Left-Thru-Right': 7,
    })
  })

  it('offers exactly the server pairs for every Metric Type group', () => {
    const { Location, PhaseNumber, Direction, Route, Detector } = YAxisType
    const location = {
      [XAxisType.Time]: [Location, Route],
      [XAxisType.TimeOfDay]: [Location, Route],
      [XAxisType.Location]: [Location],
    }
    const phase = {
      [XAxisType.Time]: [Location, PhaseNumber, Route],
      [XAxisType.TimeOfDay]: [Location, PhaseNumber, Route],
      [XAxisType.Approach]: [PhaseNumber],
      [XAxisType.Location]: [Location, PhaseNumber],
    }
    const approach = {
      [XAxisType.Time]: [Location, PhaseNumber, Direction, Route],
      [XAxisType.TimeOfDay]: [Location, PhaseNumber, Direction, Route],
      [XAxisType.Direction]: [Direction],
      [XAxisType.Approach]: [PhaseNumber],
      [XAxisType.Location]: [Location, PhaseNumber, Direction],
    }
    const detector = {
      [XAxisType.Time]: [Location, PhaseNumber, Direction, Route, Detector],
      [XAxisType.TimeOfDay]: [
        Location,
        PhaseNumber,
        Direction,
        Route,
        Detector,
      ],
      [XAxisType.Direction]: [Direction],
      [XAxisType.Approach]: [PhaseNumber],
      [XAxisType.Location]: [Location, PhaseNumber, Direction],
      [XAxisType.Detector]: [PhaseNumber, Detector],
    }
    const expectedByGroup: Record<string, Record<number, number[]>> = {
      'Detector Activation Count': detector,
      'Approach PCD': approach,
      'Approach Cycle': approach,
      'Approach Split Fail': approach,
      'Approach Speed': approach,
      'Approach Yellow Red Activations': approach,
      'Left Turn Gap': approach,
      'Signal Preemption': location,
      'Signal Priority': location,
      'Signal Event Count': location,
      'Phase Termination': phase,
      'Phase Pedestrian Delay': phase,
      'Split Monitor': phase,
    }
    for (const group of MetricTypeOptionsList) {
      const metricType = `${group.id}-${group.options[0].id}`
      const offered = Object.fromEntries(
        getXAxisOptions(metricType, BinSize.Hour).map((xAxis) => [
          xAxis.id,
          ids(getYAxisOptions(metricType, xAxis.id)),
        ])
      )
      expect([group.id, offered]).toEqual([group.id, expectedByGroup[group.id]])
    }
  })

  it('limits the Y-Axis to the pairs the server accepts', () => {
    expect(
      ids(getYAxisOptions('Approach PCD-volume', XAxisType.Direction))
    ).toEqual([YAxisType.Direction])
    expect(
      ids(getYAxisOptions('Phase Termination-gapOuts', XAxisType.Approach))
    ).toEqual([YAxisType.PhaseNumber])
  })
})

describe('getValidAxes', () => {
  it('keeps a pair the server accepts', () => {
    expect(
      getValidAxes(
        'Approach PCD-volume',
        XAxisType.TimeOfDay,
        YAxisType.Direction,
        BinSize.Hour
      )
    ).toEqual({
      xAxisType: XAxisType.TimeOfDay,
      yAxisType: YAxisType.Direction,
    })
  })

  it('moves Time Of Day to Time when Bin Size is day or month', () => {
    for (const binSize of [BinSize.Day, BinSize.Month])
      expect(
        getValidAxes(
          'Approach PCD-volume',
          XAxisType.TimeOfDay,
          YAxisType.Direction,
          binSize
        )
      ).toEqual({ xAxisType: XAxisType.Time, yAxisType: YAxisType.Direction })
  })

  it('moves the X-Axis to Time and the Y-Axis to the first choice when the Metric Type does not offer them', () => {
    expect(
      getValidAxes(
        'Signal Preemption-preemptNumber',
        XAxisType.Direction,
        YAxisType.Direction,
        BinSize.Hour
      )
    ).toEqual({ xAxisType: XAxisType.Time, yAxisType: YAxisType.Location })
  })
})
