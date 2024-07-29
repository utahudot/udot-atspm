import SelectDateTime from '@/components/selectTimeSpan'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  TextField,
  useTheme,
} from '@mui/material'
import SequenceAndCoordinationComponent from './SequenceAndCoordinationComponent'
import { TSAverageHandler } from './handlers/handlers'

interface Props {
  handler: TSAverageHandler
}

const daysOfWeekList: string[] = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
]

export const AverageOptionsComponent = (props: Props) => {
  const theme = useTheme()
  const { handler } = props

  return (
    <Box>
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
        <Box
          component={Paper}
          sx={{
            padding: theme.spacing(3),
            minWidth: '400px',
          }}
        >
          <FormControl fullWidth sx={{ mb: 2 }}>
            <InputLabel htmlFor="route-input">Route</InputLabel>
            <Select
              label="Route"
              variant="outlined"
              fullWidth
              sx={{ mb: 2 }}
              onChange={(e) => handler.setRouteId(e.target.value)}
              value={
                handler.routes?.find(
                  (route) => route.id === Number.parseInt(handler.routeId)
                )?.id || ''
              }
              inputProps={{ id: 'route-input' }}
            >
              {handler.routes.map(
                (route: { name: string; id: number }, index: number) => {
                  return (
                    <MenuItem key={index} value={route.id}>
                      {route.name}
                    </MenuItem>
                  )
                }
              )}
            </Select>
          </FormControl>
          <TextField
            label="Speed Limit (mph)"
            variant="outlined"
            helperText="Optional"
            fullWidth
            sx={{ mb: 2 }}
            onChange={(e) => handler.setSpeedLimit(parseInt(e.target.value))}
          />
          <SelectDateTime
            dateFormat={'MMM dd, yyyy'}
            startDateTime={handler.startDateTime}
            endDateTime={handler.endDateTime}
            views={['year', 'month', 'day']}
            changeStartDate={handler.changeStartDate}
            changeEndDate={handler.changeEndDate}
            startTimePeriod={handler.startTime}
            endTimePeriod={handler.endTime}
            changeStartTimePeriod={handler.changeStartTime}
            changeEndTimePeriod={handler.changeEndTime}
            timePeriod={true}
            noCalendar
          />
        </Box>
        <Box
          component={Paper}
          sx={{
            padding: theme.spacing(3),
            minWidth: '300px',
          }}
        >
          <SequenceAndCoordinationComponent
            locationWithSequence={handler.routeLocationWithSequence}
            locationWithCoordPhases={handler.routeLocationWithCoordPhases}
            updateLocationWithCoordPhases={
              handler.updateLocationWithCoordPhases
            }
            updateLocationWithSequence={handler.updateLocationWithSequence}
          />
        </Box>
        <Box display="flex">
          <MultiSelectCheckbox
            itemList={daysOfWeekList}
            selectedItems={handler.selectedDays}
            setSelectedItems={handler.updateDaysOfWeek}
            header="Days To Include"
          />
        </Box>
      </Box>
    </Box>
  )
}
