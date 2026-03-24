// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - useMissingDays.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
import {
  getAggregationDaysWithDataFromLocationIdentifierAndDataType,
  getEventLogDaysWithDataFromLocationIdentifierAndDataType,
} from '@/api/data'
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
  dataCategory: 'raw' | 'aggregation',
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
        let availableDaysRaw = []
        if (dataCategory === 'raw') {
          availableDaysRaw =
            await getEventLogDaysWithDataFromLocationIdentifierAndDataType(
              locationIdentifier,
              dataType,
              {
                start: dateToTimestamp(startDate),
                end: dateToTimestamp(endDate),
              }
            )
        } else {
          availableDaysRaw =
            await getAggregationDaysWithDataFromLocationIdentifierAndDataType(
              locationIdentifier,
              dataType,
              {
                start: dateToTimestamp(startDate),
                end: dateToTimestamp(endDate),
              }
            )
        }

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
  }, [locationIdentifier, dataType, startDate, endDate, dataCategory])

  return missingDays
}

export default useMissingDays
