// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - linkPivotPcdTimeWindow.ts
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
import { TimeSpaceOptions } from '@/features/charts/timeSpaceDiagram/shared/types'
import {
  getDateFromDateStamp,
  parseTimeParts,
  toUTCDateStamp,
} from '@/utils/dateTime'

export interface LinkPivotPcdTimeWindow {
  startDateTime: Date
  endDateTime: Date
  startTime: Date
  endTime: Date
}

const createUtcDateFromDay = (value: Date | string): Date => {
  return getDateFromDateStamp(toUTCDateStamp(value))
}

const createTimeOnlyDate = (value: string): Date => {
  const parsedTime = parseTimeParts(value)

  if (!parsedTime) {
    return new Date(new Date().setHours(0, 0, 0, 0))
  }

  const date = new Date()
  date.setHours(
    parsedTime.hours,
    parsedTime.minutes,
    parsedTime.seconds,
    parsedTime.milliseconds
  )

  return date
}

export const getLinkPivotPcdTimeWindowFromTimeSpaceOptions = (
  options: TimeSpaceOptions
): LinkPivotPcdTimeWindow => {
  if ('start' in options && 'end' in options) {
    return {
      startDateTime: createUtcDateFromDay(options.start),
      endDateTime: createUtcDateFromDay(options.end),
      startTime: new Date(options.start),
      endTime: new Date(options.end),
    }
  }

  return {
    startDateTime: createUtcDateFromDay(options.startDate),
    endDateTime: createUtcDateFromDay(options.endDate),
    startTime: createTimeOnlyDate(options.startTime),
    endTime: createTimeOnlyDate(options.endTime),
  }
}
