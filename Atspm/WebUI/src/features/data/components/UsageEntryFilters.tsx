import { UserDTO } from '@/api/identity/atspmAuthenticationApi.schemas'
import { dateToTimestamp } from '@/utils/dateTime'
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

function toTimestampOrUndefined(d: Date | null): string | undefined {
  if (!d) return undefined
  return dateToTimestamp(d)
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
    <Card sx={{ mt: 0, pt: 0, width: '100%', height: '100%' }}>
      <CardContent>
        <Stack spacing={2}>
          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            spacing={1}
            alignItems={{ xs: 'flex-start', sm: 'center' }}
            justifyContent="space-between"
          >
            <Stack direction="row" spacing={1} alignItems="center">
              <FilterAltIcon fontSize="small" />
              <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
                Filters
              </Typography>
            </Stack>

            <Button
              size="small"
              startIcon={<RestartAltIcon />}
              onClick={onReset}
              sx={{ alignSelf: { xs: 'flex-start', sm: 'auto' } }}
            >
              Reset
            </Button>
          </Stack>

          <Box
            sx={{
              display: 'grid',
              gap: 2,
              gridTemplateColumns: {
                xs: 'minmax(0, 1fr)',
                sm: 'repeat(2, minmax(0, 1fr))',
                xl: 'minmax(0, 1fr) minmax(0, 1fr) minmax(0, 1.2fr)',
              },
              alignItems: 'start',
            }}
          >
            <DatePicker
              sx={{
                width: '100%',
                minWidth: 0,
              }}
              label="From"
              value={toDateOrNull(value.fromUtc)}
              onChange={(v) => set('fromUtc', toTimestampOrUndefined(v))}
              slotProps={{
                textField: { fullWidth: true, size: 'small' },
              }}
            />

            <DatePicker
              sx={{
                width: '100%',
                minWidth: 0,
              }}
              label="To"
              value={toDateOrNull(value.toUtc)}
              onChange={(v) => set('toUtc', toTimestampOrUndefined(v))}
              slotProps={{
                textField: { fullWidth: true, size: 'small' },
              }}
            />

            <Autocomplete
              size="small"
              sx={{
                width: '100%',
                minWidth: 0,
                gridColumn: { xs: 'auto', sm: '1 / -1', xl: 'auto' },
              }}
              options={users ?? []}
              loading={Boolean(usersLoading)}
              value={selectedUser}
              onChange={(_, next) => set('userId', next?.userId ?? '')}
              isOptionEqualToValue={(a, b) => a.userId === b.userId}
              getOptionLabel={userLabel}
              renderInput={(params) => (
                <TextField
                  {...params}
                  fullWidth
                  label="User"
                  placeholder="All users"
                />
              )}
            />
          </Box>
        </Stack>
      </CardContent>
    </Card>
  )
}
