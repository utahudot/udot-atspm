import { UsageEntry } from '@/api/config'
import { UserDTO } from '@/api/identity/atspmAuthenticationApi.schemas'
import InsightsChart from '@/features/data/components/ReportApiInsightsCard/InsightsChart'
import InsightsHeader, {
  GroupBy,
  Metric,
  SortBy,
  ViewKey,
} from '@/features/data/components/ReportApiInsightsCard/InsightsHeader'
import {
  buildByAgency,
  buildByChartType,
  buildByTime,
  buildByUser,
  formatUsageLocalDateRange,
  normalizeUsers,
} from '@/features/data/components/ReportApiInsightsCard/utils'
import { Box, Card, CardContent } from '@mui/material'
import * as React from 'react'
import { memo } from 'react'

function clamp(n: number, min: number, max: number) {
  return Math.max(min, Math.min(max, n))
}

function computeChartHeight(opts: { count: number; horizontal: boolean }) {
  const { count, horizontal } = opts
  const base = horizontal ? 140 : 220
  const per = horizontal ? 24 : 10
  const min = horizontal ? 260 : 280
  const max = horizontal ? 860 : 520
  return clamp(base + count * per, min, max)
}

const CHART_RESIZE_TRANSITION_MS = 260

function metricTitle(metric: Metric) {
  return metric === 'ReportsGenerated' ? 'Reports Generated' : 'Data Downloaded'
}

function viewTitle(view: ViewKey) {
  return view === 'type'
    ? 'Type'
    : view === 'time'
      ? 'Time'
      : view === 'user'
        ? 'User'
        : 'Agency'
}

interface ReportsApiInsightsCardProps {
  rows: UsageEntry[]
  users?: UserDTO[]
  maxBars?: number
  dateRange?: { start: string | undefined; end: string | undefined }
  isLoading?: boolean
}

type RenderedInsightsChart = {
  horizontal: boolean
  items: { name: string; value: number }[]
  loading?: boolean
  metric: Metric
  subtitle: string
  title: string
}

function ReportsApiInsightsCard({
  rows,
  users,
  maxBars = 30,
  dateRange,
  isLoading,
}: ReportsApiInsightsCardProps) {
  const [view, setView] = React.useState<ViewKey>('type')
  const [groupBy, setGroupBy] = React.useState<GroupBy>('day')
  const [sortBy, setSortBy] = React.useState<SortBy>('Amount')
  const [metric, setMetric] = React.useState<Metric>('ReportsGenerated')

  const usersList = React.useMemo(() => normalizeUsers(users), [users])

  const userById = React.useMemo(() => {
    const m = new Map<string, UserDTO>()
    for (const u of usersList) {
      if (u?.userId) m.set(u.userId, u)
    }
    return m
  }, [usersList])

  const metricRows = React.useMemo(() => {
    return rows.filter((r) =>
      metric === 'ReportsGenerated'
        ? r.apiName?.includes('ReportApi')
        : r.apiName?.includes('DataApi')
    )
  }, [rows, metric])

  const byType = React.useMemo(
    () => buildByChartType(metricRows, { maxBars, sortBy, metric }),
    [metricRows, maxBars, sortBy, metric]
  )

  const byTime = React.useMemo(
    () => buildByTime(metricRows, { groupBy, metric }),
    [metricRows, groupBy, metric]
  )

  const byUser = React.useMemo(
    () => buildByUser(metricRows, { userById, maxBars, sortBy, metric }),
    [metricRows, userById, maxBars, sortBy, metric]
  )

  const byAgency = React.useMemo(
    () => buildByAgency(metricRows, { userById, maxBars, sortBy, metric }),
    [metricRows, userById, maxBars, sortBy, metric]
  )

  const active =
    view === 'type'
      ? byType
      : view === 'time'
        ? byTime
        : view === 'user'
          ? byUser
          : byAgency

  const horizontal = view !== 'time'

  const height = React.useMemo(
    () =>
      view === 'time'
        ? 500
        : computeChartHeight({ count: active.length, horizontal }),
    [active.length, horizontal, view]
  )

  const fullTitle = `${metricTitle(metric)} by ${viewTitle(view)}`
  const subtitle = formatUsageLocalDateRange(dateRange?.start, dateRange?.end)
  const chartItems = React.useMemo(
    () => active.map((x) => ({ name: x.name, value: x.count })),
    [active]
  )
  const nextChart = React.useMemo<RenderedInsightsChart>(
    () => ({
      horizontal,
      items: chartItems,
      loading: isLoading,
      metric,
      subtitle,
      title: fullTitle,
    }),
    [chartItems, fullTitle, horizontal, isLoading, metric, subtitle]
  )
  const [displayedChart, setDisplayedChart] = React.useState(nextChart)
  const [chartAnimationKey, setChartAnimationKey] = React.useState(0)
  const [isResizingChart, setIsResizingChart] = React.useState(false)
  const didInitializeChartRef = React.useRef(false)
  const pendingChartRef = React.useRef(nextChart)
  const previousHeightRef = React.useRef(height)
  const resizePendingRef = React.useRef(false)
  const resizeTimerRef = React.useRef<number | null>(null)

  const clearResizeTimer = React.useCallback(() => {
    if (resizeTimerRef.current === null) return

    window.clearTimeout(resizeTimerRef.current)
    resizeTimerRef.current = null
  }, [])

  const applyPendingChart = React.useCallback(() => {
    clearResizeTimer()
    resizePendingRef.current = false
    setDisplayedChart(pendingChartRef.current)
    setChartAnimationKey((key) => key + 1)
    setIsResizingChart(false)
  }, [clearResizeTimer])

  React.useEffect(() => {
    pendingChartRef.current = nextChart

    if (!didInitializeChartRef.current) {
      didInitializeChartRef.current = true
      return
    }

    const heightChanged = previousHeightRef.current !== height
    previousHeightRef.current = height

    if (!heightChanged && !resizePendingRef.current) {
      applyPendingChart()
      return
    }

    resizePendingRef.current = true
    setIsResizingChart(true)

    if (heightChanged) {
      clearResizeTimer()
      resizeTimerRef.current = window.setTimeout(
        applyPendingChart,
        CHART_RESIZE_TRANSITION_MS
      )
    }
  }, [applyPendingChart, clearResizeTimer, height, nextChart])

  React.useEffect(() => clearResizeTimer, [clearResizeTimer])

  const handleChartHeightTransitionEnd = React.useCallback(
    (event: React.TransitionEvent<HTMLDivElement>) => {
      if (
        event.target === event.currentTarget &&
        event.propertyName === 'height' &&
        isResizingChart
      ) {
        applyPendingChart()
      }
    },
    [applyPendingChart, isResizingChart]
  )

  return (
    <Card sx={{ width: '100%', minWidth: 0 }}>
      <CardContent sx={{ pb: 1 }}>
        <InsightsHeader
          view={view}
          onViewChange={setView}
          metric={metric}
          onMetricChange={setMetric}
          sortBy={sortBy}
          onSortByChange={setSortBy}
          groupBy={groupBy}
          onGroupByChange={setGroupBy}
        />
      </CardContent>

      <CardContent sx={{ pt: 1 }}>
        <Box
          onTransitionEnd={handleChartHeightTransitionEnd}
          sx={(theme) => ({
            height,
            overflow: 'hidden',
            transition: theme.transitions.create('height', {
              duration: CHART_RESIZE_TRANSITION_MS,
              easing: theme.transitions.easing.easeInOut,
            }),
          })}
        >
          {!isResizingChart && (
            <InsightsChart
              key={chartAnimationKey}
              chartHeight={height}
              loading={displayedChart.loading}
              horizontal={displayedChart.horizontal}
              metric={displayedChart.metric}
              title={displayedChart.title}
              subtitle={displayedChart.subtitle}
              items={displayedChart.items}
            />
          )}
        </Box>
      </CardContent>
    </Card>
  )
}

export default memo(ReportsApiInsightsCard)
