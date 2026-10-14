import { timeOfDayDefaultTuningOptions, type TimeOfDayOptions } from '../types'
import {
  deriveAllDayPrimaryDirections,
  toApiTimeOfDayOptions,
} from './getTimeOfDay'

describe('deriveAllDayPrimaryDirections', () => {
  test('passes matching AM and PM picks through, even across streets', () => {
    expect(
      deriveAllDayPrimaryDirections(
        ['Northbound', 'Eastbound'],
        ['Eastbound', 'Northbound']
      )
    ).toEqual(['Northbound', 'Eastbound'])
  })

  test('combines differing picks on the same street', () => {
    expect(deriveAllDayPrimaryDirections(['Eastbound'], ['Westbound'])).toEqual(
      ['Eastbound', 'Westbound']
    )
  })

  test('leaves the backend to infer when differing picks span streets', () => {
    expect(
      deriveAllDayPrimaryDirections(
        ['Eastbound', 'Westbound'],
        ['Northbound', 'Southbound']
      )
    ).toEqual([])
  })
})

test('sends all-day directions from the visible AM and PM picks', () => {
  const options = {
    ...timeOfDayDefaultTuningOptions,
    locationIdentifiers: ['7115'],
    selectedDates: ['2026-04-15'],
    binSizeMinutes: 15,
    dataSource: 'IndianaEvents',
    allDayPrimaryDirections: ['Northbound', 'Southbound'],
    amPrimaryDirections: ['Eastbound', 'Westbound'],
    pmPrimaryDirections: ['Eastbound', 'Westbound'],
    directionLaneCounts: {},
  } as TimeOfDayOptions

  expect(toApiTimeOfDayOptions(options).allDayPrimaryDirections).toEqual([
    'Eastbound',
    'Westbound',
  ])
})
