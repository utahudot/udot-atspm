import { act, renderHook, waitFor } from '@testing-library/react'
import { format } from 'date-fns'
import {
  getDayAvailabilityFromLocationData,
  useDayAvailability,
  type DayAvailabilityDataSource,
} from './useDayAvailability'

const mockGetAggregationDays = jest.fn()
const mockGetEventLogDays = jest.fn()

jest.mock('@/api/data', () => ({
  getAggregationDaysWithDataFromLocationIdentifierAndDataType: (
    ...args: unknown[]
  ) => mockGetAggregationDays(...args),
  getEventLogDaysWithDataFromLocationIdentifierAndDataType: (
    ...args: unknown[]
  ) => mockGetEventLogDays(...args),
}))

const dateKey = (date: Date) => format(date, 'yyyy-MM-dd')

describe('getDayAvailabilityFromLocationData', () => {
  it('summarizes data availability by location for each day', () => {
    const availability = getDayAvailabilityFromLocationData({
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
    const availability = getDayAvailabilityFromLocationData({
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
    const availability = getDayAvailabilityFromLocationData({
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
    const availability = getDayAvailabilityFromLocationData({
      locationIdentifiers: ['7192', '7191'],
      startDate: new Date(2026, 4, 5),
      endDate: new Date(2026, 4, 6),
      today: new Date(2026, 4, 5),
      availableDaysByLocation: [[], []],
    })

    expect(availability.map((day) => dateKey(day.date))).toEqual(['2026-05-05'])
  })
})

describe('useDayAvailability', () => {
  beforeEach(() => {
    mockGetAggregationDays.mockReset()
    mockGetEventLogDays.mockReset()
  })

  it('does not expose results from the previous data source while loading', async () => {
    let resolveAggregation: ((days: string[]) => void) | undefined
    mockGetEventLogDays.mockResolvedValue(['2026-05-04'])
    mockGetAggregationDays.mockImplementation(
      () =>
        new Promise<string[]>((resolve) => {
          resolveAggregation = resolve
        })
    )
    const rawSource: DayAvailabilityDataSource = {
      dataCategory: 'raw',
      dataType: 'IndianaEvent',
    }
    const aggregationSource: DayAvailabilityDataSource = {
      dataCategory: 'aggregation',
      dataType: 'DetectorEventCountAggregation',
    }
    const startDate = new Date(2026, 4, 4)
    const endDate = new Date(2026, 4, 5)
    const { result, rerender } = renderHook(
      ({ dataSource }: { dataSource: DayAvailabilityDataSource }) =>
        useDayAvailability(['7192'], startDate, endDate, undefined, dataSource),
      { initialProps: { dataSource: rawSource } }
    )

    await waitFor(() => expect(result.current).toHaveLength(2))
    expect(result.current[0].availableLocationCount).toBe(1)

    rerender({ dataSource: aggregationSource })

    expect(result.current).toEqual([])

    await act(async () => resolveAggregation?.(['2026-05-05']))
    await waitFor(() => expect(result.current).toHaveLength(2))
    expect(result.current[0].availableLocationCount).toBe(0)
    expect(result.current[1].availableLocationCount).toBe(1)
  })
})
