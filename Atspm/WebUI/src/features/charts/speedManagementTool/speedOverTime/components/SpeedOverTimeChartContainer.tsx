import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import { Box } from '@mui/material'

const SpeedOverTimeChartContainer = ({
  chartData: chartOptions,
}: {
  chartData: number
}) => {
  const chartData = chartOptions.charts[0]
  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', pt: 2 }}>
      <ApacheEChart
        id="speed-over-time-chart"
        option={chartData}
        style={{ width: '1100px', height: '500px' }}
        hideInteractionMessage
      />
    </Box>
  )
}

export default SpeedOverTimeChartContainer
