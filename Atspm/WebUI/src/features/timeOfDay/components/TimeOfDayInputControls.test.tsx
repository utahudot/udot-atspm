import { fireEvent, render, screen } from '@testing-library/react'
import type { ReactNode } from 'react'
import type {
  TimeOfDayMeasureDefaults,
  TimeOfDaySchedulePreset,
} from '../measureDefaults'
import {
  defaultPrimaryDirections,
  timeOfDayDefaultTuningOptions,
  type TimeOfDayFormState,
} from '../types'
import TimeOfDayAdvancedSidebar from './TimeOfDayAdvancedSidebar'
import TimeOfDayDirectionSelector from './TimeOfDayDirectionSelector'
import { TimeOfDayMeasureOptions } from './TimeOfDayMeasureOptions'
import TimeOfDaySchedulePresetSelect from './TimeOfDaySchedulePresetSelect'

jest.mock('@/components/RightSidebar', () => ({
  __esModule: true,
  default: ({ children }: { children: ReactNode }) => <>{children}</>,
}))

jest.mock('@mui/x-date-pickers', () => ({
  TimePicker: () => null,
}))

const options: TimeOfDayFormState = {
  selectedLocations: [],
  selectedDates: [],
  dataSource: 'IndianaEvents',
  allDayPrimaryDirections: defaultPrimaryDirections,
  amPrimaryDirections: defaultPrimaryDirections,
  pmPrimaryDirections: defaultPrimaryDirections,
  directionLaneCounts: {},
  ...timeOfDayDefaultTuningOptions,
}

const schedulePresets: TimeOfDaySchedulePreset[] = [
  {
    id: 4102,
    name: 'Suburban Mixed-Use',
    options: {
      amEntryPctOfPeak: 0.55,
      amExitPctOfPeak: 0.4,
      pmEntryPctOfPeak: 0.68,
      pmExitPctOfPeak: 0.38,
      freeEntryPctOfDailyPeak: 0.22,
      freeEntryPctOfDynamicRange: 0.18,
      entrySustainedBins: 2,
      freeSustainedBins: 4,
    },
  },
  {
    id: 4103,
    name: 'Retail / Commercial',
    options: {
      amEntryPctOfPeak: 0.5,
      amExitPctOfPeak: 0.38,
      pmEntryPctOfPeak: 0.62,
      pmExitPctOfPeak: 0.36,
      freeEntryPctOfDailyPeak: 0.25,
      freeEntryPctOfDynamicRange: 0.2,
      entrySustainedBins: 3,
      freeSustainedBins: 4,
    },
  },
]

describe('time-of-day inputs', () => {
  test('defaults to separate AM and PM primary directions', () => {
    render(
      <TimeOfDayDirectionSelector options={options} onChange={jest.fn()} />
    )

    expect(
      (
        screen.getByRole('checkbox', {
          name: 'Separate AM/PM directions',
        }) as HTMLInputElement
      ).checked
    ).toBe(true)
    expect(screen.getByText('AM')).toBeTruthy()
    expect(screen.getByText('PM')).toBeTruthy()
  })

  test('displays schedule threshold fractions as editable percents', () => {
    const handleChange = jest.fn()
    const { rerender } = render(
      <TimeOfDayAdvancedSidebar
        activeSidebar={'schedule'}
        options={options}
        onChange={handleChange}
        schedulePresets={schedulePresets}
      />
    )

    expect(
      screen.getByRole('combobox', {
        name: 'Roadway type threshold preset',
      }).textContent
    ).toBe('FHWA Suburban Mixed-Use')
    expect(
      screen.getByText(/volume levels that start and end AM\/PM plans/)
    ).toBeTruthy()
    expect(
      screen.getByText(/falling evening volume returns the schedule to FREE/)
    ).toBeTruthy()
    expect(
      screen.getByText(/configured number of consecutive bins/)
    ).toBeTruthy()
    expect(screen.getByText(/latest time each peak plan can end/)).toBeTruthy()

    const amStart = screen.getByRole('slider', {
      name: 'AM start threshold',
    })
    expect(amStart.getAttribute('aria-valuenow')).toBe('55')
    expect(screen.getAllByText('55%').length).toBeGreaterThan(0)

    fireEvent.change(amStart, { target: { value: '60' } })

    expect(handleChange).toHaveBeenLastCalledWith(
      expect.objectContaining({ amEntryPctOfPeak: 0.6 })
    )

    rerender(
      <TimeOfDayAdvancedSidebar
        activeSidebar={'schedule'}
        options={{ ...options, amEntryPctOfPeak: 0.6 }}
        onChange={handleChange}
        schedulePresets={schedulePresets}
      />
    )

    expect(
      screen.getByRole('combobox', {
        name: 'Roadway type threshold preset',
      }).textContent
    ).toBe('Custom')
  })

  test('applies a schedule preset and identifies edited values as custom', () => {
    const handleChange = jest.fn()
    const { rerender } = render(
      <TimeOfDaySchedulePresetSelect
        options={options}
        onChange={handleChange}
        presets={schedulePresets}
      />
    )

    const presetSelect = screen.getByRole('combobox', {
      name: 'Roadway type threshold preset',
    })
    expect(presetSelect.textContent).toBe('FHWA Suburban Mixed-Use')

    fireEvent.mouseDown(presetSelect)
    fireEvent.click(
      screen.getByRole('option', { name: 'FHWA Retail / Commercial' })
    )

    expect(handleChange).toHaveBeenLastCalledWith({
      ...options,
      ...schedulePresets[1].options,
    })

    rerender(
      <TimeOfDaySchedulePresetSelect
        options={{ ...options, amEntryPctOfPeak: 0.56 }}
        onChange={handleChange}
        presets={schedulePresets}
      />
    )

    expect(
      screen.getByRole('combobox', {
        name: 'Roadway type threshold preset',
      }).textContent
    ).toBe('Custom')
  })

  test('explains occupancy capacity, review thresholds, and lane overrides', () => {
    render(
      <TimeOfDayAdvancedSidebar
        activeSidebar={'occupancy'}
        options={options}
        onChange={jest.fn()}
      />
    )

    expect(screen.getByText(/Converts approach volume/)).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'Capacity' })).toBeTruthy()
    expect(
      screen.getByRole('heading', { name: 'Review thresholds' })
    ).toBeTruthy()
    const laneConfigurationHeading = screen.getByRole('heading', {
      name: 'Lane configuration',
    })
    expect(laneConfigurationHeading).toBeTruthy()
    expect(
      screen.getByText(/Optional direction counts override lanes inferred from/)
    ).toBeTruthy()
    expect(
      (
        screen.getByRole('spinbutton', {
          name: 'Per-lane capacity (veh/hr)',
        }) as HTMLInputElement
      ).value
    ).toBe('800')
    const fallbackLanesInput = screen.getByRole('spinbutton', {
      name: 'Fallback lanes per approach',
    })
    expect(fallbackLanesInput).toBeTruthy()
    expect(fallbackLanesInput.closest('section')).toBe(
      laneConfigurationHeading.closest('section')
    )
    const southboundLanesInput = screen.getByRole('spinbutton', {
      name: 'Southbound lanes',
    })
    expect(
      southboundLanesInput.compareDocumentPosition(fallbackLanesInput) &
        Node.DOCUMENT_POSITION_FOLLOWING
    ).toBeTruthy()
    expect(screen.getByText(/Used as the total lane count/)).toBeTruthy()
    expect(screen.getAllByPlaceholderText('Auto')).toHaveLength(2)
  })

  test('displays measure-default threshold fractions as percents', () => {
    const handleUpdate = jest.fn()
    const chartDefaults: TimeOfDayMeasureDefaults = {
      amEntryPctOfPeak: {
        id: 1,
        option: 'amEntryPctOfPeak',
        value: 0.55,
      },
    }

    render(
      <TimeOfDayMeasureOptions
        chartDefaults={chartDefaults}
        handleChartOptionsUpdate={handleUpdate}
      />
    )

    const amStart = screen.getByRole('spinbutton', {
      name: 'AM start threshold as percent of AM peak',
    })
    expect((amStart as HTMLInputElement).value).toBe('55')
    expect(amStart.getAttribute('max')).toBe('100')

    fireEvent.change(amStart, { target: { value: '60' } })

    expect(handleUpdate).toHaveBeenLastCalledWith({
      id: 1,
      option: 'amEntryPctOfPeak',
      value: '0.6',
    })
  })
})
