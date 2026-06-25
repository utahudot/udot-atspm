import { CongestionTrackingOptions } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { toUTCDateStamp } from '@/utils/dateTime'
import { Box } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { isValid, parse } from 'date-fns'
import { useEffect, useState } from 'react'

interface CongestionTrackingOptionsProps {
  onOptionsChange: (options: Partial<CongestionTrackingOptions>) => void
  sourceId: number
}

const CongestionTrackingChartOptions = ({
  onOptionsChange,
  sourceId,
}: CongestionTrackingOptionsProps) => {
  const { submittedRouteSpeedRequest } = useSpeedManagementStore()
  const [selectedDate, setSelectedDate] = useState(
    submittedRouteSpeedRequest?.startDate
      ? parse(submittedRouteSpeedRequest.startDate, 'yyyy-MM-dd', new Date())
      : null
  )
  useEffect(() => {
    if (!selectedDate) {
      onOptionsChange({
        startDate: '',
        endDate: '',
        sourceId,
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
      startDate: toUTCDateStamp(startDate),
      endDate: toUTCDateStamp(endDate),
      sourceId,
    })
  }, [selectedDate, sourceId, onOptionsChange])

  const handleDateChange = (date: Date | null) => {
    if (date && isValid(date)) {
      setSelectedDate(date)
    } else {
      setSelectedDate(null)
    }
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
    </Box>
  )
}

export default CongestionTrackingChartOptions
