// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - aggregateOptionsData.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
export const MetricTypeOptionsList = [
  {
    id: 'Detector Activation Count',
    options: [
      { id: 'detectorActivationCount', label: 'Detector Activation Count' },
    ],
  },
  {
    id: 'Approach PCD',
    options: [
      { id: 'arrivalsOnGreen', label: 'Arrivals On Green' },
      { id: 'arrivalsOnRed', label: 'Arrivals On Red' },
      { id: 'arrivalsOnYellow', label: 'Arrivals On Yellow' },
      { id: 'volume', label: 'Volume' },
      { id: 'totalDelay', label: 'Total Delay' },
    ],
  },
  {
    id: 'Approach Cycle',
    options: [
      { id: 'redTime', label: 'Red Time' },
      { id: 'yellowTime', label: 'Yellow Time' },
      { id: 'greenTime', label: 'Green Time' },
      { id: 'totalRedToRedCycles', label: 'Total Red To Red Cycles' },
      {
        id: 'totalGreenToGreenCycles',
        label: 'Total Green To Green Cycles',
      },
    ],
  },
  {
    id: 'Approach Split Fail',
    options: [
      { id: 'splitFailures', label: 'Split Failures' },
      { id: 'greenOccupancySum', label: 'Green Occupancy Sum' },
      { id: 'redOccupancySum', label: 'Red Occupancy Sum' },
      { id: 'greenTimeSum', label: 'Green Time Sum' },
      { id: 'redTimeSum', label: 'Red Time Sum' },
      { id: 'cycles', label: 'Cycles' },
    ],
  },
  {
    id: 'Signal Preemption',
    options: [
      { id: 'preemptNumber', label: 'Preempt Number' },
      { id: 'preemptRequests', label: 'Preempt Requests' },
      { id: 'preemptServices', label: 'Preempt Services' },
    ],
  },
  {
    id: 'Signal Priority',
    options: [
      { id: 'priorityNumber', label: 'Priority Number' },
      { id: 'priorityRequests', label: 'Priority Requests' },
      {
        id: 'priorityServiceEarlyGreen',
        label: 'Priority Service Early Green',
      },
      {
        id: 'priorityServiceExtendedGreen',
        label: 'Priority Service Extended Green',
      },
    ],
  },
  {
    id: 'Approach Speed',
    options: [
      { id: 'averageSpeed', label: 'Average Speed' },
      { id: 'speedVolume', label: 'Speed Volume' },
      { id: 'speed85th', label: 'Speed 85th' },
      { id: 'speed15th', label: 'Speed 15th' },
    ],
  },
  {
    id: 'Approach Yellow Red Activations',
    options: [
      {
        id: 'severeRedLightViolations',
        label: 'Severe Red Light Violations',
      },
      { id: 'totalRedLightViolations', label: 'Total Red Light Violations' },
      { id: 'yellowActivations', label: 'Yellow Activations' },
      { id: 'violationTime', label: 'Violation Time' },
      { id: 'cycles', label: 'Cycles' },
    ],
  },
  {
    id: 'Signal Event Count',
    options: [{ id: 'eventCount', label: 'Event Count' }],
  },
  {
    id: 'Phase Termination',
    options: [
      { id: 'gapOuts', label: 'Gap Outs' },
      { id: 'forceOffs', label: 'Force Offs' },
      { id: 'maxOuts', label: 'Max Outs' },
      { id: 'unknown', label: 'Unknown' },
    ],
  },
  {
    id: 'Phase Pedestrian Delay',
    options: [
      { id: 'pedRequests', label: 'Ped Requests' },
      { id: 'maxPedDelay', label: 'Max Ped Delay' },
      { id: 'minPedDelay', label: 'Min Ped Delay' },
      { id: 'pedDelaySum', label: 'Ped Delay Sum' },
      { id: 'pedCycles', label: 'Ped Cycles' },
    ],
  },
  {
    id: 'Left Turn Gap',
    options: [
      { id: 'gapCount1', label: 'Gap Count 1' },
      { id: 'gapCount2', label: 'Gap Count 2' },
      { id: 'gapCount3', label: 'Gap Count 3' },
      { id: 'gapCount4', label: 'Gap Count 4' },
      { id: 'gapCount5', label: 'Gap Count 5' },
      { id: 'gapCount6', label: 'Gap Count 6' },
      { id: 'gapCount7', label: 'Gap Count 7' },
      { id: 'gapCount8', label: 'Gap Count 8' },
      { id: 'gapCount9', label: 'Gap Count 9' },
      { id: 'gapCount10', label: 'Gap Count 10' },
      { id: 'gapCount11', label: 'Gap Count 11' },
      { id: 'sumGapDuration1', label: 'Sum Gap Duration 1' },
      { id: 'sumGapDuration2', label: 'Sum Gap Duration 2' },
      { id: 'sumGapDuration3', label: 'Sum Gap Duration 3' },
      { id: 'sumGreenTime', label: 'Sum Green Time' },
    ],
  },
  {
    id: 'Split Monitor',
    options: [
      {
        id: 'eightyFifthPercentileSplit',
        label: 'Eighty Fifth Percentile Split',
      },
      { id: 'skippedCount', label: 'Skipped Count' },
    ],
  },
]

export enum XAxisType {
  Time = 0,
  TimeOfDay = 1,
  Direction = 2,
  Approach = 3,
  Location = 4,
  Detector = 5,
}

export enum YAxisType {
  Location = 0,
  PhaseNumber = 1,
  Direction = 2,
  Route = 3,
  Detector = 4,
}

export const xAxisOptions = [
  { id: XAxisType.Time, label: 'Time' },
  { id: XAxisType.TimeOfDay, label: 'Time Of Day' },
  { id: XAxisType.Direction, label: 'Direction' },
  { id: XAxisType.Approach, label: 'Approach' },
  { id: XAxisType.Location, label: 'Location' },
  { id: XAxisType.Detector, label: 'Detector' },
]

export const YAxisOptions = [
  { id: YAxisType.Location, label: 'Location' },
  { id: YAxisType.PhaseNumber, label: 'Phase Number' },
  { id: YAxisType.Direction, label: 'Direction' },
  { id: YAxisType.Route, label: 'Route' },
  { id: YAxisType.Detector, label: 'Detector' },
]

// X-Axis choices whose points are named groups (a direction, approach,
// location or detector) instead of dates.
export const categoryXAxisTypes: number[] = [
  XAxisType.Direction,
  XAxisType.Approach,
  XAxisType.Location,
  XAxisType.Detector,
]

// The X-Axis and Y-Axis pairs the server accepts for each family of metric.
// Any other pair fails with "Invalid X-Axis" or "Invalid X-Axis Series
// Combination".
type AxisPairs = Partial<Record<XAxisType, YAxisType[]>>

const locationMetricAxes: AxisPairs = {
  [XAxisType.Time]: [YAxisType.Location, YAxisType.Route],
  [XAxisType.TimeOfDay]: [YAxisType.Location, YAxisType.Route],
  [XAxisType.Location]: [YAxisType.Location],
}

const phaseMetricAxes: AxisPairs = {
  [XAxisType.Time]: [
    YAxisType.Location,
    YAxisType.PhaseNumber,
    YAxisType.Route,
  ],
  [XAxisType.TimeOfDay]: [
    YAxisType.Location,
    YAxisType.PhaseNumber,
    YAxisType.Route,
  ],
  [XAxisType.Approach]: [YAxisType.PhaseNumber],
  [XAxisType.Location]: [YAxisType.Location, YAxisType.PhaseNumber],
}

const approachMetricAxes: AxisPairs = {
  [XAxisType.Time]: [
    YAxisType.Location,
    YAxisType.PhaseNumber,
    YAxisType.Direction,
    YAxisType.Route,
  ],
  [XAxisType.TimeOfDay]: [
    YAxisType.Location,
    YAxisType.PhaseNumber,
    YAxisType.Direction,
    YAxisType.Route,
  ],
  [XAxisType.Direction]: [YAxisType.Direction],
  [XAxisType.Approach]: [YAxisType.PhaseNumber],
  [XAxisType.Location]: [
    YAxisType.Location,
    YAxisType.PhaseNumber,
    YAxisType.Direction,
  ],
}

const detectorMetricAxes: AxisPairs = {
  ...approachMetricAxes,
  [XAxisType.Time]: [
    YAxisType.Location,
    YAxisType.PhaseNumber,
    YAxisType.Direction,
    YAxisType.Route,
    YAxisType.Detector,
  ],
  [XAxisType.TimeOfDay]: [
    YAxisType.Location,
    YAxisType.PhaseNumber,
    YAxisType.Direction,
    YAxisType.Route,
    YAxisType.Detector,
  ],
  [XAxisType.Detector]: [YAxisType.PhaseNumber, YAxisType.Detector],
}

const axisPairsByMetricGroup: Record<string, AxisPairs> = {
  'Detector Activation Count': detectorMetricAxes,
  'Approach PCD': approachMetricAxes,
  'Approach Cycle': approachMetricAxes,
  'Approach Split Fail': approachMetricAxes,
  'Approach Speed': approachMetricAxes,
  'Approach Yellow Red Activations': approachMetricAxes,
  'Left Turn Gap': approachMetricAxes,
  'Signal Preemption': locationMetricAxes,
  'Signal Priority': locationMetricAxes,
  'Signal Event Count': locationMetricAxes,
  'Phase Termination': phaseMetricAxes,
  'Phase Pedestrian Delay': phaseMetricAxes,
  'Split Monitor': phaseMetricAxes,
}

// metricType is "<metric group>-<metric>"; with no metric chosen every
// option is offered.
const getAxisPairs = (metricType: string): AxisPairs | undefined =>
  axisPairsByMetricGroup[metricType.split('-')[0]]

// Time Of Day groups bins by the time they start, so it needs bins shorter
// than a day.
const allowsTimeOfDay = (binSize: number) => binSize < BinSize.Day

export const getXAxisOptions = (metricType: string, binSize: number) => {
  const pairs = getAxisPairs(metricType)
  return xAxisOptions.filter(
    (option) =>
      (!pairs || option.id in pairs) &&
      (option.id !== XAxisType.TimeOfDay || allowsTimeOfDay(binSize))
  )
}

export const getYAxisOptions = (metricType: string, xAxisType: number) => {
  const pairs = getAxisPairs(metricType)
  if (!pairs) return YAxisOptions
  const allowed = pairs[xAxisType as XAxisType] ?? []
  return YAxisOptions.filter((option) => allowed.includes(option.id))
}

// Keeps the X-Axis and Y-Axis on a pair the server accepts for the metric and
// Bin Size: an X-Axis that isn't offered falls back to Time, and a Y-Axis that
// isn't offered falls back to the first one that is.
export const getValidAxes = (
  metricType: string,
  xAxisType: number,
  yAxisType: number,
  binSize: number
) => {
  const validXAxisType = getXAxisOptions(metricType, binSize).some(
    (option) => option.id === xAxisType
  )
    ? xAxisType
    : XAxisType.Time
  const yAxisOptions = getYAxisOptions(metricType, validXAxisType)
  const validYAxisType =
    yAxisOptions.length &&
    !yAxisOptions.some((option) => option.id === yAxisType)
      ? yAxisOptions[0].id
      : yAxisType
  return { xAxisType: validXAxisType, yAxisType: validYAxisType }
}

export const chartTypeOptions: {
  id: 'line' | 'bar' | 'pie'
  label: string
}[] = [
  { id: 'bar', label: 'Bar' },
  { id: 'line', label: 'Line' },
  { id: 'pie', label: 'Pie' },
]

// The server's bin size numbers. Its year size is left off the page.
export enum BinSize {
  FifteenMinute = 0,
  ThirtyMinute = 1,
  Hour = 2,
  Day = 3,
  Month = 4,
}

export const binSizeMarks = [
  { value: BinSize.FifteenMinute, label: '15 min' },
  { value: BinSize.ThirtyMinute, label: '30 min' },
  { value: BinSize.Hour, label: 'hour' },
  { value: BinSize.Day, label: 'day' },
  { value: BinSize.Month, label: 'month' },
]

// The server's time options: every minute from start to end, or only a
// daily time window on chosen days.
export enum TimePeriodOption {
  StartToEnd = 0,
  TimePeriod = 1,
}

// Checkboxes in page order, each with the server's direction number.
export const directionFilterOptions = [
  { id: 3, label: 'Eastbound' },
  { id: 4, label: 'Westbound' },
  { id: 1, label: 'Northbound' },
  { id: 2, label: 'Southbound' },
  { id: 5, label: 'Northeast' },
  { id: 6, label: 'Northwest' },
  { id: 7, label: 'Southeast' },
  { id: 8, label: 'Southwest' },
]

// Checkboxes in page order, each with the server's movement number.
export const movementFilterOptions = [
  { id: 3, label: 'Left' },
  { id: 5, label: 'Thru-Left' },
  { id: 1, label: 'Thru' },
  { id: 4, label: 'Thru-Right' },
  { id: 2, label: 'Right' },
  { id: 7, label: 'Left-Thru-Right' },
  { id: 0, label: 'None' },
]

export const detectionTypes = [
  { id: 0, label: 'Advanced Count' },
  { id: 1, label: 'Advanced Speed' },
  { id: 2, label: 'Lane-by-lane Count' },
  { id: 3, label: 'Lane-by-lane with Speed restriction' },
  { id: 4, label: 'Stop Bar Presence' },
  { id: 5, label: 'Advanced presence' },
]

export enum AggregationType {
  'Detector Activation Count' = 0,
  'Approach PCD' = 1,
  'Approach Speed' = 2,
  'Approach Split Fail' = 3,
  'Approach Yellow Red Activations' = 4,
  'Approach Cycle' = 5,
  'Left Turn Gap' = 6,
  'Phase Pedestrian Delay' = 7,
  'Split Monitor' = 8,
  'Phase Termination' = 9,
  'Signal Preemption' = 10,
  'Signal Priority' = 11,
  'Signal Event Count' = 12,
  'Signal Plan' = 13,
}
