import CongestionChart from '@/features/charts/speedManagementTool/congestionTracker/components/CongestionTrackerChart'
import { Box } from '@mui/material'

const CongestionTrackingChartsContainer = ({
  chartData,
}: {
  chartData: number
}) => {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', pt: 2 }}>
      <CongestionChart
        id="congestion-chart"
        option={chartData}
        style={{ width: '1100px', height: '550px' }}
        hideInteractionMessage
      />
    </Box>
  )
}

export default CongestionTrackingChartsContainer
