import { Box } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { useEffect, useState } from 'react'

export interface DataQualityChartOptionsValues {
  startDate: string
  endDate: string
}

const initialDates = {
  startDate: new Date(new Date().setMonth(new Date().getMonth() - 10)),
  endDate: new Date(),
}

interface DataQualityChartOptionsProps {
  onOptionsChange: (options: DataQualityChartOptionsValues) => void
}

const DataQualityChartOptions = ({
  onOptionsChange,
}: DataQualityChartOptionsProps) => {
  const [startDate, setStartDate] = useState<Date | null>(
    initialDates.startDate
  )
  const [endDate, setEndDate] = useState<Date | null>(initialDates.endDate)

  useEffect(() => {
    if (startDate && endDate) {
      onOptionsChange({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
      })
    }
  }, [startDate, endDate])

  const handleStartDateChange = (date: Date | null) => {
    setStartDate(date)
  }

  const handleEndDateChange = (date: Date | null) => {
    setEndDate(date)
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

export default DataQualityChartOptions
