import {
  usePostApiV1SpeedManagementGetDailyHistoricalSpeeds,
  usePostApiV1SpeedManagementGetMonthlyHistoricalSpeeds,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import { createAverageChart } from '@/features/speedManagementTool/api/getChartData'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { toUTCDateStamp } from '@/utils/dateTime'
import { Box, Skeleton } from '@mui/material'
import { addMonths } from 'date-fns'
import { EChartsOption } from 'echarts'
import { useEffect, useState } from 'react'

const SM_GeneralCharts = ({ selectedRouteId }: { selectedRouteId: string }) => {
  const [dailyTrends, setDailyTrends] = useState<EChartsOption>()
  const [monthlyTrends, setMonthlyTrends] = useState<EChartsOption>()
  const [isLoading, setIsLoading] = useState(true)
  const { submittedRouteSpeedRequest, routeRenderOption } = useStore()

  const { mutateAsync: getDailyTrends } =
    usePostApiV1SpeedManagementGetDailyHistoricalSpeeds()
  const { mutateAsync: getMonthlyTrends } =
    usePostApiV1SpeedManagementGetMonthlyHistoricalSpeeds()

  useEffect(() => {
    const fetchData = async () => {
      const params = {
        segmentId: selectedRouteId,
        startDate: submittedRouteSpeedRequest.startDate || '',
        endDate: submittedRouteSpeedRequest.endDate || '',
      }

      setIsLoading(true)
      const responseDaily = await getDailyTrends({ data: params })
      const monthlyTrendsParams = {
        segmentId: selectedRouteId,
        startDate:
          toUTCDateStamp(
            addMonths(new Date(submittedRouteSpeedRequest.startDate), -6)
          ) || '',
        endDate:
          toUTCDateStamp(
            addMonths(new Date(submittedRouteSpeedRequest.startDate), 6)
          ) || '',
      }
      const responseMonthly = await getMonthlyTrends({
        data: monthlyTrendsParams,
      })
      const dailyTrendsOption = createAverageChart(
        responseDaily,
        submittedRouteSpeedRequest.startDate || '',
        submittedRouteSpeedRequest.endDate || '',
        routeRenderOption,
        'daily'
      )
      const monthlyTrendsOption = createAverageChart(
        responseMonthly,
        submittedRouteSpeedRequest.startDate || '',
        submittedRouteSpeedRequest.endDate || '',
        routeRenderOption,
        'monthly'
      )
      setDailyTrends(dailyTrendsOption)
      setMonthlyTrends(monthlyTrendsOption)
    }

    fetchData()
    setIsLoading(false)
  }, [
    getDailyTrends,
    getMonthlyTrends,
    selectedRouteId,
    submittedRouteSpeedRequest,
    routeRenderOption,
  ])

  if (isLoading || !dailyTrends || !monthlyTrends) {
    return <Skeleton variant="rectangular" width="100%" height={315} />
  }

  return (
    <Box display="flex" flexDirection="row" gap={2} p={1}>
      <ApacheEChart
        id="daily-trends"
        option={dailyTrends}
        style={{ height: '250px' }}
        hideInteractionMessage
      />
      <ApacheEChart
        id="monthly-trends"
        option={monthlyTrends}
        style={{ height: '250px' }}
        hideInteractionMessage
      />
    </Box>
  )
}

export default SM_GeneralCharts
