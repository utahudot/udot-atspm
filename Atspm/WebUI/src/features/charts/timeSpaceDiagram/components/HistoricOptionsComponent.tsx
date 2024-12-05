import SelectTimeSpan from '@/components/selectTimeSpan'
import TimeSpaceRouteSelect from '@/features/charts/timeSpaceDiagram/components/TimeSpaceRouteSelect'
import { Box, Paper } from '@mui/material'
import { ToolType } from '../../common/types'
import { TSHistoricHandler } from './handlers/handlers'

interface Props {
  handler: TSHistoricHandler
}

export const HistoricOptionsComponent = ({ handler }: Props) => {
  return (
    <Box display="flex" gap={2}>
      <TimeSpaceRouteSelect handler={handler} />
      <Paper sx={{ p: 3, minWidth: '380px' }}>
        <SelectTimeSpan
          startDateTime={handler.startDateTime}
          endDateTime={handler.endDateTime}
          changeStartDate={handler.changeStartDate}
          changeEndDate={handler.changeEndDate}
          chartType={ToolType.TimeSpaceHistoric}
        />
      </Paper>
    </Box>
  )
}

export default HistoricOptionsComponent
