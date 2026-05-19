import { Feature } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/hooks/useMapClickHandler'
import * as turf from '@turf/turf'

export function getPolylineCoordinates(
  geometry: GeoJSON.LineString | GeoJSON.MultiLineString
): [number, number][] {
  return geometry.type === 'MultiLineString'
    ? ([] as [number, number][]).concat(
        ...(geometry.coordinates as [number, number][][])
      )
    : (geometry.coordinates as [number, number][])
}

export const snapToRoute = (feature: Feature, pt: [number, number]) => {
  const coords = getPolylineCoordinates(feature.geometry)

  const line = turf.lineString(coords)
  const snapped = turf.nearestPointOnLine(line, turf.point(pt))
  return snapped.geometry.coordinates as [number, number]
}
