import { ChartType, ToolType } from '@/features/charts/common/types'
import {
  Alert,
  Box,
  Button,
  Divider,
  Skeleton,
  Typography,
} from '@mui/material'
import {
  DateCalendar,
  DateOrTimeView,
  DateTimePicker,
  TimePicker,
} from '@mui/x-date-pickers'
import {
  add,
  differenceInHours,
  differenceInMinutes,
  startOfToday,
} from 'date-fns'
import { useEffect, useState } from 'react'

export interface SelectDateTimeProps {
  startDateTime: Date | null
  endDateTime: Date | null
  changeStartDate(date: Date): void
  changeEndDate(date: Date): void
  views?: DateOrTimeView[]
  chartType?: ChartType | ToolType
  dateFormat?: string
  noCalendar?: boolean
  calendarLocation?: 'bottom' | 'right'
  timePeriod?: boolean
  startTimePeriod?: Date
  endTimePeriod?: Date
  changeStartTimePeriod?(date: Date): void
  changeEndTimePeriod?(date: Date): void
}

export default function SelectDateTime({
  startDateTime,
  endDateTime,
  changeStartDate,
  changeEndDate,
  views,
  chartType,
  dateFormat = 'MMM dd, yyyy @ HH:mm',
  noCalendar,
  calendarLocation = 'bottom',
  timePeriod,
  startTimePeriod,
  endTimePeriod,
  changeStartTimePeriod,
  changeEndTimePeriod,
}: SelectDateTimeProps) {
  const [showWarning, setShowWarning] = useState(false)
  const [showWarningForTimeSpace, setShowWarningForTimeSpace] = useState(false)
  // Todo: further investigate why calendar is loading wrong date. Until then, forcing it to wait a second seems to fix it.
  const [showCalendar, setShowCalendar] = useState(false)

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowCalendar(true)
    }, 0)

    return () => clearTimeout(timer)
  }, [])

  useEffect(() => {
    if (
      chartType === ChartType.TimingAndActuation &&
      startDateTime &&
      endDateTime
    ) {
      const diffHours = differenceInHours(endDateTime, startDateTime)
      setShowWarning(diffHours > 2)
    } else {
      setShowWarning(false)
    }
  }, [startDateTime, endDateTime, chartType])

  useEffect(() => {
    if (
      chartType === ToolType.TimeSpaceHistoric &&
      startDateTime &&
      endDateTime
    ) {
      const diffMinutes = differenceInMinutes(endDateTime, startDateTime)
      setShowWarningForTimeSpace(diffMinutes > 20)
    } else {
      setShowWarningForTimeSpace(false)
    }
  }, [startDateTime, endDateTime, chartType])

  const handleCalendarChange = (newDate: Date | null) => {
    if (!newDate) return

    changeStartDate(newDate)
    changeEndDate(add(newDate, { days: 1 }))
  }

  const handleStartDateTimeChange = (date: Date | null) => {
    if (!date) return
    changeStartDate(date)
  }

  const handleEndDateTimeChange = (date: Date | null) => {
    if (!date) return
    changeEndDate(date)
  }

  const handleResetDate = () => {
    const newStart = startOfToday()
    const newEnd = startOfToday()

    changeStartDate(newStart)
    changeEndDate(newEnd)

    if (changeStartTimePeriod) {
      changeStartTimePeriod(new Date(new Date().setHours(0, 0, 0, 0)))
    }

    if (changeEndTimePeriod) {
      changeEndTimePeriod(new Date(new Date().setHours(23, 59, 0, 0)))
    }
  }

  const displayCalendarContainer = () => {
    return showCalendar ? (
      <DateCalendar
        value={startDateTime}
        onChange={handleCalendarChange}
        showDaysOutsideCurrentMonth={true}
        disableFuture={true}
      />
    ) : (
      <Skeleton width={320} height={334} />
    )
  }

  const handleSameDay = () => {
    if (!startDateTime || !endDateTime) return
    const newEndDate = new Date(startDateTime)
    newEndDate.setHours(startDateTime.getHours())
    newEndDate.setMinutes(endDateTime.getMinutes())
    newEndDate.setSeconds(endDateTime.getSeconds())
    newEndDate.setMilliseconds(endDateTime.getMilliseconds())
    changeEndDate(newEndDate)
  }

  const sideBySideStyleOuterBoxStyle = {
    display: 'flex',
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: 2,
  }

  const sideBySideStyleInnerBoxStyle = {
    display: 'flex',
    flexDirection: 'column',
    flex: 1,
    gap: 2,
    mt: 1,
  }

  return (
    <>
      <Box
        sx={calendarLocation === 'right' ? sideBySideStyleOuterBoxStyle : {}}
      >
        <Box
          sx={
            calendarLocation === 'right'
              ? sideBySideStyleInnerBoxStyle
              : {
                  display: 'flex',
                  flexDirection: 'column',
                }
          }
        >
          <DateTimePicker
            sx={{ width: '100%' }}
            value={startDateTime}
            onChange={handleStartDateTimeChange}
            views={views !== undefined ? views : undefined}
            label="Start"
            format={dateFormat}
            ampm={false}
            disableFuture={true}
            minutesStep={1}
          />
          <DateTimePicker
            sx={{ width: '100%', mt: calendarLocation === 'right' ? 0 : 3 }}
            value={endDateTime}
            onChange={handleEndDateTimeChange}
            views={views !== undefined ? views : undefined}
            label="End"
            format={dateFormat}
            ampm={false}
          />
          {timePeriod && (
            <>
              <Divider sx={{ mb: 2, margin: '15px' }}>
                <Typography sx={{ fontSize: '13px' }} variant="caption">
                  Time Period
                </Typography>
              </Divider>
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TimePicker
                  label="Start Time"
                  ampm={false}
                  closeOnSelect={true}
                  value={startTimePeriod}
                  onChange={(value: Date | null) => {
                    if (changeStartTimePeriod)
                      changeStartTimePeriod(value as Date)
                  }}
                />
                <TimePicker
                  label="End Time"
                  ampm={false}
                  closeOnSelect={true}
                  value={endTimePeriod}
                  onChange={(value: Date | null) => {
                    if (changeEndTimePeriod) changeEndTimePeriod(value as Date)
                  }}
                />
              </Box>
            </>
          )}
          <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
            <Button onClick={handleSameDay}>Same Day</Button>
            <Button onClick={handleResetDate}>Reset</Button>
          </Box>
          {!noCalendar && calendarLocation === 'bottom' && (
            <Box mx={-3}>{displayCalendarContainer()}</Box>
          )}
        </Box>
        {!noCalendar && calendarLocation === 'right' && (
          <Box sx={{ mt: -2 }}>{displayCalendarContainer()}</Box>
        )}
      </Box>
      {showWarning && (
        <Alert severity="warning" sx={{ marginTop: 2 }}>
          We generally recommend a time span of 2 hours or less for this chart.
        </Alert>
      )}
      {showWarningForTimeSpace && (
        <Alert severity="warning" sx={{ marginTop: 2, maxWidth: '330px' }}>
          We generally recommend a time span of 20 minutes or less for
          Time-Space.
        </Alert>
      )}
    </>
  )
}
