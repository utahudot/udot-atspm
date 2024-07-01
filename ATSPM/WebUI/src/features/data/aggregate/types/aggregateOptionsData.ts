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

export const xAxisOptions = [
  { id: 0, label: 'Time' },
  { id: 1, label: 'Time Of Day' },
  { id: 2, label: 'Direction' },
  { id: 3, label: 'Approach' },
  { id: 4, label: 'Location' },
  { id: 5, label: 'Detector' },
]

export const YAxisOptions = [
  { id: 0, label: 'Location' },
  { id: 1, label: 'Phase Number' },
  { id: 2, label: 'Direction' },
  { id: 3, label: 'Route' },
  { id: 4, label: 'Detector' },
]

export const chartTypeOptions: {
  id: 'line' | 'bar' | 'pie'
  label: string
}[] = [
  { id: 'bar', label: 'Bar' },
  { id: 'line', label: 'Line' },
  { id: 'pie', label: 'Pie' },
]

export const binSizeMarks = [
  { value: 0, label: '15 min' },
  { value: 1, label: '30 min' },
  { value: 2, label: 'hour' },
  { value: 3, label: 'month' },
  { value: 4, label: 'year' },
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
