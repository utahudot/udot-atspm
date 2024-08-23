import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import {
  createDailyAverageChart,
  createMonthlyAverageChart,
} from '@/features/speedManagementTool/api/getChartData'
import {
  getPriorDate,
  useHistoricalData,
} from '@/features/speedManagementTool/api/getHistoricalData'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { Box, Skeleton } from '@mui/material'

interface HistoricalDataParams {
  routeId: string
  startDate: string
  endDate: string
  startTime: string
  endTime: string
  daysOfWeek: string
  percentile?: number
}

const ChartsContainer = ({ selectedRouteId }: { selectedRouteId: number }) => {
  const { submittedRouteSpeedRequest, routeRenderOption } = useStore()

  const params: HistoricalDataParams = {
    routeId: selectedRouteId,
    startDate: submittedRouteSpeedRequest.startDate,
    endDate: submittedRouteSpeedRequest.endDate,
    startTime: '00:00',
    endTime: '23:00',
    daysOfWeek: submittedRouteSpeedRequest.daysOfWeek.join(','),
  }

  switch (routeRenderOption) {
    case RouteRenderOption.Percentile_85th:
      params.percentile = 85
      break
    case RouteRenderOption.Percentile_95th:
      params.percentile = 95
      break
    case RouteRenderOption.Percentile_99th:
      params.percentile = 99
  }

  const { data, isLoading } = useHistoricalData({ params })

  return (
    <Box display="flex" flexDirection="row" gap={2} p={2}>
      {isLoading ? (
        <Skeleton variant="rectangular" width="100%" height={250} />
      ) : (
        <ApacheEChart
          id="monthly-data"
          option={createMonthlyAverageChart(
            data?.monthlyHistoricalRouteData,
            getPriorDate(submittedRouteSpeedRequest.startDate),
            submittedRouteSpeedRequest.endDate,
            routeRenderOption
          )}
          style={{ height: '250px' }}
          hideInteractionMessage
        />
      )}
      {isLoading ? (
        <Skeleton variant="rectangular" width="100%" height={250} />
      ) : (
        <ApacheEChart
          id="yearly-data"
          option={createDailyAverageChart(
            data?.dailyHistoricalRouteData,
            getPriorDate(submittedRouteSpeedRequest.startDate),
            submittedRouteSpeedRequest.endDate,
            routeRenderOption
          )}
          style={{ height: '250px' }}
          hideInteractionMessage
        />
      )}
    </Box>
  )
}

export default ChartsContainer
