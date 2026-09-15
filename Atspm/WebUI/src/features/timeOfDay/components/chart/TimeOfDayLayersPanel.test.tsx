import { fireEvent, render, screen } from '@testing-library/react'
import type { TimeOfDayChartLayer } from '../../transformers'
import TimeOfDayLayersPanel from './TimeOfDayLayersPanel'

const directionalSeriesNames = [
  'Eastbound total profile',
  'Northbound total profile',
  'Southbound total profile',
  'Westbound total profile',
]

const directionalColors = ['#00897b', '#7b1fa2', '#f57c00', '#5d4037']

const directionalProfilesLayer: TimeOfDayChartLayer = {
  id: 'directional-profiles',
  group: 'Corridor Demand',
  label: 'Directional profiles',
  description: 'Representative directional volume profiles.',
  preview: 'dashed-line',
  color: directionalColors[0],
  additionalColors: directionalColors.slice(1),
  seriesNames: directionalSeriesNames,
  seriesControls: directionalSeriesNames.map((seriesName, index) => ({
    seriesName,
    label: seriesName,
    color: directionalColors[index],
    available: true,
  })),
  available: true,
}

describe('TimeOfDayLayersPanel directional profiles', () => {
  test('expands one combined item into parent and per-direction checkboxes', () => {
    const onSetSeriesVisibility = jest.fn()

    render(
      <TimeOfDayLayersPanel
        layers={[directionalProfilesLayer]}
        selectedSeries={Object.fromEntries(
          directionalSeriesNames.map((seriesName) => [seriesName, true])
        )}
        onSetSeriesVisibility={onSetSeriesVisibility}
      />
    )

    const parentCheckbox = screen.getByRole('checkbox', {
      name: 'Toggle Directional profiles',
    })
    expect(parentCheckbox).toHaveProperty('checked', true)
    directionalSeriesNames.forEach((seriesName) => {
      expect(
        screen.queryByRole('checkbox', { name: `Toggle ${seriesName}` })
      ).toBeNull()
    })

    fireEvent.click(
      screen.getByRole('button', {
        name: 'Show Directional profiles details',
      })
    )

    directionalSeriesNames.forEach((seriesName) => {
      expect(
        screen.getByRole('checkbox', { name: `Toggle ${seriesName}` })
      ).toHaveProperty('checked', true)
    })

    fireEvent.click(
      screen.getByRole('checkbox', {
        name: 'Toggle Northbound total profile',
      })
    )
    expect(onSetSeriesVisibility).toHaveBeenLastCalledWith(
      ['Northbound total profile'],
      false
    )

    fireEvent.click(parentCheckbox)
    expect(onSetSeriesVisibility).toHaveBeenLastCalledWith(
      directionalSeriesNames,
      false
    )
  })
})

const proposedSeriesNames = ['Proposed plan windows', 'Proposed schedule rail']
const existingSeriesNames = ['Existing plan windows', 'Existing schedule rail']
const differenceSeriesNames = ['Plan difference windows']
const scheduleSeriesNames = [
  ...proposedSeriesNames,
  ...existingSeriesNames,
  ...differenceSeriesNames,
]

const planColorLegendItems = [
  { label: 'AM peak plan', color: '#ef6c00', preview: 'area' as const },
  { label: 'Midday plan', color: '#2e7d32', preview: 'area' as const },
  { label: 'PM peak plan', color: '#1565c0', preview: 'area' as const },
  { label: 'FREE operation', color: '#607d8b', preview: 'area' as const },
]

const scheduleLayers: TimeOfDayChartLayer[] = [
  {
    id: 'proposed-schedule',
    group: 'Schedules',
    label: 'Proposed',
    description:
      'Recommended timing-plan windows and rail. Expand for the color key.',
    preview: 'schedule',
    color: '#ef6c00',
    additionalColors: ['#2e7d32', '#1565c0'],
    seriesNames: proposedSeriesNames,
    legendItems: planColorLegendItems,
    available: true,
  },
  {
    id: 'existing-schedule',
    group: 'Schedules',
    label: 'Existing',
    description:
      'Current timing-plan windows and rail. Expand for the color key.',
    preview: 'schedule',
    color: '#ef6c00',
    additionalColors: ['#2e7d32', '#1565c0'],
    seriesNames: existingSeriesNames,
    legendItems: planColorLegendItems,
    available: true,
  },
  {
    id: 'schedule-differences',
    group: 'Schedules',
    label: 'Schedule differences',
    description: 'Hatching over the windows where the two schedules differ.',
    preview: 'hatch',
    color: '#f59e0b',
    seriesNames: differenceSeriesNames,
    legendItems: [
      {
        label: 'Proposed and existing schedules differ',
        color: '#f59e0b',
        preview: 'hatch',
      },
    ],
    available: true,
  },
]

const renderScheduleLayers = (onSetSeriesVisibility: jest.Mock) =>
  render(
    <TimeOfDayLayersPanel
      layers={scheduleLayers}
      selectedSeries={Object.fromEntries(
        scheduleSeriesNames.map((seriesName) => [seriesName, true])
      )}
      onSetSeriesVisibility={onSetSeriesVisibility}
    />
  )

describe('TimeOfDayLayersPanel schedules', () => {
  test('explains schedule colors and hatching without per-item toggles', () => {
    const onSetSeriesVisibility = jest.fn()

    renderScheduleLayers(onSetSeriesVisibility)

    fireEvent.click(
      screen.getByRole('button', { name: 'Show Proposed details' })
    )
    planColorLegendItems.forEach(({ label }) => {
      expect(screen.getByText(label)).toBeTruthy()
      expect(
        screen.queryByRole('checkbox', { name: `Toggle ${label}` })
      ).toBeNull()
    })

    fireEvent.click(
      screen.getByRole('button', { name: 'Show Schedule differences details' })
    )
    expect(
      screen.getByText('Proposed and existing schedules differ')
    ).toBeTruthy()
  })

  test('turns the proposed and existing schedules off one at a time', () => {
    const onSetSeriesVisibility = jest.fn()

    renderScheduleLayers(onSetSeriesVisibility)

    const proposedCheckbox = screen.getByRole('checkbox', {
      name: 'Toggle Proposed',
    })
    const existingCheckbox = screen.getByRole('checkbox', {
      name: 'Toggle Existing',
    })
    expect(proposedCheckbox).toHaveProperty('checked', true)
    expect(existingCheckbox).toHaveProperty('checked', true)

    fireEvent.click(existingCheckbox)
    expect(onSetSeriesVisibility).toHaveBeenLastCalledWith(
      existingSeriesNames,
      false
    )

    fireEvent.click(proposedCheckbox)
    expect(onSetSeriesVisibility).toHaveBeenLastCalledWith(
      proposedSeriesNames,
      false
    )
    expect(onSetSeriesVisibility).toHaveBeenCalledTimes(2)
  })
})
