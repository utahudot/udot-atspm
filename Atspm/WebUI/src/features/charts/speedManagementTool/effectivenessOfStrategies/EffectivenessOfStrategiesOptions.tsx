import { DayOfWeek } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import { DataSource } from '@/features/speedManagementTool/enums'
import { Box } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers'
import { endOfMonth, isValid, startOfMonth } from 'date-fns'
import { useEffect, useState } from 'react'
// import { ImpactType } from '@/features/speedManagementTool/types/impact'

export interface EffectivenessOfStrategiesOptionsValues {
  treatmentDate: string
//   treatmentType: ImpactType[]
}

interface EffectiveNessOfStrategiesOptionsProps {
  onOptionsChange: (options: EffectivenessOfStrategiesOptionsValues) => void
}

const initialDate = new Date('2023-03-01');

const EffectivenessOfStrategiesOptions = ({
  onOptionsChange,
}: EffectiveNessOfStrategiesOptionsProps) => {
  const [treatmentDate, setTreatmentDate] = useState<Date | null>(
    startOfMonth(initialDate)
  )
//   const [treatmentType, settreatmentType] = useState<ImpactType | null>(null)

  useEffect(() => {
    if (treatmentDate) {
      onOptionsChange({
        strategyImplementedDate: treatmentDate.toISOString().split('T')[0],
        // treatmentType,

      })
    }
  }, [treatmentDate, onOptionsChange]) // add treatmentType back if its needed as an input

  const handletreatmentDateChange = (date: Date | null) => {
    if (date === null || isValid(date)) {
      setTreatmentDate(date)
    } else {
      setTreatmentDate(null)
    }
  }

//   const handleTreatmeantTypeChange = (date: Date | null) => {
//   }

  return (
    <Box display="flex" flexDirection="column" gap={2}>
      <Box display="flex" gap={2}>
        <DatePicker
          label="Treatment Date"
          value={treatmentDate}
          onChange={handletreatmentDateChange}
        />
      </Box>
      {/* <Box width="50%">
        <MultiSelectCheckbox
          itemList={daysOfWeekList}
          selectedItems={daysOfWeek}
          setSelectedItems={handleDaysOfWeekChange}
          header="Days To Include"
          direction="horizontal"
        />
      </Box> */}
    </Box>
  )
}

export default EffectivenessOfStrategiesOptions
