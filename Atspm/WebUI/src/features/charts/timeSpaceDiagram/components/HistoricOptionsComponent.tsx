import SelectTimeSpan from '@/components/selectTimeSpan'
import TimeSpaceRouteSelect from '@/features/charts/timeSpaceDiagram/components/TimeSpaceRouteSelect'
import { Box, Paper } from '@mui/material'
import { differenceInMinutes } from 'date-fns'
import { useMemo } from 'react'
import { TSHistoricHandler } from './handlers/handlers'

interface Props {
  handler: TSHistoricHandler
}

export const HistoricOptionsComponent = ({ handler }: Props) => {
  const timeSpaceHistoricWarning = useMemo(() => {
    const diffMinutes = differenceInMinutes(
      handler.endDateTime,
      handler.startDateTime
    )
    return diffMinutes > 20
      ? 'A time span of 20 minutes or less is recommend for this diagram.'
      : null
  }, [handler.startDateTime, handler.endDateTime])

  return (
    <Box display="flex" gap={2}>
      <TimeSpaceRouteSelect handler={handler} />
      <Paper sx={{ p: 3, maxWidth: '320px' }}>
        <SelectTimeSpan
          startDateTime={handler.startDateTime}
          endDateTime={handler.endDateTime}
          changeStartDate={handler.changeStartDate}
          changeEndDate={handler.changeEndDate}
          warning={timeSpaceHistoricWarning}
        />
      </Paper>
    </Box>
  )
}

export default HistoricOptionsComponent
