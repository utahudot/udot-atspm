import { DayOfWeek } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import { DataSource } from '@/features/speedManagementTool/enums'
import { Box } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers'
import { endOfMonth, isValid, startOfMonth } from 'date-fns'
import { useEffect, useState } from 'react'

export interface SpeedVariabilityOptionsValues {
  startDate: string
  endDate: string
  daysOfWeek: DayOfWeek[]
  isHolidaysFiltered?: boolean
  sourceId: number
}

interface SpeedVariabilityOptionsProps {
  onOptionsChange: (options: SpeedVariabilityOptionsValues) => void
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

const initialDate = new Date('2023-01-02')

const SpeedVariabilityOptions = ({
  onOptionsChange,
  sourceId,
}: SpeedVariabilityOptionsProps) => {
  const [startDate, setStartDate] = useState<Date | null>(
    startOfMonth(initialDate)
  )
  const [endDate, setEndDate] = useState<Date | null>(endOfMonth(initialDate))
  const [daysOfWeek, setDaysOfWeek] = useState<DayOfWeek[]>([
    0, 1, 2, 3, 4, 5, 6,
  ])
  const [selectedSource, setSelectedSource] = useState<DataSource>(sourceId)
  useEffect(() => {
    if (startDate && endDate && daysOfWeek) {
      onOptionsChange({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
        daysOfWeek,
        sourceId: selectedSource,
      })
    }
  }, [startDate, endDate, onOptionsChange, daysOfWeek, selectedSource])

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
          onChange={handleStartDateChange}
        />
        <DatePicker
          label="End Date"
          value={endDate}
          onChange={handleEndDateChange}
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

export default SpeedVariabilityOptions
