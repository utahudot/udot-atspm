import { AnalysisPeriod } from '@/features/speedManagementTool/enums'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import {
  Box,
  FormControl,
  FormControlLabel,
  Radio,
  RadioGroup,
} from '@mui/material'
import { useState } from 'react'
import OptionsPopupWrapper from './OptionsPopupWrapper'

export default function AnalysisPeriodOptionsPopup() {
  const [analysisPeriod, setAnalysisPeriod] = useState<AnalysisPeriod | null>(
    AnalysisPeriod.AllDay
  )

  const { setRouteSpeedRequest, routeSpeedRequest } = useStore()

  const createUtcTime = (hours: number, minutes = 0, seconds = 0) => {
    const date = new Date(Date.UTC(1970, 0, 1))
    date.setUTCHours(hours)
    date.setUTCMinutes(minutes)
    date.setUTCSeconds(seconds)
    date.setUTCMilliseconds(0)
    return date
  }

  const handleAnalysisPeriodChange = (newValue: string) => {
    setAnalysisPeriod(AnalysisPeriod[newValue as keyof typeof AnalysisPeriod])

    let newStartTime: Date | undefined
    let newEndTime: Date | undefined

    switch (newValue) {
      case 'AllDay':
        newStartTime = createUtcTime(0)
        newEndTime = createUtcTime(23, 59, 59)
        break
      case 'OffPeak':
        newStartTime = createUtcTime(22)
        newEndTime = createUtcTime(4)
        break
      case 'AMPeak':
        newStartTime = createUtcTime(6)
        newEndTime = createUtcTime(9)
        break
      case 'PMPeak':
        newStartTime = createUtcTime(16)
        newEndTime = createUtcTime(18)
        break
      case 'MidDay':
        newStartTime = createUtcTime(9)
        newEndTime = createUtcTime(16)
        break
      case 'Evening':
        newStartTime = createUtcTime(18)
        newEndTime = createUtcTime(22)
        break
      case 'EarlyMorning':
        newStartTime = createUtcTime(4)
        newEndTime = createUtcTime(6)
        break
      default:
        newStartTime = createUtcTime(0)
        newEndTime = createUtcTime(23, 59, 59)
        break
    }

    setRouteSpeedRequest({
      ...routeSpeedRequest,
      startTime: newStartTime.toISOString(),
      endTime: newEndTime.toISOString(),
    })
  }

  const getAnalysisPeriodLabel = () => {
    switch (analysisPeriod) {
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
            value={analysisPeriod ? AnalysisPeriod[analysisPeriod] : 'AllDay'}
            onChange={(event) => {
              handleAnalysisPeriodChange(event.target.value)
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
