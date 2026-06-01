import { AggClassification } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { Box, FormControlLabel, Radio, RadioGroup } from '@mui/material'
import OptionsPopupWrapper from './OptionsPopupWrapper'

export default function DaysOfWeekOptionsPopup() {
  const { routeSpeedRequest, setRouteSpeedRequest } = useStore()

  const getSelectedOption = () => {
    if (routeSpeedRequest.aggClassification === 'Total') return 'wholeWeek'
    if (routeSpeedRequest.aggClassification === 'Weekday') return 'weekdays'
    if (routeSpeedRequest.aggClassification === 'Weekend') return 'weekends'
  }

  const handleOptionChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = event.target.value
    let aggClassification

    if (value === 'wholeWeek') {
      aggClassification = AggClassification.Total
    } else if (value === 'weekdays') {
      aggClassification = AggClassification.Weekday
    } else if (value === 'weekends') {
      aggClassification = AggClassification.Weekend
    }

    setRouteSpeedRequest({
      ...routeSpeedRequest,
      aggClassification,
    })
  }

  const getDaysOfWeekLabel = () => {
    const option = getSelectedOption()
    if (option === 'wholeWeek') return 'Whole Week'
    if (option === 'weekdays') return 'Weekdays'
    if (option === 'weekends') return 'Weekends'
    return 'Select Days'
  }

  return (
    <OptionsPopupWrapper
      label="days-of-week"
      getLabel={getDaysOfWeekLabel}
      buttonStyles={{ fontSize: '14px' }}
    >
      <Box padding="10px">
        <RadioGroup value={getSelectedOption()} onChange={handleOptionChange}>
          <FormControlLabel
            value="wholeWeek"
            control={<Radio />}
            label="Whole Week"
          />
          <FormControlLabel
            value="weekdays"
            control={<Radio />}
            label="Weekdays"
          />
          <FormControlLabel
            value="weekends"
            control={<Radio />}
            label="Weekends"
          />
        </RadioGroup>
      </Box>
    </OptionsPopupWrapper>
  )
}
