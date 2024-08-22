import CongestionChart from '@/features/charts/congestionTracker/components/CongestionTrackerChart'
import { useCongestionTrackingChart } from '@/features/speedManagementTool/api/getCongestionTrackingData'
import { Box } from '@mui/material'
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

  // const { data: testData, refetch: fetchTestData } = useSpeedOverDistanceChart({
  //   options: {
  //     segmentIds: [selectedRouteId.toString()],
  //     startDate: '2024-02-01T00:00:00',
  //     endDate: '2024-08-01T23:59:59',
  //   },
  // })

  // const { data: testData, refetch: fetchTestData } = useSpeedOverTimeChart({
  //   options: {
  //     segmentId: selectedRouteId.toString(),
  //     startDate: '2024-05-01',
  //     endDate: '2024-05-10',
  //     startTime: '2024-05-01T06:00:00Z',
  //     endTime: '2024-05-10T09:00:00Z',
  //     daysOfWeek: [0],
  //     sourceId: 3,
  //     timeOptions: 0,
  //   },
  // })

  useEffect(() => {
    fetchCongestionTrackerData()
  }, [selectedRouteId, fetchCongestionTrackerData])

  if (!congestionTrackingData) return null

  return (
    // align in center
    <Box sx={{ display: 'flex', justifyContent: 'center', pt: 2 }}>
      <CongestionChart
        id="congestion-chart"
        option={congestionTrackingData}
        style={{ width: '1100px', height: '550px' }}
        hideInteractionMessage
      />
    </Box>
  )
}

export default CongestionTrackingChartsContainer
