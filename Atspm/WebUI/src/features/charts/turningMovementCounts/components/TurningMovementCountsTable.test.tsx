import '@testing-library/jest-dom'
import { render } from '@testing-library/react'
import TurningMovementCountsTable from './TurningMovementCountsTable'

const filtersMock = jest.fn(() => <div>Filters</div>)

jest.mock('./TurningMovementCountsFilters', () => ({
  __esModule: true,
  default: (props: unknown) => filtersMock(props),
}))

jest.mock('./TurningMovementCountsResultsTable', () => ({
  __esModule: true,
  default: () => <div>Results Table</div>,
}))

jest.mock('./TurningMovementCountsTableToolbar', () => ({
  __esModule: true,
  default: () => <div>Toolbar</div>,
}))

describe('TurningMovementCountsTable', () => {
  beforeEach(() => {
    filtersMock.mockClear()
  })

  it('orders combined movements between thru and thru-right in the filters', () => {
    render(
      <TurningMovementCountsTable
        chartData={{
          data: {
            labels: {
              columnGroups: [{ title: null, columns: ['Hour'] }],
              flatColumns: ['Hour'],
            },
            peakHour: null,
            table: [
              {
                direction: 'Northbound',
                movementType: 'Right',
                laneType: 'Vehicle',
                volumes: [{ timestamp: '2026-04-01T08:00:00', value: 1 }],
              },
              {
                direction: 'Northbound',
                movementType: 'Thru-Right',
                laneType: 'Vehicle',
                volumes: [{ timestamp: '2026-04-01T08:00:00', value: 1 }],
              },
              {
                direction: 'Northbound',
                movementType: 'Thru + Thru-Right',
                laneType: 'Vehicle',
                volumes: [{ timestamp: '2026-04-01T08:00:00', value: 1 }],
              },
              {
                direction: 'Northbound',
                movementType: 'Thru',
                laneType: 'Vehicle',
                volumes: [{ timestamp: '2026-04-01T08:00:00', value: 1 }],
              },
              {
                direction: 'Northbound',
                movementType: 'Left',
                laneType: 'Vehicle',
                volumes: [{ timestamp: '2026-04-01T08:00:00', value: 1 }],
              },
            ],
          },
        }}
      />
    )

    const filterProps = filtersMock.mock.calls[0][0] as {
      movementOptions: { value: string; label: string }[]
    }

    expect(filterProps.movementOptions.map((option) => option.value)).toEqual([
      'Left',
      'Thru',
      'Thru + Thru-Right',
      'Thru-Right',
      'Right',
    ])
    expect(filterProps.movementOptions[2]).toEqual({
      value: 'Thru + Thru-Right',
      label: 'Thru + Thru-Right',
    })
  })
})
