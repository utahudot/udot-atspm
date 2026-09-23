// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - getSegments.ts
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
import { useGetApiV1SegmentAllSegments } from '@/api/speedManagement/aTSPMSpeedManagementApi'
import {
  AllSegmentsProperties,
  AllSegmentsPropertiesGeoJsonFeatureCollection,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { useMemo } from 'react'

export type AllSegmentsSegment = {
  id: string
  type?: string | null | undefined
  geometry: {
    type?: string
    coordinates: number[][] | null
  }
  properties?: AllSegmentsProperties | undefined
}

const transformSegmentData = (
  segments: AllSegmentsPropertiesGeoJsonFeatureCollection
): AllSegmentsSegment[] => {
  if (!segments || !segments.features) return []

  return segments.features
    .filter((feature) => feature.geometry?.type === 'LineString')
    .map((feature) => ({
      ...feature,
      geometry: {
        ...feature.geometry,
        coordinates: feature.geometry?.coordinates
          ? feature.geometry.coordinates.map((coord) => [coord[1], coord[0]])
          : null,
      },
    }))
}

export const useTransformedSegments = () => {
  const { data, ...rest } = useGetApiV1SegmentAllSegments()

  const transformedData = useMemo(() => {
    if (!data) return []
    return transformSegmentData(data)
  }, [data])

  return { data: transformedData, ...rest }
}
