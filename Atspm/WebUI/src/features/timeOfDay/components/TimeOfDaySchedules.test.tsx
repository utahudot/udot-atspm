import type { Plan, TimeOfDayResult } from '@/api/reports'
import { fireEvent, render, screen } from '@testing-library/react'
import TimeOfDayResults from './TimeOfDayResults'
import TimeOfDaySchedules, {
  buildTimeOfDaySchedulesModel,
} from './TimeOfDaySchedules'

jest.mock('./TimeOfDayChartWorkspace', () => ({
  __esModule: true,
  default: () => <div aria-label="Mock time-of-day chart" />,
}))

const plan = (
  planNumber: string,
  start: string,
  end: string,
  planDescription = `Plan ${planNumber}`
): Plan => ({
  planNumber,
  planDescription,
  start: `2026-04-15T${start}:00`,
  end: `2026-04-${end === '00:00' ? '16' : '15'}T${end}:00`,
})

const proposedSchedule = [
  plan('Free', '00:00', '07:00', 'Free'),
  plan('1', '07:00', '09:00'),
  plan('7', '09:00', '16:00'),
  plan('13', '16:00', '20:00'),
  plan('7', '20:00', '00:00'),
]

const commonSchedule = [
  plan('Free', '00:00', '06:00', 'Free'),
  plan('1', '06:00', '09:00'),
  plan('7', '09:00', '16:00'),
  plan('13', '16:00', '21:00'),
  plan('7', '21:00', '00:00'),
]

const exceptionSchedule = [
  plan('Free', '00:00', '05:00', 'Free'),
  plan('2', '05:00', '10:00'),
  plan('8', '10:00', '00:00'),
]

const result = {
  locationIdentifiers: ['7174', '7621', '7015', '1005', '7618', '7077'],
  recommendation: { recommendedSchedule: proposedSchedule },
  planComparison: {
    commonCurrentSchedule: commonSchedule,
    exceptionLocationIdentifiers: ['1005', '7618', '7077'],
  },
  locations: [
    {
      locationIdentifier: '7174',
      locationDescription: 'Main St & 100 S',
      currentPlanSchedule: commonSchedule,
    },
    {
      locationIdentifier: '7621',
      locationDescription: 'State St & Center St',
      currentPlanSchedule: commonSchedule.map((entry) => ({ ...entry })),
    },
    {
      locationIdentifier: '7015',
      locationDescription: 'Broadway & 200 S',
    },
    {
      locationIdentifier: '1005',
      locationDescription: 'Foothill Dr & 1300 S',
      currentPlanSchedule: exceptionSchedule,
    },
    {
      locationIdentifier: '7618',
      locationDescription: '700 E & 400 S',
      currentPlanSchedule: [
        plan('Free', '00:00', '08:00', 'Free'),
        plan('9', '08:00', '00:00'),
      ],
    },
    {
      locationIdentifier: '7077',
      locationDescription: 'No Schedule Ave',
    },
  ],
} as TimeOfDayResult

describe('TimeOfDaySchedules', () => {
  test('groups locations using the common schedule and keeps each exception separate', () => {
    const model = buildTimeOfDaySchedulesModel(result)

    expect(
      model.commonLocations.map((location) => location.identifier)
    ).toEqual(['7174', '7621', '7015'])
    expect(model.exceptions.map(({ location }) => location.identifier)).toEqual(
      ['1005', '7618']
    )
    expect(
      model.unavailableLocations.map((location) => location.identifier)
    ).toEqual(['7077'])
  })

  test('derives the most-used schedule when no common schedule is reported', () => {
    const derivedModel = buildTimeOfDaySchedulesModel({
      locationIdentifiers: ['1', '2', '3'],
      locations: [
        { locationIdentifier: '1', currentPlanSchedule: commonSchedule },
        {
          locationIdentifier: '2',
          currentPlanSchedule: commonSchedule.map((entry) => ({ ...entry })),
        },
        { locationIdentifier: '3', currentPlanSchedule: exceptionSchedule },
      ],
    })

    expect(
      derivedModel.commonLocations.map((location) => location.identifier)
    ).toEqual(['1', '2'])
    expect(derivedModel.exceptions).toHaveLength(1)
    expect(derivedModel.exceptions[0].location.identifier).toBe('3')
  })

  test('renders grouped schedule rows with proposed boundary guides', () => {
    render(<TimeOfDaySchedules result={result} />)

    expect(screen.getByRole('heading', { name: 'Schedules' })).toBeTruthy()
    expect(
      screen.getByText(/Dashed guides mark the proposed schedule/)
    ).toBeTruthy()
    expect(
      screen.getByRole('region', { name: 'Proposed schedule' })
    ).toBeTruthy()
    expect(
      screen.getByRole('region', {
        name: 'Common schedule — 3 locations',
      })
    ).toBeTruthy()
    expect(
      screen.getByRole('region', { name: 'Exceptions — 2 locations' })
    ).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'Proposed' })).toBeTruthy()
    expect(screen.queryByText('Proposed schedule')).toBeNull()
    expect(screen.getByText('Proposal').tagName).toBe('EM')
    expect(screen.getByRole('heading', { name: 'Common' })).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'Exceptions' })).toBeTruthy()
    expect(screen.getByText('The recommended schedule.')).toBeTruthy()
    expect(
      screen.getByText(
        'The existing schedule used by the largest group of selected locations.'
      )
    ).toBeTruthy()
    expect(
      screen.getByText(
        'Selected locations with an existing schedule that differs from the common schedule.'
      )
    ).toBeTruthy()
    expect(screen.getByText('#7174 — Main St & 100 S')).toBeTruthy()
    expect(screen.getByText('#7621 — State St & Center St')).toBeTruthy()
    expect(screen.getByText('#7015 — Broadway & 200 S')).toBeTruthy()
    expect(screen.getByText('#1005 — Foothill Dr & 1300 S')).toBeTruthy()
    expect(screen.getByText('#7618 — 700 E & 400 S')).toBeTruthy()
    expect(screen.getAllByText('00:00')).toHaveLength(2)
    expect(screen.getAllByText('24:00')).toHaveLength(2)
    expect(screen.queryByText('12 AM')).toBeNull()
    screen.getAllByText('1').forEach((label) => {
      expect(label.getAttribute('data-plan-shape')).toBe('circle')
    })
    screen.getAllByText('FREE').forEach((label) => {
      expect(label.getAttribute('data-plan-shape')).toBe('pill')
    })
    expect(
      screen.getAllByRole('img', { name: /^Proposed schedule:/ })
    ).toHaveLength(1)
    expect(
      screen.getAllByRole('img', { name: /Existing schedule for/ })
    ).toHaveLength(2)
    expect(
      screen.getAllByRole('img', { name: /Common existing schedule for/ })
    ).toHaveLength(3)
    expect(screen.getAllByTestId('proposed-boundary-guide')).toHaveLength(24)
    expect(screen.queryByText('Different existing')).toBeNull()
    expect(screen.queryByText('All selected locations')).toBeNull()
    expect(
      screen.getByRole('region', {
        name: 'Unavailable schedules — 1 location',
      })
    ).toBeTruthy()
    expect(screen.getByText('#7077 — No Schedule Ave')).toBeTruthy()
    expect(
      screen.getByRole('img', {
        name: 'No current schedule for #7077 — No Schedule Ave: No data',
      })
    ).toBeTruthy()
    expect(screen.getByText('No data')).toBeTruthy()
    expect(screen.queryByText(/Schedule data is unavailable for/)).toBeNull()
  })

  test('shows why a recommendation is unavailable while retaining current schedules', () => {
    const reason =
      'Recommended schedule unavailable because primary direction data is unavailable for AM: Northbound.'
    render(
      <TimeOfDaySchedules
        result={{
          ...result,
          recommendation: {
            recommendedSchedule: [],
            summaryText: reason,
          },
        }}
      />
    )

    expect(screen.getByRole('alert').textContent).toContain(reason)
    expect(
      screen.getByRole('region', { name: 'Common schedule — 3 locations' })
    ).toBeTruthy()
  })

  test('places Schedules before Location Data in the results tabs', () => {
    render(<TimeOfDayResults result={result} />)

    const tabs = screen.getAllByRole('tab')
    expect(tabs.map((tab) => tab.textContent)).toEqual([
      'Time-of-Day Chart',
      'Schedules',
      'Location Data',
    ])

    fireEvent.click(screen.getByRole('tab', { name: 'Schedules' }))

    expect(screen.getByRole('tabpanel', { name: 'Schedules' })).toBeTruthy()
    expect(
      screen.getByRole('region', {
        name: 'Common schedule — 3 locations',
      })
    ).toBeTruthy()
  })
})
