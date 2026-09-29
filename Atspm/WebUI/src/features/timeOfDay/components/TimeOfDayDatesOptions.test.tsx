import { render } from '@testing-library/react'
import {
  defaultPrimaryDirections,
  timeOfDayDefaultTuningOptions,
  type TimeOfDayFormState,
} from '../types'
import TimeOfDayDatesOptions from './TimeOfDayDatesOptions'

const mockUseDayAvailability = jest.fn((...args: unknown[]) => {
  void args
  return []
})
const mockMultiDayCalendar = jest.fn((props: unknown) => {
  void props
  return null
})

jest.mock('@/features/dataAvailability/useDayAvailability', () => ({
  ...jest.requireActual('@/features/dataAvailability/useDayAvailability'),
  useDayAvailability: (...args: unknown[]) => mockUseDayAvailability(...args),
}))

jest.mock('@/components/date-selection/MultiDayCalendar', () => ({
  __esModule: true,
  default: (props: unknown) => mockMultiDayCalendar(props),
}))

const buildOptions = (
  dataSource: TimeOfDayFormState['dataSource']
): TimeOfDayFormState => ({
  selectedLocations: [
    { locationIdentifier: '1001' },
    { locationIdentifier: ' 1002 ' },
  ],
  selectedDates: [new Date(2026, 4, 10)],
  dataSource,
  allDayPrimaryDirections: defaultPrimaryDirections,
  amPrimaryDirections: defaultPrimaryDirections,
  pmPrimaryDirections: defaultPrimaryDirections,
  directionLaneCounts: {},
  ...timeOfDayDefaultTuningOptions,
})

describe('TimeOfDayDatesOptions', () => {
  beforeEach(() => {
    mockUseDayAvailability.mockClear()
    mockMultiDayCalendar.mockClear()
  })

  it.each([
    [
      'IndianaEvents' as const,
      { dataCategory: 'raw', dataType: 'IndianaEvent' },
    ],
    [
      'Aggregated' as const,
      {
        dataCategory: 'aggregation',
        dataType: 'DetectorEventCountAggregation',
      },
    ],
  ])(
    'loads %s availability for every selected location',
    (dataSource, source) => {
      render(
        <TimeOfDayDatesOptions
          options={buildOptions(dataSource)}
          onChange={jest.fn()}
        />
      )

      expect(mockUseDayAvailability).toHaveBeenCalledWith(
        ['1001', '1002'],
        expect.any(Date),
        expect.any(Date),
        undefined,
        source
      )
      expect(mockMultiDayCalendar).toHaveBeenCalledWith(
        expect.objectContaining({
          dayAvailability: [],
          onMonthChange: expect.any(Function),
        })
      )
    }
  )
})
