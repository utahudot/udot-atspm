import type { GetUsageEntryParams, UsageEntry } from '@/api/config'
import { useGetUsageEntry } from '@/api/config'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { UsageEntryFiltersState } from '@/features/data/components/UsageEntryFilters'
import UsageOverviewTab from '@/features/data/components/UsageSummaryTab'
import { useGetAllUsers } from '@/features/identity/api/getAllUsers'
import Authorization from '@/lib/Authorization'
import { dateToTimestamp } from '@/utils/dateTime'
import { Stack } from '@mui/material'
import { addDays, startOfDay, startOfMonth } from 'date-fns'
import * as React from 'react'
import { useMemo } from 'react'

export default function UsageEntriesPage() {
  const createDefaultState = (): UsageEntryFiltersState => ({
    fromUtc: dateToTimestamp(startOfMonth(new Date())),
    toUtc: dateToTimestamp(startOfDay(new Date())),
    userId: '',
    apiName: '',
    method: '',
    success: 'all',
    statusClass: 'all',
  })

  const [filters, setFilters] =
    React.useState<UsageEntryFiltersState>(createDefaultState())

  const clearFilters = () => setFilters(createDefaultState())

  const usageParams = useMemo(() => buildUsageEntryParams(filters), [filters])

  const { data: usageData, isFetching: usageLoading } =
    useGetUsageEntry(usageParams)

  const { data: userData, isLoading: usersLoading } = useGetAllUsers()

  const rows = useMemo(
    () => (usageData?.value ?? []) as UsageEntry[],
    [usageData]
  )
  const users = useMemo(() => userData || [], [userData])

  const dateRange = useMemo(
    () => ({ start: filters.fromUtc, end: filters.toUtc }),
    [filters]
  )

  return (
    <Authorization requiredClaim={'Data:View'}>
      <ResponsivePageLayout title={'Usage Analytics'}>
        <Stack
          spacing={2}
          sx={{
            width: '100%',
            pt: { xs: 1, sm: 1.5 },
            alignItems: 'center',
          }}
        >
          <Stack sx={{ width: '100%', maxWidth: 1600 }}>
            <UsageOverviewTab
              filters={filters}
              onFiltersChange={setFilters}
              onResetFilters={clearFilters}
              rows={rows}
              users={users}
              usersLoading={usersLoading}
              usageLoading={usageLoading}
              dateRange={dateRange}
            />
          </Stack>
        </Stack>
      </ResponsivePageLayout>
    </Authorization>
  )
}

function escapeODataString(s: string) {
  return s.replace(/'/g, "''")
}

function parseLocalTimestamp(value: string): Date | null {
  const d = new Date(value)
  return Number.isNaN(d.getTime()) ? null : d
}

function buildUsageEntryParams(
  filters: UsageEntryFiltersState
): GetUsageEntryParams {
  const parts: string[] = []

  if (filters.fromUtc) {
    parts.push(`Timestamp ge ${filters.fromUtc}Z`)
  }

  if (filters.toUtc) {
    const toDate = parseLocalTimestamp(filters.toUtc)
    if (toDate) {
      const nextDayStart = dateToTimestamp(addDays(startOfDay(toDate), 1))
      parts.push(`Timestamp lt ${nextDayStart}Z`)
    }
  }

  if (filters.userId) {
    const v = escapeODataString(filters.userId)
    parts.push(`UserId eq '${v}'`)
  }

  const filter = parts.length ? parts.join(' and ') : undefined

  return {
    filter,
    orderby: 'Timestamp desc',
    count: true,
  }
}
