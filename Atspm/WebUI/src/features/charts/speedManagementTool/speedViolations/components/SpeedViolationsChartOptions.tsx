import { DayOfWeek } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import { Box } from '@mui/material'
import { DatePicker, TimePicker } from '@mui/x-date-pickers'
import { endOfMonth, isValid, startOfMonth } from 'date-fns'
import { useEffect, useState } from 'react'

export interface SpeedViolationsChartOptionsValues {
  startDate: string
  endDate: string
  startTime?: string
  endTime?: string
  daysOfWeek: DayOfWeek[]
  isHolidaysFiltered?: boolean
  sourceId: number
}

interface SpeedViolationsChartOptionsProps {
  onOptionsChange: (options: Partial<SpeedViolationsChartOptionsValues>) => void
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

const initialDate = new Date('2023-01-02')

const SpeedViolationsChartOptions = ({
  onOptionsChange,
}: SpeedViolationsChartOptionsProps) => {
  const [startDate, setStartDate] = useState<Date | null>(
    startOfMonth(initialDate)
  )
  const [endDate, setEndDate] = useState<Date | null>(endOfMonth(initialDate))
  const [startTime, setStartTime] = useState<Date | null>(null)
  const [endTime, setEndTime] = useState<Date | null>(null)
  const [daysOfWeek, setDaysOfWeek] = useState<DayOfWeek[]>([
    0, 1, 2, 3, 4, 5, 6,
  ])

  useEffect(() => {
    if (startDate && endDate && daysOfWeek) {
      const options: Partial<SpeedViolationsChartOptionsValues> = {
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
      }

      if (startTime) {
        options.startTime = startTime.toTimeString().split(' ')[0]
      }
      if (endTime) {
        options.endTime = endTime.toTimeString().split(' ')[0]
      }

      onOptionsChange(options)
    }
  }, [startDate, endDate, startTime, endTime, daysOfWeek, onOptionsChange])

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
    if (time === null || isValid(time)) {
      setStartTime(time)
    } else {
      setStartTime(null)
    }
  }

  const handleEndTimeChange = (time: Date | null) => {
    if (time === null || isValid(time)) {
      setEndTime(time)
    } else {
      setEndTime(null)
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
          onChange={handleStartDateChange}
        />
        <DatePicker
          label="End Date"
          value={endDate}
          onChange={handleEndDateChange}
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
      <Box width="50%">
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
