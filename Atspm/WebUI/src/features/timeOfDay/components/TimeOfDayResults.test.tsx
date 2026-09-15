import type { TimeOfDayResult } from '@/api/reports'
import { act, fireEvent, render, screen, within } from '@testing-library/react'
import { init as initECharts } from 'echarts'
import {
  getTimeOfDayCrossTrafficDetailKey,
  getTimeOfDaySignalPeakDetailKey,
} from '../transformers'

import ChartMessages from '@/features/charts/components/chartMessages/ChartMessages'
import TimeOfDayResults from './TimeOfDayResults'

const mockChartHandlers = new Map<string, (params: unknown) => void>()
const mockChart = {
  dispatchAction: jest.fn(),
  dispose: jest.fn(),
  getDataURL: jest.fn(() => 'data:image/png;base64,test'),
  off: jest.fn((eventName: string) => mockChartHandlers.delete(eventName)),
  on: jest.fn((eventName: string, handler: (params: unknown) => void) => {
    mockChartHandlers.set(eventName, handler)
  }),
  resize: jest.fn(),
  setOption: jest.fn(),
}

jest.mock('echarts', () => ({
  ...jest.requireActual('echarts'),
  init: jest.fn(() => mockChart),
}))

const result = {
  selectedDates: ['2026-04-15'],
  locationIdentifiers: ['7190', '7191', '7192'],
  recommendation: {
    amPeakTime: '08:00',
    pmPeakTime: '17:15',
    summaryText: 'Use the proposed corridor schedule.',
    recommendedSchedule: [
      {
        planNumber: 'Free',
        planDescription: 'Free',
        start: '2026-04-15T00:00:00',
        end: '2026-04-15T07:00:00',
      },
      {
        planNumber: '1',
        planDescription: 'Plan 1',
        start: '2026-04-15T07:00:00',
        end: '2026-04-15T09:00:00',
      },
    ],
  },
  planComparison: {
    summaryText: 'The current schedule is shared by one location.',
    exceptionsText: 'Locations 7191 and 7192 use different schedules.',
    commonCurrentSchedule: [
      {
        planNumber: 'Free',
        planDescription: 'Free',
        start: '2026-04-15T00:00:00',
        end: '2026-04-15T06:00:00',
      },
      {
        planNumber: '7',
        planDescription: 'Plan 7',
        start: '2026-04-15T06:00:00',
        end: '2026-04-15T09:00:00',
      },
    ],
    exceptionLocationIdentifiers: ['7191', '7192'],
  },
  planProfile: {
    corridorProfile: {
      points: [{ minutes: 480, averageVolume: 3000, smoothedVolume: 2900 }],
    },
    peaks: [
      {
        period: 'AM',
        series: 'Location',
        locationIdentifier: '7190',
        minutes: 480,
        timeOfDay: '08:00',
        value: 2500,
      },
    ],
  },
  splitPressure: {
    summaryText: 'Cross traffic pressure is highest in the AM period.',
    reviewText: 'Review the AM split at location 7191.',
    peakCrossTrafficPercent: 27.3,
    peakCrossTrafficPercentTime: '08:00',
    thresholdPercentByName: { SplitReview: 25, ShoulderReview: 45 },
    primaryProfile: {
      points: [{ minutes: 480, averageVolume: 2400 }],
    },
    crossStreetProfile: {
      points: [{ minutes: 480, averageVolume: 900 }],
    },
    crossTrafficShare: [{ minutes: 480, crossTrafficPercent: 27.3 }],
    crossTrafficLocations: [
      {
        period: 'AM',
        locationIdentifier: '7191',
        minutes: 480,
        peakTime: '08:00',
        totalVehiclesPerHour: 900,
      },
    ],
    movementPressures: [
      {
        period: 'AM',
        locationIdentifier: '7192',
        movementLabel: 'Left',
        peakTime: '08:00',
        volume: 500,
      },
    ],
  },
  locations: [],
} as TimeOfDayResult

describe('TimeOfDayResults unified workspace', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    mockChartHandlers.clear()
  })

  test('uses top-level chart and location data tabs', () => {
    render(<TimeOfDayResults result={result} />)

    const chartTab = screen.getByRole('tab', {
      name: 'Time-of-Day Chart',
    })
    const locationDataTab = screen.getByRole('tab', { name: 'Location Data' })

    expect(chartTab.getAttribute('aria-selected')).toBe('true')
    const chart = screen.getByRole('img', {
      name: 'Corridor time-of-day analysis chart',
    })
    expect(chart).toBeTruthy()
    expect(
      screen.getByRole('heading', {
        name: 'Corridor Time-of-Day Analysis',
      })
    ).toBeTruthy()
    expect(screen.getByText(/Wed, April 15, 2026/)).toBeTruthy()
    const summary = screen.getByRole('region', { name: 'Chart summary' })
    const sidebar = screen.getByRole('complementary', {
      name: 'Time-of-day chart controls',
    })
    expect(summary.parentElement?.contains(chart)).toBe(true)
    expect(summary.parentElement?.firstElementChild).toBe(summary)
    expect(summary.parentElement?.parentElement).toBe(sidebar.parentElement)
    expect(
      within(summary).queryByRole('heading', { name: 'Summary' })
    ).toBeNull()
    expect(within(summary).getByLabelText('Measured peaks')).toBeTruthy()
    expect(within(summary).getByText('AM peak')).toBeTruthy()
    expect(within(summary).getByText('PM peak')).toBeTruthy()
    expect(within(summary).getByText('Peak cross traffic')).toBeTruthy()
    expect(within(summary).getByTestId('WbSunnyOutlinedIcon')).toBeTruthy()
    expect(within(summary).getByTestId('DarkModeOutlinedIcon')).toBeTruthy()
    expect(within(summary).getByTestId('ShuffleIcon')).toBeTruthy()
    expect(within(summary).queryByText('Proposed Plan Schedule')).toBeNull()
    expect(within(summary).getByText('Review')).toBeTruthy()
    expect(summary.textContent).toContain(
      'Review the AM split at location 7191.'
    )
    expect(screen.getByRole('tab', { name: 'Legend' })).toBeTruthy()
    expect(screen.queryByRole('tab', { name: 'Layers' })).toBeNull()

    fireEvent.click(locationDataTab)

    expect(locationDataTab.getAttribute('aria-selected')).toBe('true')
    expect(screen.getByText('Location Supporting Data')).toBeTruthy()
    expect(
      screen.queryByRole('img', {
        name: 'Corridor time-of-day analysis chart',
      })
    ).toBeNull()
  })

  test('presents below-threshold review text as status rather than a warning', () => {
    render(
      <TimeOfDayResults
        result={{
          ...result,
          splitPressure: {
            ...result.splitPressure,
            reviewText:
              'Cross traffic peaks at 27.3% at 08:00; primary street remains dominant.',
            thresholdPercentByName: { SplitReview: 35, ShoulderReview: 45 },
          },
        }}
      />
    )

    const summary = screen.getByRole('region', { name: 'Chart summary' })
    expect(within(summary).getByText('Review status')).toBeTruthy()
    expect(within(summary).queryByText('Review')).toBeNull()
  })

  test('renders multiple backend messages without rewriting them', () => {
    render(
      <ChartMessages
        severity="warning"
        ariaLabel="Analysis warnings"
        messages={[
          {
            code: 'PartialLocationData',
            locationIdentifier: '7117',
            message:
              'Location 7117 has usable volume data for 3 of 4 selected dates.',
          },
          {
            code: 'PartialLocationData',
            locationIdentifier: '7403',
            message:
              'Location 7403 has usable volume data for 3 of 4 selected dates.',
          },
        ]}
      />
    )

    const warnings = screen.getByRole('alert', {
      name: 'Analysis warnings',
    })
    const summaryToggle = within(warnings).getByRole('button', {
      name: '2 warnings',
    })
    expect(summaryToggle.getAttribute('aria-expanded')).toBe('false')
    expect(within(warnings).queryAllByRole('listitem')).toHaveLength(0)

    fireEvent.click(summaryToggle)

    expect(summaryToggle.getAttribute('aria-expanded')).toBe('true')
    expect(within(warnings).getAllByRole('listitem')).toHaveLength(2)
    expect(
      within(warnings).getByText(
        'Location 7117 has usable volume data for 3 of 4 selected dates.'
      )
    ).toBeTruthy()
    expect(
      within(warnings).getByText(
        'Location 7403 has usable volume data for 3 of 4 selected dates.'
      )
    ).toBeTruthy()
    expect(within(warnings).queryByText('Data quality')).toBeNull()
    expect(within(warnings).queryByText('Incomplete date coverage')).toBeNull()
  })

  test('switches analysis modes while toggling each schedule row on its own', () => {
    render(<TimeOfDayResults result={result} />)

    const peaksToggle = screen.getByRole('button', {
      name: 'Peaks',
    })
    const movementDemandToggle = screen.getByRole('button', {
      name: 'Movement Demand & Cross Traffic',
    })
    const proposedToggle = screen.getByRole('checkbox', {
      name: 'Toggle Proposed',
    }) as HTMLInputElement
    const existingToggle = screen.getByRole('checkbox', {
      name: 'Toggle Existing',
    }) as HTMLInputElement
    const differencesToggle = screen.getByRole('checkbox', {
      name: 'Toggle Schedule differences',
    }) as HTMLInputElement
    const existingSeriesNames = [
      'Existing plan windows',
      'Existing schedule rail',
    ]

    expect(peaksToggle.getAttribute('aria-pressed')).toBe('true')
    expect(movementDemandToggle.getAttribute('aria-pressed')).toBe('false')
    expect(proposedToggle.checked).toBe(true)
    expect(existingToggle.checked).toBe(true)
    expect(differencesToggle.checked).toBe(true)

    fireEvent.click(
      screen.getByRole('button', { name: 'Show Proposed details' })
    )
    ;['AM peak plan', 'Midday plan', 'PM peak plan', 'FREE operation'].forEach(
      (label) => {
        expect(screen.getByText(label)).toBeTruthy()
        expect(
          screen.queryByRole('checkbox', { name: `Toggle ${label}` })
        ).toBeNull()
      }
    )

    expect(
      screen.getByRole('checkbox', { name: 'Toggle Median raw volume' })
    ).toHaveProperty('checked', true)
    expect(
      screen.queryByRole('checkbox', { name: 'Toggle Movement demand' })
    ).toBeNull()

    mockChart.dispatchAction.mockClear()
    mockChart.setOption.mockClear()
    act(() => {
      mockChartHandlers.get('mouseover')?.({
        seriesName: 'Proposed schedule rail',
      })
    })
    expect(mockChart.dispatchAction).toHaveBeenCalledWith({
      type: 'highlight',
      seriesName: 'Proposed schedule rail',
      dataIndex: 0,
    })
    act(() => {
      mockChartHandlers.get('mouseout')?.({
        seriesName: 'Proposed schedule rail',
      })
    })
    expect(mockChart.dispatchAction).toHaveBeenCalledWith({
      type: 'downplay',
      seriesName: 'Proposed schedule rail',
      dataIndex: 0,
    })

    act(() => {
      mockChartHandlers.get('mouseover')?.({
        componentType: 'yAxis',
        value: 'Existing',
      })
    })
    expect(mockChart.dispatchAction).toHaveBeenCalledWith({
      type: 'highlight',
      seriesName: 'Existing schedule rail',
      dataIndex: 0,
    })
    act(() => {
      mockChartHandlers.get('mouseout')?.({
        componentType: 'yAxis',
        value: 'Existing',
      })
    })
    expect(mockChart.dispatchAction).toHaveBeenCalledWith({
      type: 'downplay',
      seriesName: 'Existing schedule rail',
      dataIndex: 0,
    })

    mockChart.dispatchAction.mockClear()
    act(() => {
      mockChartHandlers.get('click')?.({
        componentType: 'yAxis',
        value: 'Existing',
      })
    })
    expect(existingToggle.checked).toBe(false)
    expect(proposedToggle.checked).toBe(true)
    expect(differencesToggle.checked).toBe(false)
    ;['Existing plan windows', 'Plan difference windows'].forEach((name) => {
      expect(mockChart.dispatchAction).toHaveBeenCalledWith({
        type: 'legendUnSelect',
        name,
      })
    })
    expect(mockChart.dispatchAction).not.toHaveBeenCalledWith({
      type: 'legendUnSelect',
      name: 'Proposed plan windows',
    })

    mockChart.dispatchAction.mockClear()
    act(() => {
      mockChartHandlers.get('click')?.({
        componentType: 'yAxis',
        value: 'Existing',
      })
    })
    expect(existingToggle.checked).toBe(true)
    expect(differencesToggle.checked).toBe(true)
    ;[...existingSeriesNames, 'Plan difference windows'].forEach((name) => {
      expect(mockChart.dispatchAction).toHaveBeenCalledWith({
        type: 'legendSelect',
        name,
      })
    })

    fireEvent.click(differencesToggle)
    expect(differencesToggle.checked).toBe(false)
    expect(proposedToggle.checked).toBe(true)
    expect(existingToggle.checked).toBe(true)
    fireEvent.click(differencesToggle)
    expect(differencesToggle.checked).toBe(true)

    fireEvent.click(movementDemandToggle)
    expect(movementDemandToggle.getAttribute('aria-pressed')).toBe('true')
    expect(peaksToggle.getAttribute('aria-pressed')).toBe('false')
    expect(proposedToggle.checked).toBe(true)
    expect(existingToggle.checked).toBe(true)
    expect(
      screen.getByRole('checkbox', { name: 'Toggle Movement demand' })
    ).toHaveProperty('checked', false)
    expect(
      screen.getByRole('checkbox', { name: 'Toggle Cross-traffic percent' })
    ).toHaveProperty('checked', true)
    expect(mockChart.setOption).toHaveBeenCalledWith(
      expect.objectContaining({
        yAxis: [{}, expect.objectContaining({ show: true })],
      })
    )

    fireEvent.click(peaksToggle)
    expect(peaksToggle.getAttribute('aria-pressed')).toBe('true')
    expect(movementDemandToggle.getAttribute('aria-pressed')).toBe('false')
    expect(proposedToggle.checked).toBe(true)
    expect(initECharts).toHaveBeenCalledTimes(1)

    fireEvent.click(proposedToggle)
    expect(proposedToggle.checked).toBe(false)
    expect(existingToggle.checked).toBe(true)
    expect(differencesToggle.checked).toBe(false)
    const mutedProposedRail = mockChart.setOption.mock.calls
      .flatMap(([update]) => update?.series ?? [])
      .reverse()
      .find((series) => series.id === 'tod-proposed-schedule-rail')
    expect(mutedProposedRail?.silent).toBe(false)
    expect(mutedProposedRail?.cursor).toBe('pointer')
    expect(
      mutedProposedRail?.data.every(
        (datum: unknown[]) => datum[4] === '#94A3B8'
      )
    ).toBe(true)

    act(() => {
      mockChartHandlers.get('click')?.({
        seriesName: 'Proposed schedule rail',
      })
    })
    expect(proposedToggle.checked).toBe(true)
    expect(differencesToggle.checked).toBe(true)
    act(() => {
      mockChartHandlers.get('click')?.({
        seriesName: 'Existing schedule rail',
      })
    })
    expect(existingToggle.checked).toBe(false)
    expect(proposedToggle.checked).toBe(true)
    expect(differencesToggle.checked).toBe(false)

    expect(mockChart.dispose).not.toHaveBeenCalled()
    expect(
      mockChart.setOption.mock.calls.filter(
        ([, settings]) => settings?.notMerge === true
      )
    ).toHaveLength(0)
  })

  test('swaps the sidebar detail tabs with the analysis mode', () => {
    render(<TimeOfDayResults result={result} />)

    fireEvent.click(screen.getByRole('tab', { name: 'Signal Peaks' }))
    expect(
      screen
        .getByRole('tab', { name: 'Signal Peaks' })
        .getAttribute('aria-selected')
    ).toBe('true')

    fireEvent.click(
      screen.getByRole('button', { name: 'Movement Demand & Cross Traffic' })
    )
    expect(screen.queryByRole('tab', { name: 'Signal Peaks' })).toBeNull()
    expect(
      screen
        .getByRole('tab', { name: 'Cross Traffic' })
        .getAttribute('aria-selected')
    ).toBe('true')

    fireEvent.click(screen.getByRole('button', { name: 'Peaks' }))
    expect(screen.queryByRole('tab', { name: 'Cross Traffic' })).toBeNull()
    expect(
      screen
        .getByRole('tab', { name: 'Signal Peaks' })
        .getAttribute('aria-selected')
    ).toBe('true')

    fireEvent.click(screen.getByRole('tab', { name: 'Legend' }))
    fireEvent.click(
      screen.getByRole('button', { name: 'Movement Demand & Cross Traffic' })
    )
    expect(
      screen.getByRole('tab', { name: 'Legend' }).getAttribute('aria-selected')
    ).toBe('true')
  })

  test('opens Details and highlights a marker selected from the chart', () => {
    render(<TimeOfDayResults result={result} />)
    const signalPeak = result.planProfile?.peaks?.[0]
    if (!signalPeak) {
      throw new Error('Expected the test signal peak')
    }
    const detailKey = getTimeOfDaySignalPeakDetailKey(signalPeak)

    act(() => {
      mockChartHandlers.get('click')?.({ data: { detailKey } })
    })

    expect(
      screen
        .getByRole('tab', { name: 'Signal Peaks' })
        .getAttribute('aria-selected')
    ).toBe('true')
    expect(screen.queryByRole('tab', { name: 'Cross Traffic' })).toBeNull()
    expect(screen.queryByRole('tab', { name: 'Movement Demand' })).toBeNull()
    const amSignalPeaksToggle = screen.getByRole('checkbox', {
      name: 'Toggle AM Signal Peaks',
    })
    expect(
      within(screen.getByRole('region', { name: 'Peaks' })).getAllByText(
        'Show on chart'
      )
    ).toHaveLength(2)
    expect((amSignalPeaksToggle as HTMLInputElement).checked).toBe(true)
    fireEvent.click(amSignalPeaksToggle)
    expect((amSignalPeaksToggle as HTMLInputElement).checked).toBe(false)
    expect(mockChart.dispatchAction).toHaveBeenCalledWith({
      type: 'legendUnSelect',
      name: 'AM Signal Peaks',
    })
    expect(
      screen.queryByText('The current schedule is shared by one location.')
    ).toBeNull()
    expect(
      screen.queryByText('Locations 7191 and 7192 use different schedules.')
    ).toBeNull()
    const peakTable = screen.getByRole('table', {
      name: 'AM Signal Peaks peak list',
    })
    const selectedRow = within(peakTable)
      .getAllByRole('row')
      .find((row) => row.getAttribute('aria-selected') === 'true')
    expect(selectedRow).toBeTruthy()
    expect(mockChart.dispatchAction).toHaveBeenCalledWith(
      expect.objectContaining({
        type: 'highlight',
        seriesName: 'AM Signal Peaks',
      })
    )

    const crossTraffic = result.splitPressure?.crossTrafficLocations?.[0]
    if (!crossTraffic) {
      throw new Error('Expected the test cross-traffic location')
    }
    const crossTrafficDetailKey = getTimeOfDayCrossTrafficDetailKey(
      crossTraffic,
      'AM'
    )
    mockChart.dispatchAction.mockClear()
    act(() => {
      mockChartHandlers.get('click')?.({
        data: { detailKey: crossTrafficDetailKey },
      })
    })

    const crossTrafficTab = screen.getByRole('tab', { name: 'Cross Traffic' })
    const movementDemandTab = screen.getByRole('tab', {
      name: 'Movement Demand',
    })
    expect(screen.queryByRole('tab', { name: 'Signal Peaks' })).toBeNull()
    expect(crossTrafficTab.getAttribute('aria-selected')).toBe('true')
    expect(movementDemandTab.getAttribute('aria-selected')).toBe('false')
    expect(screen.queryByRole('region', { name: 'Peaks' })).toBeNull()
    expect(
      (
        screen.getByRole('checkbox', {
          name: 'Toggle AM Cross Traffic Locations',
        }) as HTMLInputElement
      ).checked
    ).toBe(true)
    expect(
      screen.queryByRole('table', { name: 'AM Signal Peaks peak list' })
    ).toBeNull()

    const crossTrafficTable = screen.getByRole('table', {
      name: 'AM Cross Traffic Locations cross traffic locations',
    })
    expect(
      screen.queryByRole('table', {
        name: 'AM Movement Demand movement demand',
      })
    ).toBeNull()
    const selectedCrossTrafficRow = within(crossTrafficTable)
      .getAllByRole('row')
      .find((row) => row.getAttribute('aria-selected') === 'true')
    expect(selectedCrossTrafficRow).toBeTruthy()

    const analysisModes = screen.getByRole('group', {
      name: 'Time-of-day analysis modes',
    })
    expect(
      within(analysisModes)
        .getByRole('button', { name: 'Peaks' })
        .getAttribute('aria-pressed')
    ).toBe('false')
    expect(
      within(analysisModes)
        .getByRole('button', { name: 'Movement Demand & Cross Traffic' })
        .getAttribute('aria-pressed')
    ).toBe('true')
    expect(mockChart.dispatchAction).toHaveBeenCalledWith(
      expect.objectContaining({
        type: 'highlight',
        seriesName: 'AM Cross Traffic Locations',
      })
    )

    fireEvent.click(movementDemandTab)
    expect(movementDemandTab.getAttribute('aria-selected')).toBe('true')
    expect(crossTrafficTab.getAttribute('aria-selected')).toBe('false')
    const amMovementDemandToggle = screen.getByRole('checkbox', {
      name: 'Toggle AM Movement Demand',
    })
    expect((amMovementDemandToggle as HTMLInputElement).checked).toBe(false)
    fireEvent.click(amMovementDemandToggle)
    expect((amMovementDemandToggle as HTMLInputElement).checked).toBe(true)
    expect(mockChart.dispatchAction).toHaveBeenCalledWith({
      type: 'legendSelect',
      name: 'AM Movement Demand',
    })
    expect(
      screen.getByRole('table', {
        name: 'AM Movement Demand movement demand',
      })
    ).toBeTruthy()
    expect(
      screen.queryByRole('table', {
        name: 'AM Cross Traffic Locations cross traffic locations',
      })
    ).toBeNull()
  })
})
