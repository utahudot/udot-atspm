import type { GetUsageEntryParams, UsageEntry } from '@/api/config'
import { useGetUsageEntry } from '@/api/config'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { UsageEntryFiltersState } from '@/features/data/components/UsageEntryFilters'
import UsageOverviewTab from '@/features/data/components/UsageSummaryTab'
import { useGetAllUsers } from '@/features/identity/api/getAllUsers'
import Authorization from '@/lib/Authorization'
import { Stack } from '@mui/material'
import { startOfDay, startOfMonth } from 'date-fns'
import * as React from 'react'
import { useMemo } from 'react'

export default function UsageEntriesPage() {
  const defaultState: UsageEntryFiltersState = {
    fromUtc: startOfMonth(new Date()).toISOString(),
    toUtc: startOfDay(new Date()).toISOString(),
    userId: '',
    apiName: '',
    method: '',
    success: 'all',
    statusClass: 'all',
  }

  const [filters, setFilters] =
    React.useState<UsageEntryFiltersState>(defaultState)

  const clearFilters = () => setFilters(defaultState)

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
        <Stack spacing={2}>
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
      </ResponsivePageLayout>
    </Authorization>
  )
}

function escapeODataString(s: string) {
  return s.replace(/'/g, "''")
}

function buildUsageEntryParams(
  filters: UsageEntryFiltersState
): GetUsageEntryParams {
  const parts: string[] = []

  if (filters.fromUtc) parts.push(`Timestamp ge ${filters.fromUtc}`)
  if (filters.toUtc) parts.push(`Timestamp le ${filters.toUtc}`)

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
