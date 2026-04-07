import '@testing-library/jest-dom'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { TurningMovementCountsChartOptions } from './TurningMovementCountsChartOptions'

const setYAxisMaxStore = jest.fn()

jest.mock('@/stores/charts', () => ({
  __esModule: true,
  useChartsStore: () => ({
    setYAxisMaxStore,
    yAxisMaxStore: '300',
  }),
}))

const chartDefaults = {
  binSize: { id: 85, value: '15', option: 'binSize' },
  yAxisDefault: { id: 120, value: '300', option: 'yAxisDefault' },
  combineThruRight: {
    id: 121,
    value: 'FALSE',
    option: 'combineThruRight',
  },
}

describe('TurningMovementCountsChartOptions', () => {
  beforeEach(() => {
    setYAxisMaxStore.mockClear()
  })

  it('renders the combine checkbox in chart options and updates the option', async () => {
    const user = userEvent.setup()
    const handleChartOptionsUpdate = jest.fn()

    render(
      <TurningMovementCountsChartOptions
        chartDefaults={chartDefaults}
        handleChartOptionsUpdate={handleChartOptionsUpdate}
      />
    )

    expect(screen.getByText('Combine Thru and Thru-Right')).toBeInTheDocument()

    const checkbox = screen.getByRole('checkbox')
    expect(checkbox).not.toBeChecked()

    await user.click(checkbox)

    expect(handleChartOptionsUpdate).toHaveBeenCalledWith({
      id: 121,
      option: 'combineThruRight',
      value: 'TRUE',
    })
  })

  it('shows an error in measure default view when the combine default is missing', () => {
    render(
      <TurningMovementCountsChartOptions
        chartDefaults={{
          binSize: chartDefaults.binSize,
          yAxisDefault: chartDefaults.yAxisDefault,
        }}
        handleChartOptionsUpdate={jest.fn()}
        isMeasureDefaultView
      />
    )

    expect(
      screen.getByText(
        'A Combine Thru and Thru-Right value is not found for this Measure Default.'
      )
    ).toBeInTheDocument()
  })
})
