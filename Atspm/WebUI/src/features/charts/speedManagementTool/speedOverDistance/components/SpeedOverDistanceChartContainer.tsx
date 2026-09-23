import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import {
  SM_ChartType,
  SMChartsDataMapping,
} from '@/features/speedManagementTool/api/getSMCharts'
import { Box } from '@mui/material'

const SpeedOverDistanceChartContainer = ({
  chartData: chartOptions,
}: {
  chartData: SMChartsDataMapping[SM_ChartType.SPEED_OVER_DISTANCE]
}) => {
  const chartData = chartOptions
  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', pt: 2 }}>
      <ApacheEChart
        id="speed-over-distance-chart"
        option={chartData}
        style={{ width: '1100px', height: '500px' }}
        hideInteractionMessage
      />
    </Box>
  )
}

export default SpeedOverDistanceChartContainer
