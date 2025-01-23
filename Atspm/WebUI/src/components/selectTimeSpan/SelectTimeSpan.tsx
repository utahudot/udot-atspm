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
import { add, startOfToday } from 'date-fns'
import { useEffect, useState } from 'react'

export interface SelectDateTimeProps {
  startDateTime: Date | null
  endDateTime: Date | null
  changeStartDate(date: Date): void
  changeEndDate(date: Date): void
  views?: DateOrTimeView[]
  dateFormat?: string
  noCalendar?: boolean
  calendarLocation?: 'bottom' | 'right'
  startDateOnly?: boolean
  timePeriod?: boolean
  startTimePeriod?: Date
  endTimePeriod?: Date
  changeStartTimePeriod?(date: Date): void
  changeEndTimePeriod?(date: Date): void
  warning?: string | null
}

export default function SelectDateTime({
  startDateTime,
  endDateTime,
  changeStartDate,
  changeEndDate,
  views,
  dateFormat = 'MMM dd, yyyy @ HH:mm',
  noCalendar,
  calendarLocation = 'bottom',
  timePeriod,
  startTimePeriod,
  endTimePeriod,
  startDateOnly,
  changeStartTimePeriod,
  changeEndTimePeriod,
  warning = null,
}: SelectDateTimeProps) {
  const [showWarning, setShowWarning] = useState(false)
  const [showCalendar, setShowCalendar] = useState(false)

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowCalendar(true)
    }, 0)

    return () => clearTimeout(timer)
  }, [])

  useEffect(() => {
    setShowWarning(!!warning)
  }, [warning])

  const handleCalendarChange = (newDate: Date | null) => {
    if (!newDate) return

    changeStartDate(newDate)
    changeEndDate(add(newDate, { days: 1 }))
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
            onChange={(date) => date && changeStartDate(date)}
            views={views}
            label="Start"
            format={dateFormat}
            ampm={false}
            disableFuture
            minutesStep={1}
          />
          {!startDateOnly && (
            <DateTimePicker
              sx={{ width: '100%', mt: calendarLocation === 'right' ? 0 : 3 }}
              value={endDateTime}
              onChange={(date) => date && changeEndDate(date)}
              format={dateFormat}
              views={views}
              label="End"
              ampm={false}
            />
          )}
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
                  closeOnSelect
                  value={startTimePeriod}
                  onChange={(value) => changeStartTimePeriod?.(value as Date)}
                />
                <TimePicker
                  label="End Time"
                  ampm={false}
                  closeOnSelect
                  value={endTimePeriod}
                  onChange={(value) => changeEndTimePeriod?.(value as Date)}
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
      {showWarning && warning && <Alert severity="warning">{warning}</Alert>}
    </>
  )
}
