import Subtitle from '@/features/speedManagementTool/components/subtitle'
import useStore, {
  AnalysisPeriod,
} from '@/features/speedManagementTool/speedManagementStore'
import {
  Box,
  FormControl,
  FormControlLabel,
  Radio,
  RadioGroup,
} from '@mui/material'
import { TimePicker } from '@mui/x-date-pickers'

const AnalysisPeriodOptions = () => {
  const { routeSpeedRequest, setRouteSpeedRequest } = useStore()

  // Convert enum to string
  const analysisPeriodString = AnalysisPeriod[routeSpeedRequest.analysisPeriod]

  return (
    <Box padding={'10px'}>
      <Box>
        <Subtitle>Analysis Period</Subtitle>
      </Box>
      <FormControl component="fieldset">
        <RadioGroup
          aria-label="Analysis Time Period"
          value={analysisPeriodString}
          onChange={(event) => {
            const newValue = event.target.value
            setRouteSpeedRequest({
              ...routeSpeedRequest,
              analysisPeriod: AnalysisPeriod[newValue],
            })
          }}
        >
          <FormControlLabel
            value="MorningPeak"
            control={<Radio />}
            label="Morning Post Peak (10AM - 12PM)"
          />
          <FormControlLabel
            value="AllDay"
            control={<Radio />}
            label="Whole Day"
            sx={{ p: '0px' }}
          />
          <FormControlLabel
            value="PeekPeriod"
            control={<Radio />}
            label="Peak Periods (6AM - 9AM & 3PM - 6PM)"
          />
          <FormControlLabel
            value="CustomHour"
            control={<Radio />}
            label="Custom"
          />
        </RadioGroup>
      </FormControl>

      {routeSpeedRequest.analysisPeriod === AnalysisPeriod.CustomHour && (
        <Box display="flex" gap={2}>
          <Box>
            <TimePicker
              label="Start Time"
              value={routeSpeedRequest.customStartTime}
              minutesStep={5}
              onChange={(date) => {
                if (!date) return
                setRouteSpeedRequest({
                  ...routeSpeedRequest,
                  customStartTime: date,
                })
              }}
            />
          </Box>
          <Box display={'flex'} alignItems={'center'}>
            –
          </Box>
          <Box>
            <TimePicker
              label="End Time"
              value={routeSpeedRequest.customEndTime}
              minutesStep={5}
              onChange={(date) => {
                if (!date) return
                setRouteSpeedRequest({
                  ...routeSpeedRequest,
                  customEndTime: date,
                })
              }}
            />
          </Box>
        </Box>
      )}
    </Box>
  )
}

export default AnalysisPeriodOptions
