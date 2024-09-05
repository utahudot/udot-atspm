import { AnalysisPeriod } from '@/features/speedManagementTool/enums'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import {
  Box,
  FormControl,
  FormControlLabel,
  Radio,
  RadioGroup,
} from '@mui/material'
import OptionsPopupWrapper from './OptionsPopupWrapper'

export default function AnalysisPeriodOptionsPopup() {
  const { routeSpeedRequest, setRouteSpeedRequest } = useStore()

  const analysisPeriodString = routeSpeedRequest.analysisPeriod
    ? AnalysisPeriod[routeSpeedRequest.analysisPeriod]
    : AnalysisPeriod.AllDay

  const getAnalysisPeriodLabel = () => {
    switch (routeSpeedRequest.analysisPeriod) {
      case AnalysisPeriod.AllDay:
        return 'All Day'
      case AnalysisPeriod.OffPeak:
        return 'Off-Peak (10 PM - 4 AM)'
      case AnalysisPeriod.AMPeak:
        return 'AM Peak (6 AM - 9 AM)'
      case AnalysisPeriod.PMPeak:
        return 'PM Peak (4 PM - 6 PM)'
      case AnalysisPeriod.MidDay:
        return 'Mid Day (9 AM - 4 PM)'
      case AnalysisPeriod.Evening:
        return 'Evening (6 PM - 10 PM)'
      case AnalysisPeriod.EarlyMorning:
        return 'Early Morning (4 AM - 6 AM)'
      default:
        return 'Analysis Period'
    }
  }

  return (
    <OptionsPopupWrapper
      label="analysis-period"
      getLabel={getAnalysisPeriodLabel}
      width="250px"
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
              label="All Day"
              sx={{ p: '0px' }}
            />
            <FormControlLabel
              value="OffPeak"
              control={<Radio />}
              label="Off-Peak (10 PM - 4 AM)"
              sx={{ p: '0px' }}
            />
            <FormControlLabel
              value="AMPeak"
              control={<Radio />}
              label="AM Peak (6 AM - 9 AM)"
              sx={{ p: '0px' }}
            />
            <FormControlLabel
              value="PMPeak"
              control={<Radio />}
              label="PM Peak (4 PM - 6 PM)"
              sx={{ p: '0px' }}
            />
            <FormControlLabel
              value="MidDay"
              control={<Radio />}
              label="Mid Day (9 AM - 4 PM)"
              sx={{ p: '0px' }}
            />
            <FormControlLabel
              value="Evening"
              control={<Radio />}
              label="Evening (6 PM - 10 PM)"
              sx={{ p: '0px' }}
            />
            <FormControlLabel
              value="EarlyMorning"
              control={<Radio />}
              label="Early Morning (4 AM - 6 AM)"
              sx={{ p: '0px' }}
            />
          </RadioGroup>
        </FormControl>
      </Box>
    </OptionsPopupWrapper>
  )
}
