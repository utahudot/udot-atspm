import * as XLSX from 'xlsx'
import { dateToTimestamp, toUtcTimestampMs } from '../../../../utils/dateTime'
import { GpxPoint } from './gpxFileParser'

export type SrmEntityTrack = {
  entityId: string
  points: GpxPoint[]
}

type ParsedSrmRow = {
  entityId: string
  lat: number
  lon: number
  time: string
  timestampMs: number
}

export const parseSrmFile = async (
  file: File,
  locations: string[] = []
): Promise<SrmEntityTrack[]> => {
  const buffer = await file.arrayBuffer()
  const rows = parseDelimitedText(buffer, locations)
  if (!rows.length) return []
  rows.sort((a, b) => a.timestampMs - b.timestampMs)
  const grouped = new Map<string, ParsedSrmRow[]>()
  for (const row of rows) {
    const key = row.entityId || 'unknown'
    const arr = grouped.get(key) ?? []
    arr.push(row)
    grouped.set(key, arr)
  }
  console.log(grouped)
  const tracks: SrmEntityTrack[] = []

  for (const [entityId, entityRows] of grouped.entries()) {
    const sorted = [...entityRows].sort((a, b) => a.timestampMs - b.timestampMs)
    let totalDistance = 0
    let lastLat: number | null = null
    let lastLon: number | null = null

    const points: GpxPoint[] = []
    for (const row of sorted) {
      if (lastLat != null && lastLon != null) {
        totalDistance += haversine(lastLat, lastLon, row.lat, row.lon)
      }

      points.push({
        time: row.time,
        distance: totalDistance,
      })

      lastLat = row.lat
      lastLon = row.lon
    }

    if (points.length) {
      tracks.push({
        entityId,
        points: points.slice(0, 500),
      })
    }
  }

  return tracks.sort((a, b) =>
    a.entityId.localeCompare(b.entityId, undefined, {
      numeric: true,
      sensitivity: 'base',
    })
  )
}

function parseDelimitedText(
  data: ArrayBuffer,
  locations: string[] = []
): ParsedSrmRow[] {
  const workbook = XLSX.read(new Uint8Array(data), {
    type: 'array',
    raw: false,
  })
  const firstSheetName = workbook.SheetNames[0]
  if (!firstSheetName) return []

  const worksheet = workbook.Sheets[firstSheetName]
  if (!worksheet) return []

  const matrix = XLSX.utils.sheet_to_json<string[]>(worksheet, {
    header: 1,
    blankrows: false,
    raw: false,
    defval: '',
  })
  if (matrix.length < 2) return []

  const headers = (matrix[0] ?? []).map((h) => cleanCell(String(h)))
  const normalized = headers.map(normalizeHeader)

  const entityIdIndex = findHeaderIndex(normalized, ['ENTITYID', 'ENTITY'])
  const intersectionIdIndex = findHeaderIndex(normalized, [
    'INTERSECTIONID',
    'LOCATIONIDENTIFIER',
    'LOCATIONID',
  ])
  const latIndex = findHeaderIndex(normalized, ['LATITUDE'])
  const lonIndex = findHeaderIndex(normalized, ['LON', 'LONG'])
  const timestampIndex = findHeaderIndex(normalized, ['TIMESTAMP', 'DATETIME'])
  const dateIndex = findHeaderIndex(normalized, ['DATE'])
  const timeIndex = findHeaderIndex(normalized, ['TIME', 'TMSTP'])

  if (latIndex === -1 || lonIndex === -1) return []

  const rows: ParsedSrmRow[] = []
  const hasLocationFilter =
    intersectionIdIndex !== -1 &&
    Array.isArray(locations) &&
    locations.length > 0
  const locationLookup = new Set(
    (locations ?? []).map((location) => normalizeFilterValue(location))
  )

  for (let i = 1; i < matrix.length; i++) {
    const cols = (matrix[i] ?? []).map((v) => cleanCell(String(v)))
    const lat = Number(cols[latIndex])
    const lon = Number(cols[lonIndex])
    if (!Number.isFinite(lat) || !Number.isFinite(lon)) continue

    const rawTimestamp =
      timestampIndex !== -1 ? cleanCell(cols[timestampIndex]) : ''
    const rawDate = dateIndex !== -1 ? cleanCell(cols[dateIndex]) : ''
    const rawTime = timeIndex !== -1 ? cleanCell(cols[timeIndex]) : ''
    const intersectionId =
      intersectionIdIndex !== -1 ? cleanCell(cols[intersectionIdIndex]) : ''

    if (hasLocationFilter) {
      const normalizedIntersectionId = normalizeFilterValue(intersectionId)
      if (!locationLookup.has(normalizedIntersectionId)) continue
    }

    const timestampMs = toUtcTimestampMs(rawTimestamp, rawDate, rawTime)
    if (!Number.isFinite(timestampMs)) continue
    const time = dateToTimestamp(new Date(timestampMs))

    const entityId =
      entityIdIndex !== -1 ? cleanCell(cols[entityIdIndex]) : 'unknown'

    rows.push({ lat, lon, time, timestampMs, entityId })
  }

  return rows
}

function cleanCell(value: string | undefined): string {
  return (value ?? '').replace(/^"(.*)"$/, '$1').trim()
}

function normalizeHeader(header: string): string {
  return header.toUpperCase().replace(/[^A-Z0-9]/g, '')
}

function findHeaderIndex(headers: string[], tokens: string[]): number {
  return headers.findIndex((h) => tokens.some((t) => h.includes(t)))
}

function normalizeFilterValue(value: string): string {
  return cleanCell(value).toUpperCase()
}

const haversine = (lat1: number, lon1: number, lat2: number, lon2: number) => {
  const R = 6371000
  const toRad = (d: number) => (d * Math.PI) / 180

  const dLat = toRad(lat2 - lat1)
  const dLon = toRad(lon2 - lon1)

  const a =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLon / 2) ** 2

  return 2 * R * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
}
