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
