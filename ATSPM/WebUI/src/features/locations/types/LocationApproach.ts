export interface LocationApproach {
  description: string
  mph: number | null
  protectedPhaseNumber: number
  isProtectedPhaseOverlap: boolean
  permissivePhaseNumber: number | null
  isPermissivePhaseOverlap: boolean
  pedestrianPhaseNumber: number | null
  isPedestrianPhaseOverlap: boolean
  pedestrianDetectors: string | null
  locationId: number
  directionTypeId: string
  id: number
}
