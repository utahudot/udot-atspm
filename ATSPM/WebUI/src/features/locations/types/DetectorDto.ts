import { directionType } from './DirectionType'

interface DetectorsDto {
  id: number
  detectionChannel: number
  direction: directionType
  phase: number
  permPhase: number
  overlap: boolean
  enabled: boolean
  detectionTypes: string
  detectionHardware: string
  latencyCorrection: string
  movementType: string
  laneNumber: number
  laneType: string
  distanceFromStopBar: number
  decisionPoint: string
  movementDelay: string
  minSpeedFilter: number
  comment: string
  detectorComments: Comment[]
}
interface Comment {
  comment: string
  timeStamp: string
  detectorId: number
  id: number
}

export default DetectorsDto
