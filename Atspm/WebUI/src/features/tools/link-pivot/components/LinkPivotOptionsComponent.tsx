import { RouteSelect } from '@/components/RouteSelect/RouteSelect'
import { StyledPaper } from '@/components/StyledPaper'
import SelectDateTime from '@/components/selectTimeSpan'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import { useGetRouteWithExpandedLocations } from '@/features/routes/api/getRouteWithExpandedLocations'
import RouteTable from '@/features/tools/link-pivot/components/RouteTable'
import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from '@mui/material'
import { useEffect } from 'react'
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
  const { handler } = props

  const { data: routeData, refetch } = useGetRouteWithExpandedLocations({
    routeId: handler.routeId,
    includeLocationDetail: true,
  })

  const routeValuesToCheck =
    routeData?.routeLocations
      .map((location) => {
        const approach = location.approaches.find((approach) => {
          return approach.protectedPhaseNumber === location.primaryPhase
        })

        return {
          locationIdentifier: `${location.locationIdentifier} - ${location.primaryName} & ${location.secondaryName}`,
          primaryName: location.primaryName,
          secondaryName: location.secondaryName,
          approachId: approach?.description,
          order: approach?.order || 0,
        }
      })
      // sort so 1 comes before 2, etc.
      .sort((a, b) => a.order - b.order) || []

  useEffect(() => {
    if (handler.routeId) refetch()
  }, [handler.routeId, refetch])

  return (
    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
      <StyledPaper
        sx={{
          padding: 3,
          minWidth: '450.85px',
        }}
      >
        <RouteSelect
          handler={handler}
          hasLocationNames={false}
          hasLocationMap={false}
        />
        <Box
          sx={{
            maxHeight: 350,
            overflowY: 'auto',
            outline: '1px solid #ccc',
            mb: 2,
            flexGrow: 1,
          }}
        >
          <RouteTable data={routeValuesToCheck} />
        </Box>
      </StyledPaper>
      <StyledPaper
        sx={{
          padding: 3,
          maxWidth: '350px',
        }}
      >
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
          padding: 3,
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
