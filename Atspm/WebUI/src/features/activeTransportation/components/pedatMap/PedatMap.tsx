import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import ControlsPanel from '@/features/activeTransportation/components/pedatMap/PedatMapControls'
import { getEnv } from '@/utils/getEnv'
import { Box } from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  CircleMarker,
  LayerGroup,
  MapContainer,
  TileLayer,
  Tooltip,
  useMap,
} from 'react-leaflet'

export type Aggregation = 'Average Hour' | 'Average Daily' | 'Total'

type Loc = {
  latitude: number
  longitude: number
  averageDailyVolume?: number
  averageVolumeByHourOfDay?: { index: number; volume: number }[]
  rawData?: {
    timestamp?: string
    timeStamp?: string
    pedestrianCount: number
  }[]
  names?: string
  locationIdentifier?: string
}

const ONE_DAY = 24 * 60 * 60 * 1000

const floorMidnightLocal = (t: number) => {
  const d = new Date(t)
  return +new Date(d.getFullYear(), d.getMonth(), d.getDate())
}

function computeDateBounds(data: PedatChartsContainerProps['data']) {
  let min = Infinity
  let max = -Infinity
  for (const loc of (data ?? []) as Loc[]) {
    for (const r of loc.rawData ?? []) {
      const tt = Date.parse(r.timestamp ?? r.timeStamp ?? '')
      if (Number.isFinite(tt)) {
        if (tt < min) min = tt
        if (tt > max) max = tt
      }
    }
  }
  if (!Number.isFinite(min) || !Number.isFinite(max)) {
    const today = floorMidnightLocal(Date.now())
    return { min: today, max: today }
  }
  return { min: floorMidnightLocal(min), max: floorMidnightLocal(max) }
}

function ymd(t: number) {
  const d = new Date(t)
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${d.getFullYear()}-${m}-${day}`
}

function getMetric(
  row: Loc,
  aggregation: Aggregation,
  dateRange: [number, number],
  hourRange: [number, number]
) {
  const [startMid, endMid] = dateRange
  const endOfEndDay = endMid + (ONE_DAY - 1)
  const [hStart, hEnd] = hourRange

  const samples = (row.rawData ?? [])
    .map((r) => {
      const t = Date.parse(r.timestamp ?? r.timeStamp ?? '')
      if (!Number.isFinite(t)) return null
      const h = new Date(t).getHours()
      const inDate = t >= startMid && t <= endOfEndDay
      const inHour = h >= hStart && h <= hEnd
      return inDate && inHour ? { t, h, c: r.pedestrianCount ?? 0 } : null
    })
    .filter(Boolean) as { t: number; h: number; c: number }[]

  if (!samples.length) return 0

  if (aggregation === 'Total') {
    return samples.reduce((s, r) => s + r.c, 0)
  }
  if (aggregation === 'Average Hour') {
    const sum = samples.reduce((s, r) => s + r.c, 0)
    return sum / samples.length
  }
  const byDay = new Map<string, number>()
  for (const s of samples) {
    const key = ymd(s.t)
    byDay.set(key, (byDay.get(key) ?? 0) + s.c)
  }
  const totals = Array.from(byDay.values())
  return totals.reduce((s, v) => s + v, 0) / totals.length
}

function makeRadiusScale(values: number[], rMin = 6, rMax = 30) {
  const vMin = Math.min(...values)
  const vMax = Math.max(...values)
  if (!isFinite(vMin) || !isFinite(vMax) || vMax <= vMin) return () => rMin
  const span = Math.sqrt(vMax - vMin)
  return (v: number) =>
    rMin + (Math.sqrt(Math.max(0, v - vMin)) / span) * (rMax - rMin)
}

// ---- color ramp: blue → green → yellow → orange → red ----------------------
// piecewise RGB interpolation across 5 stops
const STOPS = ['#2563eb', '#22c55e', '#eab308', '#f97316', '#dc2626'] // low→high
function hexToRgb(hex: string) {
  const m = hex.replace('#', '')
  const n = parseInt(m.length === 3 ? m.replace(/(.)/g, '$1$1') : m, 16)
  return { r: (n >> 16) & 255, g: (n >> 8) & 255, b: n & 255 }
}
function rgbToHex({ r, g, b }: { r: number; g: number; b: number }) {
  const toHex = (x: number) => x.toString(16).padStart(2, '0')
  return `#${toHex(Math.round(r))}${toHex(Math.round(g))}${toHex(Math.round(b))}`
}
function lerp(a: number, b: number, t: number) {
  return a + (b - a) * t
}
function lerpColor(c1: string, c2: string, t: number) {
  const a = hexToRgb(c1)
  const b = hexToRgb(c2)
  return rgbToHex({
    r: lerp(a.r, b.r, t),
    g: lerp(a.g, b.g, t),
    b: lerp(a.b, b.b, t),
  })
}
function colorFor(v: number, vMin: number, vMax: number) {
  const t = vMax > vMin ? (v - vMin) / (vMax - vMin) : 0
  const seg = Math.min(STOPS.length - 2, Math.floor(t * (STOPS.length - 1)))
  const localT = t * (STOPS.length - 1) - seg
  return lerpColor(STOPS[seg], STOPS[seg + 1], localT)
}

function Legend({ vMin, vMax }: { vMin: number; vMax: number }) {
  const map = useMap()
  useEffect(() => {
    const control = L.control({ position: 'topright' })
    control.onAdd = () => {
      const div = L.DomUtil.create('div', 'pedat-legend')
      div.style.background = 'rgba(255,255,255,0.9)'
      div.style.padding = '6px 8px'
      div.style.borderRadius = '6px'
      div.style.boxShadow = '0 1px 3px rgba(0,0,0,0.3)'
      div.style.font =
        '12px/1.2 system-ui, -apple-system, Segoe UI, Roboto, Helvetica, Arial'
      div.style.minWidth = '220px'
      const gradient = `linear-gradient(90deg, ${STOPS.join(',')})`
      const mid = vMin + (vMax - vMin) / 2
      div.innerHTML = `
        <div style="font-weight:600;margin-bottom:4px">Pedestrian Count</div>
        <div style="position:relative;height:12px;border-radius:4px;background:${gradient}"></div>
        <div style="display:flex;justify-content:space-between;margin-top:2px">
          <span>${vMin.toLocaleString(undefined, { maximumFractionDigits: 0 })}</span>
          <span>${mid.toLocaleString(undefined, { maximumFractionDigits: 0 })}</span>
          <span>${vMax.toLocaleString(undefined, { maximumFractionDigits: 0 })}</span>
        </div>
      `
      // prevent map drag when pointer is on legend
      L.DomEvent.disableClickPropagation(div)
      L.DomEvent.disableScrollPropagation(div)
      return div
    }
    control.addTo(map)
    return () => control.remove()
  }, [map, vMin, vMax])
  return null
}

function PedatLeafletMap({
  data,
  aggregation,
  dateRange,
  hourRange,
}: {
  data: PedatChartsContainerProps['data']
  aggregation: Aggregation
  dateRange: [number, number]
  hourRange: [number, number]
}) {
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const [mapCoords, setMapCoords] = useState({
    lat: 40.7608,
    lng: -111.891,
    zoom: 10,
  })

  useEffect(() => {
    const fetchEnv = async () => {
      const env = await getEnv()
      const lat = parseFloat(env?.MAP_DEFAULT_LATITUDE || '')
      const lng = parseFloat(env?.MAP_DEFAULT_LONGITUDE || '')
      const zoom = parseInt(env?.MAP_DEFAULT_ZOOM || '', 10)
      if (!isNaN(lat) && !isNaN(lng) && !isNaN(zoom))
        setMapCoords({ lat, lng, zoom })
    }
    fetchEnv()
  }, [])

  useEffect(() => {
    if (!mapRef) return
    mapRef.invalidateSize()
    const el = mapRef.getContainer()
    const ro = new ResizeObserver(() => mapRef.invalidateSize())
    ro.observe(el)
    return () => ro.disconnect()
  }, [mapRef])

  useEffect(() => {
    if (!mapRef || !data?.length) return
    const pts = (data as Loc[])
      .filter(
        (d) => Number.isFinite(d.latitude) && Number.isFinite(d.longitude)
      )
      .map((d) => [d.latitude, d.longitude] as [number, number])
    if (!pts.length) return
    const bounds = L.latLngBounds(pts)
    if (bounds.isValid()) mapRef.fitBounds(bounds.pad(0.2))
  }, [mapRef, data])

  const metrics = useMemo(() => {
    const rows = (data ?? []) as Loc[]
    const res = rows.map((row) =>
      getMetric(row, aggregation, dateRange, hourRange)
    )
    return res
  }, [data, aggregation, dateRange, hourRange])

  const vMin = useMemo(() => Math.min(...metrics), [metrics])
  const vMax = useMemo(() => Math.max(...metrics), [metrics])
  const radius = useMemo(() => makeRadiusScale(metrics, 6, 30), [metrics])

  // force a clean redraw when controls change
  const layerKey = useMemo(
    () =>
      `${aggregation}|${dateRange[0]}|${dateRange[1]}|${hourRange[0]}|${hourRange[1]}`,
    [aggregation, dateRange, hourRange]
  )

  return (
    <MapContainer
      center={[mapCoords.lat, mapCoords.lng]}
      zoom={mapCoords.zoom}
      scrollWheelZoom
      style={{ height: '100%', width: '100%' }}
      renderer={L.canvas({ tolerance: 5 })}
      ref={setMapRef}
      doubleClickZoom={false}
    >
      <TileLayer
        attribution='&copy; <a href="https://www.openaip.net/">openAIP Data</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-NC-SA</a>)'
        url="https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}{r}.png"
      />

      {/* color legend */}
      <Legend
        vMin={isFinite(vMin) ? vMin : 0}
        vMax={isFinite(vMax) ? vMax : 0}
      />

      {/* force remount when ranges change */}
      <LayerGroup key={layerKey}>
        {(data as Loc[]).map((row, i) => {
          if (!Number.isFinite(row.latitude) || !Number.isFinite(row.longitude))
            return null

          const v = metrics[i] ?? 0
          const r = radius(v)
          const col = colorFor(v, vMin, vMax)
          const key = row.locationIdentifier || row.names || String(i)

          return (
            <CircleMarker
              key={key}
              center={[row.latitude, row.longitude]}
              radius={r}
              pathOptions={{
                color: col,
                fillColor: col,
                fillOpacity: 0.65,
                weight: 1,
              }}
            >
              <Tooltip direction="top" opacity={0.9}>
                <div>
                  <strong>
                    {row.names || row.locationIdentifier || 'Location'}
                  </strong>
                  <div>
                    {aggregation}: {Math.round(v * 100) / 100}
                  </div>
                </div>
              </Tooltip>
            </CircleMarker>
          )
        })}
      </LayerGroup>
    </MapContainer>
  )
}

function PedatMapWithSidePanel({ data }: PedatChartsContainerProps) {
  const [aggregation, setAggregation] = useState<Aggregation>('Average Hour')
  const { min: dateMin, max: dateMax } = useMemo(
    () => computeDateBounds(data),
    [data]
  )

  // slider values = midnight timestamps
  const [dateRange, setDateRange] = useState<[number, number]>([
    dateMin,
    dateMax,
  ])
  useEffect(() => setDateRange([dateMin, dateMax]), [dateMin, dateMax])

  const [hourRange, setHourRange] = useState<[number, number]>([0, 23])

  const setDates = useCallback(
    (v: [number, number]) => setDateRange([v[0], v[1]]),
    []
  )
  const setHours = useCallback(
    (v: [number, number]) => setHourRange([v[0], v[1]]),
    []
  )

  return (
    <Box
      sx={{
        height: '100%',
        width: '100%',
        display: 'grid',
        gridTemplateColumns: 'minmax(0, 1fr) 320px',
        gap: 2,
      }}
    >
      <Box sx={{ minWidth: 0 }}>
        <PedatLeafletMap
          data={data}
          aggregation={aggregation}
          dateRange={dateRange}
          hourRange={hourRange}
        />
      </Box>

      <ControlsPanel
        aggregation={aggregation}
        setAggregation={setAggregation}
        dateMin={dateMin}
        dateMax={dateMax}
        dateRange={dateRange}
        setDateRange={setDates}
        hourRange={hourRange}
        setHourRange={setHours}
      />
    </Box>
  )
}

export default PedatMapWithSidePanel
