import CongestionChart from '@/features/charts/congestionTracker/components/CongestionTrackerChart'
import { useCongestionTrackingChart } from '@/features/speedManagementTool/api/getCongestionTrackingData'
import { Paper } from '@mui/material'
import { useEffect } from 'react'

const CongestionTrackingChartsContainer = ({
  selectedRouteId,
}: {
  selectedRouteId: number
}) => {
  const { data: congestionTrackingData, refetch: fetchCongestionTrackerData } =
    useCongestionTrackingChart({
      options: {
        segmentId: selectedRouteId.toString(),
        startDate: '2022-01-01T00:00:00',
        endDate: '2022-01-01T23:59:59',
      },
    })

  useEffect(() => {
    fetchCongestionTrackerData()
  }, [selectedRouteId, fetchCongestionTrackerData])

  if (!congestionTrackingData) return null

  return (
    // align in center
    <Paper sx={{ display: 'flex', justifyContent: 'center', pt: 2 }}>
      <CongestionChart
        id="congestion-chart"
        option={congestionTrackingData}
        style={{ width: '800px', height: '550px' }}
        hideInteractionMessage
      />
    </Paper>
  )
}

export default CongestionTrackingChartsContainer
