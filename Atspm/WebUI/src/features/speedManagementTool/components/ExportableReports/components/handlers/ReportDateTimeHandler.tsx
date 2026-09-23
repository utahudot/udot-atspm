import { DateTimeProps } from '@/types/TimeProps'
import { toUTCDateStamp } from '@/utils/dateTime'
import {
  format,
  isValid,
  lastDayOfMonth,
  parse,
  startOfMonth,
  subDays,
  subMonths,
} from 'date-fns'
import { useState } from 'react'

export interface ReportDateTimeHandler extends DateTimeProps {
  changeStartMonth(date: Date | null): void
  changeEndMonth(date: Date | null): void
  parsedStartMonth: Date | null
  parsedEndMonth: Date | null
}

export const useReportDateTimeHandler = () => {
  const yesterday = subDays(new Date(), 1)

  const [startDateTime, setStartDateTime] = useState(yesterday)
  const [endDateTime, setEndDateTime] = useState(new Date())

  const [startMonthDateString, setStartMonthDateString] = useState<string>(
    toUTCDateStamp(startOfMonth(subMonths(new Date(), 1)))
  )
  const [endMonthDateString, setEndMonthDateString] = useState<string>(
    toUTCDateStamp(startOfMonth(new Date()))
  )

  const parsedStartMonthDate = startDateTime
    ? parse(startMonthDateString, 'yyyy-MM-dd', new Date())
    : null
  const parsedEndMonthDate = endDateTime
    ? parse(endMonthDateString, 'yyyy-MM-dd', new Date())
    : null

  const handleStartMonth = (date: Date | null) => {
    if (date && isValid(date)) {
      const formattedDate = format(date, 'yyyy-MM-01') // Set to the first day of the month
      setStartMonthDateString(formattedDate)
    } else {
      setStartMonthDateString('')
    }
  }

  const handleEndMonth = (date: Date | null) => {
    if (date && isValid(date)) {
      const lastDay = lastDayOfMonth(date)
      const formattedDate = format(lastDay, 'yyyy-MM-dd') // Set to the first day of the month
      setEndMonthDateString(formattedDate)
    } else {
      setEndMonthDateString('')
    }
  }

  const component: ReportDateTimeHandler = {
    startDateTime,
    endDateTime,
    parsedStartMonth: parsedStartMonthDate,
    parsedEndMonth: parsedEndMonthDate,
    changeStartDate(date: Date) {
      setStartDateTime(date)
    },
    changeEndDate(date: Date) {
      setEndDateTime(date)
    },
    changeStartMonth(date: Date | null) {
      handleStartMonth(date)
    },
    changeEndMonth(date: Date | null) {
      handleEndMonth(date)
    },
  }

  return component
}
