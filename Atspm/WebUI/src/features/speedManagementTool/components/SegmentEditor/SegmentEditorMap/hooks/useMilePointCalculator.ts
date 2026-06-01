import { getPolylineCoordinates } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/utils/geometry'
import * as turf from '@turf/turf'

interface RouteFeature {
  geometry: {
    type: 'LineString'
    coordinates: [number, number][]
  }
  properties: {
    ROUTE_DIRECTION: 'P' | 'N'
    BEG_MILEAGE: number
    END_MILEAGE: number
  }
}

export const useMilePointCalculator = () => {
  const calculateMilePoint = (
    feature: RouteFeature,
    lng: number,
    lat: number
  ) => {
    const geometry = feature.geometry

    const polylineCoordinates = getPolylineCoordinates(geometry)

    const routeDirection = feature.properties.ROUTE_DIRECTION
    const begMileage = feature.properties.BEG_MILEAGE
    const endMileage = feature.properties.END_MILEAGE

    const line = turf.lineString(polylineCoordinates)
    const click = turf.point([lng, lat])
    const nearest = turf.nearestPointOnLine(line, click)

    const startPt = turf.point(polylineCoordinates[0])
    const sliced = turf.lineSlice(startPt, nearest, line)

    const distAlong = turf.length(sliced, { units: 'miles' })
    const total = turf.length(line, { units: 'miles' })
    const frac = total > 0 ? distAlong / total : 0

    const point =
      routeDirection === 'P'
        ? begMileage + frac * (endMileage - begMileage)
        : endMileage - frac * (endMileage - begMileage)

    return point
  }

  return { calculateMilePoint }
}
