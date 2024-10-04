import ApacheEChart from '@/features/charts/components/apacheEChart'
import DailySpeedViolationsTable from '@/features/charts/speedManagementTool/speedViolations/components/SpeedViolationsTable'
import { ExtendedEChartsOption } from '@/features/charts/types'
import { Box } from '@mui/material'

const SpeedViolationsChartContainer = ({
  chartData,
}: {
  chartData: ExtendedEChartsOption
}) => {
  return (
    <Box>
      {chartData.charts.map((chart, i) => (
        <Box key={i}>
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'center',
              alignContent: 'center',
              pt: 2,
            }}
          >
            <ApacheEChart
              id="speed-Violations-chart"
              option={chart.chart}
              style={{ width: '1100px', height: '500px' }}
              hideInteractionMessage
            />
          </Box>
          <DailySpeedViolationsTable response={chart.chart.response} />
        </Box>
      ))}
    </Box>
  )
}

export default SpeedViolationsChartContainer
