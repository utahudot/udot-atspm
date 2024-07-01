import { RouteSelect } from '@/components/RouteSelect/RouteSelect'
import { StyledPaper } from '@/components/StyledPaper'
import SelectDateTime from '@/components/selectTimeSpan'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  useTheme,
} from '@mui/material'
import { LinkPivotHandler, StreamType } from '../handlers/linkPivotHandlers'

interface Props {
  handler: LinkPivotHandler
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

export const LinkPivotOptionsComponent = (props: Props) => {
  const theme = useTheme()
  const { handler } = props

  return (
    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
      <StyledPaper
        sx={{
          padding: theme.spacing(3),
        }}
      >
        <RouteSelect
          handler={handler}
          hasLocationNames={false}
          hasLocationMap={false}
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
      </StyledPaper>
      <Box display="flex">
        <MultiSelectCheckbox
          itemList={daysOfWeekList}
          selectedItems={handler.selectedDays}
          setSelectedItems={handler.updateDaysOfWeek}
          header="Days To Include"
        />
      </Box>
      <StyledPaper
        sx={{
          padding: theme.spacing(3),
        }}
      >
        <Box
          sx={{
            display: 'flex',
            flexDirection: 'column',
            gap: 3,
          }}
        >
          <TextField
            label="Cycle Length"
            type="number"
            variant="outlined"
            fullWidth
            value={handler.cycleLength}
            onChange={(e) =>
              handler.changeCycleLength(parseInt(e.target.value))
            }
          />
          <FormControl fullWidth>
            <InputLabel htmlFor="starting-point-input">
              Starting Point
            </InputLabel>
            <Select
              value={handler.startingPoint}
              label="Starting Point"
              onChange={(e) =>
                handler.changeStartingPoint(e.target.value as StreamType)
              }
              inputProps={{ id: 'starting-point-input' }}
            >
              <MenuItem value="Upstream">Upstream</MenuItem>
              <MenuItem value="Downstream">Downstream</MenuItem>
            </Select>
          </FormControl>
          <TextField
            label="Bias"
            type="number"
            variant="outlined"
            fullWidth
            value={handler.bias}
            onChange={(e) => handler.changeBias(parseInt(e.target.value))}
          />
          <FormControl fullWidth>
            <InputLabel htmlFor="bias-direction-input">
              Bias Direction
            </InputLabel>
            <Select
              value={handler.biasDirection}
              label="Bias Direction"
              onChange={(e) =>
                handler.changeBiasDirection(e.target.value as StreamType)
              }
              inputProps={{ id: 'bias-direction-input' }}
            >
              <MenuItem value="Upstream">Upstream</MenuItem>
              <MenuItem value="Downstream">Downstream</MenuItem>
            </Select>
          </FormControl>
        </Box>
      </StyledPaper>
    </Box>
  )
}
