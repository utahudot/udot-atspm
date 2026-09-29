import { useCharts } from '@/features/charts/api'
import { ChartType } from '@/features/charts/common/types'
import TurningMovementCountsTable from '@/features/charts/turningMovementCounts/components/TurningMovementCountsTable'
import '@testing-library/jest-dom'
import { render, screen } from '@testing-library/react'
import { AxiosError, AxiosHeaders } from 'axios'
import ChartsContainer from './ChartsContainer'

jest.mock('@/features/charts/api', () => ({ useCharts: jest.fn() }))
jest.mock('next/navigation', () => ({
  useRouter: () => ({ replace: jest.fn() }),
  usePathname: () => '/charts',
}))
jest.mock(
  '@/features/charts/approachVolume/components/ApproachVolumeChartResults',
  () => () => null
)
jest.mock('@/features/charts/components/chartsToolbox', () => () => null)
jest.mock('@/features/charts/components/defaultChartResults', () => () => null)
jest.mock(
  '@/features/charts/prioritySummary/components/PrioritySummaryChart',
  () => () => null
)
jest.mock(
  '@/features/charts/splitMonitor/components/PhaseTable',
  () => () => null
)
jest.mock(
  '@/features/charts/timingAndActuation/components/timingAndActuationChartsResults/TimingAndActuationChartsResults',
  () => () => null
)
jest.mock(
  '@/features/charts/timingAndActuation/components/timingAndActuationChartsToolbox/TimingAndActuationChartsToolbox',
  () => () => null
)
jest.mock(
  '@/features/charts/turningMovementCounts/components/TurningMovementCountsTable',
  () => jest.fn(() => null)
)
jest.mock(
  '@/features/locations/components/locationConfigContainer',
  () => () => null
)

function responseError(data: unknown) {
  return new AxiosError(
    'Request failed with status code 400',
    'ERR_BAD_REQUEST',
    undefined,
    undefined,
    {
      data,
      status: 400,
      statusText: 'Bad Request',
      headers: {},
      config: { headers: new AxiosHeaders() },
    }
  )
}

function setQuery(state: Partial<ReturnType<typeof useCharts>>) {
  jest.mocked(useCharts).mockReturnValue({
    refetch: jest.fn(),
    data: undefined,
    error: null,
    isError: false,
    isLoading: false,
    ...state,
  } as ReturnType<typeof useCharts>)
}

function renderCharts() {
  return render(
    <ChartsContainer
      location="1001"
      chartType={ChartType.TurningMovementCounts}
      startDateTime={new Date('2026-04-01T08:00:00')}
      endDateTime={new Date('2026-04-01T09:00:00')}
      options={{ binSize: 0 }}
    />
  )
}

beforeEach(() => {
  jest.clearAllMocks()
  jest.spyOn(window, 'scrollTo').mockImplementation(() => undefined)
})
afterEach(() => jest.restoreAllMocks())

it('renders all TMC validation messages and keeps the chart controls usable', () => {
  setQuery({
    isError: true,
    error: responseError({
      type: 'https://example.test/validation',
      title: 'One or more validation errors occurred.',
      status: 400,
      errors: {
        BinSize: ['The field BinSize must be between 1 and 60.'],
        End: ['End must be after start.'],
      },
    }),
  })
  renderCharts()
  expect(screen.getByRole('alert')).toHaveTextContent(
    'The field BinSize must be between 1 and 60.'
  )
  expect(screen.getByRole('alert')).toHaveTextContent(
    'End must be after start.'
  )
  expect(screen.getByRole('button', { name: 'Generate Charts' })).toBeEnabled()
})

it.each([
  [
    'plain text',
    responseError('No Controller Event Logs found for Location'),
    'No Controller Event Logs found for Location',
  ],
  [
    'problem details',
    responseError({
      title: 'Bad request',
      detail: 'Select a supported bin size.',
    }),
    'Select a supported bin size.',
  ],
  ['problem title', responseError({ title: 'Bad request' }), 'Bad request'],
  [
    'unexpected payload',
    responseError({
      errors: { BinSize: [{ message: 'nested object' }] },
      detail: {},
    }),
    'Request failed with status code 400',
  ],
  ['empty payload', responseError(null), 'Request failed with status code 400'],
  ['network failure', new AxiosError('Network Error'), 'Network Error'],
  [
    'client error',
    new Error('Unable to transform chart data'),
    'Unable to transform chart data',
  ],
])('renders %s errors as text', (_name, error, expected) => {
  setQuery({ isError: true, error })
  renderCharts()
  expect(screen.getByRole('alert')).toHaveTextContent(expected)
})

it('shows the no-data message without a zero-filled results table', () => {
  setQuery({
    data: {
      type: ChartType.TurningMovementCounts,
      data: {
        charts: [],
        table: [],
        peakHour: null,
        labels: { columnGroups: [], flatColumns: [] },
      },
    },
  })
  renderCharts()
  expect(screen.getByRole('alert')).toHaveTextContent(
    'No data available for the selected time range.'
  )
  expect(TurningMovementCountsTable).not.toHaveBeenCalled()
})
