import { DataSource, TimeOptions } from '@/features/speedManagementTool/enums'
import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  SelectChangeEvent,
} from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers'
import { endOfMonth, startOfMonth } from 'date-fns'
import { useEffect, useState } from 'react'

export interface SpeedOverTimeOptionsValues {
  startDate: string
  endDate: string
  sourceId: number
  timeOptions: TimeOptions
  startTime: string
  endTime: string
}

interface SpeedOverTimeOptionsProps {
  onOptionsChange: (options: SpeedOverTimeOptionsValues) => void
}

// todo - remove this
const initialDate = new Date('2024-05-10')

const SpeedOverTimeOptions = ({
  onOptionsChange,
}: SpeedOverTimeOptionsProps) => {
  const [startDate, setStartDate] = useState<Date | null>(
    startOfMonth(initialDate)
  )
  const [endDate, setEndDate] = useState<Date | null>(endOfMonth(initialDate))

  const [selectedSource, setSelectedSource] = useState<DataSource>(
    DataSource.PeMS
  )
  const [selectedTimeOptions, setSelectedTimeOptions] = useState<TimeOptions>(
    TimeOptions.Hour
  )
  const [startTime, setStartTime] = useState<Date | null>(new Date())
  const [endTime, setEndTime] = useState<Date | null>(new Date())

  useEffect(() => {
    if (startDate && endDate && startTime && endTime) {
      onOptionsChange({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
        sourceId: selectedSource,
        timeOptions: selectedTimeOptions,
        startTime: startTime.toISOString(),
        endTime: endTime.toISOString(),
      })
    }
  }, [
    startDate,
    endDate,
    selectedSource,
    selectedTimeOptions,
    startTime,
    endTime,
  ])

  const handleStartDateChange = (date: Date | null) => {
    setStartDate(date)
  }

  const handleEndDateChange = (date: Date | null) => {
    setEndDate(date)
  }

  const handleSourceChange = (event: SelectChangeEvent<number>) => {
    setSelectedSource(event.target.value as DataSource)
  }

  const handleTimeOptionsChange = (event: SelectChangeEvent<TimeOptions>) => {
    setSelectedTimeOptions(event.target.value as TimeOptions)
  }

  const handleStartTimeChange = (time: Date | null) => {
    setStartTime(time)
  }

  const handleEndTimeChange = (time: Date | null) => {
    setEndTime(time)
  }

  return (
    <Box display="flex" flexDirection="column" gap={2}>
      {/* Row 1: Start Date and End Date */}
      <Box display="flex" gap={2}>
        <DatePicker
          label="Start Date"
          value={startDate}
          onChange={handleStartDateChange}
        />
        <DatePicker
          label="End Date"
          value={endDate}
          onChange={handleEndDateChange}
        />
      </Box>

      {/* Row 2: Start Time and End Time */}
      {/* <Box display="flex" gap={2}>
        <TimePicker
          label="Start Time"
          value={startTime}
          onChange={handleStartTimeChange}
          fullWidth
        />
        <TimePicker
          label="End Time"
          value={endTime}
          onChange={handleEndTimeChange}
          fullWidth
        />
      </Box> */}

      {/* Row 3: Time Options and Source Select */}
      <Box display="flex" gap={2}>
        <FormControl sx={{ width: '150px' }}>
          <InputLabel id="time-options-select-label">Bin Size</InputLabel>
          <Select
            labelId="time-options-select-label"
            id="time-options-select"
            value={selectedTimeOptions}
            label="Bin Size"
            onChange={handleTimeOptionsChange}
          >
            <MenuItem value={TimeOptions.Hour}>Hour</MenuItem>
            <MenuItem value={TimeOptions.Week}>Week</MenuItem>
            <MenuItem value={TimeOptions.Month}>Month</MenuItem>
          </Select>
        </FormControl>
        {/* <FormControl fullWidth>
          <InputLabel id="source-select-label">Source Select</InputLabel>
          <Select
            labelId="source-select-label"
            id="source-select"
            value={selectedSource}
            label="Source Select"
            onChange={handleSourceChange}
          >
            <MenuItem value={DataSource.PeMS}>PeMS</MenuItem>
            <MenuItem value={DataSource.ATSPM}>ATSPM</MenuItem>
            <MenuItem value={DataSource.ClearGuide}>Clear Guide</MenuItem>
          </Select>
        </FormControl> */}
      </Box>
    </Box>
  )
}

export default SpeedOverTimeOptions
