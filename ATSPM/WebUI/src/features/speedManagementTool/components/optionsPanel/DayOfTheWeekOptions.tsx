import Subtitle from '@/features/speedManagementTool/components/subtitle'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import {
  Box,
  Checkbox,
  FormControlLabel,
  FormGroup,
  Typography,
} from '@mui/material'

const daysOfWeekMapping = [
  { key: 'Sunday', label: 'Sun' },
  { key: 'Monday', label: 'Mon' },
  { key: 'Tuesday', label: 'Tues' },
  { key: 'Wednesday', label: 'Wed' },
  { key: 'Thursday', label: 'Thur' },
  { key: 'Friday', label: 'Fri' },
  { key: 'Saturday', label: 'Sat' },
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
        daysOfWeek: ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'],
      })
    } else {
      setRouteSpeedRequest({
        ...useStore.getState().routeSpeedRequest,
        daysOfWeek: [],
      })
    }
  }

  return (
    <Box padding={'10px'}>
      <Box>
        <Subtitle>Days of the Week</Subtitle>
      </Box>
      <Box>
        <FormGroup row>
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
                  !daysOfWeek.includes('Sunday') &&
                  !daysOfWeek.includes('Saturday')
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
    </Box>
  )
}
