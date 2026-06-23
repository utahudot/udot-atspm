import type {
  TimeSpaceBaseData,
  TimeSpaceDiagramPhaseResult,
} from '../../shared/types'
import { unwrapTimeSpaceTransformResults } from './timeSpaceTransformResult'

function buildBaseData(locationIdentifier: string): TimeSpaceBaseData {
  return {
    start: '2026-04-23T08:00:00Z',
    end: '2026-04-23T09:00:00Z',
    locationIdentifier,
    locationDescription: `${locationIdentifier} description`,
    phaseNumber: 2,
    phaseNumberSort: '2',
    distanceToNextLocation: 100,
    distanceToPreviousLocation: 0,
    speed: 35,
    approachId: 1,
    approachDescription: 'NBT Ph2',
    phaseType: 'Primary',
    calculatedDistanceToNext: 100,
    calculatedDistanceToPrevious: 0,
    isIgnoredLocation: false,
  }
}

describe('unwrapTimeSpaceTransformResults', () => {
  it('keeps failed rows that include result metadata', () => {
    const failedWithMetadata = buildBaseData('5112')
    const successful = buildBaseData('5113')
    const wrappedData: TimeSpaceDiagramPhaseResult<TimeSpaceBaseData>[] = [
      {
        error: 'No controller event logs found',
        isSuccess: false,
        result: failedWithMetadata,
      },
      {
        error: 'Configuration failed',
        isSuccess: false,
        result: null,
      },
      {
        error: null,
        isSuccess: true,
        result: successful,
      },
    ]

    const { errorMessages, successfulData } =
      unwrapTimeSpaceTransformResults(wrappedData)

    expect(errorMessages).toEqual([
      'No controller event logs found',
      'Configuration failed',
    ])
    expect(successfulData).toEqual([failedWithMetadata, successful])
  })
})
