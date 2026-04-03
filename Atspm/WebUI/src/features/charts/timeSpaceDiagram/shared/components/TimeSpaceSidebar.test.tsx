import { fireEvent, render, screen } from '@testing-library/react'
import type { EChartsOption } from 'echarts'
import TimeSpaceSidebar from './TimeSpaceSidebar'

function buildOption(): EChartsOption {
  return {
    legend: {
      data: [
        'Cycles EB',
        'Cycles WB',
        'Green Bands EB',
        'Green Bands WB',
        'TSP Request (112-115)',
      ],
      selected: {
        'Cycles EB': true,
        'Cycles WB': false,
        'Green Bands EB': true,
        'Green Bands WB': true,
        'TSP Request (112-115)': true,
      },
    },
  }
}

describe('TimeSpaceSidebar directional controls', () => {
  it('renders compact global direction buttons without the directions header', () => {
    render(
      <TimeSpaceSidebar
        option={buildOption()}
        selectedSeries={{
          'Cycles EB': true,
          'Cycles WB': false,
          'Green Bands EB': true,
          'Green Bands WB': true,
          'TSP Request (112-115)': true,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={jest.fn()}
        onToggleDirectionVisibility={jest.fn()}
        showTabs={false}
      />
    )

    expect(screen.queryByText('Directions')).toBeNull()
    expect(
      screen.getByRole('checkbox', { name: 'Toggle primary direction' })
    ).not.toBeNull()
    expect(
      screen.getByRole('checkbox', { name: 'Toggle opposing direction' })
    ).not.toBeNull()
  })

  it('toggles the primary direction from the global direction control', () => {
    const onSetSeriesVisibility = jest.fn()
    const onToggleDirectionVisibility = jest.fn()

    render(
      <TimeSpaceSidebar
        option={buildOption()}
        selectedSeries={{
          'Cycles EB': true,
          'Cycles WB': false,
          'Green Bands EB': true,
          'Green Bands WB': true,
          'TSP Request (112-115)': true,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={onSetSeriesVisibility}
        onToggleDirectionVisibility={onToggleDirectionVisibility}
        showTabs={false}
      />
    )

    fireEvent.click(
      screen.getByRole('checkbox', { name: 'Toggle primary direction' })
    )

    expect(onToggleDirectionVisibility).toHaveBeenCalledWith('primary')
    expect(onSetSeriesVisibility).not.toHaveBeenCalled()
  })

  it('toggles a single item direction independently with the inline P or O button', () => {
    const onSetSeriesVisibility = jest.fn()

    render(
      <TimeSpaceSidebar
        option={buildOption()}
        selectedSeries={{
          'Cycles EB': true,
          'Cycles WB': false,
          'Green Bands EB': true,
          'Green Bands WB': true,
          'TSP Request (112-115)': true,
        }}
        suppressedDirections={{ primary: true }}
        onSetSeriesVisibility={onSetSeriesVisibility}
        onToggleDirectionVisibility={jest.fn()}
        showTabs={false}
      />
    )

    fireEvent.click(
      screen.getByRole('checkbox', {
        name: 'Toggle opposing direction for Cycles',
      })
    )

    expect(onSetSeriesVisibility).toHaveBeenCalledWith(['Cycles WB'], true)
  })

  it('hides the section checkbox when a category only has one item', () => {
    render(
      <TimeSpaceSidebar
        option={buildOption()}
        selectedSeries={{
          'Cycles EB': true,
          'Cycles WB': false,
          'Green Bands EB': true,
          'Green Bands WB': true,
          'TSP Request (112-115)': true,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={jest.fn()}
        onToggleDirectionVisibility={jest.fn()}
        showTabs={false}
      />
    )

    expect(
      screen.getByRole('checkbox', { name: 'Toggle all Signal Timing' })
    ).not.toBeNull()
    expect(
      screen.queryByRole('checkbox', { name: 'Toggle all Transit Priority' })
    ).toBeNull()
  })
})
