import type { Plan, TimeOfDayResult } from '@/api/reports'

export interface TimeOfDaySchedulePlanDetails {
  plan: string
  description: string
}

export interface TimeOfDayScheduleRow {
  id: string
  startMinutes: number
  endMinutes: number
  start: string
  end: string
  durationMinutes: number
  recommended: TimeOfDaySchedulePlanDetails | null
  current: TimeOfDaySchedulePlanDetails | null
  comparison: 'Same' | 'Different' | 'Missing'
}

export interface TimeOfDayScheduleEntry {
  plan: Plan
  interval: { start: number; end: number }
}

export const minutesToTimeLabel = (minutes: number) => {
  const clampedMinutes = Math.max(0, Math.min(1440, Math.round(minutes)))
  if (clampedMinutes === 1440) return '24:00'

  const hours = Math.floor(clampedMinutes / 60)
  const remainingMinutes = clampedMinutes % 60

  return `${String(hours).padStart(2, '0')}:${String(remainingMinutes).padStart(
    2,
    '0'
  )}`
}

export const formatPlanTime = (value?: string) => {
  if (!value) return '-'

  const minutes = getPlanBoundaryMinutes(value)
  return minutes === null ? value : minutesToTimeLabel(minutes)
}

export const formatPlanNumber = (planNumber?: string | null) => {
  if (!planNumber) return '-'

  const normalized = planNumber.trim()
  if (!normalized) return '-'

  return normalized === '254' || normalized.toLowerCase() === 'free'
    ? 'FREE'
    : normalized
}

export const schedulePlanPalette = [
  '#ef6c00',
  '#2e7d32',
  '#1565c0',
  '#6a1b9a',
  '#c62828',
  '#00796b',
  '#ad1457',
  '#4527a0',
  '#00838f',
  '#6d4c41',
] as const

export const freeSchedulePlanColor = '#607d8b'

export const getSchedulePlanColorMap = (schedules: Plan[][]) => {
  const colorMap = new Map<string, string>()
  let paletteIndex = 0

  schedules.forEach((plans) => {
    plans.forEach((plan) => {
      const planName = formatPlanNumber(plan.planNumber)
      if (colorMap.has(planName)) return

      if (planName === 'FREE') {
        colorMap.set(planName, freeSchedulePlanColor)
        return
      }

      colorMap.set(
        planName,
        schedulePlanPalette[paletteIndex % schedulePlanPalette.length]
      )
      paletteIndex += 1
    })
  })

  return colorMap
}

export const getPlanBoundaryMinutes = (value?: string) => {
  if (!value) return null

  // Plan timestamps describe a wall-clock schedule. Read the clock portion
  // directly so the browser does not normalize nonexistent DST times.
  const timeMatch = value.match(/(?:^|T|\s)(\d{1,2}):(\d{2})/)
  if (timeMatch) {
    const hours = Number(timeMatch[1])
    const minutes = Number(timeMatch[2])
    if (hours <= 23 && minutes <= 59) return hours * 60 + minutes
  }

  const date = new Date(value)
  if (!Number.isNaN(date.getTime())) {
    return date.getHours() * 60 + date.getMinutes()
  }

  return null
}

const crossesMidnight = (start?: string, end?: string) => {
  if (!start || !end) return false

  const startDateMatch = /^(\d{4}-\d{2}-\d{2})/.exec(start)
  const endDateMatch = /^(\d{4}-\d{2}-\d{2})/.exec(end)
  if (startDateMatch && endDateMatch) {
    return endDateMatch[1] > startDateMatch[1]
  }

  const startDate = new Date(start)
  const endDate = new Date(end)
  if (Number.isNaN(startDate.getTime()) || Number.isNaN(endDate.getTime())) {
    return false
  }

  return (
    endDate.toDateString() !== startDate.toDateString() && endDate > startDate
  )
}

export const getPlanIntervalMinutes = (plan: Plan) => {
  const start = getPlanBoundaryMinutes(plan.start)
  const end = getPlanBoundaryMinutes(plan.end)
  if (start === null || end === null) return null

  let adjustedEnd = end
  if (crossesMidnight(plan.start, plan.end) || adjustedEnd <= start) {
    adjustedEnd += 1440
  }

  const clampedStart = Math.max(0, Math.min(1440, start))
  const clampedEnd = Math.max(0, Math.min(1440, adjustedEnd))
  if (clampedEnd <= clampedStart) return null

  return {
    start: clampedStart,
    end: clampedEnd,
  }
}

export const planIntervalContainsMinutes = (
  plan: Plan,
  minutes: number | null | undefined
) => {
  if (minutes === undefined || minutes === null) return false

  const interval = getPlanIntervalMinutes(plan)
  if (!interval) return false

  return minutes >= interval.start && minutes < interval.end
}

export const getScheduleEntries = (
  plans: Plan[] | null | undefined
): TimeOfDayScheduleEntry[] => {
  const entries: TimeOfDayScheduleEntry[] = []

  plans?.forEach((plan) => {
    const interval = getPlanIntervalMinutes(plan)
    if (interval) entries.push({ plan, interval })
  })

  return entries
}

const getSchedulePlanDetails = (
  entry?: TimeOfDayScheduleEntry
): TimeOfDaySchedulePlanDetails | null =>
  entry
    ? {
        plan: formatPlanNumber(entry.plan.planNumber),
        description: entry.plan.planDescription ?? '-',
      }
    : null

const formatScheduleBoundary = (minutes: number) =>
  minutes === 1440 ? '00:00' : minutesToTimeLabel(minutes)

export const buildScheduleRows = (
  result: TimeOfDayResult
): TimeOfDayScheduleRow[] => {
  const recommendedEntries = getScheduleEntries(
    result.recommendation?.recommendedSchedule
  )
  const currentEntries = getScheduleEntries(
    result.planComparison?.commonCurrentSchedule
  )
  const boundaries = [
    ...new Set(
      [...recommendedEntries, ...currentEntries].flatMap(({ interval }) => [
        interval.start,
        interval.end,
      ])
    ),
  ].sort((left, right) => left - right)

  return boundaries.slice(0, -1).flatMap((start, index) => {
    const end = boundaries[index + 1]
    const recommendedEntry = recommendedEntries.find(
      ({ interval }) => interval.start <= start && interval.end >= end
    )
    const currentEntry = currentEntries.find(
      ({ interval }) => interval.start <= start && interval.end >= end
    )
    if (!recommendedEntry && !currentEntry) return []

    const recommended = getSchedulePlanDetails(recommendedEntry)
    const current = getSchedulePlanDetails(currentEntry)
    const comparison =
      recommended && current
        ? recommended.plan === current.plan
          ? 'Same'
          : 'Different'
        : 'Missing'

    return [
      {
        id: `${start}-${end}`,
        startMinutes: start,
        endMinutes: end,
        start: formatScheduleBoundary(start),
        end: formatScheduleBoundary(end),
        durationMinutes: end - start,
        recommended,
        current,
        comparison,
      },
    ]
  })
}
