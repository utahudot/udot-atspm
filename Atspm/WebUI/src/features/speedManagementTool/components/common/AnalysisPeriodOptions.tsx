import { TimePeriodFilter } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import {
  Box,
  FormControl,
  FormControlLabel,
  Radio,
  RadioGroup,
} from '@mui/material'
import { AnalysisPeriodHandler } from './handlers/AnalysisPeriodHandler'

interface Props {
  handler: AnalysisPeriodHandler
}

export default function AnalysisPeriodOptions(props: Props) {
  const { handler } = props

  return (
    <Box paddingX={'10px'}>
      <FormControl component="fieldset">
        <RadioGroup
          aria-label="Analysis Time Period"
          value={
            handler.analysisPeriod
              ? TimePeriodFilter[handler.analysisPeriod]
              : TimePeriodFilter.AllDay
          }
          onChange={(_, value) => {
            handler.updateAnalysisPeriod(value)
          }}
        >
          <FormControlLabel
            value={TimePeriodFilter.AllDay}
            control={<Radio />}
            label="All Day"
            sx={{ p: '0px' }}
          />
          <FormControlLabel
            value={TimePeriodFilter.OffPeak}
            control={<Radio />}
            label="Off-Peak (10 PM - 4 AM)"
            sx={{ p: '0px' }}
          />
          <FormControlLabel
            value={TimePeriodFilter.AmPeak}
            control={<Radio />}
            label="AM Peak (6 AM - 9 AM)"
            sx={{ p: '0px' }}
          />
          <FormControlLabel
            value={TimePeriodFilter.PmPeak}
            control={<Radio />}
            label="PM Peak (4 PM - 6 PM)"
            sx={{ p: '0px' }}
          />
          <FormControlLabel
            value={TimePeriodFilter.MidDay}
            control={<Radio />}
            label="Mid Day (9 AM - 4 PM)"
            sx={{ p: '0px' }}
          />
          <FormControlLabel
            value={TimePeriodFilter.Evening}
            control={<Radio />}
            label="Evening (6 PM - 10 PM)"
            sx={{ p: '0px' }}
          />
          <FormControlLabel
            value={TimePeriodFilter.EarlyMorning}
            control={<Radio />}
            label="Early Morning (4 AM - 6 AM)"
            sx={{ p: '0px' }}
          />
        </RadioGroup>
      </FormControl>
    </Box>
  )
}
