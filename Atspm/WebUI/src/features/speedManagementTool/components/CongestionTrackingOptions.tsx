import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  SelectChangeEvent,
} from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { useEffect, useState } from 'react'

export interface CongestionTrackingOptionsValues {
  selectedDate: Date | null
  selectedSource: number
}

interface CongestionTrackingOptionsProps {
  onOptionsChange: (options: CongestionTrackingOptionsValues) => void
}

const CongestionTrackingOptions = ({
  onOptionsChange,
}: CongestionTrackingOptionsProps) => {
  const [selectedDate, setSelectedDate] = useState<Date | null>(new Date())
  const [selectedSource, setSelectedSource] = useState<number>(0)

  useEffect(() => {
    onOptionsChange({ selectedDate, selectedSource })
  }, [selectedDate, selectedSource])

  const handleDateChange = (date: Date | null) => {
    setSelectedDate(date)
  }

  const handleSourceChange = (event: SelectChangeEvent<number>) => {
    setSelectedSource(event.target.value as number)
  }

  return (
    <Box display="flex" gap={2}>
      <DatePicker
        views={['month', 'year']}
        label="Select Month"
        minDate={new Date('2000-01-01')}
        maxDate={new Date()}
        value={selectedDate}
        onChange={handleDateChange}
      />
      <FormControl sx={{ width: '150px' }}>
        <InputLabel id="source-select-label">Source Select</InputLabel>
        <Select
          labelId="source-select-label"
          id="source-select"
          value={selectedSource}
          label="Source Select"
          onChange={handleSourceChange}
        >
          <MenuItem value={0}>PeMS</MenuItem>
          <MenuItem value={1}>ATSPM</MenuItem>
          <MenuItem value={2}>Clear Guide</MenuItem>
        </Select>
      </FormControl>
    </Box>
  )
}

export default CongestionTrackingOptions
