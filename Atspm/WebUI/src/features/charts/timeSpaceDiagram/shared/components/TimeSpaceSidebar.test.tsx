import { act, fireEvent, render, screen } from '@testing-library/react'
import type { EChartsOption } from 'echarts'
import { createDefaultTimeSpaceAppearanceSettings } from '../timeSpaceAppearance'
import TimeSpaceSidebar from './TimeSpaceSidebar'

function createSeries(name: string) {
  return {
    name,
    type: 'line' as const,
    data: [[0, 0]],
  }
}

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
    series: [
      createSeries('Cycles EB'),
      createSeries('Cycles WB'),
      createSeries('Green Bands EB'),
      createSeries('Green Bands WB'),
      createSeries('TSP Request (112-115)'),
    ],
  }
}

function buildLocationPhaseLabelOption(): EChartsOption {
  return {
    legend: {
      data: [
        'Cycles NBT ph2',
        'Cycles SBT ph6',
        'Green Bands NBT ph2',
        'Green Bands SBT ph6',
      ],
      selected: {
        'Cycles NBT ph2': true,
        'Cycles SBT ph6': true,
        'Green Bands NBT ph2': true,
        'Green Bands SBT ph6': true,
      },
    },
    series: [
      createSeries('Cycles NBT ph2'),
      createSeries('Cycles SBT ph6'),
      createSeries('Green Bands NBT ph2'),
      createSeries('Green Bands SBT ph6'),
    ],
  }
}

function buildSrmOption(): EChartsOption {
  return {
    legend: {
      data: ['Cycles EB', 'SRM Collection EB', 'SRM Estimated Trajectory EB'],
      selected: {
        'Cycles EB': true,
        'SRM Collection EB': true,
        'SRM Estimated Trajectory EB': true,
      },
    },
  }
}

function buildTransitNoDataOption(): EChartsOption {
  return {
    legend: {
      data: [
        'Cycles EB',
        'Early Green (113)',
        'Extend Green (114)',
        'TSP Request (112-115)',
        'TSP Service (118-119)',
      ],
      selected: {
        'Cycles EB': true,
        'Early Green (113)': false,
        'Extend Green (114)': false,
        'TSP Request (112-115)': false,
        'TSP Service (118-119)': false,
      },
    },
  }
}

function buildTurnOption(): EChartsOption {
  return {
    legend: {
      data: ['Cycles EB', 'Left Turn EB', 'Right Turn EB'],
      selected: {
        'Cycles EB': true,
        'Left Turn EB': false,
        'Right Turn EB': false,
      },
    },
    series: [
      createSeries('Cycles EB'),
      createSeries('Left Turn EB'),
      createSeries('Right Turn EB'),
    ],
  }
}

function buildPedestrianNoDataOption(): EChartsOption {
  return {
    legend: {
      data: ['Cycles EB', 'Pedestrian Interval EB'],
      selected: {
        'Cycles EB': true,
        'Pedestrian Interval EB': true,
      },
    },
  }
}

function buildGpxOption(): EChartsOption {
  return {
    legend: {
      data: ['Cycles EB', 'GPX Tracks'],
      selected: {
        'Cycles EB': true,
        'GPX Tracks': true,
      },
    },
    series: [
      {
        name: 'GPX Tracks',
        type: 'line',
        data: [
          [0, 0],
          [1, 1],
        ],
      },
    ],
  }
}

function buildGpxNoDataOption(): EChartsOption {
  return {
    legend: {
      data: ['Cycles EB', 'GPX Tracks'],
      selected: {
        'Cycles EB': true,
        'GPX Tracks': true,
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

  it('toggles all legend sections from the global section control', () => {
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

    fireEvent.click(
      screen.getByRole('button', { name: 'Collapse all legend sections' })
    )

    expect(
      screen.getByRole('button', { name: 'Expand all legend sections' })
    ).not.toBeNull()
    expect(
      screen.getByRole('button', { name: 'Expand Signal Timing' })
    ).not.toBeNull()
    expect(
      screen.getByRole('button', { name: 'Expand Transit Priority' })
    ).not.toBeNull()

    fireEvent.click(
      screen.getByRole('button', { name: 'Expand all legend sections' })
    )

    expect(
      screen.getByRole('button', { name: 'Collapse Signal Timing' })
    ).not.toBeNull()
    expect(
      screen.getByRole('button', { name: 'Collapse Transit Priority' })
    ).not.toBeNull()
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

  it('explains that SRM Collection requires uploaded SRM data with an info tooltip', async () => {
    render(
      <TimeSpaceSidebar
        option={buildSrmOption()}
        selectedSeries={{
          'Cycles EB': true,
          'SRM Collection EB': true,
          'SRM Estimated Trajectory EB': true,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={jest.fn()}
        onToggleDirectionVisibility={jest.fn()}
        showTabs={false}
      />
    )

    const infoIcon = screen.getByLabelText('SRM Collection info')

    expect(infoIcon).not.toBeNull()
    expect(screen.queryByRole('alert')).toBeNull()
    expect(
      screen.queryByLabelText('SRM Collection unavailable')
    ).toBeNull()
    expect(window.getComputedStyle(infoIcon).color).toBe('rgb(148, 163, 184)')
    expect(
      window.getComputedStyle(
        screen
          .getByText('SRM Collection')
          .closest('.MuiPaper-root') as HTMLElement
      ).opacity
    ).toBe('0.6')

    fireEvent.mouseOver(infoIcon)

    expect(
      await screen.findByText(
        'Upload SRM data from the Uploads tab to show these tracks.'
      )
    ).not.toBeNull()
  })

  it('shows no-data tooltips instead of toggles when transit data is missing', async () => {
    render(
      <TimeSpaceSidebar
        option={buildTransitNoDataOption()}
        selectedSeries={{
          'Cycles EB': true,
          'Early Green (113)': false,
          'Extend Green (114)': false,
          'TSP Request (112-115)': false,
          'TSP Service (118-119)': false,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={jest.fn()}
        onToggleDirectionVisibility={jest.fn()}
        showTabs={false}
      />
    )

    expect(
      screen.getByLabelText('TSP Request unavailable')
    ).not.toBeNull()
    expect(
      screen.getByLabelText('TSP Service unavailable')
    ).not.toBeNull()
    expect(
      screen.getByLabelText('Transit Priority unavailable')
    )
    expect(
      window.getComputedStyle(
        screen.getByLabelText('Transit Priority unavailable')
      ).color
    ).toBe('rgb(148, 163, 184)')
    expect(
      screen.queryByRole('checkbox', {
        name: 'Toggle TSP Request',
      })
    ).toBeNull()
    expect(
      screen.queryByRole('checkbox', { name: 'Toggle all Transit Priority' })
    ).toBeNull()

    fireEvent.mouseOver(screen.getByLabelText('Transit Priority unavailable'))

    expect(
      await screen.findByText('No transit-signal priority data found.')
    ).not.toBeNull()

    fireEvent.mouseOver(screen.getByLabelText('Early Green unavailable'))

    expect(await screen.findByText('No early greens found.')).not.toBeNull()
  })

  it('renders a GPX Tracks legend item when GPX overlay data is available', () => {
    render(
      <TimeSpaceSidebar
        option={buildGpxOption()}
        selectedSeries={{
          'Cycles EB': true,
          'GPX Tracks': true,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={jest.fn()}
        onToggleDirectionVisibility={jest.fn()}
        showTabs={false}
      />
    )

    expect(screen.getByText('GPX Tracks')).not.toBeNull()
    expect(
      screen.getByText(
        'Uploaded GPX traces mapped to the corridor and drawn through the corridor.'
      )
    ).not.toBeNull()
    expect(
      screen.getByRole('checkbox', { name: 'Toggle GPX Tracks' })
    ).not.toBeNull()
  })

  it('shows GPX Tracks guidance in an info tooltip until GPX upload data is present', async () => {
    render(
      <TimeSpaceSidebar
        option={buildGpxNoDataOption()}
        selectedSeries={{
          'Cycles EB': true,
          'GPX Tracks': true,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={jest.fn()}
        onToggleDirectionVisibility={jest.fn()}
        showTabs={false}
      />
    )

    expect(screen.getByText('GPX Tracks')).not.toBeNull()
    expect(screen.getByLabelText('GPX Tracks info')).not.toBeNull()
    expect(screen.queryByRole('alert')).toBeNull()
    expect(screen.queryByLabelText('GPX Tracks unavailable')).toBeNull()
    expect(
      window.getComputedStyle(
        screen.getByText('GPX Tracks').closest('.MuiPaper-root') as HTMLElement
      ).opacity
    ).toBe('0.6')
    expect(
      screen.queryByRole('checkbox', { name: 'Toggle GPX Tracks' })
    ).toBeNull()

    fireEvent.mouseOver(screen.getByLabelText('GPX Tracks info'))

    expect(
      await screen.findByText(
        'Upload GPX data from the Uploads tab to show these tracks.'
      )
    ).not.toBeNull()
  })

  it('shows a GPX Tracks toggle when uploaded GPX data is available', () => {
    render(
      <TimeSpaceSidebar
        option={buildGpxNoDataOption()}
        selectedSeries={{
          'Cycles EB': true,
          'GPX Tracks': true,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={jest.fn()}
        onToggleDirectionVisibility={jest.fn()}
        gpxTracksAvailable={true}
        showTabs={false}
      />
    )

    expect(screen.getByText('GPX Tracks')).not.toBeNull()
    expect(
      screen.getByRole('checkbox', { name: 'Toggle GPX Tracks' })
    ).not.toBeNull()
    expect(screen.queryByLabelText('GPX Tracks info')).toBeNull()
    expect(screen.queryByLabelText('GPX Tracks unavailable')).toBeNull()
  })

  it('shows generic no-data language for other legend items when data is missing', async () => {
    render(
      <TimeSpaceSidebar
        option={buildPedestrianNoDataOption()}
        selectedSeries={{
          'Cycles EB': true,
          'Pedestrian Interval EB': true,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={jest.fn()}
        onToggleDirectionVisibility={jest.fn()}
        showTabs={false}
      />
    )

    const unavailableIcon = screen.getByLabelText('Pedestrian Interval unavailable')

    expect(unavailableIcon).not.toBeNull()
    expect(
      screen.queryByRole('checkbox', { name: 'Toggle Pedestrian Interval' })
    ).toBeNull()

    fireEvent.mouseOver(unavailableIcon)

    expect(
      await screen.findByText('No pedestrian intervals found.')
    ).not.toBeNull()
  })

  it('shows the styles tab to the right of uploads when appearance controls are available', () => {
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
        appearanceSettings={createDefaultTimeSpaceAppearanceSettings()}
        onAppearanceChange={jest.fn()}
        uploadContent={<div>Uploads go here</div>}
      />
    )

    expect(screen.getByRole('tab', { name: 'Legend' })).not.toBeNull()
    expect(screen.getByRole('tab', { name: 'Styles' })).not.toBeNull()
    expect(screen.getByRole('tab', { name: 'Uploads' })).not.toBeNull()
    expect(screen.getAllByRole('tab').map((tab) => tab.textContent)).toEqual([
      'Legend',
      'Uploads',
      'Styles',
    ])
  })

  it('updates appearance settings from the styles tab controls', () => {
    jest.useFakeTimers()
    const onAppearanceChange = jest.fn()

    try {
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
          appearanceSettings={createDefaultTimeSpaceAppearanceSettings()}
          onAppearanceChange={onAppearanceChange}
          activeTab="styles"
          showTabs={false}
        />
      )

      fireEvent.change(screen.getByLabelText('Cycles begin green color'), {
        target: { value: '#123456' },
      })

      expect(onAppearanceChange).not.toHaveBeenCalled()

      act(() => {
        jest.advanceTimersByTime(150)
      })

      expect(onAppearanceChange).toHaveBeenCalledWith(expect.any(Function))

      const updater = onAppearanceChange.mock.calls[0][0] as (
        current: ReturnType<typeof createDefaultTimeSpaceAppearanceSettings>
      ) => ReturnType<typeof createDefaultTimeSpaceAppearanceSettings>
      const nextAppearance = updater(createDefaultTimeSpaceAppearanceSettings())

      expect(nextAppearance.cycles.indicationColors.beginGreen).toBe('#123456')
    } finally {
      jest.useRealTimers()
    }
  })

  it('uses primary and opposing labels in the styles tab instead of raw phase names', () => {
    render(
      <TimeSpaceSidebar
        option={buildLocationPhaseLabelOption()}
        selectedSeries={{
          'Cycles NBT ph2': true,
          'Cycles SBT ph6': true,
          'Green Bands NBT ph2': true,
          'Green Bands SBT ph6': true,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={jest.fn()}
        onToggleDirectionVisibility={jest.fn()}
        appearanceSettings={createDefaultTimeSpaceAppearanceSettings()}
        onAppearanceChange={jest.fn()}
        activeTab="styles"
        showTabs={false}
      />
    )

    expect(screen.getByText('primary')).not.toBeNull()
    expect(screen.getByText('opposing')).not.toBeNull()
    expect(screen.queryByText('NBT ph2')).toBeNull()
    expect(screen.queryByText('SBT ph6')).toBeNull()
  })

  it('shows transit priority style controls even when transit data is unavailable', () => {
    render(
      <TimeSpaceSidebar
        option={buildTransitNoDataOption()}
        selectedSeries={{
          'Cycles EB': true,
          'Early Green (113)': false,
          'Extend Green (114)': false,
          'TSP Request (112-115)': false,
          'TSP Service (118-119)': false,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={jest.fn()}
        onToggleDirectionVisibility={jest.fn()}
        appearanceSettings={createDefaultTimeSpaceAppearanceSettings()}
        onAppearanceChange={jest.fn()}
        activeTab="styles"
        showTabs={false}
      />
    )

    expect(screen.getByText('Transit Priority')).not.toBeNull()
    expect(screen.getByText('TSP Request')).not.toBeNull()
    expect(screen.getByText('TSP Service')).not.toBeNull()
  })

  it('renders the TSP Service legend preview as a hollow interval', () => {
    const appearanceSettings = createDefaultTimeSpaceAppearanceSettings()
    appearanceSettings.tspService.color = '#123456'
    appearanceSettings.tspService.opacity = 0.42

    render(
      <TimeSpaceSidebar
        option={buildTransitNoDataOption()}
        selectedSeries={{
          'Cycles EB': true,
          'Early Green (113)': false,
          'Extend Green (114)': false,
          'TSP Request (112-115)': false,
          'TSP Service (118-119)': false,
        }}
        suppressedDirections={{}}
        onSetSeriesVisibility={jest.fn()}
        onToggleDirectionVisibility={jest.fn()}
        appearanceSettings={appearanceSettings}
        showTabs={false}
      />
    )

    const tspServiceCard = screen
      .getByText('TSP Service')
      .closest('.MuiPaper-root') as HTMLElement
    const tspServicePreview = tspServiceCard.querySelector(
      'rect[x="12"][y="25"]'
    )

    expect(tspServicePreview?.getAttribute('fill')).toBe('transparent')
    expect(tspServicePreview?.getAttribute('stroke')).toBe('#123456')
    expect(tspServicePreview?.getAttribute('stroke-opacity')).toBe('0.42')
  })

  it('updates turn appearance settings from the styles tab controls', () => {
    jest.useFakeTimers()
    const onAppearanceChange = jest.fn()

    try {
      render(
        <TimeSpaceSidebar
          option={buildTurnOption()}
          selectedSeries={{
            'Cycles EB': true,
            'Left Turn EB': false,
            'Right Turn EB': false,
          }}
          suppressedDirections={{}}
          onSetSeriesVisibility={jest.fn()}
          onToggleDirectionVisibility={jest.fn()}
          appearanceSettings={createDefaultTimeSpaceAppearanceSettings()}
          onAppearanceChange={onAppearanceChange}
          activeTab="styles"
          showTabs={false}
        />
      )

      expect(screen.getByText('Turns')).not.toBeNull()

      fireEvent.change(screen.getByLabelText('Left Turn color'), {
        target: { value: '#222222' },
      })

      act(() => {
        jest.advanceTimersByTime(150)
      })

      const updater = onAppearanceChange.mock.calls[0][0] as (
        current: ReturnType<typeof createDefaultTimeSpaceAppearanceSettings>
      ) => ReturnType<typeof createDefaultTimeSpaceAppearanceSettings>
      const nextAppearance = updater(createDefaultTimeSpaceAppearanceSettings())

      expect(nextAppearance.turns.leftTurn.color).toBe('#222222')
    } finally {
      jest.useRealTimers()
    }
  })
})
