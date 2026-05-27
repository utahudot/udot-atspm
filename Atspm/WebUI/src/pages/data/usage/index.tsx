import type { GetUsageEntryParams, UsageEntry } from '@/api/config'
import { useGetUsageEntry } from '@/api/config'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { UsageEntryFiltersState } from '@/features/data/components/UsageEntryFilters'
import UsageOverviewTab from '@/features/data/components/UsageSummaryTab'
import { useGetAllUsers } from '@/features/identity/api/getAllUsers'
import Authorization from '@/lib/Authorization'
import {
  localDateTimeToUtcODataLiteral,
  parseWallClockDateTimeLiteral,
  toWallClockDateTimeLiteral,
} from '@/utils/dateTime'
import { Stack } from '@mui/material'
import { addDays, startOfDay, startOfMonth } from 'date-fns'
import * as React from 'react'
import { useMemo } from 'react'

export default function UsageEntriesPage() {
  const createDefaultState = (): UsageEntryFiltersState => ({
    fromUtc: toWallClockDateTimeLiteral(startOfMonth(new Date())),
    toUtc: toWallClockDateTimeLiteral(startOfDay(new Date())),
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

export function buildUsageEntryParams(
  filters: UsageEntryFiltersState
): GetUsageEntryParams {
  const parts: string[] = []

  if (filters.fromUtc) {
    const from = localDateTimeToUtcODataLiteral(filters.fromUtc)
    if (from) parts.push(`Timestamp ge ${from}`)
  }

  if (filters.toUtc) {
    const toDate = parseWallClockDateTimeLiteral(filters.toUtc)
    if (toDate) {
      const nextDayStart = addDays(startOfDay(toDate), 1)
      const to = localDateTimeToUtcODataLiteral(nextDayStart)
      if (to) parts.push(`Timestamp lt ${to}`)
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
