import { format } from 'date-fns'
import { getTimeSpaceDayAvailabilityFromLocationData } from './useTimeSpaceMissingDays'

const dateKey = (date: Date) => format(date, 'yyyy-MM-dd')

describe('getTimeSpaceDayAvailabilityFromLocationData', () => {
  it('summarizes data availability by route location for each day', () => {
    const availability = getTimeSpaceDayAvailabilityFromLocationData({
      locationIdentifiers: ['7192', '7191'],
      startDate: new Date(2026, 4, 4),
      endDate: new Date(2026, 4, 6),
      today: new Date(2026, 4, 6),
      availableDaysByLocation: [
        ['2026-05-04', '2026-05-05', '2026-05-06'],
        ['2026-05-04', '2026-05-06'],
      ],
    })

    expect(availability.map((day) => day.availableLocationCount)).toEqual([
      2, 1, 2,
    ])
    expect(availability[1]).toMatchObject({
      availableLocationCount: 1,
      totalLocationCount: 2,
      locations: [
        { locationIdentifier: '7192', hasData: true },
        { locationIdentifier: '7191', hasData: false },
      ],
    })
  })

  it('ignores days outside the included days of week', () => {
    const availability = getTimeSpaceDayAvailabilityFromLocationData({
      locationIdentifiers: ['7192', '7191'],
      startDate: new Date(2026, 4, 2),
      endDate: new Date(2026, 4, 4),
      today: new Date(2026, 4, 4),
      includedDaysOfWeek: [1],
      availableDaysByLocation: [['2026-05-04'], ['2026-05-04']],
    })

    expect(availability.map((day) => dateKey(day.date))).toEqual(['2026-05-04'])
  })

  it('includes weekend availability when no weekday filter is provided', () => {
    const availability = getTimeSpaceDayAvailabilityFromLocationData({
      locationIdentifiers: ['7192', '7191'],
      startDate: new Date(2026, 4, 2),
      endDate: new Date(2026, 4, 4),
      today: new Date(2026, 4, 4),
      availableDaysByLocation: [
        ['2026-05-02', '2026-05-03', '2026-05-04'],
        ['2026-05-02', '2026-05-03', '2026-05-04'],
      ],
    })

    expect(availability.map((day) => dateKey(day.date))).toEqual([
      '2026-05-02',
      '2026-05-03',
      '2026-05-04',
    ])
  })

  it('does not mark future days as missing', () => {
    const availability = getTimeSpaceDayAvailabilityFromLocationData({
      locationIdentifiers: ['7192', '7191'],
      startDate: new Date(2026, 4, 5),
      endDate: new Date(2026, 4, 6),
      today: new Date(2026, 4, 5),
      availableDaysByLocation: [[], []],
    })

    expect(availability.map((day) => dateKey(day.date))).toEqual(['2026-05-05'])
  })
})
