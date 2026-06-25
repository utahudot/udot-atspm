import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import { Box, Divider } from '@mui/material'

const DataQualityChartsContainer = ({ chartData }: { chartData: any }) => {
  if (!chartData?.charts) {
    return null
  }
  return (
    <Box sx={{ pt: 2 }}>
      {chartData?.charts.map((item, index) => (
        <Box key={index}>
          <Box key={index} sx={{ mx: 4 }}>
            <ApacheEChart
              id="congestion-chart"
              option={item.chart}
              style={{ width: '1100px', height: '550px' }}
              hideInteractionMessage
            />
          </Box>
          {index !== chartData.charts.length - 1 && <Divider sx={{ my: 4 }} />}
        </Box>
      ))}
    </Box>
  )
}

export default DataQualityChartsContainer
