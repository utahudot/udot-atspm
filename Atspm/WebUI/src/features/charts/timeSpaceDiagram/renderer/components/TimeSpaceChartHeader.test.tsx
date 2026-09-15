import { fireEvent, render, screen } from '@testing-library/react'
import { createRef } from 'react'
import { TimeSpaceChartHeader } from './TimeSpaceChartHeader'

describe('TimeSpaceChartHeader route orientation', () => {
  it('exposes an accessible pressed-state route flip control', () => {
    const onToggleRouteOrientation = jest.fn()
    const commonProps = {
      hasStyleContent: false,
      hasUploadContent: false,
      headerRef: createRef<HTMLDivElement>(),
      isFullscreen: false,
      isGuideCollapsed: true,
      onDownloadChart: jest.fn(),
      onResetChart: jest.fn(),
      onSidebarTabChange: jest.fn(),
      onToggleFullscreen: jest.fn(),
      onToggleGuide: jest.fn(),
      onTogglePhaseInfo: jest.fn(),
      onToggleRouteOrientation,
      distanceSpacingMode: 'distance' as const,
      rangeText: '',
      showPhaseInfo: true,
      sidebarTab: 'legend' as const,
      titleText: 'Time Space Diagram',
    }

    const { rerender } = render(
      <TimeSpaceChartHeader {...commonProps} routeOrientation={'configured'} />
    )

    const flipButton = screen.getByRole('button', {
      name: 'Reverse route direction',
    })
    expect(flipButton.getAttribute('aria-pressed')).toBe('false')

    fireEvent.click(flipButton)
    expect(onToggleRouteOrientation).toHaveBeenCalledTimes(1)

    rerender(
      <TimeSpaceChartHeader {...commonProps} routeOrientation={'reversed'} />
    )
    expect(
      screen
        .getByRole('button', {
          name: 'Restore configured route direction',
        })
        .getAttribute('aria-pressed')
    ).toBe('true')
  })
})
