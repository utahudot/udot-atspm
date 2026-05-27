import '@testing-library/jest-dom'
import { render } from '@testing-library/react'
import TurningMovementCountsTable, {
  buildTurningMovementCountsCsvFilename,
} from './TurningMovementCountsTable'

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

  it('builds CSV export names with chart-style location and date context', () => {
    expect(
      buildTurningMovementCountsCsvFilename(
        'Turning_Movement_Counts_1001_2026-04-01_08-00_to_2026-04-01_09-00',
        'Vehicle',
        'split',
        'combine'
      )
    ).toBe(
      'Turning_Movement_Counts_1001_2026-04-01_08-00_to_2026-04-01_09-00_Vehicle_split_combine.csv'
    )
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
