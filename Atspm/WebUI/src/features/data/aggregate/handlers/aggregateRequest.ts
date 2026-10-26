import { dateToTimestamp } from '@/utils/dateTime'
import { addDays, isValid, startOfDay } from 'date-fns'
import {
  AggregateFilterDirection,
  AggregateFilterMovement,
  AggregateTimeOptions,
} from '../types/aggregateApiData'
import {
  BinSize,
  TimePeriodOption,
  directionFilterOptions,
  movementFilterOptions,
} from '../types/aggregateOptionsData'

export interface AggregateTimeInputs {
  startDateTime: Date
  endDateTime: Date
  startTime: Date
  endTime: Date
  selectedDays: number[]
  binSize: number
}

const allDays = [0, 1, 2, 3, 4, 5, 6]

// The server charts a month only as whole days: with a daily time window it
// splits each month into days and most charts don't add them back up.
export const usesWholeDays = (binSize: number) => binSize === BinSize.Month

// The pickers pass null when cleared and an invalid date while half typed.
const isValidDate = (date: Date | null) => !!date && isValid(date)

const getMinuteOfDay = (time: Date) => time.getHours() * 60 + time.getMinutes()

// An End Time of 00:00 means the end of the day. The server takes hours 0 to
// 23, so the end of the day is sent as 23:59, the same value the server
// itself uses for whole days.
const getEndMinuteOfDay = (time: Date) =>
  Math.min(getMinuteOfDay(time) || 1440, 1439)

// The server includes each day that starts before End.
const hasSelectedDayInRange = (start: Date, end: Date, days: number[]) => {
  let date = startOfDay(start)
  for (let i = 0; i < 7 && date < end; i++, date = addDays(date, 1))
    if (days.includes(date.getDay())) return true
  return false
}

export const getTimeValidationMessage = ({
  startDateTime,
  endDateTime,
  startTime,
  endTime,
  selectedDays,
  binSize,
}: AggregateTimeInputs): string | null => {
  if (!isValidDate(startDateTime)) return 'Enter a valid Start date.'
  if (!isValidDate(endDateTime)) return 'Enter a valid End date.'
  if (endDateTime <= startDateTime) return 'End must be later than Start.'
  if (usesWholeDays(binSize)) return null
  if (!selectedDays.length) return 'Select at least one day.'
  if (!isValidDate(startTime)) return 'Enter a valid Start Time.'
  if (!isValidDate(endTime)) return 'Enter a valid End Time.'
  if (getEndMinuteOfDay(endTime) <= getMinuteOfDay(startTime))
    return 'End Time must be later than Start Time.'
  if (!hasSelectedDayInRange(startDateTime, endDateTime, selectedDays))
    return 'None of the selected Days fall in the date range.'
  return null
}

export const createTimeOptions = ({
  startDateTime,
  endDateTime,
  startTime,
  endTime,
  selectedDays,
  binSize,
}: AggregateTimeInputs): AggregateTimeOptions => {
  const range = {
    start: dateToTimestamp(startDateTime),
    end: dateToTimestamp(endDateTime),
    selectedBinSize: binSize,
  }
  if (usesWholeDays(binSize))
    return {
      ...range,
      timeOfDayStartHour: 0,
      timeOfDayStartMinute: 0,
      timeOfDayEndHour: 23,
      timeOfDayEndMinute: 59,
      daysOfWeek: allDays,
      timeOption: TimePeriodOption.StartToEnd,
    }
  const endMinute = getEndMinuteOfDay(endTime)
  return {
    ...range,
    timeOfDayStartHour: startTime.getHours(),
    timeOfDayStartMinute: startTime.getMinutes(),
    timeOfDayEndHour: Math.floor(endMinute / 60),
    timeOfDayEndMinute: endMinute % 60,
    daysOfWeek: selectedDays,
    timeOption: TimePeriodOption.TimePeriod,
  }
}

// Every checkbox is sent, checked as included and unchecked as excluded:
// the server removes approaches and detectors only for excluded entries,
// and the Direction X-Axis shows only included directions.
export const createFilterDirections = (
  selectedPositions: number[]
): AggregateFilterDirection[] =>
  directionFilterOptions.map((direction, index) => ({
    directionTypeId: direction.id,
    description: direction.label,
    include: selectedPositions.includes(index),
  }))

export const createFilterMovements = (
  selectedPositions: number[]
): AggregateFilterMovement[] =>
  movementFilterOptions.map((movement, index) => ({
    movementTypeId: movement.id,
    description: movement.label,
    include: selectedPositions.includes(index),
  }))
