import { DetectorDto } from '@/features/locations/types'
import { directionType } from './DirectionType'

interface ApproachesDto {
  direction: directionType
  description: string
  protectedPhase: number
  permissivePhase: number
  pedestrianPhase: number
  isProtectedPhaseOverlap: boolean
  isPermPhaseOverlap: boolean
  isPedPhaseOverlap: boolean
  pedDetector: string
  approachSpeed: number
  detectors?: DetectorDto[]
}
export default ApproachesDto
