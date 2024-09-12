import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import {
  createDailyAverageChart,
  createMonthlyAverageChart,
} from '@/features/speedManagementTool/api/getChartData'
import { useHistoricalData } from '@/features/speedManagementTool/api/getHistoricalData'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { Box, Paper, Skeleton } from '@mui/material'

interface HistoricalDataParams {
  segmentId: string
  startDate: string
  endDate: string
  daysOfWeek: number[]
}

const ChartsContainer = ({ selectedRouteId }: { selectedRouteId: number }) => {
  const { submittedRouteSpeedRequest, routeRenderOption } = useStore()

  const params: HistoricalDataParams = {
    segmentId: selectedRouteId,
    startDate: submittedRouteSpeedRequest.startDate,
    endDate: submittedRouteSpeedRequest.endDate,
    daysOfWeek: submittedRouteSpeedRequest.daysOfWeek,
  }

  const { data, isLoading } = useHistoricalData({ params })

  return (
    <Box display="flex" flexDirection="row" gap={2} p={2}>
      {isLoading ? (
        <Skeleton variant="rectangular" width="100%" height={250} />
      ) : (
        <Paper
          sx={{
            p: 2,
            width: '100%',
          }}
        >
          <ApacheEChart
            id="monthly-data"
            option={createMonthlyAverageChart(
              data?.monthlyHistoricalRouteData,
              submittedRouteSpeedRequest.startDate,
              submittedRouteSpeedRequest.endDate,
              routeRenderOption
            )}
            style={{ height: '250px' }}
            hideInteractionMessage
          />
        </Paper>
      )}
      {isLoading ? (
        <Skeleton variant="rectangular" width="100%" height={250} />
      ) : (
        <Paper
          sx={{
            p: 2,
            width: '100%',
          }}
        >
          <ApacheEChart
            id="yearly-data"
            option={createDailyAverageChart(
              data?.dailyHistoricalRouteData,
              submittedRouteSpeedRequest.startDate,
              submittedRouteSpeedRequest.endDate,
              routeRenderOption
            )}
            style={{ height: '250px' }}
            hideInteractionMessage
          />
        </Paper>
      )}
    </Box>
  )
}

export default ChartsContainer
