import { set } from 'date-fns'

export const pad2 = (n: number) => String(n).padStart(2, '0')

export const parseBool = (v: string | null) => {
  if (v == null) return undefined
  if (v === 'true') return true
  if (v === 'false') return false
  return undefined
}

export const parseNum = (v: string | null) => {
  if (v == null || v === '') return undefined
  const n = Number(v)
  return Number.isFinite(n) ? n : undefined
}

export const parseDate = (v: string | null) => {
  if (!v) return undefined
  const d = new Date(v)
  return Number.isNaN(d.getTime()) ? undefined : d
}

export const parseNumArray = (values: string[]) =>
  values.map((v) => Number(v)).filter((n) => Number.isFinite(n))

/** Format time-of-day as HH:mm:ss from a Date (local time). */
export const formatTime = (d: Date) => {
  return `${pad2(d.getHours())}:${pad2(d.getMinutes())}:${pad2(d.getSeconds())}`
}

/**
 * Parse time-of-day "HH:mm" or "HH:mm:ss" into a Date anchored to today (local time).
 * Returns undefined if invalid.
 */
export const parseTimeOfDayToDate = (time: string | undefined | null) => {
  if (!time) return undefined
  const m = /^(\d{2}):(\d{2})(?::(\d{2}))?$/.exec(time.trim())
  if (!m) return undefined

  const hh = Number(m[1])
  const mm = Number(m[2])
  const ss = m[3] ? Number(m[3]) : 0

  if (
    !Number.isFinite(hh) ||
    !Number.isFinite(mm) ||
    !Number.isFinite(ss) ||
    hh < 0 ||
    hh > 23 ||
    mm < 0 ||
    mm > 59 ||
    ss < 0 ||
    ss > 59
  )
    return undefined

  return set(new Date(), {
    hours: hh,
    minutes: mm,
    seconds: ss,
    milliseconds: 0,
  })
}

/**
 * Parse "YYYY-MM-DD" as a Date (UTC midnight), which keeps your existing UTC formatting approach stable.
 */
export const parseYYYYMMDDToUtcDate = (s: string | undefined | null) => {
  if (!s) return undefined
  const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(s.trim())
  if (!m) return undefined

  const yyyy = Number(m[1])
  const mm = Number(m[2])
  const dd = Number(m[3])

  if (
    !Number.isFinite(yyyy) ||
    !Number.isFinite(mm) ||
    !Number.isFinite(dd) ||
    mm < 1 ||
    mm > 12 ||
    dd < 1 ||
    dd > 31
  )
    return undefined

  return new Date(Date.UTC(yyyy, mm - 1, dd, 0, 0, 0))
}

export const formatUtcDateToYYYYMMDD = (date: Date) => {
  const year = date.getUTCFullYear()
  const month = pad2(date.getUTCMonth() + 1)
  const day = pad2(date.getUTCDate())
  return `${year}-${month}-${day}`
}
