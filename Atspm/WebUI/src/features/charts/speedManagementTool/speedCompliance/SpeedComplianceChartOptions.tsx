import { SpeedOverDistanceOptions } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { toUTCDateStamp } from '@/utils/dateTime'
import { Box, TextField } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { isValid, startOfMonth, subMonths } from 'date-fns'
import { useEffect, useState } from 'react'

interface SpeedOverDistanceExtendedChartOptions
  extends SpeedOverDistanceOptions {
  customSpeedLimit: number | null
}

interface SpeedComplianceChartOptionsProps {
  onOptionsChange: (
    options: Partial<SpeedOverDistanceExtendedChartOptions>
  ) => void
}

const SpeedComplianceChartOptions = ({
  onOptionsChange,
}: SpeedComplianceChartOptionsProps) => {
  const [startDate, setStartDate] = useState<Date | null>(
    startOfMonth(subMonths(new Date(), 1))
  )
  const [endDate, setEndDate] = useState<Date | null>(new Date())
  const [customSpeedLimit, setCustomSpeedLimit] = useState<number | null>(null)

  useEffect(() => {
    if (startDate && endDate) {
      onOptionsChange({
        startDate: toUTCDateStamp(startDate),
        endDate: toUTCDateStamp(endDate),
        customSpeedLimit: customSpeedLimit ?? null,
      })
    } else {
      onOptionsChange({
        startDate: '',
        endDate: '',
        customSpeedLimit: null,
      })
    }
  }, [startDate, endDate, customSpeedLimit, onOptionsChange])

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
      <TextField
        label="Custom Speed Limit"
        variant="outlined"
        type="number"
        onChange={(e) =>
          setCustomSpeedLimit(
            e.target.value ? parseInt(e.target.value, 10) : null
          )
        }
      />
    </Box>
  )
}

export default SpeedComplianceChartOptions
