import {
  getDisplayDistanceScale,
  getTimeSpaceChartHeight,
  getTimeSpacePhaseRowDistances,
} from '../math/timeSpaceLayout'
import type {
  TimeSpaceCoreRow,
  TimeSpacePhaseLayout,
} from '../types/timeSpaceCore.types'

export function buildTimeSpacePhaseLayout<T extends TimeSpaceCoreRow>(
  data: T[],
  options?: { sortByOrder?: boolean }
): TimeSpacePhaseLayout<T> {
  const byOrder = (a: T, b: T) =>
    (a.order ?? Number.MAX_SAFE_INTEGER) - (b.order ?? Number.MAX_SAFE_INTEGER)

  const primaryPhaseData = data
    .filter((location) => location.phaseType === 'Primary')
    .sort(options?.sortByOrder ? byOrder : undefined)
  const opposingPhaseData = data
    .filter((location) => location.phaseType === 'Opposing')
    .sort(options?.sortByOrder ? byOrder : undefined)

  let initialDistance = 0
  const rawDistanceData: number[] = []
  primaryPhaseData.forEach((location) => {
    rawDistanceData.push(initialDistance)
    initialDistance += location.distanceToNextLocation
  })

  const distanceScale = getDisplayDistanceScale(rawDistanceData)
  const locationCenterDistanceData = rawDistanceData.map(
    (distance) => distance * distanceScale
  )
  const { primaryDistanceData, opposingDistanceData } =
    getTimeSpacePhaseRowDistances(locationCenterDistanceData)
  const minDisplayDistance = Math.min(
    ...primaryDistanceData,
    ...opposingDistanceData
  )
  const maxDisplayDistance = Math.max(
    ...primaryDistanceData,
    ...opposingDistanceData
  )

  return {
    primaryDirection: primaryPhaseData[0].approachDescription,
    opposingDirection: opposingPhaseData[0].approachDescription,
    primaryPhaseData,
    opposingPhaseData,
    rawDistanceData,
    distanceScale,
    locationCenterDistanceData,
    primaryDistanceData,
    opposingDistanceData,
    minDisplayDistance,
    maxDisplayDistance,
    chartHeight: getTimeSpaceChartHeight(
      minDisplayDistance,
      maxDisplayDistance,
      primaryPhaseData.length
    ),
  }
}
