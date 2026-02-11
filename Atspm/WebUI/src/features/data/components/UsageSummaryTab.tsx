import { UsageEntry } from '@/api/config'
import { UserDTO } from '@/api/identity/atspmAuthenticationApi.schemas'
import ReportsApiInsightsCard from '@/features/data/components/ReportApiInsightsCard/ReportApiInsightsCard'
import UsageEntryFilters, {
  UsageEntryFiltersState,
} from '@/features/data/components/UsageEntryFilters'
import UsageTable from '@/features/data/components/UsageTable'
import { formatBytes } from '@/utils/formatting'
import { Card, CardContent, Paper, Stack, Typography } from '@mui/material'
import * as React from 'react'
import { memo } from 'react'

interface UsageSummaryTabProps {
  rows: UsageEntryWithUser[]
  users?: UserDTO[]
  usersLoading?: boolean
  usageLoading?: boolean
  filters: UsageEntryFiltersState
  onFiltersChange: (next: UsageEntryFiltersState) => void
  onResetFilters: () => void
  dateRange?: { start: string | undefined; end: string | undefined }
}

interface UsageEntryWithUser extends UsageEntry {
  user: string
}

function StatCard({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <Paper sx={{ minWidth: 240, flex: '1 1 240px' }}>
      <CardContent>
        <Typography variant="h5" color="text.secondary">
          {label}
        </Typography>
        <Typography variant="h3" sx={{ fontWeight: 600 }}>
          {value}
        </Typography>
      </CardContent>
    </Paper>
  )
}

function UsageSummaryTab({
  rows,
  users,
  filters,
  onFiltersChange,
  onResetFilters,
  usersLoading,
  usageLoading,
  dateRange,
}: UsageSummaryTabProps) {
  const reportRows = React.useMemo(
    () => rows.filter((r) => r.apiName?.includes('ReportApi')),
    [rows]
  )

  const dataRows = React.useMemo(
    () => rows.filter((r) => r.apiName?.includes('DataApi')),
    [rows]
  )

  const totalDataDownloaded = React.useMemo(() => {
    let total = 0
    for (const r of dataRows) total += r.resultSizeBytes ?? 0
    return formatBytes(total)
  }, [dataRows])

  const tableData = React.useMemo(() => {
    return reportRows.map((r) => {
      const user = users?.find((u) => u.userId === r.userId)
      r.user = user ? user.userName : 'Anonymous'
      return r
    }) as UsageEntryWithUser[]
  }, [reportRows, users])

  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        spacing={2}
        sx={{ flexWrap: 'wrap', alignItems: 'stretch' }}
      >
        <UsageEntryFilters
          value={filters}
          onChange={onFiltersChange}
          onReset={onResetFilters}
          users={users}
          usersLoading={usersLoading}
        />

        <StatCard
          label="Reports generated"
          value={reportRows.length.toLocaleString()}
        />

        <StatCard label="Total data downloaded" value={totalDataDownloaded} />
      </Stack>

      <Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap' }}>
        <ReportsApiInsightsCard
          rows={rows}
          users={users}
          dateRange={dateRange}
          isLoading={usageLoading}
        />
      </Stack>

      <Stack direction="row" spacing={2} sx={{ flexWrap: 'wrap' }}>
        <Card sx={{ width: '100%' }}>
          <CardContent sx={{ p: 0 }}>
            <UsageTable isLoading={usageLoading} rows={tableData} />
          </CardContent>
        </Card>
      </Stack>
    </Stack>
  )
}

export default memo(UsageSummaryTab)
