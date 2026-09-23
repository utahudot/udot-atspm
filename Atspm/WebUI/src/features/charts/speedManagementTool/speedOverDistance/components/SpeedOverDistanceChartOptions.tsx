import { toUTCDateStamp } from '@/utils/dateTime'
import { Box } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { isValid, startOfMonth, subMonths } from 'date-fns'
import { useEffect, useState } from 'react'

export interface SpeedOverDistanceChartOptionsValues {
  startDate: string
  endDate: string
}

interface SpeedOverDistanceChartOptionsProps {
  onOptionsChange: (options: SpeedOverDistanceChartOptionsValues) => void
}

const SpeedOverDistanceOptions = ({
  onOptionsChange,
}: SpeedOverDistanceChartOptionsProps) => {
  const [startDate, setStartDate] = useState<Date | null>(
    startOfMonth(subMonths(new Date(), 1))
  )
  const [endDate, setEndDate] = useState<Date | null>(new Date())

  useEffect(() => {
    if (startDate && endDate) {
      onOptionsChange({
        startDate: toUTCDateStamp(startDate),
        endDate: toUTCDateStamp(endDate),
      })
    } else {
      onOptionsChange({
        startDate: '',
        endDate: '',
      })
    }
  }, [startDate, endDate, onOptionsChange])

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
    </Box>
  )
}

export default SpeedOverDistanceOptions
