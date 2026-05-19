import { getRouteColor } from '@/features/speedManagementTool/components/SM_Map/SM_Legend'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import type { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { ViolationColors } from '@/features/speedManagementTool/utils/colors'
import { lineString } from '@turf/helpers'
import lineOffset from '@turf/line-offset'
import L from 'leaflet'
import 'leaflet.vectorgrid'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useMap } from 'react-leaflet'

type Props = {
  routes: SpeedManagementRoute[]
  selectedRouteIds: string[]
  setSelectedRouteId: (routeId: string) => void
  setHoveredSegment: (route: SpeedManagementRoute | null) => void
}

const getPolylineWeight = (zoom: number) => {
  if (zoom >= 18) return 10
  if (zoom >= 15) return 4
  if (zoom >= 12) return 3
  if (zoom >= 10) return 1.5
  if (zoom >= 8) return 1.5
  return 1
}

function colorFromProps(
  p: any,
  opt: RouteRenderOption,
  mediumMin: number,
  mediumMax: number
) {
  if (opt === RouteRenderOption.Violations) {
    const v = p?.violations ?? null
    if (v === null) return '#000'
    if (v <= mediumMin) return ViolationColors.Low
    if (v < mediumMax) return ViolationColors.Medium
    return ViolationColors.High
  }
  const v =
    opt === RouteRenderOption.Posted_Speed
      ? (p?.Speed_Limit ?? null)
      : opt === RouteRenderOption.Percentile_85th
        ? (p?.averageEightyFifthSpeed ?? null)
        : (p?.averageSpeed ?? null)
  if (v === null) return '#000'
  return getRouteColor(v)
}

const hasDir = (s: string | undefined, dir: 'nb' | 'sb' | 'eb' | 'wb') => {
  return !!s && s.toLowerCase().includes(dir)
}

const offsetMetersForZoom = (z: number) => {
  if (z >= 14) return 10
  if (z >= 13) return 30
  if (z >= 12) return 70
  if (z >= 10) return 100
  if (z >= 8) return 0
  return 0
}

const dirSignedOffset = (name?: string, z = 12) => {
  const m = offsetMetersForZoom(z)
  if (!m) return 0
  if (hasDir(name, 'nb') || hasDir(name, 'eb')) return +m
  if (hasDir(name, 'sb') || hasDir(name, 'wb')) return -m
  return 0
}

const isNum = (n: any) => Number.isFinite(n)
const isPt = (p: any) =>
  Array.isArray(p) && p.length >= 2 && isNum(p[0]) && isNum(p[1])
const cleanLine = (line: any): number[][] =>
  Array.isArray(line) ? line.filter(isPt) : []
const cleanMulti = (multi: any): number[][][] =>
  Array.isArray(multi)
    ? multi
        .map((part: any) => cleanLine(part))
        .filter((part: number[][]) => part.length >= 2)
    : []

const isLatLng = (c: number[]) => Math.abs(c[0]) <= 90 && Math.abs(c[1]) <= 180
const swap = (c: number[]) => [c[1], c[0]]
const normalizeLine = (coords: number[][]) =>
  coords.map((c) => (isLatLng(c) ? swap(c) : c))

export default function VectorRoutesSlicerLayer({
  routes,
  selectedRouteIds,
  setSelectedRouteId,
  setHoveredSegment,
}: Props) {
  const map = useMap()
  const layerRef = useRef<L.VectorGrid>(null)
  const [zoom, setZoom] = useState(map.getZoom())

  const onSelectRef = useRef(setSelectedRouteId)
  const onHoverRef = useRef(setHoveredSegment)
  const selectedIdsRef = useRef<string[]>(selectedRouteIds)
  const prevSelectedRef = useRef<string[]>(selectedRouteIds)

  useEffect(() => {
    onSelectRef.current = setSelectedRouteId
  }, [setSelectedRouteId])

  useEffect(() => {
    onHoverRef.current = setHoveredSegment
  }, [setHoveredSegment])

  useEffect(() => {
    selectedIdsRef.current = selectedRouteIds
  }, [selectedRouteIds])

  useEffect(() => {
    const onZoom = () => setZoom(map.getZoom())
    map.on('zoomend', onZoom)
    return () => map.off('zoomend', onZoom)
  }, [map])

  const featureCollection = useMemo(() => {
    const feats = routes
      .map((f) => {
        const name = f?.properties?.name as string | undefined
        const meters = dirSignedOffset(name, zoom)
        const baseCoords =
          f.geometry.type === 'LineString'
            ? normalizeLine(f.geometry.coordinates)
            : f.geometry.type === 'MultiLineString'
              ? f.geometry.coordinates.map(normalizeLine)
              : null
        if (!baseCoords) return null
        if (meters === 0) {
          if (f.geometry.type === 'LineString') {
            const coords = cleanLine(baseCoords as number[][])
            return coords.length >= 2
              ? { ...f, geometry: { type: 'LineString', coordinates: coords } }
              : null
          } else {
            const parts = cleanMulti(baseCoords as number[][][])
            return parts.length
              ? {
                  ...f,
                  geometry: { type: 'MultiLineString', coordinates: parts },
                }
              : null
          }
        }
        if (f.geometry.type === 'LineString') {
          let coords = cleanLine(baseCoords as number[][])
          if (coords.length < 2) return null
          try {
            const off = lineOffset(lineString(coords, f.properties), meters, {
              units: 'meters',
            })
            coords = cleanLine(off.geometry.coordinates)
          } catch {
            return null
          }
          return coords.length >= 2
            ? { ...f, geometry: { type: 'LineString', coordinates: coords } }
            : null
        } else {
          let parts = cleanMulti(baseCoords as number[][][])
          if (!parts.length) return null
          try {
            parts = parts
              .map((part) => {
                const off = lineOffset(lineString(part, f.properties), meters, {
                  units: 'meters',
                })
                return cleanLine(off.geometry.coordinates)
              })
              .filter((p) => p.length >= 2)
          } catch {
            return null
          }
          return parts.length
            ? {
                ...f,
                geometry: { type: 'MultiLineString', coordinates: parts },
              }
            : null
        }
      })
      .filter(Boolean)
    return { type: 'FeatureCollection', features: feats } as const
  }, [routes, zoom])

  const { routeRenderOption, mediumMin, mediumMax } = useSpeedManagementStore()

  const styleFn = useMemo(
    () => (props: any, z: number) => ({
      color: colorFromProps(props, routeRenderOption, mediumMin, mediumMax),
      weight: getPolylineWeight(z),
      opacity: 1,
      lineCap: 'round',
    }),
    [routeRenderOption, mediumMin, mediumMax]
  )

  const applySelected = useCallback(
    (vg: L.VectorGrid, id: string) => {
      vg.setFeatureStyle(id, (pp: any, z: number) => ({
        ...styleFn(pp, z),
        color: 'blue',
        weight: getPolylineWeight(z) + 3,
      }))
    },
    [styleFn]
  )

  useEffect(() => {
    if (!featureCollection.features?.length) return
    if (layerRef.current) {
      map.removeLayer(layerRef.current)
      layerRef.current = null
    }
    const vg = L.vectorGrid.slicer(featureCollection, {
      maxZoom: 22,
      indexMaxZoom: 14,
      indexMaxPoints: 100_000,
      tolerance: 3,
      interactive: true,
      vectorTileLayerStyles: { sliced: styleFn },
      getFeatureId: (f) => f.properties.route_id,
    })
    vg.on('click', (e) => {
      const id = e.propagatedFrom?.properties?.route_id
      if (!id) return
      applySelected(vg, id)
      onSelectRef.current(id)
    })
    vg.on('mouseover', (e) => {
      const p = e.propagatedFrom?.properties
      if (!p) return
      onHoverRef.current({
        type: 'Feature',
        geometry: { type: 'LineString', coordinates: [] },
        properties: {
          route_id: p.route_id,
          name: p.name,
          speedLimit: p.speedLimit || p.Speed_Limit,
          averageSpeed: p.averageSpeed,
          averageEightyFifthSpeed: p.averageEightyFifthSpeed,
          violations: p.violations,
        },
      } as unknown as SpeedManagementRoute)
      const id = p.route_id
      if (!id) return
      if (selectedIdsRef.current.includes(id)) {
        applySelected(vg, id)
        return
      }
      const zNow = map.getZoom()
      const w =
        zNow >= 18 ? 10 : zNow >= 15 ? 5 : zNow >= 12 ? 4 : zNow >= 10 ? 3 : 2
      vg.setFeatureStyle(id, (pp: any, z: number) => ({
        ...styleFn(pp, z),
        color: 'blue',
        weight: w,
      }))
    })
    vg.on('mouseout', (e) => {
      onHoverRef.current(null)
      const id = e.propagatedFrom?.properties?.route_id
      if (!id) return
      if (selectedIdsRef.current.includes(id)) {
        applySelected(vg, id)
      } else {
        vg.resetFeatureStyle(id)
      }
    })
    vg.addTo(map)
    layerRef.current = vg
    selectedIdsRef.current.forEach((id) => applySelected(vg, id))
    return () => {
      if (layerRef.current) map.removeLayer(layerRef.current)
      layerRef.current = null
    }
  }, [map, featureCollection, styleFn, applySelected])

  useEffect(() => {
    const vg = layerRef.current
    if (!vg) return
    const prev = prevSelectedRef.current
    const next = new Set(selectedRouteIds)
    prev.forEach((id) => {
      if (!next.has(id)) vg.resetFeatureStyle(id)
    })
    selectedRouteIds.forEach((id) => applySelected(vg, id))
    prevSelectedRef.current = selectedRouteIds
  }, [selectedRouteIds, styleFn, applySelected])

  return null
}
