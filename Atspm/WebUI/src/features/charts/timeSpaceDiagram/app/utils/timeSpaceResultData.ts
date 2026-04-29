import { ToolType } from '@/features/charts/common/types'
import { nanoid } from 'nanoid'
import type {
  GpxUploadOptions,
  RawTimeSpaceAverageData,
  RawTimeSpaceDiagramResponse,
  RawTimeSpaceHistoricData,
  TimeSpaceBaseData,
  TimeSpaceDiagramPhaseResult,
  TimeSpaceSrmPhaseOverlay,
} from '../../shared/types'

export function createEmptyTimeSpaceEntry(
  locations: string[],
  primary = false
): GpxUploadOptions {
  return {
    id: nanoid(),
    startLocation: locations[0] ?? '',
    endLocation: locations[locations.length - 1] ?? '',
    error: null,
    primary,
  }
}

export function recomputeTimeSpaceData<T extends TimeSpaceBaseData>(
  baseData: T[],
  ignoredLocations: string[]
): T[] {
  const isIgnored = (id: string) => ignoredLocations.includes(id)

  const recomputeLane = (lane: T[]): T[] => {
    if (!lane.some((location) => isIgnored(location.locationIdentifier))) {
      return lane
    }

    const recomputed: T[] = []

    for (let i = 0; i < lane.length; i++) {
      const current = lane[i]

      if (isIgnored(current.locationIdentifier)) {
        recomputed.push({
          start: current.start,
          end: current.end,
          locationIdentifier: current.locationIdentifier,
          locationDescription: current.locationDescription,
          phaseType: current.phaseType,
          distanceToNextLocation: current.distanceToNextLocation,
          distanceToPreviousLocation: current.distanceToPreviousLocation,
          phaseNumber: current.phaseNumber,
          Description: current.description,
          speed: current.speed,
          approachId: current.approachId,
          approachDescription: current.approachDescription,
          calculatedDistanceToNext: 0,
          calculatedDistanceToPrevious: 0,
          isIgnoredLocation: true,
        } as T)
        continue
      }

      let distanceToPrevious = 0
      for (let j = i - 1; j >= 0; j--) {
        distanceToPrevious += lane[j].distanceToNextLocation
        if (!isIgnored(lane[j].locationIdentifier)) break
      }

      let distanceToNext = 0
      for (let j = i; j < lane.length - 1; j++) {
        distanceToNext += lane[j].distanceToNextLocation
        if (!isIgnored(lane[j + 1].locationIdentifier)) break
      }

      recomputed.push({
        ...current,
        calculatedDistanceToPrevious: distanceToPrevious,
        calculatedDistanceToNext: distanceToNext,
        isIgnoredLocation: false,
      })
    }

    return recomputed
  }

  const primaryLane = baseData.filter((phase) => phase.phaseType === 'Primary')
  const opposingLane = baseData.filter(
    (phase) => phase.phaseType === 'Opposing'
  )

  return [...recomputeLane(primaryLane), ...recomputeLane(opposingLane)]
}

export function recomputeWrappedTimeSpaceData(
  wrappedData: RawTimeSpaceDiagramResponse['data'],
  ignoredLocations: string[]
): RawTimeSpaceDiagramResponse['data'] {
  type WrappedResult =
    | TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>
    | TimeSpaceDiagramPhaseResult<RawTimeSpaceAverageData>
  type SuccessfulWrappedResult = WrappedResult & {
    isSuccess: true
    result: RawTimeSpaceHistoricData | RawTimeSpaceAverageData
  }

  const unwrappedData = wrappedData
    .filter(
      (item): item is SuccessfulWrappedResult => item.isSuccess && !!item.result
    )
    .map((item) => item.result)

  const recomputed = recomputeTimeSpaceData(unwrappedData, ignoredLocations)

  let recomputedIndex = 0
  return wrappedData.map((item) => {
    if (!item.isSuccess || !item.result) {
      return item
    }

    const nextResult = recomputed[recomputedIndex++] ?? item.result
    return {
      error: null,
      result: nextResult,
      isSuccess: true,
    }
  }) as RawTimeSpaceDiagramResponse['data']
}

export function canFetchLinkPivotForTimeSpace(
  toolType: ToolType.TimeSpaceHistoric | ToolType.TimeSpaceAverage
) {
  return toolType === ToolType.TimeSpaceHistoric
}

export function supportsLinkPivotForTimeSpace(
  timeSpaceType: RawTimeSpaceDiagramResponse['type']
) {
  return timeSpaceType === ToolType.TimeSpaceHistoric
}

export function mergeSrmOverlaysIntoWrappedData(
  wrappedData: TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>[],
  overlays: TimeSpaceSrmPhaseOverlay[]
): TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>[] {
  const overlayMap = new Map(
    overlays.map((overlay) => [
      `${overlay.locationIdentifier}|${overlay.phaseType}|${overlay.order}`,
      overlay.srmEntityTracks ?? [],
    ])
  )

  return wrappedData.map((item) => {
    if (!item.isSuccess || !item.result) {
      return item
    }

    const key = `${item.result.locationIdentifier}|${item.result.phaseType}|${item.result.order}`

    return {
      ...item,
      result: {
        ...item.result,
        srmEntityTracks: overlayMap.get(key) ?? [],
      },
    }
  })
}

export function addDefaultTimeSpaceValues(
  timeSpaceData: RawTimeSpaceDiagramResponse
): RawTimeSpaceDiagramResponse {
  const processedData = timeSpaceData.data.map((wrappedItem) => {
    if (!wrappedItem.isSuccess || !wrappedItem.result) {
      return wrappedItem
    }

    const lane = wrappedItem.result
    return {
      ...wrappedItem,
      result: {
        ...lane,
        calculatedDistanceToNext: lane.distanceToNextLocation,
        calculatedDistanceToPrevious: lane.distanceToPreviousLocation,
        isIgnoredLocation: false,
      },
    }
  })

  return {
    type: timeSpaceData.type,
    data: processedData as RawTimeSpaceDiagramResponse['data'],
  }
}

export function getPrimaryTimeSpaceLocations(
  timeSpaceData: RawTimeSpaceDiagramResponse
) {
  return timeSpaceData.data
    .filter(
      (phase) =>
        phase.isSuccess &&
        !!phase.result &&
        phase.result.phaseType === 'Primary'
    )
    .map((phase) => phase.result.locationIdentifier)
}
