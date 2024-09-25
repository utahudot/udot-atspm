import { DataSource } from '@/features/speedManagementTool/enums'
import { Box, SelectChangeEvent } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { isValid, parse } from 'date-fns'
import { useEffect, useState } from 'react'

export interface CongestionTrackingOptionsValues {
  startDate: string
  endDate: string
  sourceId: number
}

interface CongestionTrackingOptionsProps {
  onOptionsChange: (options: CongestionTrackingOptionsValues) => void
}

const initialDateString = '2023-01-01'
const parsedInitialDate = parse(initialDateString, 'yyyy-MM-dd', new Date())
const initialDate = isValid(parsedInitialDate) ? parsedInitialDate : null

const CongestionTrackingOptions = ({
  onOptionsChange,
}: CongestionTrackingOptionsProps) => {
  const [selectedDate, setSelectedDate] = useState<Date | null>(initialDate)
  const [selectedSource, setSelectedSource] = useState<DataSource>(
    DataSource.PeMS
  )
  const [dateError, setDateError] = useState<boolean>(false)

  useEffect(() => {
    if (!selectedDate) {
      onOptionsChange({
        startDate: '',
        endDate: '',
        sourceId: selectedSource,
      })
      return
    }

    const startDate = new Date(
      selectedDate.getFullYear(),
      selectedDate.getMonth(),
      1
    )
    const endDate = new Date(
      selectedDate.getFullYear(),
      selectedDate.getMonth() + 1,
      0
    )

    onOptionsChange({
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0],
      sourceId: selectedSource,
    })
  }, [selectedDate, selectedSource])

  const handleDateChange = (date: Date | null) => {
    if (date && isValid(date)) {
      setSelectedDate(date)
      setDateError(false)
    } else {
      setSelectedDate(null)
      setDateError(true)
    }
  }

  const handleSourceChange = (event: SelectChangeEvent<number>) => {
    setSelectedSource(event.target.value as DataSource)
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
      {/* Uncomment and update the Select component if needed */}
      {/* <FormControl sx={{ width: '150px' }}>
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
  )
}

export default CongestionTrackingOptions
