import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import '@testing-library/jest-dom'
import { act, fireEvent, render, screen } from '@testing-library/react'
import HotspotSidebar from './HotspotSidebar'

const mockFetchMonthlyHotspots = jest.fn()
const mockFetchImpactHotspots = jest.fn()

jest.mock('@/api/speedManagement/aTSPMSpeedManagementApi', () => ({
  usePostApiV1MonthlyAggregationHotspots: () => ({
    mutateAsync: mockFetchMonthlyHotspots,
  }),
  usePostApiV1ImpactHotspots: () => ({
    mutateAsync: mockFetchImpactHotspots,
  }),
  useGetApiV1MonthlyAggregationSpeedCategoryFilters: () => ({
    data: [{ number: 0, displayName: 'Average Speed' }],
  }),
}))

jest.mock('@/features/speedManagementTool/components/SM_Map', () => ({
  SM_Height: '600px',
}))

jest.mock('@mui/x-date-pickers', () => ({
  DatePicker: () => null,
}))

function deferred<T>() {
  let resolve!: (value: T) => void
  let reject!: (reason: Error) => void
  const promise = new Promise<T>((resolvePromise, rejectPromise) => {
    resolve = resolvePromise
    reject = rejectPromise
  })
  return { promise, resolve, reject }
}

function selectEffectiveness() {
  fireEvent.mouseDown(screen.getAllByRole('combobox')[0])
  fireEvent.click(
    screen.getByRole('option', { name: 'Effectiveness of Strategies' })
  )
}

describe('HotspotSidebar request lifecycle', () => {
  beforeEach(() => {
    mockFetchMonthlyHotspots.mockReset()
    mockFetchImpactHotspots.mockReset()
    useSpeedManagementStore.getState().resetToDefaults()
    useSpeedManagementStore.getState().setHotspotRoutes([])
  })

  afterEach(() => {
    jest.restoreAllMocks()
  })

  it.each(['empty response', 'error'])(
    'keeps Effectiveness loading when the previous request finishes with an %s',
    async (completion) => {
      const monthly = deferred<unknown[]>()
      const impact = deferred<unknown[]>()
      mockFetchMonthlyHotspots.mockReturnValue(monthly.promise)
      mockFetchImpactHotspots.mockReturnValue(impact.promise)
      jest.spyOn(console, 'error').mockImplementation(() => undefined)

      render(<HotspotSidebar handleRouteSelection={jest.fn()} />)
      selectEffectiveness()
      expect(mockFetchImpactHotspots).toHaveBeenCalledTimes(1)
      expect(screen.queryByText('No hotspots found')).not.toBeInTheDocument()

      await act(async () => {
        if (completion === 'error')
          monthly.reject(new Error('Older request failed'))
        else monthly.resolve([])
      })

      expect(screen.queryByText('No hotspots found')).not.toBeInTheDocument()
      expect(screen.getByRole('table')).toBeInTheDocument()

      await act(async () => impact.resolve([]))
      expect(screen.getByRole('status')).toHaveTextContent('No hotspots found')
    }
  )

  it('ignores a response after the sidebar unmounts', async () => {
    const monthly = deferred<unknown[]>()
    mockFetchMonthlyHotspots.mockReturnValue(monthly.promise)

    const { unmount } = render(
      <HotspotSidebar handleRouteSelection={jest.fn()} />
    )
    unmount()
    await act(async () => monthly.resolve([{ id: 'stale-hotspot' }]))

    expect(useSpeedManagementStore.getState().hotspotRoutes).toEqual([])
  })

  it('keeps the current results when an older request finishes afterward', async () => {
    const monthly = deferred<unknown[]>()
    const impact = deferred<unknown[]>()
    const impactFeature = { id: 'impact-1', properties: { speedLimit: 55 } }
    mockFetchMonthlyHotspots.mockReturnValue(monthly.promise)
    mockFetchImpactHotspots.mockReturnValue(impact.promise)

    render(<HotspotSidebar handleRouteSelection={jest.fn()} />)
    selectEffectiveness()
    await act(async () => impact.resolve([impactFeature]))
    await act(async () => monthly.resolve([]))

    expect(useSpeedManagementStore.getState().hotspotRoutes).toEqual([
      expect.objectContaining({ id: 'impact-1', hotspotSource: 'impact' }),
    ])
    expect(screen.queryByText('No hotspots found')).not.toBeInTheDocument()
  })
})
