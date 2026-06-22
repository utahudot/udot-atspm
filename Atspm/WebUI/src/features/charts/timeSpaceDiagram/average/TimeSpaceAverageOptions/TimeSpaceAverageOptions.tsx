import SelectDateTime from '@/components/selectTimeSpan'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import { TSAverageHandler } from '@/features/charts/timeSpaceDiagram/average/TimeSpaceAverageOptions/timeSpaceAverageOptions.handler'
import { Box, Paper } from '@mui/material'
import TimeSpaceAverageRouteSelect from './TimeSpaceAverageRouteSelect'

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
        <TimeSpaceAverageRouteSelect handler={handler} />
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
          <Paper>
            <MultiSelectCheckbox
              itemList={daysOfWeekList}
              selectedItems={handler.selectedDays}
              setSelectedItems={handler.updateDaysOfWeek}
              header="Days To Include"
              direction="vertical"
            />
          </Paper>
        </Box>
      </Box>
    </Box>
  )
}
