import { UsageEntry } from '@/api/config'
import { UserDTO } from '@/api/identity/atspmAuthenticationApi.schemas'
import ReportsApiInsightsCard from '@/features/data/components/ReportApiInsightsCard/ReportApiInsightsCard'
import UsageEntryFilters, {
  UsageEntryFiltersState,
} from '@/features/data/components/UsageEntryFilters'
import UsageTable from '@/features/data/components/UsageTable'
import { formatBytes } from '@/utils/formatting'
import { Box, Card, CardContent, Paper, Stack, Typography } from '@mui/material'
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
    <Paper sx={{ width: '100%', minWidth: 0, height: '100%' }}>
      <CardContent
        sx={{
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
        }}
      >
        <Typography variant="subtitle1" color="text.secondary">
          {label}
        </Typography>
        <Typography
          variant="h3"
          sx={{
            fontWeight: 600,
            fontSize: { xs: '1.85rem', sm: '2.25rem' },
            overflowWrap: 'anywhere',
          }}
        >
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
      <Box
        sx={{
          display: 'grid',
          gap: 2,
          alignItems: 'stretch',
          gridTemplateColumns: {
            xs: 'minmax(0, 1fr)',
            sm: 'repeat(2, minmax(0, 1fr))',
            xl: 'minmax(0, 2fr) repeat(2, minmax(0, 1fr))',
          },
        }}
      >
        <Box
          sx={{
            minWidth: 0,
            gridColumn: { xs: 'auto', sm: '1 / -1', xl: 'auto' },
          }}
        >
          <UsageEntryFilters
            value={filters}
            onChange={onFiltersChange}
            onReset={onResetFilters}
            users={users}
            usersLoading={usersLoading}
          />
        </Box>

        <StatCard
          label="Reports generated"
          value={reportRows.length.toLocaleString()}
        />

        <StatCard label="Total data downloaded" value={totalDataDownloaded} />
      </Box>

      <Box sx={{ width: '100%', minWidth: 0 }}>
        <ReportsApiInsightsCard
          rows={rows}
          users={users}
          dateRange={dateRange}
          isLoading={usageLoading}
        />
      </Box>

      <Box sx={{ width: '100%', minWidth: 0 }}>
        <Card sx={{ width: '100%', minWidth: 0 }}>
          <CardContent sx={{ p: 0 }}>
            <UsageTable isLoading={usageLoading} rows={tableData} />
          </CardContent>
        </Card>
      </Box>
    </Stack>
  )
}

export default memo(UsageSummaryTab)
