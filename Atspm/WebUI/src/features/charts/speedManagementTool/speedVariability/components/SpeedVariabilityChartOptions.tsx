import {
  DayOfWeek,
  SpeedVariabilityOptions,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { toUTCDateStamp } from '@/utils/dateTime'
import { Box } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers'
import { endOfMonth, isValid, parse } from 'date-fns'
import { useEffect, useState } from 'react'

const daysOfWeekList: string[] = [
  'Sun',
  'Mon',
  'Tue',
  'Wed',
  'Thu',
  'Fri',
  'Sat',
]

interface SpeedVariabilityOptionsProps {
  onOptionsChange: (options: SpeedVariabilityOptions) => void
  sourceId: number
}

const SpeedVariabilityChartOptions = ({
  onOptionsChange,
  sourceId,
}: SpeedVariabilityOptionsProps) => {
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
  const [daysOfWeek, setDaysOfWeek] = useState<DayOfWeek[]>([
    0, 1, 2, 3, 4, 5, 6,
  ])
  useEffect(() => {
    if (startDate && endDate && daysOfWeek) {
      onOptionsChange({
        startDate: toUTCDateStamp(startDate),
        endDate: toUTCDateStamp(endDate),
        sourceId,
        daysOfWeek,
      })
    }
  }, [startDate, endDate, onOptionsChange, daysOfWeek])

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

  const handleDaysOfWeekChange = (days: DayOfWeek[]) => {
    setDaysOfWeek(days)
  }

  return (
    <Box display="flex" flexDirection="column" gap={2}>
      <Box display="flex" gap={2}>
        <DatePicker
          label="Start Date"
          value={startDate}
          maxDate={endDate ?? undefined}
          onChange={handleStartDateChange}
        />
        <DatePicker
          label="End Date"
          value={endDate}
          onChange={handleEndDateChange}
          minDate={startDate ?? undefined}
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

export default SpeedVariabilityChartOptions
