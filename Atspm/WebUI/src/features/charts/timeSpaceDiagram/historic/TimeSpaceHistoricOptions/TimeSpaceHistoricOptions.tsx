import SelectTimeSpan from '@/components/selectTimeSpan'
import { TSHistoricHandler } from '@/features/charts/timeSpaceDiagram/historic/TimeSpaceHistoricOptions/historicTimeSpaceOptions.handler'
import TimeSpaceRouteSelect from '@/features/charts/timeSpaceDiagram/shared/components/TimeSpaceRouteSelect/TimeSpaceRouteSelect'
import {
  getDayAvailabilityCalendarRange,
  useDayAvailability,
} from '@/features/dataAvailability/useDayAvailability'
import { Box, Paper } from '@mui/material'
import { differenceInMinutes } from 'date-fns'
import { useEffect, useMemo, useState } from 'react'

interface Props {
  handler: TSHistoricHandler
}

export const HistoricOptionsComponent = ({ handler }: Props) => {
  const [calendarRange, setCalendarRange] = useState(() =>
    getDayAvailabilityCalendarRange(handler.startDateTime)
  )

  useEffect(() => {
    setCalendarRange(getDayAvailabilityCalendarRange(handler.startDateTime))
  }, [handler.startDateTime])

  const routeLocationIdentifiers = useMemo(() => {
    const selectedRoute = handler.routes.find(
      (route) => String(route.id) === handler.routeId
    )

    return (
      selectedRoute?.routeLocations
        .slice()
        .sort((a, b) => a.order - b.order)
        .map((routeLocation) => routeLocation.locationIdentifier.trim())
        .filter((locationIdentifier) => locationIdentifier.length > 0) ?? []
    )
  }, [handler.routeId, handler.routes])

  const dayAvailability = useDayAvailability(
    routeLocationIdentifiers,
    calendarRange.start,
    calendarRange.end
  )

  const timeSpaceHistoricWarning = useMemo(() => {
    const diffMinutes = differenceInMinutes(
      handler.endDateTime,
      handler.startDateTime
    )
    return diffMinutes > 20
      ? 'A time span of 20 minutes or less is recommend for this diagram.'
      : null
  }, [handler.startDateTime, handler.endDateTime])

  const handleCalendarDateChange = (date: Date | null) => {
    if (!date) return

    setCalendarRange(getDayAvailabilityCalendarRange(date))
  }

  return (
    <Box display="flex" gap={2}>
      <TimeSpaceRouteSelect handler={handler} />
      <Paper sx={{ p: 3, maxWidth: '320px' }}>
        <SelectTimeSpan
          startDateTime={handler.startDateTime}
          endDateTime={handler.endDateTime}
          changeStartDate={handler.changeStartDate}
          changeEndDate={handler.changeEndDate}
          dayAvailability={
            routeLocationIdentifiers.length ? dayAvailability : undefined
          }
          onMonthChange={handleCalendarDateChange}
          onChange={handleCalendarDateChange}
          warning={timeSpaceHistoricWarning}
        />
      </Paper>
    </Box>
  )
}

export default HistoricOptionsComponent
