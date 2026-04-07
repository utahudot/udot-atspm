import { getRequestedLegendVisibility } from './TimeSpaceEChart'

describe('getRequestedLegendVisibility', () => {
  const directionRoleBySeriesName = new Map([
    ['Cycles EB', 'primary' as const],
    ['Cycle Durations EB', 'primary' as const],
    ['Green Bands EB', 'primary' as const],
    ['Lane by Lane Count EB', 'primary' as const],
    ['Cycles WB', 'opposing' as const],
  ])

  it('hides a suppressed direction without overwriting the underlying requested state', () => {
    const requestedSelections = {
      'Cycles EB': true,
      'Green Bands EB': true,
      'Lane by Lane Count EB': false,
    }

    expect(
      getRequestedLegendVisibility(
        'Cycles EB',
        requestedSelections,
        { primary: true },
        directionRoleBySeriesName
      )
    ).toBe(false)

    expect(
      getRequestedLegendVisibility(
        'Green Bands EB',
        requestedSelections,
        { primary: true },
        directionRoleBySeriesName
      )
    ).toBe(false)

    expect(
      getRequestedLegendVisibility(
        'Lane by Lane Count EB',
        requestedSelections,
        {},
        directionRoleBySeriesName
      )
    ).toBe(false)
  })

  it('restores the previously requested series state when the direction is unsuppressed', () => {
    const requestedSelections = {
      'Cycles EB': true,
      'Green Bands EB': true,
      'Lane by Lane Count EB': false,
    }

    expect(
      getRequestedLegendVisibility(
        'Cycles EB',
        requestedSelections,
        {},
        directionRoleBySeriesName
      )
    ).toBe(true)

    expect(
      getRequestedLegendVisibility(
        'Green Bands EB',
        requestedSelections,
        {},
        directionRoleBySeriesName
      )
    ).toBe(true)

    expect(
      getRequestedLegendVisibility(
        'Lane by Lane Count EB',
        requestedSelections,
        {},
        directionRoleBySeriesName
      )
    ).toBe(false)
  })

  it('keeps cycle duration labels hidden when their parent cycles are off', () => {
    const requestedSelections = {
      'Cycles EB': false,
      'Cycle Durations EB': true,
    }

    expect(
      getRequestedLegendVisibility(
        'Cycle Durations EB',
        requestedSelections,
        {},
        directionRoleBySeriesName
      )
    ).toBe(false)
  })
})
