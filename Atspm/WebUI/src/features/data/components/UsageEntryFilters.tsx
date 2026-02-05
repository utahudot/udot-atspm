import { UserDTO } from '@/api/identity/atspmAuthenticationApi.schemas'
import FilterAltIcon from '@mui/icons-material/FilterAlt'
import RestartAltIcon from '@mui/icons-material/RestartAlt'
import {
  Autocomplete,
  Box,
  Button,
  Card,
  CardContent,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import * as React from 'react'

function toIsoUtcOrUndefined(d: Date | null): string | undefined {
  if (!d) return undefined
  return d.toISOString()
}

function toDateOrNull(iso?: string): Date | null {
  if (!iso) return null
  const d = new Date(iso)
  return Number.isNaN(d.getTime()) ? null : d
}

export type UsageEntryFiltersState = {
  fromUtc?: string
  toUtc?: string
  apiName: string
  method: string
  success: 'all' | 'true' | 'false'
  statusClass: 'all' | '2xx' | '4xx' | '5xx'
  userId: string
}

export default function UsageEntryFilters({
  value: valueProp,
  onChange,
  onReset,
  users,
  usersLoading,
}: {
  value?: UsageEntryFiltersState
  onChange: (next: UsageEntryFiltersState) => void
  onReset: () => void
  users?: UserDTO[]
  usersLoading?: boolean
}) {
  const value: UsageEntryFiltersState = valueProp ?? {
    fromUtc: undefined,
    toUtc: undefined,
    userId: '',
    apiName: '',
    method: '',
    success: 'all',
    statusClass: 'all',
  }

  const set = <K extends keyof UsageEntryFiltersState>(
    key: K,
    nextValue: UsageEntryFiltersState[K]
  ) => {
    onChange({ ...value, [key]: nextValue })
  }

  const selectedUser = React.useMemo(() => {
    const uid = value.userId
    if (!uid) return null
    return users?.find((u) => u.userId === uid) ?? null
  }, [value.userId, users])

  const userLabel = (u: UserDTO) => {
    const name =
      u.fullName?.trim() || `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim()
    const email = u.email || u.userName || ''
    if (name && email) return `${name} — ${email}`
    return name || email || u.userId
  }

  return (
    <Card sx={{ mt: 0, pt: 0 }}>
      <CardContent>
        {' '}
        <Stack spacing={2}>
          <Stack
            direction="row"
            spacing={1}
            alignItems="center"
            sx={{ flexWrap: 'wrap' }}
          >
            <FilterAltIcon fontSize="small" />
            <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
              Filters
            </Typography>

            <Box sx={{ flex: 1 }} />

            <Button
              size="small"
              startIcon={<RestartAltIcon />}
              onClick={onReset}
            >
              Reset
            </Button>
          </Stack>

          <Stack
            direction="row"
            spacing={2}
            alignItems="center"
            sx={{ flexWrap: 'wrap' }}
          >
            <DatePicker
              label="From"
              value={toDateOrNull(value.fromUtc)}
              onChange={(v) => set('fromUtc', toIsoUtcOrUndefined(v))}
              slotProps={{
                textField: { size: 'small', sx: { minWidth: 220 } },
              }}
            />

            <DatePicker
              label="To"
              value={toDateOrNull(value.toUtc)}
              onChange={(v) => set('toUtc', toIsoUtcOrUndefined(v))}
              slotProps={{
                textField: { size: 'small', sx: { minWidth: 220 } },
              }}
            />

            <Autocomplete
              size="small"
              sx={{ minWidth: 320 }}
              options={users ?? []}
              loading={Boolean(usersLoading)}
              value={selectedUser}
              onChange={(_, next) => set('userId', next?.userId ?? '')}
              isOptionEqualToValue={(a, b) => a.userId === b.userId}
              getOptionLabel={userLabel}
              renderInput={(params) => (
                <TextField {...params} label="User" placeholder="All users" />
              )}
            />
          </Stack>
        </Stack>
      </CardContent>
    </Card>
  )
}
