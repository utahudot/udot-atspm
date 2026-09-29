import { MenuItem, TextField } from '@mui/material'
import {
  findMatchingTimeOfDaySchedulePreset,
  type TimeOfDaySchedulePreset,
} from '../measureDefaults'
import type { TimeOfDayFormState } from '../types'

interface TimeOfDaySchedulePresetSelectProps {
  options: TimeOfDayFormState
  onChange: (options: TimeOfDayFormState) => void
  presets: TimeOfDaySchedulePreset[]
}

export default function TimeOfDaySchedulePresetSelect({
  options,
  onChange,
  presets,
}: TimeOfDaySchedulePresetSelectProps) {
  const matchingPreset = findMatchingTimeOfDaySchedulePreset(options, presets)

  const handlePresetChange = (presetId: string) => {
    const preset = presets.find(
      (candidate) => String(candidate.id) === presetId
    )
    if (preset) onChange({ ...options, ...preset.options })
  }

  return (
    <TextField
      select
      fullWidth
      size="small"
      label="Roadway type threshold preset"
      value={matchingPreset ? String(matchingPreset.id) : 'custom'}
      onChange={(event) => handlePresetChange(event.target.value)}
      sx={{ flex: 1, minWidth: 0 }}
    >
      <MenuItem value="custom" disabled>
        Custom
      </MenuItem>
      {presets.map((preset) => (
        <MenuItem key={preset.id} value={String(preset.id)}>
          FHWA {preset.name}
        </MenuItem>
      ))}
    </TextField>
  )
}
