import { act, render, screen } from '@testing-library/react'
import { init as initECharts } from 'echarts'
import TimeOfDayEChart from './TimeOfDayEChart'

const mockChart = {
  dispatchAction: jest.fn(),
  dispose: jest.fn(),
  getDataURL: jest.fn(),
  off: jest.fn(),
  on: jest.fn(),
  resize: jest.fn(),
  setOption: jest.fn(),
}

jest.mock('echarts', () => ({
  ...jest.requireActual('echarts'),
  init: jest.fn(() => mockChart),
}))

describe('TimeOfDayEChart resizing', () => {
  let resizeCallback: ResizeObserverCallback
  const observe = jest.fn()
  const disconnect = jest.fn()

  beforeAll(() => {
    class MockResizeObserver implements ResizeObserver {
      constructor(callback: ResizeObserverCallback) {
        resizeCallback = callback
      }

      observe = observe
      unobserve = jest.fn()
      disconnect = disconnect
    }

    global.ResizeObserver = MockResizeObserver
  })

  beforeEach(() => {
    jest.clearAllMocks()
  })

  test('resizes synchronously to each distinct observed size and cleans up', () => {
    const { unmount } = render(
      <TimeOfDayEChart
        option={{}}
        selectedSeries={{}}
        showPercentAxis={false}
        onSelectDetail={jest.fn()}
        onToggleScheduleView={jest.fn()}
      />
    )
    const chartElement = screen.getByRole('img', {
      name: 'Corridor time-of-day analysis chart',
    })

    Object.defineProperties(chartElement, {
      clientWidth: { configurable: true, value: 640 },
      clientHeight: { configurable: true, value: 840 },
    })

    expect(observe).toHaveBeenCalledWith(chartElement)
    act(() => resizeCallback([], {} as ResizeObserver))
    act(() => resizeCallback([], {} as ResizeObserver))

    expect(mockChart.resize).toHaveBeenCalledTimes(1)
    expect(mockChart.resize).toHaveBeenLastCalledWith({
      width: 640,
      height: 840,
      animation: { duration: 0 },
      silent: true,
    })

    Object.defineProperty(chartElement, 'clientWidth', {
      configurable: true,
      value: 650,
    })
    act(() => resizeCallback([], {} as ResizeObserver))

    expect(mockChart.resize).toHaveBeenCalledTimes(2)
    expect(mockChart.resize).toHaveBeenLastCalledWith({
      width: 650,
      height: 840,
      animation: { duration: 0 },
      silent: true,
    })

    unmount()
    expect(disconnect).toHaveBeenCalledTimes(1)
    expect(mockChart.dispose).toHaveBeenCalledTimes(1)

    Object.defineProperty(chartElement, 'clientWidth', {
      configurable: true,
      value: 700,
    })
    window.dispatchEvent(new Event('resize'))
    expect(mockChart.resize).toHaveBeenCalledTimes(2)

    expect(initECharts).toHaveBeenCalledTimes(1)
  })
})
