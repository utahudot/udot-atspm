import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import {
  Box,
  Checkbox,
  FormControlLabel,
  FormGroup,
  Typography,
} from '@mui/material'

const daysOfWeekMapping = [
  { key: 0, label: 'Sun' },
  { key: 1, label: 'Mon' },
  { key: 2, label: 'Tues' },
  { key: 3, label: 'Wed' },
  { key: 4, label: 'Thur' },
  { key: 5, label: 'Fri' },
  { key: 6, label: 'Sat' },
]

export const DaysOfWeekOptions = () => {
  const daysOfWeek = useStore((state) => state.routeSpeedRequest.daysOfWeek)
  const setRouteSpeedRequest = useStore((state) => state.setRouteSpeedRequest)

  const isChecked = (day) => daysOfWeek.includes(day)

  const handleDayChange = (day, checked) => {
    let updatedDays
    if (checked) {
      updatedDays = [...daysOfWeek, day]
    } else {
      updatedDays = daysOfWeek.filter((d) => d !== day)
    }
    setRouteSpeedRequest({
      ...useStore.getState().routeSpeedRequest, // This gets the current state values
      daysOfWeek: updatedDays,
    })
  }

  const handleWholeWeekChange = (checked) => {
    if (checked) {
      setRouteSpeedRequest({
        ...useStore.getState().routeSpeedRequest,
        daysOfWeek: daysOfWeekMapping.map((day) => day.key),
      })
    } else {
      setRouteSpeedRequest({
        ...useStore.getState().routeSpeedRequest,
        daysOfWeek: [],
      })
    }
  }

  const handleWorkWeekChange = (checked) => {
    if (checked) {
      setRouteSpeedRequest({
        ...useStore.getState().routeSpeedRequest,
        daysOfWeek: [1, 2, 3, 4, 5],
      })
    } else {
      setRouteSpeedRequest({
        ...useStore.getState().routeSpeedRequest,
        daysOfWeek: [],
      })
    }
  }

  return (
    <>
      <StyledComponentHeader header={'Days of the Week'} />
      <Box padding={'10px'}>
        <FormGroup row sx={{ ml: '12px' }}>
          <FormControlLabel
            control={
              <Checkbox
                checked={daysOfWeek.length === 7}
                onChange={(e) => handleWholeWeekChange(e.target.checked)}
              />
            }
            label="Whole Week"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={
                  daysOfWeek.length === 5 &&
                  !daysOfWeek.includes(0) &&
                  !daysOfWeek.includes(6)
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
    </>
  )
}
