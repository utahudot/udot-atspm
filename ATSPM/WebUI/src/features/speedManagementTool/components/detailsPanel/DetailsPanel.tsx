import { useCongestionTrackingChart } from '@/features/speedManagementTool/api/getCongestionTrackingData'
import { Box } from '@mui/material'
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
  return null
}
