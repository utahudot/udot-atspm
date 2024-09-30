import { Box, TextField } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { isValid, parse } from 'date-fns'
import { useEffect, useState } from 'react'

export interface SpeedComplianceChartOptionsValues {
  startDate: string
  endDate: string
  customSpeedLimit?: number | null
}

const initialStartDateString = '2023-01-01'
const initialEndDateString = '2023-02-01'

const parsedInitialStartDate = parse(
  initialStartDateString,
  'yyyy-MM-dd',
  new Date()
)
const initialStartDate = isValid(parsedInitialStartDate)
  ? parsedInitialStartDate
  : null

const parsedInitialEndDate = parse(
  initialEndDateString,
  'yyyy-MM-dd',
  new Date()
)
const initialEndDate = isValid(parsedInitialEndDate)
  ? parsedInitialEndDate
  : null

interface SpeedComplianceChartOptionsProps {
  onOptionsChange: (options: SpeedComplianceChartOptionsValues) => void
}

const SpeedComplianceChartOptions = ({
  onOptionsChange,
}: SpeedComplianceChartOptionsProps) => {
  const [startDate, setStartDate] = useState<Date | null>(initialStartDate)
  const [endDate, setEndDate] = useState<Date | null>(initialEndDate)
  const [customSpeedLimit, setCustomSpeedLimit] = useState<number | null>(null)
  const [startDateError, setStartDateError] = useState<boolean>(false)
  const [endDateError, setEndDateError] = useState<boolean>(false)

  useEffect(() => {
    if (startDate && endDate) {
      onOptionsChange({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
        customSpeedLimit: customSpeedLimit ?? null,

      })
    } else {
      onOptionsChange({
        startDate: '',
        endDate: '',
        customSpeedLimit: null,
      })
    }
  }, [startDate, endDate, customSpeedLimit])

  const handleStartDateChange = (date: Date | null) => {
    if (date && isValid(date)) {
      setStartDate(date)
      setStartDateError(false)
    } else {
      setStartDate(null)
      setStartDateError(true)
    }
  }

  const handleEndDateChange = (date: Date | null) => {
    if (date && isValid(date)) {
      setEndDate(date)
      setEndDateError(false)
    } else {
      setEndDate(null)
      setEndDateError(true)
    }
  }

  return (
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
    <TextField 
        label="Custom Speed Limit" 
        variant="outlined" 
        type="number"
        onChange={(e) =>
            setCustomSpeedLimit(e.target.value ? parseInt(e.target.value, 10) : null)
          }        
          />
        

    </Box>
  )
}

export default SpeedComplianceChartOptions
