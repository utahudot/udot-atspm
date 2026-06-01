import { DataQualityOptions } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { toUTCDateStamp } from '@/utils/dateTime'
import { Box } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { endOfMonth, isValid, parse, startOfMonth } from 'date-fns'
import { useEffect, useState } from 'react'

interface DataQualityChartOptionsProps {
  onOptionsChange: (options: DataQualityOptions) => void
}

const DataQualityChartOptions = ({
  onOptionsChange,
}: DataQualityChartOptionsProps) => {
  const { submittedRouteSpeedRequest } = useSpeedManagementStore()

  const [startDate, setStartDate] = useState(
    submittedRouteSpeedRequest?.startDate
      ? startOfMonth(
          parse(submittedRouteSpeedRequest.startDate, 'yyyy-MM-dd', new Date())
        )
      : null
  )
  const [endDate, setEndDate] = useState(
    submittedRouteSpeedRequest?.endDate
      ? endOfMonth(
          parse(submittedRouteSpeedRequest.endDate, 'yyyy-MM-dd', new Date())
        )
      : null
  )

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
        maxDate={endDate ?? undefined}
      />
      <DatePicker
        label="End Date"
        value={endDate}
        onChange={handleEndDateChange}
        minDate={startDate ?? undefined}
      />
    </Box>
  )
}

export default DataQualityChartOptions
