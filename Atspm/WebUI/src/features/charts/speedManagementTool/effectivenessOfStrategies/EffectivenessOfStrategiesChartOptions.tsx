import { EffectivenessOfStrategiesOptions } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { toUTCDateStamp } from '@/utils/dateTime'
import { Box } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers'
import { isValid, parse } from 'date-fns'
import { useEffect, useState } from 'react'

interface EffectiveNessOfStrategiesOptionsProps {
  onOptionsChange: (options: Partial<EffectivenessOfStrategiesOptions>) => void
}

const EffectivenessOfStrategiesChartOptions = ({
  onOptionsChange,
}: EffectiveNessOfStrategiesOptionsProps) => {
  const { submittedRouteSpeedRequest } = useSpeedManagementStore()

  const [treatmentDate, setTreatmentDate] = useState(
    submittedRouteSpeedRequest?.startDate
      ? parse(submittedRouteSpeedRequest.startDate, 'yyyy-MM-dd', new Date())
      : null
  )

  useEffect(() => {
    if (treatmentDate) {
      onOptionsChange({
        strategyImplementedDate: toUTCDateStamp(treatmentDate),
      })
    }
  }, [treatmentDate, onOptionsChange])

  const handletreatmentDateChange = (date: Date | null) => {
    if (date === null || isValid(date)) {
      setTreatmentDate(date)
    } else {
      setTreatmentDate(null)
    }
  }

  return (
    <Box display="flex" flexDirection="column" gap={2}>
      <Box display="flex" gap={2}>
        <DatePicker
          label="Treatment Date"
          value={treatmentDate}
          onChange={handletreatmentDateChange}
        />
      </Box>
    </Box>
  )
}

export default EffectivenessOfStrategiesChartOptions
