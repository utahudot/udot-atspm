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
import { endOfMonth, isValid, parse, startOfMonth } from 'date-fns'
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
  sourceId: number
}

const initialDateString = '2023-01-01'
const parsedInitialDate = parse(initialDateString, 'yyyy-MM-dd', new Date())
const initialDate = isValid(parsedInitialDate) ? parsedInitialDate : null

const SpeedOverTimeOptions = ({
  onOptionsChange,
  sourceId,
}: SpeedOverTimeOptionsProps) => {
  const [startDate, setStartDate] = useState<Date | null>(
    initialDate ? startOfMonth(initialDate) : null
  )
  const [endDate, setEndDate] = useState<Date | null>(
    initialDate ? endOfMonth(initialDate) : null
  )

  const [selectedSource, setSelectedSource] = useState<DataSource>(sourceId)
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
    } else {
      // Handle invalid dates by sending empty strings
      onOptionsChange({
        startDate: '',
        endDate: '',
        sourceId: selectedSource,
        timeOptions: selectedTimeOptions,
        startTime: '',
        endTime: '',
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
    if (date && isValid(date)) {
      setStartDate(date)
    } else {
      setStartDate(null)
    }
  }

  const handleEndDateChange = (date: Date | null) => {
    if (date && isValid(date)) {
      setEndDate(date)
    } else {
      setEndDate(null)
    }
  }

  const handleSourceChange = (event: SelectChangeEvent<number>) => {
    setSelectedSource(event.target.value as DataSource)
  }

  const handleTimeOptionsChange = (event: SelectChangeEvent<TimeOptions>) => {
    setSelectedTimeOptions(event.target.value as TimeOptions)
  }

  const handleStartTimeChange = (time: Date | null) => {
    if (time && isValid(time)) {
      setStartTime(time)
      // You can add error handling for time if needed
    } else {
      setStartTime(null)
      // Handle time errors if necessary
    }
  }

  const handleEndTimeChange = (time: Date | null) => {
    if (time && isValid(time)) {
      setEndTime(time)
      // You can add error handling for time if needed
    } else {
      setEndTime(null)
      // Handle time errors if necessary
    }
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
      {/* Uncomment and update if needed */}
      {/* <Box display="flex" gap={2}>
        <TimePicker
          label="Start Time"
          value={startTime}
          onChange={handleStartTimeChange}
          renderInput={(params) => (
            <TextField
              {...params}
              // Add error handling if necessary
              InputLabelProps={{ shrink: true }}
            />
          )}
          fullWidth
        />
        <TimePicker
          label="End Time"
          value={endTime}
          onChange={handleEndTimeChange}
          renderInput={(params) => (
            <TextField
              {...params}
              // Add error handling if necessary
              InputLabelProps={{ shrink: true }}
            />
          )}
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
        {/* Uncomment and update the Source Select component if needed */}
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
