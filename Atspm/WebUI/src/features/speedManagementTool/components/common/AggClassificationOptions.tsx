import { AggClassification } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { Box, FormControlLabel, Radio, RadioGroup } from '@mui/material'
import { ERMonthlyHandler } from '../ExportableReports/components/handlers/handlers'

interface Props {
  handler: ERMonthlyHandler
}

export const AggClassificationOptions = (props: Props) => {
  const { handler } = props
  return (
    <Box padding="10px">
      <RadioGroup
        value={handler.aggClassification}
        onChange={(_, value) =>
          handler.changeAggClassification(value as AggClassification)
        }
      >
        <FormControlLabel
          value={AggClassification.Total}
          control={<Radio />}
          label="Whole Week"
        />
        <FormControlLabel
          value={AggClassification.Weekday}
          control={<Radio />}
          label="Weekdays"
        />
        <FormControlLabel
          value={AggClassification.Weekend}
          control={<Radio />}
          label="Weekends"
        />
      </RadioGroup>
    </Box>
  )
}
