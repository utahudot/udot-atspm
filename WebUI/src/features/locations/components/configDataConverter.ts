import ApproachesDto from '@/features/locations/types/ApproachDto'
import ConfigDataDto from '@/features/locations/types/ConfigDataDto'
import DetectorsDto from '@/features/locations/types/DetectorDto'

export const convertConfigDataToTables = (data: any): ConfigDataDto => {
  const approachesInfo: any[] = createApproachesInfo(data)
  const detectorsInfo: any[] = createDetectorsInfo(data)

  return { approachesInfo, detectorsInfo }
}

const createApproachesInfo = (data: any): ApproachesDto[] => {
  return data.map((approach: any) => {
    return {
      direction: approach?.directionType?.description,
      description: approach?.description,
      protectedPhase: approach?.protectedPhaseNumber,
      permissivePhase: approach?.permissivePhaseNumber,
      pedestrianPhase: approach?.pedestrianPhaseNumber,
      isProtectedPhaseOverlap: approach?.isProtectedPhaseOverlap,
      isPermPhaseOverlap: approach?.isPermissivePhaseOverlap,
      isPedPhaseOverlap: approach?.isPedestrianPhaseOverlap,
      pedDetector: approach?.pedestrianDetectors,
      approachSpeed: approach?.mph,
    }
  })
}

const createDetectorsInfo = (data: any): DetectorsDto[] => {
  return data
    .map((approach: any) => {
      return approach.detectors.map((dectector: any) => {
        return {
          id: dectector?.dectectorIdentifier,
          detectionChannel: dectector?.detectorChannel,
          dateAdded: dectector?.dateAdded,
          direction: approach?.directionType?.description,
          phase: approach?.protectedPhaseNumber,
          permPhase: approach?.permissivePhaseNumber,
          overlap: approach?.isProtectedPhaseOverlap,
          enabled: data?.chartEnabled,
          detectionTypes: dectector?.detectionTypes[0]?.description,
          detectionHardware: dectector?.detectionHardware?.name,
          latencyCorrection: dectector?.latencyCorrection,
          movementType: dectector?.movementType?.description,
          laneNumber: dectector?.laneNumber,
          laneType: dectector?.laneType?.description,
          distanceFromStopBar: dectector?.distanceFromStopBar,
          decisionPoint: dectector?.decisionPoint,
          movementDelay: dectector?.movementDelay,
          minSpeedFilter: dectector?.minSpeedFilter,
          comment: dectector?.detectorComments[0]?.commentText,
        }
      })
    })
    .flatMap((x: DetectorsDto) => x)
}
