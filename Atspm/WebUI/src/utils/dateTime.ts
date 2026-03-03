// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - dateTime.ts
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

/**
 * Converts a Date or date string into a timezone-free timestamp string.
 * Output format: "YYYY-MM-DDTHH:mm:ss"
 * - Strips any timezone info (Z, ±HH:mm) if input is a string.
 * - Uses local date/time parts if input is a Date.
 *
 * @param {Date|string} value The input date object or date string
 * @returns {string} A timezone-free timestamp string, or original string if invalid date
 */
export const dateToTimestamp = (value: Date | string): string => {
  const d = typeof value === 'string' ? new Date(value) : value
  if (!(d instanceof Date) || isNaN(d.getTime())) return value as string // return original value if not a valid date

  const pad = (n: number) => String(n).padStart(2, '0')
  const y = d.getFullYear()
  const m = pad(d.getMonth() + 1)
  const day = pad(d.getDate())
  const hh = pad(d.getHours())
  const mm = pad(d.getMinutes())
  const ss = pad(d.getSeconds())

  return `${y}-${m}-${day}T${hh}:${mm}:${ss}`
}

export const toUTCDateStamp = (date: Date | string): string => {
  if (typeof date === 'string') {
    date = new Date(date)
  }
  const year = date.getUTCFullYear()
  const month = String(date.getUTCMonth() + 1).padStart(2, '0')
  const day = String(date.getUTCDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

export const toUTCDateWithTimeStamp = (dateWithTime: Date): string => {
  const options: Intl.DateTimeFormatOptions = {
    hour12: false,
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  }

  const formattedTime = dateWithTime.toLocaleString('en-US', options)
  return formattedTime
}

export const getDateFromDateStamp = (dateStamp: string): Date => {
  return new Date(dateStamp)
}

/**
 * Parse a timestamp string into epoch milliseconds while treating timezone-less
 * inputs as UTC.
 */
export const parseUtcTimestampToMs = (value: string): number | null => {
  if (!value) return null

  const raw = value.trim()
  if (!raw) return null

  const hasTimezone = /(Z|[+-]\d{2}:?\d{2})$/i.test(raw)
  if (hasTimezone) {
    const ms = Date.parse(raw)
    return Number.isFinite(ms) ? ms : null
  }

  const isoLike = raw.includes('T') ? raw : raw.replace(' ', 'T')
  const utcCandidate = `${isoLike}Z`
  const utcMs = Date.parse(utcCandidate)
  if (Number.isFinite(utcMs)) return utcMs

  const fallbackMs = Date.parse(raw)
  return Number.isFinite(fallbackMs) ? fallbackMs : null
}

export const parseUtcDateToMidnightMs = (value: string): number | null => {
  if (!value) return null

  const raw = value.trim()
  if (!raw) return null

  const ymd = /^(\d{4})[-/](\d{1,2})[-/](\d{1,2})$/.exec(raw)
  if (ymd) {
    const year = Number(ymd[1])
    const month = Number(ymd[2])
    const day = Number(ymd[3])
    if (Number.isFinite(year) && Number.isFinite(month) && Number.isFinite(day)) {
      return Date.UTC(year, month - 1, day, 0, 0, 0, 0)
    }
  }

  const ms = Date.parse(value)
  if (!Number.isFinite(ms)) return null

  const parsed = new Date(ms)
  return Date.UTC(
    parsed.getFullYear(),
    parsed.getMonth(),
    parsed.getDate(),
    0,
    0,
    0,
    0
  )
}

export const parseTimeParts = (
  value: string
):
  | {
      hours: number
      minutes: number
      seconds: number
      milliseconds: number
    }
  | null => {
  const raw = (value ?? '').trim()
  if (!raw) return { hours: 0, minutes: 0, seconds: 0, milliseconds: 0 }

  const tokens = raw.split(':')
  if (tokens.length < 2 || tokens.length > 3) return null

  const last = tokens[tokens.length - 1]
  const secParts = last.split('.')
  const seconds = Number(secParts[0])
  const fractionalMs = Number(`0.${secParts[1] ?? '0'}`) * 1000

  if (!Number.isFinite(seconds) || !Number.isFinite(fractionalMs)) return null

  let hours = 0
  let minutes = 0

  if (tokens.length === 2) {
    minutes = Number(tokens[0])
  } else {
    hours = Number(tokens[0])
    minutes = Number(tokens[1])
  }

  if (!Number.isFinite(hours) || !Number.isFinite(minutes)) return null

  return {
    hours,
    minutes,
    seconds,
    milliseconds: Math.floor(fractionalMs),
  }
}

export const toUtcTimestampMs = (
  timestampValue: string,
  dateValue: string,
  timeValue: string
): number | null => {
  const fromTimestamp = parseUtcTimestampToMs(timestampValue)
  if (fromTimestamp != null) return fromTimestamp

  if (!dateValue) return null

  const utcDateMs = parseUtcDateToMidnightMs(dateValue)
  if (utcDateMs == null) return null

  const parsedTime = parseTimeParts(timeValue)
  if (!parsedTime) return null

  return (
    utcDateMs +
    parsedTime.hours * 60 * 60 * 1000 +
    parsedTime.minutes * 60 * 1000 +
    parsedTime.seconds * 1000 +
    parsedTime.milliseconds
  )
}
