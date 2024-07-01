import { Detector, DirectionType } from '@/features/locations/types'

export interface Approach {
  locationId: number
  directionTypeId: string
  description: string
  mph: number | null
  protectedPhaseNumber: number | null
  isProtectedPhaseOverlap: boolean
  permissivePhaseNumber: number | null
  isPermissivePhaseOverlap: boolean
  pedestrianPhaseNumber: number | null
  isPedestrianPhaseOverlap: boolean
  pedestrianDetectors: string | null
  id: number
  directionType: DirectionType
  detectors: Detector[]
}
