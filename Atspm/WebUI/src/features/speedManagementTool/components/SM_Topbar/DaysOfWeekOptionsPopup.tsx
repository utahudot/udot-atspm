import useStore from '@/features/speedManagementTool/speedManagementStore'
import {
  Box,
  Checkbox,
  FormControlLabel,
  FormGroup,
  Typography,
} from '@mui/material'
import OptionsPopupWrapper from './OptionsPopupWrapper'

const daysOfWeekMapping = [
  { key: 0, label: 'Sun' },
  { key: 1, label: 'Mon' },
  { key: 2, label: 'Tues' },
  { key: 3, label: 'Wed' },
  { key: 4, label: 'Thur' },
  { key: 5, label: 'Fri' },
  { key: 6, label: 'Sat' },
]

export default function DaysOfWeekOptionsPopup() {
  const { routeSpeedRequest, setRouteSpeedRequest } = useStore()

  const isChecked = (day: number) => routeSpeedRequest.daysOfWeek.includes(day)

  const handleDayChange = (day: number, checked: boolean) => {
    const updatedDays = checked
      ? [...routeSpeedRequest.daysOfWeek, day]
      : routeSpeedRequest.daysOfWeek.filter((d) => d !== day)

    setRouteSpeedRequest({
      ...routeSpeedRequest,
      daysOfWeek: updatedDays,
    })
  }

  const handleWholeWeekChange = (checked: boolean) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      daysOfWeek: checked ? daysOfWeekMapping.map((day) => day.key) : [],
    })
  }

  const handleWorkWeekChange = (checked: boolean) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      daysOfWeek: checked ? [1, 2, 3, 4, 5] : [],
    })
  }

  const getDaysOfWeekLabel = () => {
    const days = routeSpeedRequest.daysOfWeek
    const dayLabels = days.map((day) => daysOfWeekMapping[day].label)

    if (days.length === 7) {
      return 'Whole Week'
    }

    if (
      days.length === 5 &&
      days.includes(1) &&
      days.includes(5) &&
      !days.includes(0) &&
      !days.includes(6)
    ) {
      return 'Work Week'
    }

    return dayLabels.join(', ')
  }

  return (
    <OptionsPopupWrapper
      label="days-of-week"
      getLabel={getDaysOfWeekLabel}
      buttonStyles={{ fontSize: '14px' }}
    >
      <Box padding={'10px'}>
        <FormGroup row sx={{ ml: '12px' }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={routeSpeedRequest.daysOfWeek.length === 7}
                onChange={(e) => handleWholeWeekChange(e.target.checked)}
              />
            }
            label="Whole Week"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={
                  routeSpeedRequest.daysOfWeek.length === 5 &&
                  !routeSpeedRequest.daysOfWeek.includes(0) &&
                  !routeSpeedRequest.daysOfWeek.includes(6)
                }
                onChange={(e) => handleWorkWeekChange(e.target.checked)}
              />
            }
            label="Work Week"
          />
        </FormGroup>

        <FormGroup row>
          {daysOfWeekMapping.map(({ key, label }) => (
            <FormControlLabel
              key={key}
              control={
                <Checkbox
                  checked={isChecked(key)}
                  onChange={(e) => handleDayChange(key, e.target.checked)}
                />
              }
              label={<Typography variant="caption">{label}</Typography>}
              labelPlacement="bottom"
              sx={{ m: 0 }}
            />
          ))}
        </FormGroup>
      </Box>
    </OptionsPopupWrapper>
  )
}
