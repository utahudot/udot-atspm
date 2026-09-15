import type {
  Plan,
  TimeOfDayCrossTrafficLocationDto,
  TimeOfDayMovementPressureDto,
  TimeOfDayPeakEventDto,
  TimeOfDayProfileDto,
  TimeOfDayProfilePointDto,
  TimeOfDayResult,
} from '@/api/reports'
import {
  createDataZoom,
  createGrid,
  createInfoString,
  createLegend,
  createTitle,
  createYAxis,
} from '@/features/charts/common/transformers'
import {
  DashedLineSeriesSymbol,
  SolidLineSeriesSymbol,
} from '@/features/charts/utils'
import type {
  CustomSeriesRenderItemAPI,
  CustomSeriesRenderItemParams,
  CustomSeriesRenderItemReturn,
  EChartsOption,
  LegendComponentOption,
  SeriesOption,
} from 'echarts'
import { graphic } from 'echarts'
import type { TimeOfDayScheduleEntry } from './schedule'
import {
  buildScheduleRows,
  formatPlanNumber,
  freeSchedulePlanColor,
  getPlanBoundaryMinutes,
  getPlanIntervalMinutes,
  getScheduleEntries,
  getSchedulePlanColorMap,
  minutesToTimeLabel,
  planIntervalContainsMinutes,
} from './schedule'

export {
  buildScheduleRows,
  formatPlanNumber,
  formatPlanTime,
  minutesToTimeLabel,
} from './schedule'
export type {
  TimeOfDaySchedulePlanDetails,
  TimeOfDayScheduleRow,
} from './schedule'

type ProfileValueKey = keyof Pick<
  TimeOfDayProfilePointDto,
  'averageVolume' | 'smoothedVolume' | 'rollingHourVph'
>

interface TimeOfDayPlanColorContext {
  amPeakMinutes: number | null
  middayMinutes: number | null
  pmPeakMinutes: number | null
  amPlanIndex: number | null
  middayPlanIndex: number | null
  pmPlanIndex: number | null
}

export type TimeOfDayNumberedPeakEvent = TimeOfDayPeakEventDto & {
  badgeNumber: number
  badgeColor: string
  markerSymbol?: 'circle' | 'rect'
  detailKey?: string
  movementLabel?: string
}

export type TimeOfDayLocationNumberMap = Record<string, number>

export type TimeOfDayChartPreset = 'recommendation' | 'pressure' | 'combined'

export type TimeOfDayChartLayerGroup =
  | 'Schedules'
  | 'Corridor Demand'
  | 'Movement Demand'
  | 'Locations'

export type TimeOfDayChartLayerId =
  | 'proposed-schedule'
  | 'existing-schedule'
  | 'schedule-differences'
  | 'raw-volume'
  | 'smoothed-volume'
  | 'directional-profiles'
  | `directional-profile-${number}`
  | 'corridor-peaks'
  | 'primary-volume'
  | 'cross-volume'
  | 'cross-percent'
  | 'review-thresholds'
  | 'split-review-threshold'
  | 'shoulder-review-threshold'
  | 'pressure-peaks'
  | 'signal-peaks'
  | 'cross-traffic-locations'
  | 'movement-pressure'

export type TimeOfDayChartLayerPreview =
  | 'schedule'
  | 'area'
  | 'hatch'
  | 'solid-line'
  | 'dashed-line'
  | 'circle'
  | 'star'
  | 'square'

export interface TimeOfDayChartLayerSeriesControl {
  seriesName: string
  label: string
  color: string
  available: boolean
}

export interface TimeOfDayChartLayerLegendItem {
  label: string
  color: string
  preview: 'area' | 'hatch'
}

export interface TimeOfDayChartLayer {
  id: TimeOfDayChartLayerId
  group: TimeOfDayChartLayerGroup
  label: string
  description: string
  preview: TimeOfDayChartLayerPreview
  color: string
  additionalColors?: string[]
  previewLabel?: string
  seriesNames: string[]
  seriesControls?: TimeOfDayChartLayerSeriesControl[]
  legendItems?: TimeOfDayChartLayerLegendItem[]
  available: boolean
}

export interface TimeOfDayChartDetailTarget {
  detailKey: string
  layerId: TimeOfDayChartLayerId
  seriesName: string
  dataIndex: number
}

export interface TimeOfDayAnalysisModel {
  header: {
    title: string
    dateRange: string
    summaryItems: Array<{
      label: string
      value: string
    }>
  }
  option: EChartsOption
  layers: TimeOfDayChartLayer[]
  defaultSelectedSeries: Record<string, boolean>
  percentSeriesNames: string[]
  detailTargets: Record<string, TimeOfDayChartDetailTarget>
}

const chartColors = {
  raw: '#455a64',
  smooth: '#1565c0',
  primary: '#1565c0',
  cross: '#c62828',
  percent: '#6a1b9a',
  amPeak: '#c62828',
  pmPeak: '#6a1b9a',
  amSignalPeak: '#ef6c00',
  middaySignalPeak: '#1b5e20',
  pmSignalPeak: '#1565c0',
  amPlanBackground: '#ef6c00',
  middayPlanBackground: '#2e7d32',
  pmPlanBackground: '#1565c0',
  defaultPlanBackground: '#f0f0f0',
  volumePeak: '#ef6c00',
  splitReview: '#f9a825',
  shoulderReview: '#c62828',
}

const directionalColors = [
  '#00897b',
  '#7b1fa2',
  '#f57c00',
  '#5d4037',
  '#3949ab',
  '#546e7a',
  '#2e7d32',
  '#ad1457',
]

const StarSeriesSymbol =
  'path://M12 17.27L18.18 21L16.54 13.97L22 9.24L14.81 8.63L12 2L9.19 8.63L2 9.24L7.46 13.97L5.82 21Z'
const peakMarkerZ = 50
const corridorPeakMarkerZ = 100

const numberFormatter = new Intl.NumberFormat('en-US', {
  maximumFractionDigits: 0,
})

const dateFormatter = new Intl.DateTimeFormat('en-US', {
  weekday: 'short',
  year: 'numeric',
  month: 'long',
  day: '2-digit',
})

export const formatNumber = (
  value?: number | null,
  maximumFractionDigits = 0
) => {
  if (value === undefined || value === null || Number.isNaN(value)) return '-'

  return new Intl.NumberFormat('en-US', {
    maximumFractionDigits,
  }).format(value)
}

const formatPercentAxisLabel = (value: number | string) => {
  const numericValue = Number(value)

  return Number.isFinite(numericValue)
    ? `${formatNumber(numericValue, 1)}%`
    : `${value}%`
}

const formatSelectedDateRange = (selectedDates?: string[] | null) => {
  const validDates =
    selectedDates
      ?.map((selectedDate) => {
        const date = new Date(
          selectedDate.includes('T') ? selectedDate : `${selectedDate}T00:00:00`
        )
        return Number.isNaN(date.getTime()) ? null : date
      })
      .filter((date): date is Date => date !== null)
      .sort((left, right) => left.getTime() - right.getTime()) ?? []

  if (!validDates.length) return ''

  const firstDate = dateFormatter.format(validDates[0])
  const lastDate = dateFormatter.format(validDates[validDates.length - 1])

  return firstDate === lastDate ? firstDate : `${firstDate} - ${lastDate}`
}

const formatPeakMeasurement = (
  value?: number | null,
  units?: string | null,
  maximumFractionDigits = 0
) => {
  if (value === undefined || value === null) return ''

  const normalizedUnits = units?.toLowerCase()
  if (normalizedUnits?.includes('percent') || units === '%') {
    return `${formatNumber(value, maximumFractionDigits)}%`
  }

  return [formatNumber(value, maximumFractionDigits), units]
    .filter(Boolean)
    .join(' ')
}

const formatPeakInfoValue = (
  time?: string | null,
  value?: number | null,
  units?: string | null,
  maximumFractionDigits = 0
) =>
  [time, formatPeakMeasurement(value, units, maximumFractionDigits)]
    .filter(Boolean)
    .join(' - ')

const formatPeakInfo = (items: Array<[string, string]>) => {
  const populatedItems = items.filter(([, value]) => Boolean(value))

  return populatedItems.length ? createInfoString(...populatedItems) : undefined
}

const normalizeToken = (value?: string | null) =>
  value?.toLowerCase().replace(/[^a-z0-9]/g, '') ?? ''

const buildDetailKey = (...parts: Array<string | number | null | undefined>) =>
  parts.map((part) => normalizeToken(String(part ?? ''))).join(':')

export const getTimeOfDaySignalPeakDetailKey = (peak: TimeOfDayPeakEventDto) =>
  buildDetailKey(
    'signal-peak',
    peak.period,
    peak.locationIdentifier,
    peak.minutes,
    peak.value
  )

export const getTimeOfDayCrossTrafficDetailKey = (
  location: TimeOfDayCrossTrafficLocationDto,
  period = location.period
) =>
  buildDetailKey(
    'cross-traffic',
    period,
    location.locationIdentifier,
    location.minutes ?? location.peakTime,
    location.totalVehiclesPerHour
  )

export const getTimeOfDayMovementPressureDetailKey = (
  movement: TimeOfDayMovementPressureDto,
  period = movement.period
) =>
  buildDetailKey(
    'movement-pressure',
    period,
    movement.locationIdentifier,
    movement.movementLabel ?? movement.movement,
    movement.peakTime,
    movement.volume
  )

const getProfilePoints = (profile?: TimeOfDayProfileDto) =>
  profile?.points?.filter((point) => point.minutes !== undefined) ?? []

export const hasProfileData = (profile?: TimeOfDayProfileDto) =>
  getProfilePoints(profile).length > 0

export const hasPlanProfileData = (result: TimeOfDayResult) =>
  hasProfileData(result.planProfile?.corridorProfile)

export const hasSplitPressureData = (result: TimeOfDayResult) => {
  const splitPressure = result.splitPressure

  return Boolean(
    hasProfileData(splitPressure?.primaryProfile) ||
      hasProfileData(splitPressure?.crossStreetProfile) ||
      splitPressure?.crossTrafficShare?.length
  )
}

const isPercentPeakEvent = (peak: TimeOfDayPeakEventDto) =>
  normalizeToken(peak.series) === 'crosstrafficpercent' ||
  peak.valueUnits?.toLowerCase().includes('percent') === true

const getSharedVolumeAxisMax = (result: TimeOfDayResult) => {
  let maximumVolume = 0
  const includeVolume = (value?: number | null) => {
    if (typeof value === 'number' && Number.isFinite(value) && value >= 0) {
      maximumVolume = Math.max(maximumVolume, value)
    }
  }
  const includeProfile = (
    profile: TimeOfDayProfileDto | null | undefined,
    valueKeys: ProfileValueKey[]
  ) => {
    profile?.points?.forEach((point) => {
      valueKeys.forEach((valueKey) => includeVolume(point[valueKey]))
    })
  }

  includeProfile(result.planProfile?.corridorProfile, [
    'averageVolume',
    'smoothedVolume',
  ])
  result.planProfile?.directionalProfiles?.forEach((profile) =>
    includeProfile(profile, ['averageVolume'])
  )
  result.planProfile?.peaks?.forEach((peak) => {
    if (!isPercentPeakEvent(peak)) includeVolume(peak.value)
  })

  includeProfile(result.splitPressure?.primaryProfile, ['averageVolume'])
  includeProfile(result.splitPressure?.crossStreetProfile, ['averageVolume'])
  result.splitPressure?.periodPeaks?.forEach((peak) => {
    if (!isPercentPeakEvent(peak)) includeVolume(peak.value)
  })
  result.splitPressure?.crossTrafficLocations?.forEach((location) =>
    includeVolume(location.totalVehiclesPerHour)
  )
  result.splitPressure?.movementPressures?.forEach((movement) =>
    includeVolume(movement.volume)
  )
  includeVolume(result.splitPressure?.primaryPeakVolume)
  includeVolume(result.splitPressure?.crossStreetPeakVolume)

  if (maximumVolume === 0) return undefined

  return Math.ceil((maximumVolume * 1.1) / 1000) * 1000
}

const getProfileSeriesData = (
  profile: TimeOfDayProfileDto | undefined,
  valueKey: ProfileValueKey
) =>
  getProfilePoints(profile)
    .map((point) => {
      const value = point[valueKey]
      if (value === undefined || value === null) return null

      return [point.minutes ?? 0, value]
    })
    .filter((point): point is number[] => point !== null)

const buildProfileLineSeries = ({
  profile,
  name,
  valueKey,
  color,
  yAxisIndex = 0,
  lineStyle,
}: {
  profile?: TimeOfDayProfileDto
  name: string
  valueKey: ProfileValueKey
  color: string
  yAxisIndex?: number
  lineStyle?: Record<string, unknown>
}): SeriesOption => ({
  name,
  type: 'line',
  data: getProfileSeriesData(profile, valueKey),
  showSymbol: false,
  smooth: true,
  yAxisIndex,
  lineStyle: {
    width: 2,
    color,
    ...lineStyle,
  },
  itemStyle: { color },
})

const getPlanIndexForMinutes = (
  plans: Plan[] | null | undefined,
  minutes: number | null
) => {
  if (minutes === null) return null

  const index = plans?.findIndex((plan) =>
    planIntervalContainsMinutes(plan, minutes)
  )

  return index === undefined || index < 0 ? null : index
}

const getFallbackPlanIndex = (
  plans: Plan[] | null | undefined,
  period: 'am' | 'midday' | 'pm'
) => {
  const planCount = plans?.length ?? 0
  if (planCount === 0) return null

  if (planCount >= 4) {
    if (period === 'am') return 1
    if (period === 'midday') return 2
    return 3
  }

  if (planCount === 3) {
    if (period === 'am') return 0
    if (period === 'midday') return 1
    return 2
  }

  return null
}

const getPlanColorContext = (
  result: TimeOfDayResult,
  plans: Plan[] | null | undefined = result.recommendation?.recommendedSchedule
): TimeOfDayPlanColorContext => {
  const amPeakMinutes =
    getPlanBoundaryMinutes(result.recommendation?.amPeakTime ?? undefined) ??
    null
  const middayMinutes =
    getPlanBoundaryMinutes(
      result.recommendation?.middayValleyTime ?? undefined
    ) ?? null
  const pmPeakMinutes =
    getPlanBoundaryMinutes(result.recommendation?.pmPeakTime ?? undefined) ??
    null

  return {
    amPeakMinutes,
    middayMinutes,
    pmPeakMinutes,
    amPlanIndex:
      getPlanIndexForMinutes(plans, amPeakMinutes) ??
      getFallbackPlanIndex(plans, 'am'),
    middayPlanIndex:
      getPlanIndexForMinutes(plans, middayMinutes) ??
      getFallbackPlanIndex(plans, 'midday'),
    pmPlanIndex:
      getPlanIndexForMinutes(plans, pmPeakMinutes) ??
      getFallbackPlanIndex(plans, 'pm'),
  }
}

export const getTimeOfDayPlanBackgroundColor = (
  plan: Plan,
  context?: TimeOfDayPlanColorContext,
  planIndex?: number
) => {
  if (
    planIndex === context?.amPlanIndex ||
    planIntervalContainsMinutes(plan, context?.amPeakMinutes)
  ) {
    return chartColors.amPlanBackground
  }

  if (
    planIndex === context?.middayPlanIndex ||
    planIntervalContainsMinutes(plan, context?.middayMinutes)
  ) {
    return chartColors.middayPlanBackground
  }

  if (
    planIndex === context?.pmPlanIndex ||
    planIntervalContainsMinutes(plan, context?.pmPeakMinutes)
  ) {
    return chartColors.pmPlanBackground
  }

  const normalizedDescription = normalizeToken(plan.planDescription)

  if (normalizedDescription.includes('ampeak')) {
    return chartColors.amPlanBackground
  }

  if (normalizedDescription.includes('midday')) {
    return chartColors.middayPlanBackground
  }

  if (normalizedDescription.includes('pmpeak')) {
    return chartColors.pmPlanBackground
  }

  return chartColors.defaultPlanBackground
}

/**
 * Plan windows are painted opaque so overlapping proposed and existing bands
 * read the same as a single band; the hatching marks where they differ.
 */
const flattenColorOnWhite = (color: string, weight: number) => {
  const hex = color.replace('#', '')
  if (!/^[0-9a-f]{6}$/i.test(hex)) return color

  const [red, green, blue] = [0, 2, 4].map((offset) =>
    Math.round(
      255 + (Number.parseInt(hex.slice(offset, offset + 2), 16) - 255) * weight
    )
  )

  return `rgb(${red}, ${green}, ${blue})`
}

// Matches the tone two stacked 14% bands used to produce, which is what the
// chart showed whenever both schedules were on.
const planWindowColorWeight = 0.26

const buildPlanMarkAreas = (
  plans: Plan[] | null | undefined,
  label: string,
  context?: TimeOfDayPlanColorContext
) => {
  const markAreas: Array<[Record<string, unknown>, Record<string, unknown>]> =
    []
  const scheduleColorMap = getSchedulePlanColorMap([plans ?? []])
  const contextualColorByPlan = new Map<string, string>()

  plans?.forEach((plan, index) => {
    const planNumber = formatPlanNumber(plan.planNumber)
    const contextualColor = getTimeOfDayPlanBackgroundColor(
      plan,
      context,
      index
    )
    if (
      planNumber !== 'FREE' &&
      contextualColor !== chartColors.defaultPlanBackground &&
      !contextualColorByPlan.has(planNumber)
    ) {
      contextualColorByPlan.set(planNumber, contextualColor)
    }
  })

  plans?.forEach((plan, index) => {
    const interval = getPlanIntervalMinutes(plan)
    if (!interval) return

    const planNumber = formatPlanNumber(plan.planNumber)
    const contextualColor = getTimeOfDayPlanBackgroundColor(
      plan,
      context,
      index
    )
    const isFreePlan = planNumber === 'FREE'
    const color = isFreePlan
      ? '#ffffff'
      : contextualColor !== chartColors.defaultPlanBackground
        ? contextualColor
        : (contextualColorByPlan.get(planNumber) ??
          scheduleColorMap.get(planNumber) ??
          chartColors.amPlanBackground)

    markAreas.push([
      {
        name: `${label} ${planNumber}`.trim(),
        xAxis: interval.start,
        itemStyle: {
          color: isFreePlan
            ? color
            : flattenColorOnWhite(color, planWindowColorWeight),
          opacity: isFreePlan ? 0 : 1,
        },
      },
      { xAxis: interval.end },
    ])
  })

  return markAreas
}

const getPlanDifferenceMarkAreas = (
  result: TimeOfDayResult
): Array<[Record<string, unknown>, Record<string, unknown>]> => {
  if (
    !result.recommendation?.recommendedSchedule?.length ||
    !result.planComparison?.commonCurrentSchedule?.length
  ) {
    return []
  }

  return buildScheduleRows(result)
    .filter((row) => row.comparison !== 'Same')
    .map((row) => [
      {
        name: 'Existing and proposed plans differ',
        xAxis: row.startMinutes,
        itemStyle: {
          color: 'rgba(245, 158, 11, 0.04)',
          decal: {
            symbol: 'rect',
            symbolSize: 1,
            color: 'rgba(245, 158, 11, 0.6)',
            backgroundColor: 'rgba(255, 255, 255, 0)',
            dashArrayX: [1, 0],
            dashArrayY: [4, 4],
            rotation: -Math.PI / 4,
          },
        },
      },
      { xAxis: row.endMinutes },
    ])
}

type PlanDifferenceOverlayDatum = [
  number,
  number,
  string,
  string,
  string,
  string,
]

interface TimeOfDayTooltipParams {
  value?: unknown
}

const tooltipHtmlEntities: Record<string, string> = {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
}

const escapeTooltipHtml = (value: unknown) =>
  String(value ?? '').replace(
    /[&<>]/g,
    (character) => tooltipHtmlEntities[character]
  )

const getTooltipValues = (params: unknown) => {
  const value = (params as TimeOfDayTooltipParams | undefined)?.value
  return Array.isArray(value) ? value : []
}

export const formatTimeOfDayLocationLabel = (
  identifier?: string | null,
  description?: string | null
) => {
  if (identifier && description) {
    return description.includes(identifier)
      ? description
      : `${identifier} - ${description}`
  }

  return description ?? identifier ?? undefined
}

interface TimeOfDayAxisTooltipParam {
  axisValue?: unknown
  color?: unknown
  data?: unknown
  marker?: string
  seriesName?: string
  value?: unknown
}

const starTooltipMarker = (color: unknown) =>
  `<span style="display:inline-block;margin-right:5px;font-size:13px;line-height:1;color:${
    typeof color === 'string' ? escapeTooltipHtml(color) : 'currentColor'
  }">★</span>`

const getAxisTooltipPointValue = (param: TimeOfDayAxisTooltipParam) =>
  Array.isArray(param.value) ? param.value[1] : param.value

const getAxisTooltipPointName = (param: TimeOfDayAxisTooltipParam) => {
  const data = param.data
  if (!data || typeof data !== 'object' || Array.isArray(data)) return undefined

  const name = (data as { tooltipName?: unknown }).tooltipName

  return typeof name === 'string' && name.trim() ? name.trim() : undefined
}

const getAxisTooltipPointLabel = (param: TimeOfDayAxisTooltipParam) => {
  const label = Array.isArray(param.value) ? param.value[2] : undefined

  return typeof label === 'string' && label.trim() ? label.trim() : undefined
}

const formatAxisTooltipValue = (value: unknown, isPercent: boolean) => {
  if (typeof value !== 'number' || !Number.isFinite(value)) {
    return String(value ?? '-')
  }

  return isPercent ? `${formatNumber(value, 1)}%` : formatNumber(value)
}

const buildAxisTooltipFormatter =
  (percentSeriesNames: Set<string>, starSeriesNames: Set<string>) =>
  (params: unknown) => {
    const entries = (Array.isArray(params) ? params : [params]).filter(
      (entry): entry is TimeOfDayAxisTooltipParam => Boolean(entry)
    )
    if (!entries.length) return ''

    const axisValue = Number(entries[0].axisValue)
    const header = Number.isFinite(axisValue)
      ? minutesToTimeLabel(axisValue)
      : ''
    const rows = entries.map((entry) => {
      const seriesName = entry.seriesName ?? ''
      const pointLabel = getAxisTooltipPointLabel(entry)
      const name =
        getAxisTooltipPointName(entry) ??
        (pointLabel && pointLabel !== seriesName
          ? `${seriesName} – ${pointLabel}`
          : seriesName)
      const value = formatAxisTooltipValue(
        getAxisTooltipPointValue(entry),
        percentSeriesNames.has(seriesName)
      )

      const marker = starSeriesNames.has(seriesName)
        ? starTooltipMarker(entry.color)
        : (entry.marker ?? '')

      return `<div style="display:flex;align-items:center;justify-content:space-between;gap:16px"><span>${marker}${escapeTooltipHtml(
        name
      )}</span><strong>${escapeTooltipHtml(value)}</strong></div>`
    })

    return [
      header
        ? `<div style="margin-bottom:4px">${escapeTooltipHtml(header)}</div>`
        : '',
      ...rows,
    ].join('')
  }

const getStarSeriesNames = (series: SeriesOption[]) =>
  new Set(
    series
      .filter(
        (seriesOption) =>
          (seriesOption as SeriesOption & { symbol?: unknown }).symbol ===
          StarSeriesSymbol
      )
      .map((seriesOption) =>
        typeof seriesOption.name === 'string' ? seriesOption.name : ''
      )
      .filter(Boolean)
  )

const getPercentSeriesNames = (series: SeriesOption[]) =>
  new Set(
    series
      .filter(
        (seriesOption) =>
          (seriesOption as SeriesOption & { yAxisIndex?: number })
            .yAxisIndex === 1
      )
      .map((seriesOption) =>
        typeof seriesOption.name === 'string' ? seriesOption.name : ''
      )
      .filter(Boolean)
  )

const getPlanDisplayLabel = (plan: unknown) => {
  const planName = String(plan ?? '-')
  return planName === 'FREE' || planName === 'No plan'
    ? planName
    : `Plan ${planName}`
}

const getPlanDescription = (plan: unknown, description: unknown) => {
  const planName = String(plan ?? '-')
  const value = String(description ?? '').trim()
  if (
    !value ||
    value === '-' ||
    normalizeToken(value) === normalizeToken(planName) ||
    normalizeToken(value) === normalizeToken(`Plan ${planName}`)
  ) {
    return undefined
  }

  return value
}

const formatTooltipPlan = (plan: unknown, description: unknown) => {
  const label = getPlanDisplayLabel(plan)
  const planDescription = getPlanDescription(plan, description)
  return planDescription ? `${label} — ${planDescription}` : label
}

const formatScheduleRailTooltip = (
  scheduleLabel: 'Proposed' | 'Existing',
  params: unknown
) => {
  const [start, end, , plan, , description] = getTooltipValues(params)
  return [
    `<strong>${scheduleLabel} schedule</strong>`,
    `${minutesToTimeLabel(Number(start))}–${minutesToTimeLabel(Number(end))}`,
    escapeTooltipHtml(formatTooltipPlan(plan, description)),
  ].join('<br/>')
}

const formatScheduleDifferenceTooltip = (params: unknown) => {
  const [
    start,
    end,
    proposedPlan,
    proposedDescription,
    existingPlan,
    existingDescription,
  ] = getTooltipValues(params)

  return [
    '<strong>Schedules differ</strong>',
    `${minutesToTimeLabel(Number(start))}–${minutesToTimeLabel(Number(end))}`,
    `Proposed: ${escapeTooltipHtml(
      formatTooltipPlan(proposedPlan, proposedDescription)
    )}`,
    `Existing: ${escapeTooltipHtml(
      formatTooltipPlan(existingPlan, existingDescription)
    )}`,
  ].join('<br/>')
}

interface DiagonalStripeSegment {
  x1: number
  y1: number
  x2: number
  y2: number
}

const getPlanDifferenceOverlayData = (
  result: TimeOfDayResult
): PlanDifferenceOverlayDatum[] => {
  if (
    !result.recommendation?.recommendedSchedule?.length ||
    !result.planComparison?.commonCurrentSchedule?.length
  ) {
    return []
  }

  return buildScheduleRows(result)
    .filter((row) => row.comparison !== 'Same')
    .map((row) => [
      row.startMinutes,
      row.endMinutes,
      row.recommended?.plan ?? 'No plan',
      row.recommended?.description ?? '',
      row.current?.plan ?? 'No plan',
      row.current?.description ?? '',
    ])
}

const getDiagonalStripeSegments = (rect: {
  x: number
  y: number
  width: number
  height: number
}): DiagonalStripeSegment[] => {
  const spacing = 16
  const right = rect.x + rect.width
  const bottom = rect.y + rect.height
  const firstDiagonal = Math.floor((rect.x + rect.y) / spacing) * spacing
  const lastDiagonal = right + bottom
  const segments: DiagonalStripeSegment[] = []

  for (
    let diagonal = firstDiagonal;
    diagonal <= lastDiagonal;
    diagonal += spacing
  ) {
    const candidates = [
      { x: diagonal - rect.y, y: rect.y },
      { x: diagonal - bottom, y: bottom },
      { x: rect.x, y: diagonal - rect.x },
      { x: right, y: diagonal - right },
    ].filter(
      (point) =>
        point.x >= rect.x - 0.5 &&
        point.x <= right + 0.5 &&
        point.y >= rect.y - 0.5 &&
        point.y <= bottom + 0.5
    )
    const points = candidates.filter(
      (point, index) =>
        candidates.findIndex(
          (candidate) =>
            Math.abs(candidate.x - point.x) < 0.5 &&
            Math.abs(candidate.y - point.y) < 0.5
        ) === index
    )

    if (points.length < 2) continue

    segments.push({
      x1: points[0].x,
      y1: points[0].y,
      x2: points[1].x,
      y2: points[1].y,
    })
  }

  return segments
}

const renderPlanDifferenceOverlay = (
  params: CustomSeriesRenderItemParams,
  api: CustomSeriesRenderItemAPI
): CustomSeriesRenderItemReturn => {
  const coordSys = params.coordSys as unknown as {
    x: number
    y: number
    width: number
    height: number
  }
  const start = api.coord([api.value(0), 0])
  const end = api.coord([api.value(1), 0])
  const rectShape = graphic.clipRectByRect(
    {
      x: start[0],
      y: coordSys.y,
      width: end[0] - start[0],
      height: coordSys.height,
    },
    coordSys
  )
  if (!rectShape) return

  return {
    type: 'group',
    children: [
      {
        type: 'rect',
        shape: rectShape,
        style: { fill: 'rgba(226, 232, 240, 0.78)' },
      },
      ...getDiagonalStripeSegments(rectShape).map((segment) => ({
        type: 'line' as const,
        shape: segment,
        style: {
          stroke: 'rgba(245, 158, 11, 0.72)',
          lineWidth: 1,
        },
      })),
      ...[rectShape.x, rectShape.x + rectShape.width].map((x) => ({
        type: 'line' as const,
        shape: {
          x1: x,
          y1: rectShape.y,
          x2: x,
          y2: rectShape.y + rectShape.height,
        },
        style: {
          stroke: 'rgba(217, 119, 6, 0.9)',
          lineWidth: 1.25,
          lineDash: [5, 4],
        },
      })),
    ],
  }
}

const getPlanMarkAreas = (result: TimeOfDayResult) => [
  ...buildPlanMarkAreas(
    result.recommendation?.recommendedSchedule,
    'Recommended',
    getPlanColorContext(result, result.recommendation?.recommendedSchedule)
  ),
  ...getPlanDifferenceMarkAreas(result),
]

// Plan windows and the difference hatching sit under the grid lines, which in
// turn sit under the data series, so nothing paints over the profile lines.
// The hatching sits above the windows themselves, which are opaque.
const scheduleContextZ = -2
const scheduleDifferenceZ = -1

// Grid lines are translucent slate so they tint whatever sits beneath them,
// the white plot or a plan window, instead of drawing pale lines over color.
const gridLineColor = 'rgba(71, 85, 105, 0.18)'
const minorGridLineColor = 'rgba(71, 85, 105, 0.09)'
const valueGridLineColor = 'rgba(71, 85, 105, 0.15)'

const buildScheduleContextSeries = (
  result: TimeOfDayResult
): SeriesOption[] => {
  const proposedWindows = buildPlanMarkAreas(
    result.recommendation?.recommendedSchedule,
    'Proposed',
    getPlanColorContext(result, result.recommendation?.recommendedSchedule)
  )
  const existingWindows = buildPlanMarkAreas(
    result.planComparison?.commonCurrentSchedule,
    'Existing',
    getPlanColorContext(result, result.planComparison?.commonCurrentSchedule)
  )
  const differenceWindows = getPlanDifferenceOverlayData(result)
  const buildContextSeries = (
    id: string,
    name: string,
    markAreaData: Array<[Record<string, unknown>, Record<string, unknown>]>
  ): SeriesOption => ({
    id,
    name,
    type: 'line',
    data: [],
    showSymbol: false,
    silent: true,
    z: scheduleContextZ,
    lineStyle: { opacity: 0 },
    markArea: {
      silent: true,
      z: scheduleContextZ,
      label: { show: false },
      data: markAreaData,
    },
  })

  return [
    ...(proposedWindows.length
      ? [
          buildContextSeries(
            'tod-proposed-plan-windows',
            'Proposed plan windows',
            proposedWindows
          ),
        ]
      : []),
    ...(existingWindows.length
      ? [
          buildContextSeries(
            'tod-existing-plan-windows',
            'Existing plan windows',
            existingWindows
          ),
        ]
      : []),
    ...(differenceWindows.length
      ? [
          {
            id: 'tod-plan-difference-windows',
            name: 'Plan difference windows',
            type: 'custom',
            renderItem: renderPlanDifferenceOverlay,
            dimensions: [
              'Start',
              'End',
              'Proposed Plan',
              'Proposed Description',
              'Existing Plan',
              'Existing Description',
            ],
            encode: { x: [0, 1] },
            data: differenceWindows,
            silent: false,
            cursor: 'help',
            tooltip: {
              show: true,
              trigger: 'item',
              formatter: formatScheduleDifferenceTooltip,
            },
            animation: false,
            z: scheduleDifferenceZ,
          } as SeriesOption,
        ]
      : []),
  ]
}

const buildPeakScatterSeries = (
  peaks: TimeOfDayPeakEventDto[] | null | undefined,
  name: string,
  color: string,
  yAxisIndex = 0,
  symbolSize = 13,
  z = peakMarkerZ,
  resolveColor?: (peak: TimeOfDayPeakEventDto) => string
): SeriesOption => ({
  name,
  type: 'scatter',
  yAxisIndex,
  z,
  data:
    peaks?.map((peak) => {
      const value = [
        peak.minutes ?? 0,
        peak.value ?? 0,
        formatTimeOfDayLocationLabel(
          peak.locationIdentifier,
          peak.locationDescription
        ) ?? '',
      ]
      const pointColor = resolveColor?.(peak)

      return {
        value,
        tooltipName: peak.label?.trim() || undefined,
        ...(pointColor ? { itemStyle: { color: pointColor } } : {}),
      }
    }) ?? [],
  symbol: StarSeriesSymbol,
  symbolSize,
  itemStyle: {
    color,
    borderColor: '#ffffff',
    borderWidth: 1,
  },
  tooltip: {
    valueFormatter: (value) =>
      typeof value === 'number' ? numberFormatter.format(value) : String(value),
  },
})

const getNumberedPeakTooltipLabel = (peak: TimeOfDayNumberedPeakEvent) =>
  [
    formatTimeOfDayLocationLabel(
      peak.locationIdentifier,
      peak.locationDescription
    ),
    peak.movementLabel?.trim(),
  ]
    .filter(Boolean)
    .join(' · ')

const getNumberedPeakTooltipName = (peak: TimeOfDayNumberedPeakEvent) => {
  const locationLabel = formatTimeOfDayLocationLabel(
    peak.locationIdentifier,
    peak.locationDescription
  )
  const peakLabel = peak.label?.trim()

  return peakLabel && locationLabel
    ? `${peakLabel} - ${locationLabel}`
    : undefined
}

const buildNumberedSignalPeakSeries = (
  peaks: TimeOfDayNumberedPeakEvent[],
  name: string,
  color: string,
  yAxisIndex = 0
): SeriesOption => ({
  name,
  type: 'scatter',
  yAxisIndex,
  z: peakMarkerZ,
  data: peaks.map((peak) => ({
    name: String(peak.badgeNumber),
    value: [
      peak.minutes ?? 0,
      peak.value ?? 0,
      getNumberedPeakTooltipLabel(peak),
    ],
    symbol: peak.markerSymbol ?? 'circle',
    detailKey: peak.detailKey,
    tooltipName: getNumberedPeakTooltipName(peak),
    itemStyle: {
      color: peak.badgeColor,
      opacity: 0.82,
    },
  })),
  symbol: peaks[0]?.markerSymbol ?? 'circle',
  symbolSize: 18,
  label: {
    show: true,
    formatter: '{b}',
    color: '#ffffff',
    fontSize: 9,
    fontWeight: 600,
  },
  itemStyle: {
    borderWidth: 0,
    color,
    opacity: 0.82,
  },
  tooltip: {
    valueFormatter: (value) =>
      typeof value === 'number' ? numberFormatter.format(value) : String(value),
  },
})
const withPlanMarkAreas = (
  series: SeriesOption[],
  result: TimeOfDayResult
): SeriesOption[] => {
  const markAreaData = getPlanMarkAreas(result)
  if (!series.length || !markAreaData.length) return series

  return [
    {
      ...series[0],
      markArea: {
        silent: true,
        label: { show: false },
        data: markAreaData,
      },
    },
    ...series.slice(1),
  ]
}

const createTimeOfDayTitle = ({
  title,
  dateRange,
  info,
}: {
  title: string
  dateRange?: string
  info?: string
}) => {
  const titleTops = [0]

  if (dateRange) {
    titleTops.push(32)
  }

  if (info) {
    titleTops.push(dateRange ? 58 : 32)
  }

  return createTitle({ title, dateRange, info }).map((titleOption, index) => ({
    ...titleOption,
    left: 0,
    top: titleTops[index],
    textAlign: 'left' as const,
  }))
}

const buildBaseOption = ({
  result,
  title,
  series,
  yAxis,
  top = 190,
  right,
  legendData,
  legendConfig,
  showLegend = true,
  dateRange = '',
  info,
  externalHeader = false,
}: {
  result: TimeOfDayResult
  title: string
  series: SeriesOption[]
  yAxis: EChartsOption['yAxis']
  right?: number
  legendData?: LegendComponentOption['data']
  legendConfig?: Partial<LegendComponentOption>
  top?: number
  showLegend?: boolean
  dateRange?: string
  info?: string
  externalHeader?: boolean
}): EChartsOption => {
  const yAxes = (
    Array.isArray(yAxis) ? yAxis : yAxis === undefined ? [] : [yAxis]
  ).filter((axis): axis is NonNullable<typeof axis> => axis !== undefined)
  const scheduleYAxisIndex = yAxes.length
  const scheduleSeries = buildScheduleOverlaySeries(
    result,
    1,
    scheduleYAxisIndex
  )
  const hasScheduleRails = scheduleSeries.length > 0
  const scheduleTop = externalHeader ? 24 : 108
  const scheduleGridHeight = externalHeader ? 76 : 52
  const scheduleGridGap = externalHeader ? 12 : 9
  const plotTop = externalHeader
    ? hasScheduleRails
      ? scheduleTop + scheduleGridHeight + scheduleGridGap
      : scheduleTop
    : top
  const grid = createGrid({
    left: 90,
    right,
    top: plotTop,
    bottom: externalHeader ? 116 : 110,
    borderColor: '#d0d5dd',
    borderWidth: 1,
  })
  const dataZoom = createDataZoom([
    {
      type: 'slider',
      xAxisIndex: hasScheduleRails ? [0, 1] : 0,
      start: 0,
      end: 100,
      left: grid.left,
      right: grid.right,
      bottom: 24,
      height: 26,
    },
    {
      type: 'inside',
      xAxisIndex: hasScheduleRails ? [0, 1] : 0,
      start: 0,
      end: 100,
    },
  ])
  const xAxis = {
    type: 'value' as const,
    min: 0,
    max: 1440,
    interval: 60,
    name: 'Time of Day',
    nameLocation: 'middle' as const,
    nameGap: 42,
    axisLabel: {
      formatter: (value: number) => minutesToTimeLabel(value),
    },
    splitLine: {
      show: true,
      lineStyle: {
        color: gridLineColor,
      },
    },
    minorTick: {
      show: true,
      splitNumber: 4,
    },
    minorSplitLine: {
      show: true,
      lineStyle: {
        color: minorGridLineColor,
      },
    },
  }
  return {
    title: externalHeader
      ? []
      : createTimeOfDayTitle({ title, dateRange, info }),
    color: [
      chartColors.raw,
      chartColors.smooth,
      chartColors.primary,
      chartColors.cross,
      chartColors.percent,
    ],
    grid: hasScheduleRails
      ? [
          grid,
          {
            left: 90,
            right,
            top: scheduleTop,
            height: scheduleGridHeight,
            borderColor: '#d0d5dd',
            borderWidth: 1,
          },
        ]
      : grid,
    legend: createLegend({
      data: legendData,
      orient: 'vertical',
      show: showLegend,
      right: 0,
      top,
      type: 'scroll',
      backgroundColor: '#f0f0f0',
      borderRadius: 5,
      padding: [10, 12],
      ...legendConfig,
    }),
    tooltip: {
      trigger: 'axis',
      formatter: buildAxisTooltipFormatter(
        getPercentSeriesNames(series),
        getStarSeriesNames(series)
      ),
    },
    xAxis: hasScheduleRails
      ? [
          xAxis,
          {
            type: 'value',
            gridIndex: 1,
            min: 0,
            max: 1440,
            axisLabel: { show: false },
            axisLine: { show: false },
            axisTick: { show: false },
            splitLine: { show: false },
            axisPointer: { show: false },
          },
        ]
      : xAxis,
    yAxis: hasScheduleRails
      ? [
          ...yAxes,
          {
            type: 'category',
            gridIndex: 1,
            data: ['Existing', 'Proposed'],
            axisTick: { show: false },
            axisLine: { show: false },
            axisPointer: { show: false },
            triggerEvent: true,
            axisLabel: {
              color: '#475467',
              fontSize: 12,
              fontWeight: 600,
            },
          },
        ]
      : yAxes,
    dataZoom,
    series: [...series, ...scheduleSeries],
  }
}

export const getTimeOfDayPeriodBadgeColor = (period?: string | null) => {
  const normalizedPeriod = normalizeToken(period)

  if (normalizedPeriod.startsWith('am')) return chartColors.amSignalPeak
  if (normalizedPeriod.startsWith('midday')) return chartColors.middaySignalPeak
  if (normalizedPeriod.startsWith('pm')) return chartColors.pmSignalPeak

  return chartColors.pmSignalPeak
}

export const getLocationNumber = (
  locationNumberMap: TimeOfDayLocationNumberMap,
  locationIdentifier?: string | null
) => locationNumberMap[normalizeToken(locationIdentifier)]

const getProfileDisplayName = (
  profile: TimeOfDayProfileDto,
  fallback: string
) => profile.label ?? profile.direction ?? profile.movementLabel ?? fallback

const formatDirectionProfileName = (
  profile: TimeOfDayProfileDto,
  index: number
) => {
  const name = getProfileDisplayName(profile, `Direction ${index + 1}`)

  return normalizeToken(name).includes('totalprofile')
    ? name
    : `${name} total profile`
}

const formatDirectionList = (directions?: string[] | null) =>
  directions?.filter(Boolean).join(', ') ?? ''

const formatRepresentativeSeriesName = (
  directions: string[] | null | undefined,
  role: string,
  fallback: string
) => {
  const directionList = formatDirectionList(directions)

  return directionList
    ? `Representative ${directionList} ${role}`
    : `Representative ${fallback}`
}

const peakPeriodMatches = (peak: TimeOfDayPeakEventDto, period: string) =>
  normalizeToken(peak.period).startsWith(normalizeToken(period))

const getPressurePeakColor = (
  peak: TimeOfDayPeakEventDto,
  fallbackColor: string
) => {
  if (peakPeriodMatches(peak, 'AM')) return chartColors.amPeak
  if (peakPeriodMatches(peak, 'Midday')) return chartColors.middaySignalPeak
  if (peakPeriodMatches(peak, 'PM')) return chartColors.pmPeak

  return fallbackColor
}

const isSignalPeak = (peak: TimeOfDayPeakEventDto) =>
  normalizeToken(peak.series) === 'location' || Boolean(peak.locationIdentifier)

const isCorridorPeak = (peak: TimeOfDayPeakEventDto) =>
  !isSignalPeak(peak) &&
  (normalizeToken(peak.series).includes('corridor') ||
    normalizeToken(peak.label).includes('corridor'))

const getCorridorPeakEvents = (
  peaks: TimeOfDayPeakEventDto[] | null | undefined,
  period: 'AM' | 'PM'
) =>
  peaks?.filter(
    (peak) => isCorridorPeak(peak) && peakPeriodMatches(peak, period)
  ) ?? []

const getSignalPeakEvents = (
  peaks: TimeOfDayPeakEventDto[] | null | undefined
) => peaks?.filter(isSignalPeak) ?? []

const getSignalPeakBadgeColor = (peak: TimeOfDayPeakEventDto) => {
  if (peakPeriodMatches(peak, 'AM')) return chartColors.amSignalPeak
  if (peakPeriodMatches(peak, 'PM')) return chartColors.pmSignalPeak

  return (peak.minutes ?? 0) < 12 * 60
    ? chartColors.amSignalPeak
    : chartColors.pmSignalPeak
}

const getNumberedSignalPeakEvents = (
  peaks: TimeOfDayPeakEventDto[] | null | undefined
): TimeOfDayNumberedPeakEvent[] => {
  const locationNumbers = new Map<string, number>()
  let nextBadgeNumber = 1

  return getSignalPeakEvents(peaks).map((peak) => {
    const locationKey = normalizeToken(peak.locationIdentifier)
    let badgeNumber = locationKey
      ? locationNumbers.get(locationKey)
      : undefined

    if (badgeNumber === undefined) {
      badgeNumber = nextBadgeNumber
      nextBadgeNumber += 1

      if (locationKey) {
        locationNumbers.set(locationKey, badgeNumber)
      }
    }

    return {
      ...peak,
      badgeNumber,
      badgeColor: getSignalPeakBadgeColor(peak),
    }
  })
}

const createLegendItem = (name: string, icon: string, color: string) => ({
  name,
  icon,
  itemStyle: { color },
})

const getPlanProfileLegendData = (
  directionalSeriesNames: string[]
): LegendComponentOption['data'] => [
  createLegendItem('Median Raw Volume', SolidLineSeriesSymbol, chartColors.raw),
  createLegendItem(
    'Smoothed For Breakpoints',
    SolidLineSeriesSymbol,
    chartColors.smooth
  ),
  ...directionalSeriesNames.map((name, index) =>
    createLegendItem(
      name,
      DashedLineSeriesSymbol,
      directionalColors[index % directionalColors.length]
    )
  ),
  createLegendItem('AM Corridor Peak', StarSeriesSymbol, chartColors.amPeak),
  createLegendItem('PM Corridor Peak', StarSeriesSymbol, chartColors.pmPeak),
  createLegendItem('AM Signal Peaks', 'circle', chartColors.amSignalPeak),
  createLegendItem('PM Signal Peaks', 'circle', chartColors.pmSignalPeak),
]

const formatPercentThresholdName = (value: number, label: string) =>
  `${formatNumber(value, 1)}% ${label}`

const getThresholdPercent = (
  thresholds: Record<string, number> | null | undefined,
  candidates: string[],
  fallback: number
) => {
  const normalizedCandidates = candidates.map(normalizeToken)
  const entry = Object.entries(thresholds ?? {}).find(([name]) =>
    normalizedCandidates.includes(normalizeToken(name))
  )

  return entry?.[1] ?? fallback
}

const buildPercentThresholdSeries = (
  name: string,
  value: number,
  color: string
): SeriesOption => ({
  name,
  type: 'line',
  yAxisIndex: 1,
  data: [
    [0, value],
    [1440, value],
  ],
  showSymbol: false,
  symbol: 'none',
  silent: true,
  lineStyle: {
    width: 1.5,
    type: 'dashed',
    color,
  },
  itemStyle: { color },
  tooltip: {
    valueFormatter: (tooltipValue) =>
      typeof tooltipValue === 'number'
        ? `${formatNumber(tooltipValue, 1)}%`
        : String(tooltipValue),
  },
})

export const buildPlanProfileOption = (
  result: TimeOfDayResult
): EChartsOption => {
  const corridorProfile = result.planProfile?.corridorProfile
  const directionalProfiles = result.planProfile?.directionalProfiles ?? []

  const directionalSeriesNames = directionalProfiles.map((profile, index) =>
    formatDirectionProfileName(profile, index)
  )
  const directionalSeries = directionalProfiles.map((profile, index) =>
    buildProfileLineSeries({
      profile,
      name: directionalSeriesNames[index],
      valueKey: 'averageVolume',
      color: directionalColors[index % directionalColors.length],
      lineStyle: { width: 1.5, opacity: 0.75, type: 'dashed' },
    })
  )

  const amCorridorPeaks = getCorridorPeakEvents(result.planProfile?.peaks, 'AM')
  const pmCorridorPeaks = getCorridorPeakEvents(result.planProfile?.peaks, 'PM')
  const amSignalPeaks = getLocationPeakEvents(
    result.planProfile?.peaks,
    'AM'
  ).map((peak) => ({
    ...peak,
    detailKey: getTimeOfDaySignalPeakDetailKey(peak),
  }))
  const pmSignalPeaks = getLocationPeakEvents(
    result.planProfile?.peaks,
    'PM'
  ).map((peak) => ({
    ...peak,
    detailKey: getTimeOfDaySignalPeakDetailKey(peak),
  }))

  const amPeakSeries = buildPeakScatterSeries(
    amCorridorPeaks,
    'AM Corridor Peak',
    chartColors.amPeak,
    0,
    15,
    corridorPeakMarkerZ
  )
  const pmPeakSeries = buildPeakScatterSeries(
    pmCorridorPeaks,
    'PM Corridor Peak',
    chartColors.pmPeak,
    0,
    15,
    corridorPeakMarkerZ
  )
  const amSignalPeakSeries = buildNumberedSignalPeakSeries(
    amSignalPeaks,
    'AM Signal Peaks',
    chartColors.amSignalPeak
  )
  const pmSignalPeakSeries = buildNumberedSignalPeakSeries(
    pmSignalPeaks,
    'PM Signal Peaks',
    chartColors.pmSignalPeak
  )

  const series = withPlanMarkAreas(
    [
      buildProfileLineSeries({
        profile: corridorProfile,
        name: 'Median Raw Volume',
        valueKey: 'averageVolume',
        color: chartColors.raw,
        lineStyle: { width: 1.5, opacity: 0.85 },
      }),
      buildProfileLineSeries({
        profile: corridorProfile,
        name: 'Smoothed For Breakpoints',
        valueKey: 'smoothedVolume',
        color: chartColors.smooth,
        lineStyle: { width: 3 },
      }),
      ...directionalSeries,
      amPeakSeries,
      pmPeakSeries,
      amSignalPeakSeries,
      pmSignalPeakSeries,
    ],
    result
  )

  const plans = result.recommendation?.recommendedSchedule
  const sharedVolumeAxisMax = getSharedVolumeAxisMax(result)
  const yAxis = createYAxis(Boolean(plans?.length), {
    name: 'Volume (vph)',
    nameGap: 60,
    max: sharedVolumeAxisMax,
    splitLine: {
      lineStyle: { color: valueGridLineColor },
    },
  })

  const titleDateRange = formatSelectedDateRange(result.selectedDates)
  const titleInfo = formatPeakInfo([
    [
      'AM Corridor Peak:',
      formatPeakInfoValue(
        result.recommendation?.amPeakTime ?? amCorridorPeaks[0]?.timeOfDay,
        amCorridorPeaks[0]?.value,
        amCorridorPeaks[0]?.valueUnits ?? 'vph'
      ),
    ],
    [
      'PM Corridor Peak:',
      formatPeakInfoValue(
        result.recommendation?.pmPeakTime ?? pmCorridorPeaks[0]?.timeOfDay,
        pmCorridorPeaks[0]?.value,
        pmCorridorPeaks[0]?.valueUnits ?? 'vph'
      ),
    ],
  ])

  return buildBaseOption({
    result,
    title: 'Corridor Plan Recommendation',
    dateRange: titleDateRange,
    info: titleInfo,
    series,
    legendData: getPlanProfileLegendData(directionalSeriesNames),
    right: 250,
    yAxis,
  })
}

export const buildSplitPressureOption = (
  result: TimeOfDayResult
): EChartsOption => {
  const splitPressure = result.splitPressure
  const crossTrafficPercentData =
    splitPressure?.crossTrafficShare
      ?.map((point) => {
        if (
          point.minutes === undefined ||
          point.crossTrafficPercent === undefined ||
          point.crossTrafficPercent === null
        ) {
          return null
        }

        return [point.minutes, point.crossTrafficPercent]
      })
      .filter((point): point is number[] => point !== null) ?? []

  const splitReviewPercent = getThresholdPercent(
    splitPressure?.thresholdPercentByName,
    ['SplitReview', '35% split review', 'split review'],
    35
  )
  const shoulderReviewPercent = getThresholdPercent(
    splitPressure?.thresholdPercentByName,
    ['ShoulderReview', '45% shoulder review', 'shoulder review'],
    45
  )
  const splitReviewName = formatPercentThresholdName(
    splitReviewPercent,
    'split review'
  )
  const shoulderReviewName = formatPercentThresholdName(
    shoulderReviewPercent,
    'shoulder review'
  )
  const primarySeriesName = formatRepresentativeSeriesName(
    splitPressure?.primaryDirections,
    'primary',
    'primary street'
  )
  const crossSeriesName = formatRepresentativeSeriesName(
    splitPressure?.crossDirections,
    'cross street',
    'cross street'
  )

  const volumePeaks =
    splitPressure?.periodPeaks?.filter((peak) => !isPercentPeakEvent(peak)) ??
    []
  const percentPeaks =
    splitPressure?.periodPeaks?.filter(isPercentPeakEvent) ?? []
  const locationNumberMap = buildSplitPressureLocationNumberMap(result)
  const locationPeakSeries = buildSplitPressureLocationPeakSeries(
    result,
    locationNumberMap
  )
  const unnumberedVolumePeaks = getUnnumberedSplitPressurePeakEvents(
    volumePeaks,
    locationNumberMap
  )
  const unnumberedPercentPeaks = getUnnumberedSplitPressurePeakEvents(
    percentPeaks,
    locationNumberMap
  )

  const series = withPlanMarkAreas(
    [
      buildProfileLineSeries({
        profile: splitPressure?.primaryProfile,
        name: primarySeriesName,
        valueKey: 'averageVolume',
        color: chartColors.primary,
        lineStyle: { width: 2.5 },
      }),
      buildProfileLineSeries({
        profile: splitPressure?.crossStreetProfile,
        name: crossSeriesName,
        valueKey: 'averageVolume',
        color: chartColors.cross,
        lineStyle: { width: 2.5 },
      }),
      {
        name: 'Cross-traffic percent',
        type: 'line',
        yAxisIndex: 1,
        data: crossTrafficPercentData,
        showSymbol: false,
        smooth: true,
        lineStyle: {
          width: 2.5,
          type: 'dashed',
          color: chartColors.percent,
        },
        itemStyle: { color: chartColors.percent },
        tooltip: {
          valueFormatter: (tooltipValue) =>
            typeof tooltipValue === 'number'
              ? `${formatNumber(tooltipValue, 1)}%`
              : String(tooltipValue),
        },
      },
      buildPercentThresholdSeries(
        splitReviewName,
        splitReviewPercent,
        chartColors.splitReview
      ),
      buildPercentThresholdSeries(
        shoulderReviewName,
        shoulderReviewPercent,
        chartColors.shoulderReview
      ),
      ...locationPeakSeries,
      ...(unnumberedVolumePeaks.length
        ? [
            buildPeakScatterSeries(
              unnumberedVolumePeaks,
              'Volume Peaks',
              chartColors.volumePeak,
              0,
              13,
              corridorPeakMarkerZ,
              (peak) => getPressurePeakColor(peak, chartColors.volumePeak)
            ),
          ]
        : []),
      ...(unnumberedPercentPeaks.length
        ? [
            buildPeakScatterSeries(
              unnumberedPercentPeaks,
              'Cross Traffic Percent Peaks',
              chartColors.percent,
              1,
              13,
              corridorPeakMarkerZ,
              (peak) => getPressurePeakColor(peak, chartColors.percent)
            ),
          ]
        : []),
    ],
    result
  )

  const plans = result.recommendation?.recommendedSchedule
  const sharedVolumeAxisMax = getSharedVolumeAxisMax(result)
  const yAxis = createYAxis(
    Boolean(plans?.length),
    {
      name: 'Volume (vph)',
      nameGap: 60,
      max: sharedVolumeAxisMax,
      splitLine: {
        lineStyle: { color: valueGridLineColor },
      },
    },
    {
      name: 'Cross Traffic (%)',
      nameGap: 48,
      min: 0,
      max: (value) => Math.max(100, Math.ceil(value.max / 10) * 10),
      position: 'right',
      axisLabel: {
        formatter: formatPercentAxisLabel,
      },
      axisLine: { show: false },
    }
  )

  const titleDateRange = formatSelectedDateRange(result.selectedDates)
  const titleInfo = formatPeakInfo([
    [
      'Primary Peak:',
      formatPeakInfoValue(
        splitPressure?.primaryPeakTime,
        splitPressure?.primaryPeakVolume,
        'vph'
      ),
    ],
    [
      'Cross Peak:',
      formatPeakInfoValue(
        splitPressure?.crossStreetPeakTime,
        splitPressure?.crossStreetPeakVolume,
        'vph'
      ),
    ],
    [
      'Peak Cross Traffic:',
      formatPeakInfoValue(
        splitPressure?.peakCrossTrafficPercentTime,
        splitPressure?.peakCrossTrafficPercent,
        '%',
        1
      ),
    ],
  ])

  return buildBaseOption({
    result,
    title: 'Corridor Movement Demand',
    dateRange: titleDateRange,
    info: titleInfo,
    series,
    right: 410,
    legendData: [
      createLegendItem(
        primarySeriesName,
        SolidLineSeriesSymbol,
        chartColors.primary
      ),
      createLegendItem(
        crossSeriesName,
        SolidLineSeriesSymbol,
        chartColors.cross
      ),
      createLegendItem(
        'Cross-traffic percent',
        DashedLineSeriesSymbol,
        chartColors.percent
      ),
      createLegendItem(
        splitReviewName,
        DashedLineSeriesSymbol,
        chartColors.splitReview
      ),
      createLegendItem(
        shoulderReviewName,
        DashedLineSeriesSymbol,
        chartColors.shoulderReview
      ),
      createLegendItem(
        'AM Cross Traffic Locations',
        'circle',
        chartColors.amSignalPeak
      ),
      createLegendItem(
        'Midday Cross Traffic Locations',
        'circle',
        chartColors.middaySignalPeak
      ),
      createLegendItem(
        'PM Cross Traffic Locations',
        'circle',
        chartColors.pmSignalPeak
      ),
      createLegendItem('AM Movement Demand', 'rect', chartColors.amSignalPeak),
      createLegendItem('PM Movement Demand', 'rect', chartColors.pmSignalPeak),
    ],
    legendConfig: {
      selected: {
        'AM Movement Demand': false,
        'PM Movement Demand': false,
      },
    },
    yAxis,
  })
}

type ScheduleTimelineDatum = [number, number, number, string, string, string]

const withColorOpacity = (color: string, opacity: number) => {
  const hex = color.replace('#', '')
  if (!/^[0-9a-f]{6}$/i.test(hex)) return color

  const red = Number.parseInt(hex.slice(0, 2), 16)
  const green = Number.parseInt(hex.slice(2, 4), 16)
  const blue = Number.parseInt(hex.slice(4, 6), 16)

  return `rgba(${red}, ${green}, ${blue}, ${opacity})`
}

const fullScheduleTimeLabelWidth = 72
const compactScheduleTimeLabelWidth = 48
const schedulePlanLabelWidth = 32

const getScheduleTimeLabel = (start: number, end: number, width: number) => {
  if (width >= fullScheduleTimeLabelWidth) {
    return `${minutesToTimeLabel(start)}\u2013${minutesToTimeLabel(end)}`
  }

  return width >= compactScheduleTimeLabelWidth ? minutesToTimeLabel(start) : ''
}

const renderSchedulePlanBlock = (
  params: CustomSeriesRenderItemParams,
  api: CustomSeriesRenderItemAPI
): CustomSeriesRenderItemReturn => {
  const start = api.coord([api.value(0), api.value(2)])
  const end = api.coord([api.value(1), api.value(2)])
  const laneSize = api.size?.([0, 1])
  const laneHeight = Array.isArray(laneSize)
    ? Number(laneSize[1])
    : Number(laneSize ?? 0)
  const height = Math.max(0, laneHeight - 6)
  const buttonHeight = Math.max(0, laneHeight)
  const buttonLeftExtension = 72
  const buttonRightExtension = 72
  const coordSys = params.coordSys as unknown as {
    x: number
    y: number
    width: number
    height: number
  }
  const rectShape = graphic.clipRectByRect(
    {
      x: start[0],
      y: start[1] - height / 2,
      width: end[0] - start[0],
      height,
    },
    {
      x: coordSys.x,
      y: coordSys.y,
      width: coordSys.width,
      height: coordSys.height,
    }
  )
  if (!rectShape) return

  const width = Math.max(0, end[0] - start[0])
  const color = String(api.value(4))
  const freePlan = String(api.value(3)) === 'FREE'
  const isProposedSchedule = Number(api.value(2)) === 1
  const timeLabelText = isProposedSchedule
    ? getScheduleTimeLabel(Number(api.value(0)), Number(api.value(1)), width)
    : ''
  const rectElement = {
    type: 'rect' as const,
    shape: rectShape,
    silent: false,
    style: {
      fill: withColorOpacity(color, freePlan ? 0.14 : 0.2),
    },
  }
  const buttonSurface =
    params.dataIndex === 0
      ? {
          type: 'rect' as const,
          shape: {
            x: coordSys.x - buttonLeftExtension,
            y: start[1] - buttonHeight / 2,
            width: coordSys.width + buttonLeftExtension + buttonRightExtension,
            height: buttonHeight,
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
        }
      : undefined
  const timeLabel = timeLabelText
    ? {
        type: 'text' as const,
        silent: true,
        style: {
          x: (start[0] + end[0]) / 2,
          y: start[1] - height / 2 - 8,
          text: timeLabelText,
          fill: '#475569',
          fontSize: 11,
          fontWeight: 600,
          align: 'center' as const,
          verticalAlign: 'middle' as const,
          width: Math.max(0, width - 8),
          overflow: 'truncate' as const,
        },
      }
    : undefined
  const planLabel =
    width >= schedulePlanLabelWidth
      ? {
          type: 'text' as const,
          silent: true,
          style: {
            x: (start[0] + end[0]) / 2,
            y: start[1],
            text: String(api.value(3)),
            fill: color,
            fontSize: 12,
            fontWeight: 700,
            align: 'center' as const,
            verticalAlign: 'middle' as const,
            width: Math.max(0, width - 8),
            overflow: 'truncate' as const,
          },
        }
      : undefined
  const children = [buttonSurface, rectElement, timeLabel, planLabel].filter(
    (child): child is NonNullable<typeof child> => Boolean(child)
  )

  return children.length === 1
    ? rectElement
    : {
        type: 'group',
        children,
      }
}

const getScheduleTimelineData = (
  entries: TimeOfDayScheduleEntry[],
  lane: number,
  colorMap: Map<string, string>
): ScheduleTimelineDatum[] =>
  entries.map(({ plan, interval }) => {
    const planName = formatPlanNumber(plan.planNumber)

    return [
      interval.start,
      interval.end,
      lane,
      planName,
      colorMap.get(planName) ?? freeSchedulePlanColor,
      plan.planDescription ?? '-',
    ]
  })

const buildScheduleOverlaySeries = (
  result: TimeOfDayResult,
  xAxisIndex: number,
  yAxisIndex: number
): SeriesOption[] => {
  const existingEntries = getScheduleEntries(
    result.planComparison?.commonCurrentSchedule
  )
  const proposedEntries = getScheduleEntries(
    result.recommendation?.recommendedSchedule
  )
  if (!existingEntries.length && !proposedEntries.length) return []

  const colorMap = getSchedulePlanColorMap([
    proposedEntries.map(({ plan }) => plan),
    existingEntries.map(({ plan }) => plan),
  ])
  const buildRailSeries = (
    name: string,
    entries: TimeOfDayScheduleEntry[],
    lane: number
  ): SeriesOption => {
    const scheduleLabel = name.startsWith('Existing') ? 'Existing' : 'Proposed'

    return {
      id:
        name === 'Existing schedule rail'
          ? 'tod-existing-schedule-rail'
          : 'tod-proposed-schedule-rail',
      name,
      type: 'custom',
      renderItem: renderSchedulePlanBlock,
      clip: false,
      xAxisIndex,
      yAxisIndex,
      encode: { x: [0, 1], y: 2 },
      dimensions: ['Start', 'End', 'Lane', 'Plan', 'Color', 'Description'],
      tooltip: {
        show: true,
        trigger: 'item',
        formatter: (params) => formatScheduleRailTooltip(scheduleLabel, params),
      },
      data: getScheduleTimelineData(entries, lane, colorMap),
      z: 30,
    }
  }

  return [
    ...(existingEntries.length
      ? [buildRailSeries('Existing schedule rail', existingEntries, 0)]
      : []),
    ...(proposedEntries.length
      ? [buildRailSeries('Proposed schedule rail', proposedEntries, 1)]
      : []),
  ]
}

export const getLocationPeakEvents = (
  peaks: TimeOfDayPeakEventDto[] | null | undefined,
  period: string
) => {
  const locationPeaks = getNumberedSignalPeakEvents(peaks)
  const periodPeaks = locationPeaks.filter((peak) =>
    peakPeriodMatches(peak, period)
  )

  if (periodPeaks.length > 0) {
    return periodPeaks
  }

  return locationPeaks.filter((peak) => {
    if (peak.period || peak.minutes === undefined) return false

    if (period.toLowerCase() === 'am') return peak.minutes < 12 * 60
    if (period.toLowerCase() === 'pm') return peak.minutes >= 12 * 60

    return true
  })
}

export const getCrossTrafficLocations = (
  locations: TimeOfDayCrossTrafficLocationDto[] | null | undefined,
  period: string
) =>
  [
    ...(locations?.filter(
      (location) => location.period?.toLowerCase() === period.toLowerCase()
    ) ?? []),
  ].sort(
    (left, right) =>
      (right.totalVehiclesPerHour ?? 0) - (left.totalVehiclesPerHour ?? 0)
  )

const movementOrder: Record<string, number> = {
  left: 0,
  thru: 1,
  through: 1,
  straight: 1,
  right: 2,
}

const getMovementName = (movement: TimeOfDayMovementPressureDto) =>
  movement.movementLabel ?? movement.movement ?? ''

const compareMovementPressures = (
  left: TimeOfDayMovementPressureDto,
  right: TimeOfDayMovementPressureDto,
  locationNumberMap?: TimeOfDayLocationNumberMap
) => {
  if (locationNumberMap) {
    const leftLocationNumber = getLocationNumber(
      locationNumberMap,
      left.locationIdentifier
    )
    const rightLocationNumber = getLocationNumber(
      locationNumberMap,
      right.locationIdentifier
    )
    const locationNumberComparison =
      (leftLocationNumber ?? Number.MAX_SAFE_INTEGER) -
      (rightLocationNumber ?? Number.MAX_SAFE_INTEGER)
    if (locationNumberComparison !== 0) return locationNumberComparison
  }

  const locationComparison = (left.locationIdentifier ?? '').localeCompare(
    right.locationIdentifier ?? '',
    undefined,
    { numeric: true, sensitivity: 'base' }
  )
  if (locationComparison !== 0) return locationComparison

  const leftMovement = normalizeToken(getMovementName(left))
  const rightMovement = normalizeToken(getMovementName(right))
  const movementOrderComparison =
    (movementOrder[leftMovement] ?? Number.MAX_SAFE_INTEGER) -
    (movementOrder[rightMovement] ?? Number.MAX_SAFE_INTEGER)

  return (
    movementOrderComparison ||
    leftMovement.localeCompare(rightMovement, undefined, {
      sensitivity: 'base',
    })
  )
}

export const getMovementPressures = (
  movements: TimeOfDayMovementPressureDto[] | null | undefined,
  period: string,
  locationNumberMap?: TimeOfDayLocationNumberMap
) =>
  [
    ...(movements?.filter(
      (movement) => movement.period?.toLowerCase() === period.toLowerCase()
    ) ?? []),
  ].sort((left, right) =>
    compareMovementPressures(left, right, locationNumberMap)
  )

const splitPressureLocationPeriods = ['AM', 'Midday', 'PM']
const splitPressureMovementPeriods = ['AM', 'PM']

const addLocationNumber = (
  locationNumberMap: TimeOfDayLocationNumberMap,
  locationIdentifier?: string | null
) => {
  const key = normalizeToken(locationIdentifier)
  if (!key || locationNumberMap[key]) return

  locationNumberMap[key] = Object.keys(locationNumberMap).length + 1
}

export const buildSplitPressureLocationNumberMap = (
  result: TimeOfDayResult
): TimeOfDayLocationNumberMap => {
  const locationNumberMap: TimeOfDayLocationNumberMap = {}

  splitPressureLocationPeriods.forEach((period) => {
    getCrossTrafficLocations(
      result.splitPressure?.crossTrafficLocations,
      period
    ).forEach((location) => {
      addLocationNumber(locationNumberMap, location.locationIdentifier)
    })
  })

  splitPressureMovementPeriods.forEach((period) => {
    getMovementPressures(
      result.splitPressure?.movementPressures,
      period
    ).forEach((movement) => {
      addLocationNumber(locationNumberMap, movement.locationIdentifier)
    })
  })

  result.splitPressure?.periodPeaks?.forEach((peak) => {
    addLocationNumber(locationNumberMap, peak.locationIdentifier)
  })

  return locationNumberMap
}

const buildSplitPressureLocationPeakEvents = (
  result: TimeOfDayResult,
  locationNumberMap: TimeOfDayLocationNumberMap
): TimeOfDayNumberedPeakEvent[] => {
  const locationPeaks: TimeOfDayNumberedPeakEvent[] = []
  const seen = new Set<string>()

  const addPeak = (
    peak: TimeOfDayPeakEventDto & {
      detailKey?: string
      movementLabel?: string
    },
    markerSymbol: 'circle' | 'rect' = 'circle'
  ) => {
    const badgeNumber = getLocationNumber(
      locationNumberMap,
      peak.locationIdentifier
    )
    if (
      !badgeNumber ||
      peak.minutes === undefined ||
      peak.value === undefined
    ) {
      return
    }

    const key = [
      normalizeToken(peak.period),
      normalizeToken(peak.locationIdentifier),
      normalizeToken(peak.movementLabel),
      peak.minutes,
      peak.value,
    ].join('|')
    if (seen.has(key)) return
    seen.add(key)

    locationPeaks.push({
      ...peak,
      badgeNumber,
      badgeColor: getTimeOfDayPeriodBadgeColor(peak.period),
      markerSymbol,
    })
  }

  splitPressureLocationPeriods.forEach((period) => {
    getCrossTrafficLocations(
      result.splitPressure?.crossTrafficLocations,
      period
    ).forEach((location) => {
      addPeak({
        period,
        locationIdentifier: location.locationIdentifier ?? undefined,
        locationDescription: location.locationDescription ?? undefined,
        timeOfDay: location.peakTime ?? undefined,
        minutes:
          location.minutes ??
          getPlanBoundaryMinutes(location.peakTime ?? undefined) ??
          undefined,
        value: location.totalVehiclesPerHour,
        valueUnits: 'vph',
        detailKey: getTimeOfDayCrossTrafficDetailKey(location, period),
      })
    })
  })

  splitPressureMovementPeriods.forEach((period) => {
    getMovementPressures(
      result.splitPressure?.movementPressures,
      period,
      locationNumberMap
    ).forEach((movement) => {
      addPeak(
        {
          period,
          movementLabel:
            movement.movementLabel ?? movement.movement ?? undefined,
          locationIdentifier: movement.locationIdentifier ?? undefined,
          timeOfDay: movement.peakTime ?? undefined,
          minutes:
            getPlanBoundaryMinutes(movement.peakTime ?? undefined) ?? undefined,
          value: movement.volume,
          valueUnits: 'vph',
          detailKey: getTimeOfDayMovementPressureDetailKey(movement, period),
        },
        'rect'
      )
    })
  })

  return locationPeaks
}

const buildSplitPressureLocationPeakSeries = (
  result: TimeOfDayResult,
  locationNumberMap: TimeOfDayLocationNumberMap
): SeriesOption[] => {
  const locationPeaks = buildSplitPressureLocationPeakEvents(
    result,
    locationNumberMap
  )

  return ['AM', 'Midday', 'PM'].flatMap((period) => {
    const periodPeaks = locationPeaks.filter((peak) =>
      peakPeriodMatches(peak, period)
    )
    const crossTrafficPeaks = periodPeaks.filter(
      (peak) => peak.markerSymbol !== 'rect'
    )
    const movementPressurePeaks = periodPeaks.filter(
      (peak) => peak.markerSymbol === 'rect'
    )

    return [
      ...(crossTrafficPeaks.length
        ? [
            buildNumberedSignalPeakSeries(
              crossTrafficPeaks,
              `${period} Cross Traffic Locations`,
              getTimeOfDayPeriodBadgeColor(period)
            ),
          ]
        : []),
      ...(movementPressurePeaks.length
        ? [
            buildNumberedSignalPeakSeries(
              movementPressurePeaks,
              `${period} Movement Demand`,
              getTimeOfDayPeriodBadgeColor(period)
            ),
          ]
        : []),
    ]
  })
}

const getUnnumberedSplitPressurePeakEvents = (
  peaks: TimeOfDayPeakEventDto[],
  locationNumberMap: TimeOfDayLocationNumberMap
) =>
  peaks.filter(
    (peak) => !getLocationNumber(locationNumberMap, peak.locationIdentifier)
  )

const presetLayerIds: Record<TimeOfDayChartPreset, TimeOfDayChartLayerId[]> = {
  recommendation: [
    'raw-volume',
    'smoothed-volume',
    'directional-profiles',
    'corridor-peaks',
    'signal-peaks',
  ],
  pressure: [
    'primary-volume',
    'cross-volume',
    'cross-percent',
    'review-thresholds',
    'pressure-peaks',
    'cross-traffic-locations',
  ],
  combined: [
    'raw-volume',
    'smoothed-volume',
    'directional-profiles',
    'corridor-peaks',
    'primary-volume',
    'cross-volume',
    'cross-percent',
    'review-thresholds',
    'pressure-peaks',
    'signal-peaks',
    'cross-traffic-locations',
  ],
}

const reviewThresholdLayerIds = new Set<TimeOfDayChartLayerId>([
  'split-review-threshold',
  'shoulder-review-threshold',
])

const getPresetLayerId = (
  layerId: TimeOfDayChartLayerId
): TimeOfDayChartLayerId => {
  if (layerId.startsWith('directional-profile-')) {
    return 'directional-profiles'
  }
  if (reviewThresholdLayerIds.has(layerId)) {
    return 'review-thresholds'
  }

  return layerId
}

const scheduleRowLayerIds: TimeOfDayChartLayerId[] = [
  'proposed-schedule',
  'existing-schedule',
]

/**
 * The difference hatching only means something while both schedules are drawn,
 * so it follows them: visible when both rows are, hidden as soon as one is off.
 */
export const withScheduleDifferenceVisibility = (
  layers: TimeOfDayChartLayer[],
  selection: Record<string, boolean>,
  changedSeriesNames: string[]
) => {
  const scheduleRows = layers.filter((layer) =>
    scheduleRowLayerIds.includes(layer.id)
  )
  const changed = new Set(changedSeriesNames)
  const changedASchedule = scheduleRows.some((layer) =>
    layer.seriesNames.some((seriesName) => changed.has(seriesName))
  )
  const differenceLayer = layers.find(
    (layer) => layer.id === 'schedule-differences'
  )
  if (!changedASchedule || !differenceLayer?.available) return selection

  const bothRowsVisible =
    scheduleRows.length === scheduleRowLayerIds.length &&
    scheduleRows.every(
      (layer) =>
        layer.available &&
        layer.seriesNames.every((seriesName) => selection[seriesName] === true)
    )
  const next = { ...selection }
  differenceLayer.seriesNames.forEach((seriesName) => {
    next[seriesName] = bothRowsVisible
  })

  return next
}

export const getTimeOfDayPresetSeriesSelection = (
  layers: TimeOfDayChartLayer[],
  preset: TimeOfDayChartPreset,
  currentSelection: Record<string, boolean> = {}
) => {
  const visibleLayerIds = new Set(presetLayerIds[preset])
  const nextSelection: Record<string, boolean> = {}

  layers.forEach((layer) => {
    if (layer.group === 'Schedules') {
      const hasCurrentSelection = layer.seriesNames.some(
        (seriesName) => currentSelection[seriesName] !== undefined
      )
      const visible =
        layer.available &&
        (hasCurrentSelection
          ? layer.seriesNames.every(
              (seriesName) => currentSelection[seriesName] === true
            )
          : true)

      layer.seriesNames.forEach((seriesName) => {
        nextSelection[seriesName] = visible
      })
      return
    }

    layer.seriesNames.forEach((seriesName) => {
      nextSelection[seriesName] =
        layer.available && visibleLayerIds.has(getPresetLayerId(layer.id))
    })
  })

  return nextSelection
}

const getOptionSeries = (option: EChartsOption): SeriesOption[] => {
  if (!option.series) return []

  return Array.isArray(option.series)
    ? (option.series as SeriesOption[])
    : [option.series as SeriesOption]
}

const getSeriesName = (series: SeriesOption) =>
  typeof series.name === 'string' ? series.name : ''

const withStableSeriesIdentity = (
  series: SeriesOption,
  prefix: string,
  index: number
) => {
  const nextSeries = { ...series } as SeriesOption & {
    markArea?: unknown
  }
  delete nextSeries.markArea

  return {
    ...nextSeries,
    id: `tod-${prefix}-${normalizeToken(getSeriesName(series)) || index}`,
  } as SeriesOption
}

const getUnifiedSourceSeries = (option: EChartsOption, prefix: string) =>
  getOptionSeries(option)
    .filter((series) => !getSeriesName(series).endsWith('schedule rail'))
    .map((series, index) => withStableSeriesIdentity(series, prefix, index))

const seriesHasData = (series?: SeriesOption) => {
  if (!series) return false

  const data = (series as SeriesOption & { data?: unknown[] }).data
  if (Array.isArray(data) && data.length > 0) return true

  const markAreaData = (
    series as SeriesOption & {
      markArea?: { data?: unknown[] }
    }
  ).markArea?.data

  return Array.isArray(markAreaData) && markAreaData.length > 0
}

const buildDetailTargets = (
  series: SeriesOption[],
  layers: TimeOfDayChartLayer[]
) => {
  const targets: Record<string, TimeOfDayChartDetailTarget> = {}
  const layerBySeriesName = new Map<string, TimeOfDayChartLayerId>()

  layers.forEach((layer) => {
    layer.seriesNames.forEach((seriesName) => {
      layerBySeriesName.set(seriesName, layer.id)
    })
  })

  series.forEach((seriesOption) => {
    const seriesName = getSeriesName(seriesOption)
    const layerId = layerBySeriesName.get(seriesName)
    const data = (seriesOption as SeriesOption & { data?: unknown[] }).data
    if (!layerId || !Array.isArray(data)) return

    data.forEach((datum, dataIndex) => {
      if (!datum || typeof datum !== 'object' || Array.isArray(datum)) return

      const detailKey = (datum as { detailKey?: unknown }).detailKey
      if (typeof detailKey !== 'string' || !detailKey) return

      targets[detailKey] = {
        detailKey,
        layerId,
        seriesName,
        dataIndex,
      }
    })
  })

  return targets
}

export const buildTimeOfDayAnalysisModel = (
  result: TimeOfDayResult
): TimeOfDayAnalysisModel => {
  const planSeries = getUnifiedSourceSeries(
    buildPlanProfileOption(result),
    'recommendation'
  )
  const pressureSeries = getUnifiedSourceSeries(
    buildSplitPressureOption(result),
    'pressure'
  )
  const contextSeries = buildScheduleContextSeries(result)
  const dataSeries = [...contextSeries, ...planSeries, ...pressureSeries]
  const directionalSeriesNames =
    result.planProfile?.directionalProfiles?.map((profile, index) =>
      formatDirectionProfileName(profile, index)
    ) ?? []
  const splitPressure = result.splitPressure
  const primarySeriesName = formatRepresentativeSeriesName(
    splitPressure?.primaryDirections,
    'primary',
    'primary street'
  )
  const crossSeriesName = formatRepresentativeSeriesName(
    splitPressure?.crossDirections,
    'cross street',
    'cross street'
  )
  const splitReviewName = formatPercentThresholdName(
    getThresholdPercent(
      splitPressure?.thresholdPercentByName,
      ['SplitReview', '35% split review', 'split review'],
      35
    ),
    'split review'
  )
  const shoulderReviewName = formatPercentThresholdName(
    getThresholdPercent(
      splitPressure?.thresholdPercentByName,
      ['ShoulderReview', '45% shoulder review', 'shoulder review'],
      45
    ),
    'shoulder review'
  )
  const existingEntries = getScheduleEntries(
    result.planComparison?.commonCurrentSchedule
  )
  const proposedEntries = getScheduleEntries(
    result.recommendation?.recommendedSchedule
  )
  const hasScheduleDifferences = getPlanDifferenceOverlayData(result).length > 0
  const proposedScheduleSeriesNames = proposedEntries.length
    ? ['Proposed plan windows', 'Proposed schedule rail']
    : []
  const existingScheduleSeriesNames = existingEntries.length
    ? ['Existing plan windows', 'Existing schedule rail']
    : []
  const scheduleDifferenceSeriesNames = hasScheduleDifferences
    ? ['Plan difference windows']
    : []
  const scheduleSeriesNames = [
    ...proposedScheduleSeriesNames,
    ...existingScheduleSeriesNames,
    ...scheduleDifferenceSeriesNames,
  ]
  const allSeriesNames = [
    ...new Set([
      ...dataSeries.map(getSeriesName).filter(Boolean),
      ...scheduleSeriesNames,
    ]),
  ]
  const sharedVolumeAxisMax = getSharedVolumeAxisMax(result)
  const yAxis = createYAxis(
    false,
    {
      name: 'Volume (vph)',
      nameGap: 60,
      max: sharedVolumeAxisMax,
      splitLine: { lineStyle: { color: valueGridLineColor } },
    },
    {
      name: 'Cross Traffic (%)',
      nameGap: 48,
      min: 0,
      max: (value) => Math.max(100, Math.ceil(value.max / 10) * 10),
      position: 'right',
      show: false,
      axisLabel: { formatter: formatPercentAxisLabel },
      axisLine: { show: false },
    }
  )
  const amCorridorPeaks = getCorridorPeakEvents(result.planProfile?.peaks, 'AM')
  const pmCorridorPeaks = getCorridorPeakEvents(result.planProfile?.peaks, 'PM')
  const dateRange = formatSelectedDateRange(result.selectedDates)
  const summaryItems = [
    {
      label: 'AM Corridor Peak',
      value: formatPeakInfoValue(
        result.recommendation?.amPeakTime ?? amCorridorPeaks[0]?.timeOfDay,
        amCorridorPeaks[0]?.value,
        amCorridorPeaks[0]?.valueUnits ?? 'vph'
      ),
    },
    {
      label: 'PM Corridor Peak',
      value: formatPeakInfoValue(
        result.recommendation?.pmPeakTime ?? pmCorridorPeaks[0]?.timeOfDay,
        pmCorridorPeaks[0]?.value,
        pmCorridorPeaks[0]?.valueUnits ?? 'vph'
      ),
    },
    {
      label: 'Peak Cross Traffic',
      value: formatPeakInfoValue(
        splitPressure?.peakCrossTrafficPercentTime,
        splitPressure?.peakCrossTrafficPercent,
        '%',
        1
      ),
    },
  ].filter((item) => Boolean(item.value))
  const option = buildBaseOption({
    result,
    title: 'Corridor Time-of-Day Analysis',
    externalHeader: true,
    series: dataSeries,
    right: 90,
    legendData: allSeriesNames,
    showLegend: false,
    yAxis,
  })
  const optionSeries = getOptionSeries(option)
  const seriesByName = new Map(
    optionSeries.map((seriesOption) => [
      getSeriesName(seriesOption),
      seriesOption,
    ])
  )
  const createLayer = (
    layer: Omit<TimeOfDayChartLayer, 'available'>
  ): TimeOfDayChartLayer => ({
    ...layer,
    available:
      (!reviewThresholdLayerIds.has(layer.id) ||
        hasSplitPressureData(result)) &&
      layer.seriesNames.some((seriesName) =>
        seriesHasData(seriesByName.get(seriesName))
      ),
  })
  const planColorLegendItems = [
    {
      label: 'AM peak plan',
      color: chartColors.amPlanBackground,
      preview: 'area' as const,
    },
    {
      label: 'Midday plan',
      color: chartColors.middayPlanBackground,
      preview: 'area' as const,
    },
    {
      label: 'PM peak plan',
      color: chartColors.pmPlanBackground,
      preview: 'area' as const,
    },
    {
      label: 'FREE operation',
      color: freeSchedulePlanColor,
      preview: 'area' as const,
    },
  ]
  const layers: TimeOfDayChartLayer[] = [
    createLayer({
      id: 'proposed-schedule',
      group: 'Schedules',
      label: 'Proposed',
      description:
        'Recommended timing-plan windows and rail. Expand for the color key.',
      preview: 'schedule',
      color: chartColors.amPlanBackground,
      additionalColors: [
        chartColors.middayPlanBackground,
        chartColors.pmPlanBackground,
      ],
      seriesNames: proposedScheduleSeriesNames,
      legendItems: planColorLegendItems,
    }),
    createLayer({
      id: 'existing-schedule',
      group: 'Schedules',
      label: 'Existing',
      description:
        'Current timing-plan windows and rail. Expand for the color key.',
      preview: 'schedule',
      color: chartColors.amPlanBackground,
      additionalColors: [
        chartColors.middayPlanBackground,
        chartColors.pmPlanBackground,
      ],
      seriesNames: existingScheduleSeriesNames,
      legendItems: planColorLegendItems,
    }),
    createLayer({
      id: 'schedule-differences',
      group: 'Schedules',
      label: 'Schedule differences',
      description: 'Hatching over the windows where the two schedules differ.',
      preview: 'hatch',
      color: '#f59e0b',
      seriesNames: scheduleDifferenceSeriesNames,
      legendItems: [
        {
          label: 'Proposed and existing schedules differ',
          color: '#f59e0b',
          preview: 'hatch' as const,
        },
      ],
    }),
    createLayer({
      id: 'raw-volume',
      group: 'Corridor Demand',
      label: 'Median raw volume',
      description: 'Median observed corridor volume by time of day.',
      preview: 'solid-line',
      color: chartColors.raw,
      seriesNames: ['Median Raw Volume'],
    }),
    createLayer({
      id: 'smoothed-volume',
      group: 'Corridor Demand',
      label: 'Smoothed breakpoints',
      description:
        'Smoothed corridor demand used to recommend plan boundaries.',
      preview: 'solid-line',
      color: chartColors.smooth,
      seriesNames: ['Smoothed For Breakpoints'],
    }),
    ...(directionalSeriesNames.length
      ? [
          createLayer({
            id: 'directional-profiles',
            group: 'Corridor Demand',
            label: 'Directional profiles',
            description: 'Representative directional volume profiles.',
            preview: 'dashed-line',
            color: directionalColors[0],
            additionalColors: directionalSeriesNames
              .slice(1)
              .map(
                (_, index) =>
                  directionalColors[(index + 1) % directionalColors.length]
              ),
            seriesNames: directionalSeriesNames,
            seriesControls: directionalSeriesNames.map((seriesName, index) => ({
              seriesName,
              label: seriesName,
              color: directionalColors[index % directionalColors.length],
              available: seriesHasData(seriesByName.get(seriesName)),
            })),
          }),
        ]
      : []),
    createLayer({
      id: 'corridor-peaks',
      group: 'Corridor Demand',
      label: 'Corridor peaks',
      description: 'AM and PM corridor peak markers.',
      preview: 'star',
      color: chartColors.amPeak,
      additionalColors: [chartColors.pmPeak],
      seriesNames: ['AM Corridor Peak', 'PM Corridor Peak'],
    }),
    createLayer({
      id: 'primary-volume',
      group: 'Movement Demand',
      label: 'Primary traffic',
      description: 'Representative primary-street volume profile.',
      preview: 'solid-line',
      color: chartColors.primary,
      seriesNames: [primarySeriesName],
    }),
    createLayer({
      id: 'cross-volume',
      group: 'Movement Demand',
      label: 'Cross traffic',
      description: 'Representative cross-street volume profile.',
      preview: 'solid-line',
      color: chartColors.cross,
      seriesNames: [crossSeriesName],
    }),
    createLayer({
      id: 'cross-percent',
      group: 'Movement Demand',
      label: 'Cross-traffic percent',
      description: 'Cross traffic as a percentage of total corridor demand.',
      preview: 'dashed-line',
      color: chartColors.percent,
      seriesNames: ['Cross-traffic percent'],
    }),
    createLayer({
      id: 'split-review-threshold',
      group: 'Movement Demand',
      label: splitReviewName,
      description: 'Cross-traffic split-review percentage threshold.',
      preview: 'dashed-line',
      color: chartColors.splitReview,
      seriesNames: [splitReviewName],
    }),
    createLayer({
      id: 'shoulder-review-threshold',
      group: 'Movement Demand',
      label: shoulderReviewName,
      description: 'Cross-traffic shoulder-review percentage threshold.',
      preview: 'dashed-line',
      color: chartColors.shoulderReview,
      seriesNames: [shoulderReviewName],
    }),
    createLayer({
      id: 'pressure-peaks',
      group: 'Movement Demand',
      label: 'Movement demand peaks',
      description: 'AM, midday, and PM movement demand peak markers.',
      preview: 'star',
      color: chartColors.amPeak,
      additionalColors: [chartColors.middaySignalPeak, chartColors.pmPeak],
      seriesNames: ['Volume Peaks', 'Cross Traffic Percent Peaks'],
    }),
    createLayer({
      id: 'signal-peaks',
      group: 'Locations',
      label: 'Signal peaks',
      description: 'Numbered AM and PM signal peak locations.',
      preview: 'circle',
      color: chartColors.amSignalPeak,
      additionalColors: [chartColors.pmSignalPeak],
      previewLabel: '1',
      seriesNames: ['AM Signal Peaks', 'PM Signal Peaks'],
    }),
    createLayer({
      id: 'cross-traffic-locations',
      group: 'Locations',
      label: 'Cross-traffic locations',
      description: 'Numbered cross-traffic locations by analysis period.',
      preview: 'circle',
      color: chartColors.amSignalPeak,
      additionalColors: [
        chartColors.middaySignalPeak,
        chartColors.pmSignalPeak,
      ],
      previewLabel: '1',
      seriesNames: [
        'AM Cross Traffic Locations',
        'Midday Cross Traffic Locations',
        'PM Cross Traffic Locations',
      ],
    }),
    createLayer({
      id: 'movement-pressure',
      group: 'Locations',
      label: 'Movement demand',
      description: 'Numbered movement-demand locations, shown as squares.',
      preview: 'square',
      color: chartColors.amSignalPeak,
      additionalColors: [chartColors.pmSignalPeak],
      previewLabel: '1',
      seriesNames: ['AM Movement Demand', 'PM Movement Demand'],
    }),
  ]
  const defaultSelectedSeries = getTimeOfDayPresetSeriesSelection(
    layers,
    'recommendation'
  )
  const legend = option.legend as LegendComponentOption
  option.legend = {
    ...legend,
    show: false,
    selected: defaultSelectedSeries,
  }
  const percentSeriesNames = optionSeries
    .filter(
      (seriesOption) =>
        (seriesOption as SeriesOption & { yAxisIndex?: number }).yAxisIndex ===
        1
    )
    .map(getSeriesName)
    .filter(Boolean)

  return {
    header: {
      title: 'Corridor Time-of-Day Analysis',
      dateRange,
      summaryItems,
    },
    option,
    layers,
    defaultSelectedSeries,
    percentSeriesNames,
    detailTargets: buildDetailTargets(optionSeries, layers),
  }
}

export const buildTimeOfDayAnalysisOption = (result: TimeOfDayResult) =>
  buildTimeOfDayAnalysisModel(result).option
