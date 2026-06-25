import { UsageEntry } from '@/api/config'
import { UserDTO } from '@/api/identity/atspmAuthenticationApi.schemas'
import { formatChartDateTimeRange } from '@/features/charts/utils'
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
  const subtitle = formatChartDateTimeRange(dateRange?.start, dateRange?.end)

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
        <Box sx={{ height }}>
          <InsightsChart
            chartHeight={height}
            loading={isLoading}
            horizontal={horizontal}
            metric={metric}
            title={fullTitle}
            subtitle={subtitle}
            items={active.map((x) => ({ name: x.name, value: x.count }))}
          />
        </Box>
      </CardContent>
    </Card>
  )
}

export default memo(ReportsApiInsightsCard)
