import { ChartType } from '@/features/charts/common/types'
import type { TransformedTurningMovementCountsResponse } from '@/features/charts/types'
import '@testing-library/jest-dom'
import { fireEvent, render, screen } from '@testing-library/react'
import type { EChartsOption } from 'echarts'
import TurningMovementCountsTable from './components/TurningMovementCountsTable'
import transformTmc from './turningMovementCounts.transformer'
import type {
  RawTurningMovementCountTableRow,
  RawTurningMovementCountsData,
  RawTurningMovementCountsResponse,
} from './types'

const chart = (
  overrides: Partial<RawTurningMovementCountsData> = {}
): RawTurningMovementCountsData => ({
  locationIdentifier: '1001',
  locationDescription: 'Main & State',
  start: '2026-04-01T08:00:00',
  end: '2026-04-01T09:00:00',
  direction: 'Northbound',
  movementType: 'Thru',
  laneType: 'Vehicle',
  plans: [],
  lanes: [{ laneNumber: 1, laneType: 1, movementType: 'Thru', volume: [] }],
  totalVolume: 100,
  peakHour: '08:00 - 09:00',
  peakHourVolume: 100,
  peakHourFactor: 1,
  laneUtilizationFactor: 1,
  ...overrides,
})

const row = (
  laneType: string,
  movementType: string,
  value: number,
  direction = 'Northbound'
): RawTurningMovementCountTableRow => ({
  direction,
  laneType,
  movementType,
  volumes: [{ timestamp: '2026-04-01T08:00:00', value }],
  peakHourVolume: { value },
})

function transform(
  table: RawTurningMovementCountTableRow[],
  charts = [chart()],
  peakHour: RawTurningMovementCountsResponse['data']['peakHour'] = {
    key: '2026-04-01T08:00:00',
    value: 100,
  }
) {
  return transformTmc({
    type: ChartType.TurningMovementCounts,
    data: { charts, table, peakHour, peakHourFactor: peakHour ? 1 : null },
  }) as TransformedTurningMovementCountsResponse
}

describe('TMC regression coverage', () => {
  afterEach(() => {
    jest.restoreAllMocks()
  })

  it('uses only vehicle rows for peak-hour values and headings, regardless of row order', () => {
    const data = transform([
      row('Unknown', 'Thru', 7),
      row('Vehicle', 'Thru', 100),
      row('Bike', 'Right', 9),
      row('Bike', 'Right', 20, 'Westbound'),
    ]).data
    expect(data.peakHour?.peakHourData).toEqual([
      ['08:00 - 09:00', 100, 100, 100],
    ])
    expect(data.labels.columnGroups).toEqual([
      { title: null, columns: ['Hour'] },
      { title: 'Northbound', columns: ['Thru', 'Total'] },
      { title: null, columns: ['Bin Total'] },
    ])
    expect(data.table).toHaveLength(4)
  })

  it('labels a nullable lane consistently in both the series and legend', () => {
    const result = transform(
      [row('Vehicle', 'Thru', 100)],
      [
        chart({
          lanes: [
            { laneNumber: null, laneType: 1, movementType: 'Thru', volume: [] },
          ],
          laneUtilizationFactor: null,
        }),
      ]
    )
    const option = result.data.charts[0].chart as EChartsOption
    expect(option.series).toEqual(
      expect.arrayContaining([
        expect.objectContaining({ name: 'Unassigned lane' }),
      ])
    )
    expect(option.legend).toEqual(
      expect.objectContaining({
        data: expect.arrayContaining([
          expect.objectContaining({ name: 'Unassigned lane' }),
        ]),
      })
    )
  })

  it('includes the peak date when the report spans multiple days', () => {
    const result = transform(
      [row('Vehicle', 'Thru', 100)],
      [chart({ end: '2026-04-02T09:00:00' })],
      { key: '2026-04-02T08:00:00', value: 100 }
    )
    expect(result.data.peakHour?.peakHourData[0][0]).toBe(
      '2026-04-02 08:00 - 2026-04-02 09:00'
    )
    expect(result.data.labels.flatColumns[0]).toBe('Date / Time')
  })

  it('renders and exports distinct dates for equal times on different days', async () => {
    const table = [row('Vehicle', 'Thru', 10)]
    table[0].volumes.push({ timestamp: '2026-04-02T08:00:00', value: 20 })
    const result = transform(
      table,
      [chart({ end: '2026-04-02T09:00:00' })],
      null
    )
    const createObjectURL = jest.fn<string, [Blob]>(() => 'blob:tmc-test')
    const revokeObjectURL = jest.fn()
    Object.defineProperty(URL, 'createObjectURL', {
      configurable: true,
      writable: true,
      value: createObjectURL,
    })
    Object.defineProperty(URL, 'revokeObjectURL', {
      configurable: true,
      writable: true,
      value: revokeObjectURL,
    })
    let filename = ''
    jest
      .spyOn(HTMLAnchorElement.prototype, 'click')
      .mockImplementation(function (this: HTMLAnchorElement) {
        filename = this.download
      })

    render(<TurningMovementCountsTable chartData={result} />)
    expect(screen.getByText('2026-04-01 08:00')).toBeInTheDocument()
    expect(screen.getByText('2026-04-02 08:00')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Download CSV' }))
    const blob = createObjectURL.mock.calls[0][0] as Blob
    const csv = await new Promise<string>((resolve) => {
      const reader = new FileReader()
      reader.onload = () => resolve(String(reader.result))
      reader.readAsText(blob)
    })
    expect(csv).toContain('Date / Time')
    expect(csv).toContain('2026-04-01 08:00,10,10,10')
    expect(csv).toContain('2026-04-02 08:00,20,20,20')
    expect(csv).toContain('Total,30,30,30')
    expect(filename).toContain('_Vehicle_split_split.csv')
    expect(revokeObjectURL).toHaveBeenCalledWith('blob:tmc-test')
  })
})
