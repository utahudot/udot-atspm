import {
  Alert,
  Badge,
  Box,
  Button,
  Divider,
  Skeleton,
  Tooltip,
  Typography,
} from '@mui/material'
import {
  DateCalendar,
  DateOrTimeView,
  DateTimePicker,
  PickersDay,
  PickersDayProps,
  TimePicker,
} from '@mui/x-date-pickers'
import { add, isSameDay, startOfToday } from 'date-fns'
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
  markDays?: Date[]
  onMonthChange?(date: Date): void
  onChange?(date: Date): void
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
  markDays = [],
  onMonthChange,
  onChange,
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
    onChange?.(newDate as Date)
    if (!newDate) return
    if (!endDateTime || !startDateTime) return

    changeStartDate(newDate)
    const newEndDate = new Date(newDate)
    newEndDate.setHours(endDateTime.getHours())
    newEndDate.setMinutes(endDateTime.getMinutes())
    if (
      startDateTime.getMonth() === endDateTime.getMonth() &&
      startDateTime.getDate() === endDateTime.getDate()
    ) {
      changeEndDate(newEndDate)
    } else {
      changeEndDate(add(newEndDate, { days: 1 }))
    }
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
        onMonthChange={onMonthChange}
        onYearChange={onMonthChange}
        showDaysOutsideCurrentMonth={true}
        disableFuture={true}
        slots={{
          day: MarkedDay,
        }}
        slotProps={{
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          day: { highlightedDays: markDays } as any,
        }}
        shouldDisableDate={(date) => {
          if (!markDays) return false
          return markDays.some((missing: Date) => isSameDay(missing, date))
        }}
      />
    ) : (
      <Skeleton width={320} height={334} />
    )
  }

  const handleSameDay = () => {
    if (!startDateTime || !endDateTime) return
    const newEndDate = new Date(startDateTime)
    newEndDate.setHours(endDateTime.getHours())
    newEndDate.setMinutes(endDateTime.getMinutes())
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
              : { display: 'flex', flexDirection: 'column' }
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
            {!startDateOnly && (
              <Button onClick={handleSameDay}>Same Day</Button>
            )}
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

interface MarkedDayProps extends PickersDayProps<Date> {
  highlightedDays?: Date[] | undefined
}

function MarkedDay(props: MarkedDayProps) {
  const { highlightedDays, day, outsideCurrentMonth, ...other } = props

  // If there are no highlighted days, render normally.
  if (highlightedDays === undefined) {
    return (
      <PickersDay
        {...other}
        outsideCurrentMonth={outsideCurrentMonth}
        day={day}
      />
    )
  }

  // Determine if the day is marked as missing.
  const isMissing = highlightedDays.some((missing: Date) =>
    isSameDay(missing, day)
  )
  const badgeContent = isMissing ? (
    <span
      style={{
        color: 'red',
        fontSize: '0.6rem',
        transform: 'translate(-50%, 50%)',
      }}
    >
      ✖
    </span>
  ) : null

  return (
    <Tooltip title={isMissing ? 'No data available' : ''} enterDelay={500}>
      <Badge overlap="circular" badgeContent={badgeContent}>
        <PickersDay
          {...other}
          outsideCurrentMonth={outsideCurrentMonth}
          day={day}
        />
      </Badge>
    </Tooltip>
  )
}
