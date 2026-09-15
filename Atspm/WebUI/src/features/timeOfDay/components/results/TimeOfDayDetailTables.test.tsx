import type { TimeOfDayMovementPressureDto } from '@/api/reports'
import { render, screen, within } from '@testing-library/react'
import { MovementPressureList } from './TimeOfDayDetailTables'

const movements = [
  {
    period: 'AM',
    locationIdentifier: '7621',
    movementLabel: 'Left',
    peakTime: '08:45',
    volume: 526,
  },
  {
    period: 'AM',
    locationIdentifier: '7621',
    movementLabel: 'Thru',
    peakTime: '08:45',
    volume: 2334,
  },
  {
    period: 'AM',
    locationIdentifier: '7621',
    movementLabel: 'Right',
    peakTime: '08:30',
    volume: 472,
  },
  {
    period: 'AM',
    locationIdentifier: '7016',
    movementLabel: 'Left',
    peakTime: '07:45',
    volume: 482,
  },
] as TimeOfDayMovementPressureDto[]

const renderList = () =>
  render(
    <MovementPressureList
      title="AM Movement Demand"
      period="AM"
      movements={movements}
      locationNumberMap={{ '7621': 1, '7016': 2 }}
      seriesVisible
      onSelectDetail={() => undefined}
      onSetSeriesVisibility={() => undefined}
    />
  )

describe('MovementPressureList', () => {
  test('states each location once and spans it across its movements', () => {
    renderList()
    const rows = within(
      screen.getByRole('table', { name: 'AM Movement Demand movement demand' })
    ).getAllByRole('row')

    expect(screen.getAllByText('7621')).toHaveLength(1)
    expect(screen.getAllByText('7016')).toHaveLength(1)
    expect(rows).toHaveLength(5)

    const [locationCell] = within(rows[1]).getAllByRole('cell')
    const locationLabelCell = within(rows[1]).getAllByRole('cell')[1]
    expect(locationCell.getAttribute('rowspan')).toBe('3')
    expect(locationLabelCell.getAttribute('rowspan')).toBe('3')
    expect(locationLabelCell.textContent).toBe('7621')
  })

  test('keeps every movement on its own row', () => {
    renderList()
    const rows = within(
      screen.getByRole('table', { name: 'AM Movement Demand movement demand' })
    ).getAllByRole('row')

    expect(within(rows[1]).getAllByRole('cell')).toHaveLength(5)
    expect(within(rows[2]).getAllByRole('cell')).toHaveLength(3)
    expect(within(rows[2]).getByText('Thru')).toBeTruthy()
    expect(within(rows[4]).getAllByRole('cell')).toHaveLength(5)
    expect(within(rows[4]).getByText('7016')).toBeTruthy()
  })
})
