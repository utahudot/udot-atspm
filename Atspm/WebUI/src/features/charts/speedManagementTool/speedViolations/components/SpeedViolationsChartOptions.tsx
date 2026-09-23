import {
  DayOfWeek,
  SpeedViolationsOptions,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { toUTCDateStamp } from '@/utils/dateTime'
import { Box } from '@mui/material'
import { DatePicker, TimePicker } from '@mui/x-date-pickers'
import { endOfMonth, format, isValid, parse } from 'date-fns'
import { useEffect, useState } from 'react'

interface SpeedViolationsChartOptionsProps {
  onOptionsChange: (options: SpeedViolationsOptions) => void
  sourceId: number
}

const daysOfWeekList: string[] = [
  'Sun',
  'Mon',
  'Tue',
  'Wed',
  'Thu',
  'Fri',
  'Sat',
]

const SpeedViolationsChartOptions = ({
  onOptionsChange,
  sourceId,
}: SpeedViolationsChartOptionsProps) => {
  const { submittedRouteSpeedRequest } = useSpeedManagementStore()

  const [startDate, setStartDate] = useState(
    submittedRouteSpeedRequest?.startDate
      ? parse(submittedRouteSpeedRequest.startDate, 'yyyy-MM-dd', new Date())
      : null
  )
  const [endDate, setEndDate] = useState(
    submittedRouteSpeedRequest?.endDate
      ? endOfMonth(
          parse(submittedRouteSpeedRequest.endDate, 'yyyy-MM-dd', new Date())
        )
      : null
  )
  const [startTime, setStartTime] = useState<Date | null>(null)
  const [endTime, setEndTime] = useState<Date | null>(null)
  const [daysOfWeek, setDaysOfWeek] = useState<DayOfWeek[]>([
    0, 1, 2, 3, 4, 5, 6,
  ])

  useEffect(() => {
    if (startDate && endDate && daysOfWeek) {
      const options: Partial<SpeedViolationsOptions> = {
        startDate: toUTCDateStamp(startDate),
        endDate: toUTCDateStamp(endDate),
        sourceId,
        daysOfWeek,
      }

      if (startTime !== null && isValid(startTime)) {
        options.startTime = format(startTime, "yyyy-MM-dd'T'HH:mm:ss")
      }
      if (endTime !== null && isValid(endTime)) {
        options.endTime = format(endTime, "yyyy-MM-dd'T'HH:mm:ss")
      }

      onOptionsChange(options)
    }
  }, [
    startDate,
    endDate,
    startTime,
    endTime,
    daysOfWeek,
    onOptionsChange,
    sourceId,
  ])

  const handleStartDateChange = (date: Date | null) => {
    if (date === null || isValid(date)) {
      setStartDate(date)
    } else {
      setStartDate(null)
    }
  }

  const handleEndDateChange = (date: Date | null) => {
    if (date === null || isValid(date)) {
      setEndDate(date)
    } else {
      setEndDate(null)
    }
  }

  const handleStartTimeChange = (time: Date | null) => {
    setStartTime(time)
  }

  const handleEndTimeChange = (time: Date | null) => {
    setEndTime(time)
  }

  const handleDaysOfWeekChange = (days: DayOfWeek[]) => {
    setDaysOfWeek(days)
  }

  return (
    <Box display="flex" flexDirection="column" gap={2}>
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
      <Box display="flex" gap={2}>
        <TimePicker
          label="Start Time"
          value={startTime}
          onChange={handleStartTimeChange}
          ampm={false}
        />
        <TimePicker
          label="End Time"
          value={endTime}
          onChange={handleEndTimeChange}
          ampm={false}
        />
      </Box>
      <Box maxWidth="510px">
        <MultiSelectCheckbox
          itemList={daysOfWeekList}
          selectedItems={daysOfWeek}
          setSelectedItems={handleDaysOfWeekChange}
          header="Days To Include"
          direction="horizontal"
        />
      </Box>
    </Box>
  )
}

export default SpeedViolationsChartOptions
