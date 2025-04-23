import { getEventLogDaysWithEventLogsFromLocationIdentifier } from '@/api/data/aTSPMLogDataApi'
import { dateToTimestamp } from '@/utils/dateTime'
import {
  eachDayOfInterval,
  isAfter,
  isSameDay,
  parse,
  startOfToday,
} from 'date-fns'
import { useEffect, useState } from 'react'

const useMissingDays = (
  locationIdentifier: string,
  dataType: string,
  startDate: Date,
  endDate: Date
): Date[] => {
  const [missingDays, setMissingDays] = useState<Date[]>([])

  useEffect(() => {
    if (!locationIdentifier || !dataType) {
      setMissingDays([])
      return
    }

    const computeMissingDays = async () => {
      try {
        const availableDaysRaw =
          await getEventLogDaysWithEventLogsFromLocationIdentifier(
            locationIdentifier,
            {
              dataType,
              start: dateToTimestamp(startDate),
              end: dateToTimestamp(endDate),
            }
          )

        const availableDays = availableDaysRaw.map((dayStr: string) =>
          parse(dayStr, 'yyyy-MM-dd', new Date())
        )

        const allDays = eachDayOfInterval({ start: startDate, end: endDate })
        const today = startOfToday()

        const missing = allDays.filter((day) => {
          if (isAfter(day, today)) return false
          return !availableDays.some((avDay) => isSameDay(avDay, day))
        })

        setMissingDays(missing)
      } catch (error) {
        console.error('Error computing missing days in hook:', error)
        setMissingDays([])
      }
    }

    computeMissingDays()
  }, [locationIdentifier, dataType, startDate, endDate])

  return missingDays
}

export default useMissingDays
