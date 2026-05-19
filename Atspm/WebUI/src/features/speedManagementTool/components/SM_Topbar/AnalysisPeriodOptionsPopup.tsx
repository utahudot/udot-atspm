import { TimePeriodFilter } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { createUtcTime } from '@/utils/dateTime'
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
  const [analysisPeriod, setAnalysisPeriod] = useState<TimePeriodFilter>(
    TimePeriodFilter.AllDay
  )

  const { setRouteSpeedRequest, routeSpeedRequest } = useStore()

  const handleAnalysisPeriodChange = (newValue: string) => {
    setAnalysisPeriod(
      TimePeriodFilter[newValue as keyof typeof TimePeriodFilter]
    )

    setRouteSpeedRequest({
      ...routeSpeedRequest,
      timePeriod: TimePeriodFilter[newValue as keyof typeof TimePeriodFilter],
    })
  }

  const getAnalysisPeriodLabel = () => {
    switch (analysisPeriod) {
      case TimePeriodFilter.AllDay:
        return 'All Day'
      case TimePeriodFilter.OffPeak:
        return 'Off-Peak (10 PM - 4 AM)'
      case TimePeriodFilter.AmPeak:
        return 'AM Peak (6 AM - 9 AM)'
      case TimePeriodFilter.PmPeak:
        return 'PM Peak (4 PM - 6 PM)'
      case TimePeriodFilter.MidDay:
        return 'Mid Day (9 AM - 4 PM)'
      case TimePeriodFilter.Evening:
        return 'Evening (6 PM - 10 PM)'
      case TimePeriodFilter.EarlyMorning:
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
            value={TimePeriodFilter[analysisPeriod]}
            onChange={(event) => {
              handleAnalysisPeriodChange(event.target.value)
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
    </OptionsPopupWrapper>
  )
}
