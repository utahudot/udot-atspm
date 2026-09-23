import { ROUTE_COLORS } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/utils/colors'
import {
  Segment,
  useSegmentEditorStore,
} from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import { useRouter } from 'next/router'
import React, { useMemo } from 'react'
import { Polyline } from 'react-leaflet'

const EARTH_RADIUS_METERS = 6_371_000
const DEG_TO_RAD = Math.PI / 180
const METERS_PER_DEG_LAT = 111_320

const distanceMeters = (a: [number, number], b: [number, number]): number => {
  const order = (pair: [number, number]): [number, number] =>
    Math.abs(pair[0]) > 90 ? [pair[1], pair[0]] : pair

  const [lat1, lng1] = order(a)
  const [lat2, lng2] = order(b)

  const latRad1 = lat1 * DEG_TO_RAD
  const latRad2 = lat2 * DEG_TO_RAD
  const deltaLat = (lat2 - lat1) * DEG_TO_RAD
  const deltaLng = (lng2 - lng1) * DEG_TO_RAD

  const sinLat = Math.sin(deltaLat / 2)
  const sinLng = Math.sin(deltaLng / 2)

  const hav =
    sinLat * sinLat + Math.cos(latRad1) * Math.cos(latRad2) * sinLng * sinLng

  return (
    2 * EARTH_RADIUS_METERS * Math.atan2(Math.sqrt(hav), Math.sqrt(1 - hav))
  )
}

const padBoundingBox = (
  bb: { minLat: number; maxLat: number; minLng: number; maxLng: number },
  padMeters: number
) => {
  const padLatDeg = padMeters / METERS_PER_DEG_LAT
  const midLat = (bb.minLat + bb.maxLat) / 2
  const padLngDeg =
    padMeters / (METERS_PER_DEG_LAT * Math.cos(midLat * DEG_TO_RAD))

  return {
    minLat: bb.minLat - padLatDeg,
    maxLat: bb.maxLat + padLatDeg,
    minLng: bb.minLng - padLngDeg,
    maxLng: bb.maxLng + padLngDeg,
  }
}

const boundingBoxOfCoords = (coords: [number, number][]) => {
  let minLat = 90,
    maxLat = -90,
    minLng = 180,
    maxLng = -180

  coords.forEach(([x, y]) => {
    const [lat, lng] = Math.abs(x) > 90 ? [y, x] : [x, y]
    minLat = Math.min(minLat, lat)
    maxLat = Math.max(maxLat, lat)
    minLng = Math.min(minLng, lng)
    maxLng = Math.max(maxLng, lng)
  })

  return { minLat, maxLat, minLng, maxLng }
}

const boxesIntersect = (
  a: { minLat: number; maxLat: number; minLng: number; maxLng: number },
  b: { minLat: number; maxLat: number; minLng: number; maxLng: number }
) =>
  !(
    a.maxLat < b.minLat ||
    a.minLat > b.maxLat ||
    a.maxLng < b.minLng ||
    a.minLng > b.maxLng
  )

const PROXIMITY_METERS = 1_000

interface DisplayNearBySegmentsProps {
  setHoveredSegment: (segment: Segment | null) => void
}

const DisplayNearBySegments = ({
  setHoveredSegment,
}: DisplayNearBySegmentsProps) => {
  const { allSegments, polylineCoordinates } = useSegmentEditorStore()
  const show = useSegmentEditorStore((s) => s.legendVisibility.existing)

  const { query } = useRouter()

  const currentId = query.id as string | undefined

  const segmentsToRender = useMemo(() => {
    if (allSegments && currentId === 'new') return allSegments

    if (!allSegments || polylineCoordinates.length === 0) return []

    const userBoxPadded = padBoundingBox(
      boundingBoxOfCoords(polylineCoordinates as [number, number][]),
      PROXIMITY_METERS
    )

    return allSegments.filter((seg) => {
      if (seg.id === currentId) return false
      const coords = seg.geometry?.coordinates as [number, number][] | undefined
      if (!coords?.length) return false

      const segBox = boundingBoxOfCoords(coords)
      if (!boxesIntersect(segBox, userBoxPadded)) return false

      return coords.some((segCoord) =>
        (polylineCoordinates as [number, number][]).some(
          (polyCoord) => distanceMeters(segCoord, polyCoord) <= PROXIMITY_METERS
        )
      )
    })
  }, [allSegments, polylineCoordinates, currentId])

  if (!show) return null

  return (
    <>
      {segmentsToRender.map((segment, index) => (
        <React.Fragment key={`segment-${segment.id}-${index}`}>
          <Polyline
            key={`segment-${segment.id}-main${segment.properties.udotRouteNumber}`}
            positions={segment.geometry!.coordinates as [number, number][]}
            color={ROUTE_COLORS.Nearby.main}
            weight={3}
            lineCap="square"
            lineJoin="round"
            eventHandlers={{
              mouseover: (e) => {
                e.target.setStyle({
                  weight: 4,
                  color: ROUTE_COLORS.Nearby.hover,
                })
                if (typeof setHoveredSegment === 'function') {
                  setHoveredSegment(segment)
                }
              },
              mouseout: (e) => {
                if (typeof setHoveredSegment === 'function') {
                  e.target.setStyle({
                    weight: 3,
                    color: ROUTE_COLORS.Nearby.main,
                  })
                  setHoveredSegment(null)
                }
              },
            }}
          />
        </React.Fragment>
      ))}
    </>
  )
}

export default DisplayNearBySegments
