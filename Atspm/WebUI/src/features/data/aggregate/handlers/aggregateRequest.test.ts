import { BinSize, TimePeriodOption } from '../types/aggregateOptionsData'
import {
  AggregateTimeInputs,
  createFilterDirections,
  createFilterMovements,
  createTimeOptions,
  getTimeValidationMessage,
} from './aggregateRequest'

const at = (hours: number, minutes = 0) => new Date(2026, 9, 1, hours, minutes)

// Thursday October 1 up to Thursday October 15, weekdays, 06:00 to 00:00.
const makeInputs = (
  changes: Partial<AggregateTimeInputs> = {}
): AggregateTimeInputs => ({
  startDateTime: new Date(2026, 9, 1),
  endDateTime: new Date(2026, 9, 15),
  startTime: at(6),
  endTime: at(0),
  selectedDays: [1, 2, 3, 4, 5],
  binSize: BinSize.Hour,
  ...changes,
})

describe('createTimeOptions', () => {
  it('sends the time window on the selected Days, with an End Time of 00:00 as 23:59', () => {
    expect(createTimeOptions(makeInputs())).toMatchObject({
      timeOfDayStartHour: 6,
      timeOfDayStartMinute: 0,
      timeOfDayEndHour: 23,
      timeOfDayEndMinute: 59,
      daysOfWeek: [1, 2, 3, 4, 5],
      timeOption: TimePeriodOption.TimePeriod,
      selectedBinSize: BinSize.Hour,
    })
  })

  it('sends whole days on every day for month', () => {
    expect(
      createTimeOptions(makeInputs({ binSize: BinSize.Month }))
    ).toMatchObject({
      timeOfDayStartHour: 0,
      timeOfDayStartMinute: 0,
      timeOfDayEndHour: 23,
      timeOfDayEndMinute: 59,
      daysOfWeek: [0, 1, 2, 3, 4, 5, 6],
      timeOption: TimePeriodOption.StartToEnd,
    })
  })
})

describe('getTimeValidationMessage', () => {
  it('accepts the default time window', () => {
    expect(getTimeValidationMessage(makeInputs())).toBeNull()
  })

  it('catches a cleared or half typed time', () => {
    expect(
      getTimeValidationMessage(makeInputs({ endTime: null as unknown as Date }))
    ).toBe('Enter a valid End Time.')
    expect(
      getTimeValidationMessage(makeInputs({ startTime: new Date(NaN) }))
    ).toBe('Enter a valid Start Time.')
  })

  it('catches an End that is not later than Start', () => {
    expect(
      getTimeValidationMessage(
        makeInputs({ endDateTime: new Date(2026, 9, 1) })
      )
    ).toBe('End must be later than Start.')
  })

  it('catches an End Time that is not later than Start Time', () => {
    expect(getTimeValidationMessage(makeInputs({ endTime: at(6) }))).toBe(
      'End Time must be later than Start Time.'
    )
    // An End Time of 00:00 is sent as 23:59, which leaves no time after 23:59.
    expect(
      getTimeValidationMessage(makeInputs({ startTime: at(23, 59) }))
    ).toBe('End Time must be later than Start Time.')
  })

  it('catches Days that fall outside the date range', () => {
    // Thursday and Friday only.
    expect(
      getTimeValidationMessage(
        makeInputs({ endDateTime: new Date(2026, 9, 3), selectedDays: [1] })
      )
    ).toBe('None of the selected Days fall in the date range.')
  })

  it('ignores Start Time, End Time and Days for month', () => {
    expect(
      getTimeValidationMessage(
        makeInputs({
          binSize: BinSize.Month,
          selectedDays: [],
          endTime: null as unknown as Date,
        })
      )
    ).toBeNull()
  })
})

describe('filters', () => {
  it('sends every direction, with unchecked ones excluded', () => {
    // Every box but Eastbound and Southwest is checked.
    const filters = createFilterDirections([1, 2, 3, 4, 5, 6])
    expect(filters).toHaveLength(8)
    expect(
      filters
        .filter((filter) => !filter.include)
        .map((filter) => filter.directionTypeId)
    ).toEqual([3, 8])
  })

  it('sends every movement, with unchecked ones excluded', () => {
    // Only Left is checked.
    const filters = createFilterMovements([0])
    expect(filters).toHaveLength(7)
    expect(
      filters
        .filter((filter) => filter.include)
        .map((filter) => filter.movementTypeId)
    ).toEqual([3])
  })
})
