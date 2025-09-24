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
