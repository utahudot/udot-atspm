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

  const { data } = useHistoricalData({ params })

  if (!data) return null

  const monthlyOptions = createMonthlyAverageChart(
    data.monthlyHistoricalRouteData,
    getPriorDate(submittedRouteSpeedRequest.startDate),
    submittedRouteSpeedRequest.endDate,
    routeRenderOption
  )

  const yearlyOptions = createDailyAverageChart(
    data.dailyHistoricalRouteData,
    getPriorDate(submittedRouteSpeedRequest.startDate),
    submittedRouteSpeedRequest.endDate,
    routeRenderOption
  )

  return (
    <>
      <ApacheEChart
        id="monthly-data"
        option={monthlyOptions}
        style={{ height: '250px' }}
      />
      <ApacheEChart
        id="yearly-data"
        option={yearlyOptions}
        style={{ height: '250px' }}
      />
    </>
  )
}

export default ChartsContainer
