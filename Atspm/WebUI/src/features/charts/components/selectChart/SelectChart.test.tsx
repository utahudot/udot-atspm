import { render } from '@testing-library/react'
import { ChartType } from '@/features/charts/common/types'
import SelectChart from './SelectChart'

const defaults = { value: [
  { chartType: ChartType.ApproachVolume, measureOptions: { binSize: { value: 60 } } },
  { chartType: ChartType.TurningMovementCounts, measureOptions: { binSize: { value: 15 }, yAxisDefault: { value: 300 } } },
] }
jest.mock('@/features/charts/api/getChartDefaults', () => ({ useChartDefaults: () => ({ data: defaults, isLoading: false }) }))
jest.mock('@/features/charts/api/getMeasureTypes', () => ({ useGetMeasureTypes: () => ({ data: { value: [] } }) }))
jest.mock('../../turningMovementCounts/useTmcSources', () => ({ useTmcSources: () => ({ sources: [], loading: false }) }))
jest.mock('@/features/charts/turningMovementCounts/components/TurningMovementCountsChartOptions', () => ({ TurningMovementCountsChartOptions: () => null }))
jest.mock('@/features/charts/approachVolume/components/ApproachVolumeChartOptions', () => ({ ApproachVolumeChartOptions: () => null }))

test('switching from another chart to TMC uses TMC defaults', () => {
  let options = { binSize: 60, yAxisDefault: 999 }
  const update = jest.fn((next) => { options = typeof next === 'function' ? next(options) : next })
  const props = { location: null, setChartType: jest.fn(), setChartOptions: update }
  const page = render(<SelectChart {...props} chartType={ChartType.ApproachVolume} />)
  options = { binSize: 60, yAxisDefault: 999 }
  page.rerender(<SelectChart {...props} chartType={ChartType.TurningMovementCounts} />)
  expect(options).toEqual({ binSize: 15, yAxisDefault: 300 })
})

test('initial TMC URL options are retained', () => {
  let options = { binSize: 30, source: 'devices', deviceIds: [42] }
  const update = jest.fn((next) => { options = typeof next === 'function' ? next(options) : next })
  render(<SelectChart location={null} setChartType={jest.fn()} setChartOptions={update} chartType={ChartType.TurningMovementCounts} />)
  expect(options).toEqual({ binSize: 30, source: 'devices', deviceIds: [42], yAxisDefault: 300 })
})
