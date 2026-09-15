import RightSidebar from '@/components/RightSidebar'
import { directionList } from '@/features/locations/types/DirectionType'
import { Box, Slider, Stack, TextField, Typography } from '@mui/material'
import { alpha } from '@mui/material/styles'
import { TimePicker } from '@mui/x-date-pickers'
import { format } from 'date-fns'
import type { ChangeEvent, ReactNode } from 'react'
import type { TimeOfDaySchedulePreset } from '../measureDefaults'
import type { TimeOfDayFormState, TimeOfDayTuningOptionKey } from '../types'
import TimeOfDaySchedulePresetSelect from './TimeOfDaySchedulePresetSelect'

export type AnalysisSidebar = 'schedule' | 'occupancy'

type TuningFieldDefinition = {
  option: TimeOfDayTuningOptionKey
  label: string
  unitLabel?: string
  helperText?: string
  type?: 'number' | 'time'
  inputProps?: Record<string, string | number>
}

const scheduleThresholdFields: TuningFieldDefinition[] = [
  {
    option: 'amEntryPctOfPeak',
    label: 'AM start threshold',
    inputProps: { min: 0, max: 1, step: 0.01 },
  },
  {
    option: 'amExitPctOfPeak',
    label: 'AM end threshold',
    inputProps: { min: 0, max: 1, step: 0.01 },
  },
  {
    option: 'pmEntryPctOfPeak',
    label: 'PM start threshold',
    inputProps: { min: 0, max: 1, step: 0.01 },
  },
  {
    option: 'pmExitPctOfPeak',
    label: 'PM end threshold',
    inputProps: { min: 0, max: 1, step: 0.01 },
  },
  {
    option: 'freeEntryPctOfDailyPeak',
    label: 'Daily peak threshold',
    inputProps: { min: 0, max: 1, step: 0.01 },
  },
  {
    option: 'freeEntryPctOfDynamicRange',
    label: 'Dynamic range threshold',
    inputProps: { min: 0, max: 1, step: 0.01 },
  },
  {
    option: 'entrySustainedBins',
    label: 'Consecutive bins for AM/PM transitions',
    inputProps: { min: 1, step: 1 },
  },
  {
    option: 'freeSustainedBins',
    label: 'Consecutive bins before FREE',
    inputProps: { min: 1, step: 1 },
  },
  {
    option: 'freeFallbackTime',
    label: 'Fallback FREE start time',
    type: 'time',
  },
  {
    option: 'maxAmEndTime',
    label: 'Latest allowed AM plan end',
    type: 'time',
  },
  {
    option: 'maxPmEndTime',
    label: 'Latest allowed PM plan end',
    type: 'time',
  },
]

const occupancyReviewFields: TuningFieldDefinition[] = [
  {
    option: 'laneCapacityVehiclesPerHour',
    label: 'Per-lane capacity',
    unitLabel: '(veh/hr)',
    helperText:
      'Converts approach volume into an estimated occupancy percentage.',
    inputProps: { min: 1, step: 1 },
  },
  {
    option: 'approachVolumeAssumedLanes',
    label: 'Fallback lanes per approach',
    helperText:
      'Used as the total lane count when lanes cannot be inferred from configured vehicle detectors and no direction override is provided.',
    inputProps: { min: 1, step: 1 },
  },
  {
    option: 'splitReviewThresholdPercent',
    label: 'Split review threshold',
    helperText: 'Below this share, splits default to standard allocation.',
    inputProps: { min: 0, max: 100, step: 1 },
  },
  {
    option: 'shoulderReviewThresholdPercent',
    label: 'Shoulder review threshold',
    helperText:
      'Above this share, the approach is flagged for shoulder-timing review.',
    inputProps: { min: 0, max: 100, step: 1 },
  },
]

const parseTimeString = (value: unknown) => {
  if (typeof value !== 'string') return null

  const match = /^(\d{1,2}):(\d{2})$/.exec(value)
  if (!match) return null

  const hours = Number(match[1])
  const minutes = Number(match[2])
  if (hours > 23 || minutes > 59) return null

  return new Date(2000, 0, 1, hours, minutes)
}

interface TimeOfDayAdvancedSidebarProps {
  activeSidebar: AnalysisSidebar
  options: TimeOfDayFormState
  onChange: (options: TimeOfDayFormState) => void
  schedulePresets?: TimeOfDaySchedulePreset[]
}

export default function TimeOfDayAdvancedSidebar({
  activeSidebar,
  options,
  onChange,
  schedulePresets = [],
}: TimeOfDayAdvancedSidebarProps) {
  const updateOptions = (patch: Partial<TimeOfDayFormState>) =>
    onChange({ ...options, ...patch })

  const handleTuningFieldChange =
    (field: TuningFieldDefinition) =>
    (event: ChangeEvent<HTMLInputElement>) => {
      updateOptions({
        [field.option]: Number(event.target.value),
      } as Partial<TimeOfDayFormState>)
    }

  const handleLaneCountChange = (direction: string, rawValue: string) => {
    const directionLaneCounts = { ...options.directionLaneCounts }
    const laneCount = Number(rawValue)

    if (rawValue === '' || Number.isNaN(laneCount)) {
      delete directionLaneCounts[direction]
    } else {
      directionLaneCounts[direction] = laneCount
    }

    updateOptions({ directionLaneCounts })
  }

  const selectedLaneDirections = directionList.filter(
    (direction) =>
      options.amPrimaryDirections.includes(direction) ||
      options.pmPrimaryDirections.includes(direction)
  )

  const renderTuningSlider = (
    field: TuningFieldDefinition,
    maximum: number,
    valueSuffix = '',
    displayMultiplier = 1
  ) => {
    const value = Number(
      (Number(options[field.option]) * displayMultiplier).toFixed(10)
    )

    return (
      <Box key={field.option}>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'baseline',
            justifyContent: 'space-between',
            gap: 1,
            mb: 0.25,
          }}
        >
          <Typography
            variant="body2"
            sx={{ fontSize: '0.75rem', fontWeight: 500 }}
          >
            {field.label}
          </Typography>
          <Typography
            variant="caption"
            sx={{
              px: 1,
              py: 0.125,
              borderRadius: 10,
              color: 'primary.main',
              bgcolor: (theme) => alpha(theme.palette.primary.main, 0.12),
              fontWeight: 700,
              lineHeight: 1.5,
            }}
          >
            {maximum === 1 ? value.toFixed(2) : value}
            {valueSuffix}
          </Typography>
        </Box>
        <Slider
          size="small"
          value={value}
          min={0}
          max={maximum}
          step={maximum === 1 ? 0.01 : 1}
          valueLabelDisplay="auto"
          valueLabelFormat={(sliderValue) =>
            maximum === 1
              ? Number(sliderValue).toFixed(2)
              : `${sliderValue}${valueSuffix}`
          }
          onChange={(_, sliderValue) =>
            updateOptions({
              [field.option]: Number(
                (Number(sliderValue) / displayMultiplier).toFixed(10)
              ),
            } as Partial<TimeOfDayFormState>)
          }
          aria-label={field.label}
          sx={{
            py: 0.5,
            color: 'primary.main',
            '& .MuiSlider-rail': {
              opacity: 1,
              bgcolor: 'grey.200',
            },
            '& .MuiSlider-track': {
              border: 0,
            },
            '& .MuiSlider-thumb': {
              width: 16,
              height: 16,
            },
          }}
        />
        {field.helperText && (
          <Typography
            variant="caption"
            color="text.disabled"
            sx={{ display: 'block', mt: 0.25, lineHeight: 1.45 }}
          >
            {field.helperText}
          </Typography>
        )}
      </Box>
    )
  }

  const renderCompactTuningField = (field: TuningFieldDefinition) => (
    <Box
      key={field.option}
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: 2,
      }}
    >
      <Typography variant="body2" sx={{ fontSize: '0.75rem', fontWeight: 500 }}>
        {field.label}
      </Typography>
      {field.type === 'time' ? (
        <TimePicker
          ampm={false}
          format="HH:mm"
          closeOnSelect
          value={parseTimeString(options[field.option])}
          onChange={(value) =>
            updateOptions({
              [field.option]:
                value && !Number.isNaN(value.getTime())
                  ? format(value, 'HH:mm')
                  : '',
            } as Partial<TimeOfDayFormState>)
          }
          slotProps={{
            textField: {
              size: 'small',
              inputProps: { 'aria-label': field.label },
            },
          }}
          sx={{ width: 140, flexShrink: 0 }}
        />
      ) : (
        <TextField
          size="small"
          type="number"
          value={options[field.option]}
          onChange={handleTuningFieldChange(field)}
          inputProps={{
            ...field.inputProps,
            'aria-label': field.label,
          }}
          sx={{ width: 104, flexShrink: 0 }}
        />
      )}
    </Box>
  )

  const renderStackedTuningField = (field: TuningFieldDefinition) => (
    <Box key={field.option} sx={{ minWidth: 0 }}>
      <Typography
        variant="body2"
        sx={{
          mb: field.helperText ? 0.25 : 0.75,
          fontSize: '0.75rem',
          fontWeight: 500,
        }}
      >
        {field.label}{' '}
        {field.unitLabel && (
          <Box component="span" sx={{ color: 'text.secondary' }}>
            {field.unitLabel}
          </Box>
        )}
      </Typography>
      {field.helperText && (
        <Typography
          variant="caption"
          color="text.secondary"
          sx={{ display: 'block', mb: 0.75, lineHeight: 1.45 }}
        >
          {field.helperText}
        </Typography>
      )}
      <TextField
        fullWidth
        size="small"
        type="number"
        value={options[field.option]}
        onChange={handleTuningFieldChange(field)}
        inputProps={{
          ...field.inputProps,
          'aria-label': [field.label, field.unitLabel]
            .filter(Boolean)
            .join(' '),
        }}
      />
    </Box>
  )

  const renderSidebarSection = (
    title: string,
    children: ReactNode,
    description?: string
  ) => (
    <Box
      component="section"
      sx={{
        border: 1,
        borderColor: 'divider',
        borderRadius: 1.5,
        p: 1.5,
      }}
    >
      <Typography
        variant="subtitle2"
        sx={{ mb: description ? 0.5 : 1.5, fontWeight: 500 }}
      >
        {title}
      </Typography>
      {description && (
        <Typography
          variant="caption"
          color="text.secondary"
          sx={{ display: 'block', mb: 1.5, lineHeight: 1.55 }}
        >
          {description}
        </Typography>
      )}
      <Stack spacing={1.5}>{children}</Stack>
    </Box>
  )

  return (
    <RightSidebar
      width={420}
      dismissOnBackdrop
      title={
        activeSidebar === 'schedule'
          ? 'Roadway Type Thresholds'
          : 'Occupancy and Review'
      }
      subtitle={
        activeSidebar === 'occupancy'
          ? 'Capacity, review flags, and lane configuration'
          : undefined
      }
    >
      <Box
        sx={{
          p: 2,
          pt: 1,
          flex: 1,
          minHeight: 0,
          overflowY: 'auto',
        }}
      >
        {activeSidebar === 'schedule' ? (
          <Stack spacing={2}>
            <TimeOfDaySchedulePresetSelect
              options={options}
              onChange={onChange}
              presets={schedulePresets}
            />
            {renderSidebarSection(
              'AM / PM Transition Thresholds',
              <Stack spacing={1}>
                {scheduleThresholdFields
                  .slice(0, 4)
                  .map((field) => renderTuningSlider(field, 100, '%', 100))}
              </Stack>,
              'Sets the volume levels that start and end AM/PM plans, expressed relative to each period’s peak.'
            )}
            {renderSidebarSection(
              'FREE Transition',
              <Stack spacing={1}>
                {scheduleThresholdFields
                  .slice(4, 6)
                  .map((field) => renderTuningSlider(field, 100, '%', 100))}
                {renderCompactTuningField(scheduleThresholdFields[8])}
              </Stack>,
              'Uses the daily peak and baseline-to-peak dynamic range to decide when falling evening volume returns the schedule to FREE. The fallback time is used when no sustained transition is found.'
            )}
            {renderSidebarSection(
              'Transition Confirmation',
              <Stack spacing={1.5}>
                {renderCompactTuningField(scheduleThresholdFields[6])}
                {renderCompactTuningField(scheduleThresholdFields[7])}
              </Stack>,
              'Requires each transition threshold to be met for its configured number of consecutive bins, preventing brief spikes or dips from changing the schedule.'
            )}
            {renderSidebarSection(
              'Plan Length Limits',
              <>
                {renderCompactTuningField(scheduleThresholdFields[9])}
                {renderCompactTuningField(scheduleThresholdFields[10])}
              </>,
              'Sets the latest time each peak plan can end. These limits also serve as fallbacks when the volume profile does not show a clear transition.'
            )}
          </Stack>
        ) : (
          <Stack spacing={1.5}>
            {renderSidebarSection(
              'Capacity',
              renderStackedTuningField(occupancyReviewFields[0])
            )}
            {renderSidebarSection(
              'Review thresholds',
              <>
                {occupancyReviewFields
                  .slice(2)
                  .map((field) => renderTuningSlider(field, 100, '%'))}
              </>,
              'Flags when the cross street’s share of combined traffic warrants split-allocation or shoulder-timing review.'
            )}
            {renderSidebarSection(
              'Lane configuration',
              <>
                {selectedLaneDirections.length === 0 ? (
                  <Typography variant="body2" color="text.secondary">
                    Select at least one primary direction to configure lanes.
                  </Typography>
                ) : (
                  <Box
                    sx={{
                      display: 'grid',
                      gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
                      gap: 1.25,
                    }}
                  >
                    {selectedLaneDirections.map((direction) => (
                      <Box key={direction} sx={{ minWidth: 0 }}>
                        <Typography
                          variant="body2"
                          sx={{
                            mb: 0.75,
                            fontSize: '0.75rem',
                            fontWeight: 500,
                          }}
                        >
                          {direction} lanes
                        </Typography>
                        <TextField
                          fullWidth
                          size="small"
                          type="number"
                          placeholder="Auto"
                          value={options.directionLaneCounts[direction] ?? ''}
                          onChange={(event) =>
                            handleLaneCountChange(direction, event.target.value)
                          }
                          inputProps={{
                            min: 0,
                            step: 1,
                            'aria-label': [direction, 'lanes'].join(' '),
                          }}
                        />
                      </Box>
                    ))}
                  </Box>
                )}
                {renderStackedTuningField(occupancyReviewFields[1])}
              </>,
              'Optional direction counts override lanes inferred from configured vehicle detectors. Leave them blank to use detector data or the fallback lane count below.'
            )}
          </Stack>
        )}
      </Box>
    </RightSidebar>
  )
}
