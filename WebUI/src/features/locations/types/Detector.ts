import { Approach, DetectionType } from '@/features/locations/types'

export interface Detector {
  dectectorIdentifier: string
  detectorChannel: number
  distanceFromStopBar?: number | null
  minSpeedFilter?: number | null
  dateAdded: string
  dateDisabled: string | null
  laneNumber: number | null
  movementTypeId: string
  laneTypeId: string
  decisionPoint?: number | null
  movementDelay?: number | null
  approachId: number
  detectionHardwareId: string
  latencyCorrection: number | null
  id: number
  detectionTypes: DetectionType[]
  detectionHardware: string
  hardwareType: string
  movementType: number
  detectorComments: string[]
  laneType: string
  approach?: Approach
  isNew?: boolean
}
