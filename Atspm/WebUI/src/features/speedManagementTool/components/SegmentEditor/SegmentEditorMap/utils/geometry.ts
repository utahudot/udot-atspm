// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - geometry.ts
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
