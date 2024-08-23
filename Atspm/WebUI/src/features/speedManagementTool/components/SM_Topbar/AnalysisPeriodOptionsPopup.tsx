import { AnalysisPeriod } from '@/features/speedManagementTool/enums'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import {
  Box,
  FormControl,
  FormControlLabel,
  Radio,
  RadioGroup,
} from '@mui/material'
import { TimePicker } from '@mui/x-date-pickers'
import { format } from 'date-fns'
import OptionsPopupWrapper from './OptionsPopupWrapper'

export default function AnalysisPeriodOptionsPopup() {
  const { routeSpeedRequest, setRouteSpeedRequest } = useStore()

  const analysisPeriodString = AnalysisPeriod[routeSpeedRequest.analysisPeriod]

  const getAnalysisPeriodLabel = () => {
    switch (routeSpeedRequest.analysisPeriod) {
      case AnalysisPeriod.AllDay:
        return 'Whole Day'
      case AnalysisPeriod.PeekPeriod:
        return 'Peak Periods'
      case AnalysisPeriod.CustomHour:
        const formattedStartTime = routeSpeedRequest.customStartTime
          ? format(routeSpeedRequest.customStartTime, 'HH:mm')
          : 'N/A'
        const formattedEndTime = routeSpeedRequest.customEndTime
          ? format(routeSpeedRequest.customEndTime, 'HH:mm')
          : 'N/A'
        return `Custom: ${formattedStartTime} - ${formattedEndTime}`
      default:
        return 'Analysis Period'
    }
  }

  return (
    <OptionsPopupWrapper
      label="analysis-period"
      getLabel={getAnalysisPeriodLabel}
    >
      <Box paddingX={'10px'}>
        <FormControl component="fieldset">
          <RadioGroup
            aria-label="Analysis Time Period"
            value={analysisPeriodString}
            onChange={(event) => {
              const newValue = event.target.value
              setRouteSpeedRequest({
                ...routeSpeedRequest,
                analysisPeriod:
                  AnalysisPeriod[newValue as keyof typeof AnalysisPeriod],
              })
            }}
          >
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
          <Box display="flex" gap={2} marginTop="10px">
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
              -
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
    </OptionsPopupWrapper>
  )
}
