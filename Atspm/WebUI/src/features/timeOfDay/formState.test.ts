import {
  getChangedTimeOfDayFormFields,
  mergeUntouchedTimeOfDayFormState,
} from './formState'
import {
  defaultPrimaryDirections,
  timeOfDayDefaultTuningOptions,
  type TimeOfDayFormState,
} from './types'

const formState: TimeOfDayFormState = {
  selectedLocations: [],
  selectedDates: [new Date(2026, 0, 1)],
  dataSource: 'IndianaEvents',
  allDayPrimaryDirections: defaultPrimaryDirections,
  amPrimaryDirections: defaultPrimaryDirections,
  pmPrimaryDirections: defaultPrimaryDirections,
  directionLaneCounts: {},
  ...timeOfDayDefaultTuningOptions,
}

describe('time-of-day form initialization', () => {
  test('applies delayed defaults only to fields the user has not edited', () => {
    const editedState = {
      ...formState,
      laneCapacityVehiclesPerHour: 950,
    }
    const editedFields = getChangedTimeOfDayFormFields(formState, editedState)
    const merged = mergeUntouchedTimeOfDayFormState(
      editedState,
      {
        laneCapacityVehiclesPerHour: 1200,
        approachVolumeAssumedLanes: 3,
      },
      editedFields
    )

    expect(merged.laneCapacityVehiclesPerHour).toBe(950)
    expect(merged.approachVolumeAssumedLanes).toBe(3)
  })
})
