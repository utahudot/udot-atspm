import { useGetDetectionType, useGetLocationType } from '@/api/config'
import type { DetectionType } from '@/features/locations/types'

// Config endpoints can return a collection or its OData envelope.
const rows = <T>(data: T[] | { value?: T[] } | undefined): T[] =>
  Array.isArray(data) ? data : (data?.value ?? [])

/** Use the same lookup data for location editors and reconciliation drafts. */
export function useLocationEditorOptions(locationTypeId?: number) {
  const { data: detectionTypes } = useGetDetectionType()
  const { data: locationTypes } = useGetLocationType()
  return {
    detectionTypes: rows(detectionTypes) as unknown as DetectionType[],
    locationTypeName: rows(locationTypes).find(
      (type) => type.id === locationTypeId
    )?.name ?? undefined,
  }
}
