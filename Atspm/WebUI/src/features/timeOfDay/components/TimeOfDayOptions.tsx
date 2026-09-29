import { Box } from '@mui/material'
import type { TimeOfDaySchedulePreset } from '../measureDefaults'
import type { TimeOfDayFormState } from '../types'
import TimeOfDayAnalysisOptions from './TimeOfDayAnalysisOptions'
import TimeOfDayCorridorOptions from './TimeOfDayCorridorOptions'
import TimeOfDayDatesOptions from './TimeOfDayDatesOptions'

interface TimeOfDayOptionsProps {
  options: TimeOfDayFormState
  onChange: (options: TimeOfDayFormState) => void
  schedulePresets?: TimeOfDaySchedulePreset[]
}

export default function TimeOfDayOptions({
  options,
  onChange,
  schedulePresets = [],
}: TimeOfDayOptionsProps) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexWrap: 'wrap',
        gap: 2,
        alignItems: 'stretch',
      }}
    >
      <TimeOfDayCorridorOptions options={options} onChange={onChange} />
      <TimeOfDayDatesOptions options={options} onChange={onChange} />
      <TimeOfDayAnalysisOptions
        options={options}
        onChange={onChange}
        schedulePresets={schedulePresets}
      />
    </Box>
  )
}
