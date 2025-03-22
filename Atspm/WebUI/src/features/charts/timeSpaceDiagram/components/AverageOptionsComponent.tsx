import SelectDateTime from '@/components/selectTimeSpan'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import TimeSpaceRouteSelect from '@/features/charts/timeSpaceDiagram/components/TimeSpaceRouteSelect'
import { Box, Paper } from '@mui/material'
import SequenceAndCoordinationComponent from './SequenceAndCoordinationComponent'
import { TSAverageHandler } from './handlers/handlers'

interface Props {
  handler: TSAverageHandler
}

const daysOfWeekList: string[] = [
  'Sun',
  'Mon',
  'Tue',
  'Wed',
  'Thu',
  'Fri',
  'Sat',
]

export const AverageOptionsComponent = (props: Props) => {
  const { handler } = props

  return (
    <Box>
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
        <TimeSpaceRouteSelect handler={handler} />
        <Paper sx={{ p: 3, width: '380px' }}>
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
            noCalendar={true}
          />
        </Paper>
        <Box display={'flex'} flexDirection={'column'} gap={2}>
          <MultiSelectCheckbox
            itemList={daysOfWeekList}
            selectedItems={handler.selectedDays}
            setSelectedItems={handler.updateDaysOfWeek}
            header="Days To Include"
            direction="horizontal"
          />
          <SequenceAndCoordinationComponent
            locationWithSequence={handler.routeLocationWithSequence}
            locationWithCoordPhases={handler.routeLocationWithCoordPhases}
            updateLocationWithCoordPhases={
              handler.updateLocationWithCoordPhases
            }
            updateLocationWithSequence={handler.updateLocationWithSequence}
          />
        </Box>
      </Box>
    </Box>
  )
}
