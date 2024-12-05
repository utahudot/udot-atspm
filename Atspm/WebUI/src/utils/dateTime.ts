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
import { format } from 'date-fns'

export const dateToTimestamp = (date: Date) => {
  return format(date, "yyyy-MM-dd'T'HH:mm:ss")
}

export const toUTCDateStamp = (date: Date): string => {
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

export function formatTimestampToDDMMYYYY(timestamp: string | Date) {
  const date = typeof timestamp === 'string' ? new Date(timestamp) : timestamp

  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const year = date.getFullYear()

  return `${day}/${month}/${year}`
}
