import {
  getPolylineCoordinates,
  snapToRoute,
} from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/utils/geometry'
import { useSegmentEditorStore } from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import { round } from '@/utils/math'
import * as turf from '@turf/turf'
import { Map as LeafletMap, LeafletMouseEvent } from 'leaflet'
import { useCallback, useEffect } from 'react'

interface FeatureCollection {
  type: 'FeatureCollection'
  features: Feature[]
}

export interface Feature {
  geometry: {
    coordinates: [number, number][]
  }
  properties: {
    BEG_MILEAGE: number
    END_MILEAGE: number
    ROUTE_ID: string
    ROUTE_DIRECTION: 'P' | 'N'
    ROUTE_DESC: string
    ROUTE_ALIAS_COMMON: string
  }
}

const useFetchRoute = () =>
  useCallback(
    async (pt: [number, number]): Promise<FeatureCollection | null> => {
      try {
        const res = await fetch(
          'https://roads.udot.utah.gov/server/rest/services/Public/UDOT_Routes/MapServer/0/query',
          {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({
              where: '1=1',
              geometry: JSON.stringify({
                x: pt[0],
                y: pt[1],
                spatialReference: { wkid: 4326 },
              }),
              geometryType: 'esriGeometryPoint',
              spatialRel: 'esriSpatialRelIntersects',
              distance: '25',
              units: 'esriSRUnit_Meter',
              inSR: '4326',
              outSR: '4326',
              outFields: '*',
              f: 'geojson',
            }),
          }
        )
        return (await res.json()) as FeatureCollection
      } catch {
        return null
      }
    },
    []
  )

const dist = (a: [number, number], b: [number, number]) =>
  Math.hypot(a[0] - b[0], a[1] - b[1])

export const useMapClickHandler = (
  mapRef: LeafletMap | null,
  segmentRouteId: string | null,
  setSegmentRouteId: (id: string | null) => void,
  setPendingPoint: (point: [number, number] | null) => void,
  setRouteOptions: (options: Feature[]) => void,
  setIsRouteDialogOpen: (open: boolean) => void,
  calculateMilePoint: (
    feature: Feature,
    lng: number,
    lat: number
  ) => Promise<number | null>,
  onRouteError: (pt: [number, number] | null) => void,
  canEditSegmentLines: boolean,
  isInitialPolyline?: boolean
) => {
  const {
    polylineCoordinates,
    addPolylineCoordinate,
    setPolylineCoordinates,
    segmentProperties: { startMilePoint },
    updateSegmentProperties,
    lockedRoute,
    setLockedRoute,
  } = useSegmentEditorStore()

  const fetchRoute = useFetchRoute()

  const addFreeform = useCallback(
    (pt: [number, number]) => {
      if (!polylineCoordinates.length) {
        addPolylineCoordinate(pt)
      } else {
        const first = polylineCoordinates[0]
        const last = polylineCoordinates[polylineCoordinates.length - 1]
        const prepend = dist(pt, first) < dist(pt, last)
        setPolylineCoordinates(
          prepend ? [pt, ...polylineCoordinates] : [...polylineCoordinates, pt]
        )
      }
    },
    [addPolylineCoordinate, polylineCoordinates, setPolylineCoordinates]
  )

  const handleRouteClick = useCallback(
    async (feature: Feature, clickedPoint: [number, number]) => {
      onRouteError(null)

      const coords = getPolylineCoordinates(feature.geometry)
      const line = turf.lineString(coords)
      const snapped = turf.nearestPointOnLine(line, turf.point(clickedPoint))
      const snapCoord = snapped.geometry.coordinates as [number, number]
      const isFirst = polylineCoordinates.length === 0
      const isSecondClick = polylineCoordinates.length === 1
      const routeDirection = feature.properties.ROUTE_DIRECTION

      const mile = await calculateMilePoint(feature, snapCoord[0], snapCoord[1])
      if (mile == null) return
      const roundedMile = round(mile, 2)

      const update: {
        startMilePoint?: number
        endMilePoint?: number
        polarity?: 'PM' | 'NM'
      } = {}

      if (isFirst) {
        setSegmentRouteId(feature.properties.ROUTE_ID)
        setPolylineCoordinates([snapCoord])
        update.startMilePoint = roundedMile
        update.polarity = routeDirection === 'P' ? 'PM' : 'NM'
      } else {
        if (isInitialPolyline) {
          setSegmentRouteId(feature.properties.ROUTE_ID)
        }
        const lineStart = turf.point(polylineCoordinates[0])
        const lineEnd = turf.point(
          polylineCoordinates[polylineCoordinates.length - 1]
        )

        if (routeDirection === 'P') {
          if (roundedMile > (startMilePoint ?? 0)) {
            const slice = turf.lineSlice(lineEnd, turf.point(snapCoord), line)
            const segmentCoords = slice.geometry.coordinates as [
              number,
              number,
            ][]
            setPolylineCoordinates([
              ...polylineCoordinates,
              ...segmentCoords.slice(1),
            ])
            update.endMilePoint = roundedMile
          } else if (roundedMile < (startMilePoint ?? 0)) {
            const slice = turf.lineSlice(turf.point(snapCoord), lineStart, line)
            const segmentCoords = slice.geometry.coordinates as [
              number,
              number,
            ][]
            setPolylineCoordinates([
              ...segmentCoords.slice(0, -1),
              ...polylineCoordinates,
            ])
            update.startMilePoint = roundedMile
            if (isSecondClick) update.endMilePoint = startMilePoint
          } else {
            // console.log('Point is between start and end. Middle handling TBD.')
            return
          }
        } else {
          if (roundedMile < (startMilePoint ?? 0)) {
            const slice = turf.lineSlice(lineEnd, turf.point(snapCoord), line)
            const segmentCoords = slice.geometry.coordinates as [
              number,
              number,
            ][]
            setPolylineCoordinates([
              ...polylineCoordinates,
              ...segmentCoords.slice(1),
            ])
            update.endMilePoint = roundedMile
          } else if (roundedMile > (startMilePoint ?? 0)) {
            const slice = turf.lineSlice(turf.point(snapCoord), lineStart, line)
            const segmentCoords = slice.geometry.coordinates as [
              number,
              number,
            ][]
            setPolylineCoordinates([
              ...segmentCoords.slice(0, -1),
              ...polylineCoordinates,
            ])
            update.startMilePoint = roundedMile
            if (isSecondClick) update.endMilePoint = startMilePoint
          } else {
            // console.log('Point is between start and end. Middle handling TBD.')
            return
          }
        }
      }

      updateSegmentProperties(update)
    },
    [
      calculateMilePoint,
      startMilePoint,
      polylineCoordinates,
      setPolylineCoordinates,
      setSegmentRouteId,
      updateSegmentProperties,
      onRouteError,
      isInitialPolyline,
    ]
  )

  const handleFirstClick = useCallback(
    async (pt: [number, number], features: Feature[]) => {
      // if (features.length === 1) {
      //   const feature = features[0]
      //   const snappedPt = await snapToRoute(feature, pt)
      //   setLockedRoute(feature)
      //   await handleRouteClick(feature, snappedPt)
      //   return
      // }

      if (features.length > 0) {
        setPendingPoint(pt)
        setRouteOptions(features)
        setIsRouteDialogOpen(true)
        return
      }

      setLockedRoute(null)
      addFreeform(pt)
    },
    [
      addFreeform,
      handleRouteClick,
      setIsRouteDialogOpen,
      setLockedRoute,
      setPendingPoint,
      setRouteOptions,
    ]
  )

  const handleSubsequentClick = useCallback(
    async (pt: [number, number], features: Feature[]) => {
      if (!lockedRoute) {
        addFreeform(pt)
        return
      }

      const match = features.find(
        (f) => f.properties.ROUTE_ID === lockedRoute?.properties.ROUTE_ID
      )
      if (!match) {
        console.log('No match found')
        onRouteError(pt)
        return
      }

      const snappedPt = await snapToRoute(match, pt)
      await handleRouteClick(match, snappedPt)
    },
    [addFreeform, lockedRoute, onRouteError, handleRouteClick]
  )

  const handleMapClick = useCallback(
    async (e: LeafletMouseEvent) => {
      const target = e.originalEvent.target as HTMLElement
      if (target.closest?.('.selected-point-icon')) return

      const { lat, lng } = e.latlng
      const pt: [number, number] = [lng, lat]

      let fc: FeatureCollection | null = null
      try {
        fc = await fetchRoute(pt)
      } catch {
        fc = null
      }

      const features = fc?.features ?? []

      if (polylineCoordinates.length === 0 || isInitialPolyline) {
        await handleFirstClick(pt, features)
      } else {
        await handleSubsequentClick(pt, features)
      }
    },
    [
      fetchRoute,
      polylineCoordinates,
      handleFirstClick,
      handleSubsequentClick,
      isInitialPolyline,
    ]
  )

  useEffect(() => {
    if (!mapRef) return
    const obs = new ResizeObserver(() => mapRef.invalidateSize())
    obs.observe(mapRef.getContainer())
    if (canEditSegmentLines) {
      mapRef.on('click', handleMapClick)
    }
    return () => {
      obs.disconnect()
      mapRef.off('click', handleMapClick)
    }
  }, [mapRef, handleMapClick, canEditSegmentLines])

  return { handleMapClick, handleRouteClick }
}
