import type { Default } from '@/features/charts/types'
import {
  TimeOfDayMeasureDefaults,
  timeOfDayMeasureTypeId,
} from '@/features/timeOfDay/measureDefaults'
import type { TimeOfDayTuningOptionKey } from '@/features/timeOfDay/types'
import {
  Alert,
  Box,
  InputAdornment,
  TextField,
  Typography,
} from '@mui/material'

interface TimeOfDayMeasureOptionsProps {
  chartDefaults: TimeOfDayMeasureDefaults
  handleChartOptionsUpdate: (update: Default) => void
  isMeasureDefaultView?: boolean
}

type TimeOfDayMeasureOptionField = {
  option: TimeOfDayTuningOptionKey
  label: string
  type?: 'number' | 'time'
  inputProps?: Record<string, string | number>
  displayAsPercent?: boolean
}

const thresholdFields: TimeOfDayMeasureOptionField[] = [
  {
    option: 'amEntryPctOfPeak',
    label: 'AM start threshold as percent of AM peak',
    type: 'number',
    inputProps: { min: 0, max: 100, step: 1 },
    displayAsPercent: true,
  },
  {
    option: 'amExitPctOfPeak',
    label: 'AM end threshold as percent of AM peak',
    type: 'number',
    inputProps: { min: 0, max: 100, step: 1 },
    displayAsPercent: true,
  },
  {
    option: 'pmEntryPctOfPeak',
    label: 'PM start threshold as percent of PM peak',
    type: 'number',
    inputProps: { min: 0, max: 100, step: 1 },
    displayAsPercent: true,
  },
  {
    option: 'pmExitPctOfPeak',
    label: 'PM end threshold as percent of PM peak',
    type: 'number',
    inputProps: { min: 0, max: 100, step: 1 },
    displayAsPercent: true,
  },
  {
    option: 'freeEntryPctOfDailyPeak',
    label: 'FREE threshold as percent of daily peak',
    type: 'number',
    inputProps: { min: 0, max: 100, step: 1 },
    displayAsPercent: true,
  },
  {
    option: 'freeEntryPctOfDynamicRange',
    label: 'FREE threshold above baseline as percent of dynamic range',
    type: 'number',
    inputProps: { min: 0, max: 100, step: 1 },
    displayAsPercent: true,
  },
  {
    option: 'entrySustainedBins',
    label: 'Consecutive bins for AM/PM entry-exit',
    type: 'number',
    inputProps: { min: 1, step: 1 },
  },
  {
    option: 'freeSustainedBins',
    label: 'Consecutive bins before going FREE',
    type: 'number',
    inputProps: { min: 1, step: 1 },
  },
  {
    option: 'freeFallbackTime',
    label: 'Fallback FREE start time',
    type: 'time',
  },
  {
    option: 'maxAmEndTime',
    label: 'Latest allowed end time for AM plan',
    type: 'time',
  },
  {
    option: 'maxPmEndTime',
    label: 'Latest allowed end time for PM plan',
    type: 'time',
  },
]

const capacityFields: TimeOfDayMeasureOptionField[] = [
  {
    option: 'laneCapacityVehiclesPerHour',
    label: 'Lane capacity for 100% occupancy',
    type: 'number',
    inputProps: { min: 1, step: 1 },
  },
  {
    option: 'approachVolumeAssumedLanes',
    label: 'Assumed lanes per approach volume',
    type: 'number',
    inputProps: { min: 1, step: 1 },
  },
  {
    option: 'splitReviewThresholdPercent',
    label: 'Split review threshold percent',
    type: 'number',
    inputProps: { min: 0, max: 100, step: 1 },
  },
  {
    option: 'shoulderReviewThresholdPercent',
    label: 'Shoulder review threshold percent',
    type: 'number',
    inputProps: { min: 0, max: 100, step: 1 },
  },
]

const optionValueToString = (value: Default['value'] | undefined) => {
  if (value === undefined || value === null) return ''
  if (Array.isArray(value)) return value.join(',')

  return String(value)
}

const fractionToPercentString = (value: string) => {
  if (value === '') return value

  const fraction = Number(value)
  return Number.isFinite(fraction)
    ? String(Number((fraction * 100).toFixed(10)))
    : value
}

const percentToFractionString = (value: string) => {
  if (value === '') return value

  const percent = Number(value)
  return Number.isFinite(percent)
    ? String(Number((percent / 100).toFixed(10)))
    : value
}

export const TimeOfDayMeasureOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: TimeOfDayMeasureOptionsProps) => {
  const renderField = (field: TimeOfDayMeasureOptionField) => {
    const defaultOption = chartDefaults[field.option]
    const serializedValue = optionValueToString(defaultOption?.value)
    const displayValue = field.displayAsPercent
      ? fractionToPercentString(serializedValue)
      : serializedValue

    return (
      <TextField
        key={field.option}
        size="small"
        type={field.type ?? 'number'}
        label={field.label}
        value={displayValue}
        onChange={(event) => {
          if (!defaultOption) return

          handleChartOptionsUpdate({
            id: defaultOption.id,
            option: defaultOption.option,
            value: field.displayAsPercent
              ? percentToFractionString(event.target.value)
              : event.target.value,
          })
        }}
        inputProps={field.inputProps}
        InputProps={
          field.displayAsPercent
            ? {
                endAdornment: <InputAdornment position="end">%</InputAdornment>,
              }
            : undefined
        }
        InputLabelProps={field.type === 'time' ? { shrink: true } : undefined}
        disabled={!defaultOption}
      />
    )
  }

  const hasTimeOfDayDefaults = Boolean(Object.keys(chartDefaults ?? {}).length)

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {!hasTimeOfDayDefaults && (
        <Alert severity="warning">
          Time Of Day defaults are not present for measure type{' '}
          {timeOfDayMeasureTypeId}.
        </Alert>
      )}
      <Typography variant="subtitle2">Threshold Settings</Typography>
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', md: 'repeat(2, minmax(0, 1fr))' },
          gap: 2,
        }}
      >
        {thresholdFields.map(renderField)}
      </Box>
      <Typography variant="subtitle2">Occupancy / Capacity Settings</Typography>
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', md: 'repeat(2, minmax(0, 1fr))' },
          gap: 2,
        }}
      >
        {capacityFields.map(renderField)}
      </Box>
    </Box>
  )
}
