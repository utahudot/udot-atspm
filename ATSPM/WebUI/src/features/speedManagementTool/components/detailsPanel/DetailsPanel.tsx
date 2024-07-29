import ApacheEChart from '@/features/charts/components/apacheEChart'
import { useCongestionTrackingChart } from '@/features/speedManagementTool/api/getCongestionTrackingData'
import { Box } from '@mui/material'
import { EChartsOption, time } from 'echarts'
import RoutesToggle from './RoutesToggle'
import ViolationRangeInput from './ViolationRangeSlider'

function SidePanel({ selectedRouteId }: { selectedRouteId: number }) {
  return (
    <div id="sidepanel-container">
      <RoutesToggle />
      <ViolationRangeInput />
      <hr />
      {selectedRouteId ? (
        <Box sx={{ height: '500px', overflowY: 'auto' }}>
          <CongestionTrackingChartsContainer
            selectedRouteId={selectedRouteId}
          />
        </Box>
      ) : null}
    </div>
  )
}

export default SidePanel

const CongestionTrackingChartsContainer = ({
  selectedRouteId,
}: {
  selectedRouteId: number
}) => {
  const { data: congestionTrackingData } = useCongestionTrackingChart({
    options: {
      segmentId: selectedRouteId.toString(),
      startDate: '2022-01-01T00:00:00',
      endDate: '2022-01-01T23:59:59',
    },
  })

  if (!congestionTrackingData?.data?.data) return null

  const cellSize = [80, 80]

  const transformData = (data) => {
    return data.map((item) => {
      const date = item.date.split('T')[0]
      const averageSeries = item.series.average.map((s) => [
        time.parse(s.start),
        s.value,
      ])
      const eightyFifthSeries = item.series.eightyFifth.map((s) => [
        time.parse(s.start),
        s.value,
      ])
      return { date, averageSeries, eightyFifthSeries }
    })
  }

  const scatterData = transformData(congestionTrackingData.data.data)

  const lineSeries = scatterData.flatMap((item, index) => [
    {
      type: 'scatter',
      id: `scatter-average-${index}`,
      coordinateSystem: 'calendar',
      calendarIndex: 0,
      data: item.averageSeries.map(([date, value]) => ({
        coord: [date, value],
      })),
      symbolSize: 8,
      name: `Average ${item.date}`,
    },
    {
      type: 'scatter',
      id: `scatter-eightyFifth-${index}`,
      coordinateSystem: 'calendar',
      calendarIndex: 0,
      data: item.eightyFifthSeries.map(([date, value]) => ({
        coord: [date, value],
      })),
      symbolSize: 8,
      name: `EightyFifth ${item.date}`,
    },
  ])

  const option: EChartsOption = {
    tooltip: {},
    legend: {
      data: ['Average', 'EightyFifth'],
      bottom: 20,
    },
    calendar: {
      top: 'middle',
      left: 'center',
      orient: 'vertical',
      cellSize: cellSize,
      yearLabel: { show: false },
      dayLabel: {
        margin: 20,
        firstDay: 1,
        nameMap: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
      },
      monthLabel: { show: false },
      range: ['2022-01'],
    },
    series: [
      {
        id: 'label',
        type: 'scatter',
        coordinateSystem: 'calendar',
        symbolSize: 0,
        label: {
          show: true,
          formatter: (params) => time.format(params.value[0], '{dd}', false),
          offset: [-cellSize[0] / 2 + 10, -cellSize[1] / 2 + 10],
          fontSize: 14,
        },
        data: scatterData.map((item) => [item.date]),
      },
      ...lineSeries,
    ],
  }

  console.log('scatterData', option)

  return (
    <ApacheEChart
      id="calendar-chart"
      option={option}
      // chartType={ChartType.}
      style={{ width: '100%', height: '600px' }}
      theme="light"
    />
  )
}
