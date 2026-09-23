import TimeOfDayPage from '@/pages/time-of-day'
import { fireEvent, render, screen } from '@testing-library/react'
import type { ReactNode } from 'react'
import { timeOfDayDefaultTuningOptions, type TimeOfDayFormState } from './types'

const mockUseChartDefaults = jest.fn()
let mockQuery: Record<string, unknown> = {}

jest.mock('@/api/config', () => ({
  useGetMeasureTypeMeasureOptionPresetsFromKey: () => ({}),
  getLocationLocationsForSearch: jest.fn(async () => []),
}))
jest.mock('@/features/charts/api', () => ({
  useChartDefaults: () => mockUseChartDefaults(),
}))
jest.mock('@/features/timeOfDay/api/getTimeOfDay', () => ({
  useTimeOfDayReport: () => ({ mutateAsync: jest.fn(), isLoading: false }),
}))
jest.mock('@/components/ResponsivePage', () => ({
  ResponsivePageLayout: ({ children }: { children: ReactNode }) => (
    <>{children}</>
  ),
}))
jest.mock(
  '@/features/charts/components/chartMessages/ChartMessages',
  () => () => null
)
jest.mock('@/features/timeOfDay/components/TimeOfDayResults', () => () => null)
jest.mock(
  '@/features/timeOfDay/components/TimeOfDayOptions',
  () =>
    function MockTimeOfDayOptions({
      options,
      onChange,
    }: {
      options: TimeOfDayFormState
      onChange: (options: TimeOfDayFormState) => void
    }) {
      return (
        <>
          <input
            aria-label="Per-lane capacity"
            type="number"
            value={options.laneCapacityVehiclesPerHour}
            onChange={(event) =>
              onChange({
                ...options,
                laneCapacityVehiclesPerHour: Number(event.target.value),
              })
            }
          />
          <output data-testid="fallback-lanes">
            {options.approachVolumeAssumedLanes}
          </output>
        </>
      )
    }
)

jest.mock('nuqs', () => {
  const parser = {
    withDefault: (defaultValue: unknown) => ({ defaultValue }),
  }
  return {
    createParser: () => parser,
    parseAsArrayOf: () => parser,
    parseAsString: parser,
    parseAsFloat: parser,
    parseAsInteger: parser,
    useQueryStates: (keyMap: Record<string, { defaultValue?: unknown }>) => {
      const { useRef } = jest.requireActual<typeof import('react')>('react')
      const defaults = Object.fromEntries(
        Object.entries(keyMap).map(([key, value]) => [
          key,
          value.defaultValue ?? null,
        ])
      )
      // Match nuqs: missing parameters receive defaults with stable references.
      const key = JSON.stringify(defaults)
      const defaultsRef = useRef({ key, values: defaults })
      if (defaultsRef.current.key !== key) {
        defaultsRef.current = { key, values: defaults }
      }
      return [{ ...defaultsRef.current.values, ...mockQuery }, jest.fn()]
    },
  }
})

const capacityInput = () =>
  screen.getByRole('spinbutton', {
    name: 'Per-lane capacity',
  }) as HTMLInputElement

const editCapacity = () =>
  fireEvent.change(capacityInput(), { target: { value: '950' } })

describe('TimeOfDayPage form synchronization', () => {
  beforeEach(() => {
    mockUseChartDefaults.mockClear()
    mockQuery = {}
  })

  test('initializes from code defaults without requesting database defaults', () => {
    render(<TimeOfDayPage />)

    expect(capacityInput().value).toBe(
      String(timeOfDayDefaultTuningOptions.laneCapacityVehiclesPerHour)
    )
    expect(screen.getByTestId('fallback-lanes').textContent).toBe(
      String(timeOfDayDefaultTuningOptions.approachVolumeAssumedLanes)
    )
    expect(mockUseChartDefaults).not.toHaveBeenCalled()
  })

  test('applies explicit URL values after a local edit', () => {
    const { rerender } = render(<TimeOfDayPage />)
    editCapacity()

    mockQuery = { laneCapacity: 1300 }
    rerender(<TimeOfDayPage />)

    expect(capacityInput().value).toBe('1300')
  })

  test('restores the default when a URL parameter is removed after an edit', () => {
    mockQuery = { laneCapacity: 1300 }
    const { rerender } = render(<TimeOfDayPage />)
    editCapacity()

    mockQuery = {}
    rerender(<TimeOfDayPage />)

    expect(capacityInput().value).toBe(
      String(timeOfDayDefaultTuningOptions.laneCapacityVehiclesPerHour)
    )
  })

  test('preserves local edits across rerenders', () => {
    const { rerender } = render(<TimeOfDayPage />)
    editCapacity()

    rerender(<TimeOfDayPage />)

    expect(capacityInput().value).toBe('950')
    expect(screen.getByTestId('fallback-lanes').textContent).toBe(
      String(timeOfDayDefaultTuningOptions.approachVolumeAssumedLanes)
    )
  })
})
