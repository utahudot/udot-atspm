import { SpeedViolationsOptions } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas.zod'
import CongestionTrackerChartsContainer from '@/features/charts/speedManagementTool/congestionTracker/components/CongestionTrackerChartsContainer'
import CongestionTrackingOptions, {
  CongestionTrackingOptionsValues,
} from '@/features/charts/speedManagementTool/congestionTracker/components/CongestionTrackingOptions'
import DataQualityChartContainer from '@/features/charts/speedManagementTool/dataQuality/components/DataQualityChartContainer'
import DataQualityChartOptions, {
  DataQualityChartOptionsValues,
} from '@/features/charts/speedManagementTool/dataQuality/components/DataQualityChartOptions'
import SpeedComplianceChartOptions, {
  SpeedComplianceChartOptionsValues,
} from '@/features/charts/speedManagementTool/speedCompliance/SpeedComplianceChartOptions'
import SpeedComplianceChartsContainer from '@/features/charts/speedManagementTool/speedCompliance/SpeedComplianceChartsContainer'
import SpeedOverDistanceOptions, {
  SpeedOverDistanceChartOptionsValues,
} from '@/features/charts/speedManagementTool/speedOverDistance/components/SpeedOverDistanceChartOptions'
import SpeedOverTimeChartContainer from '@/features/charts/speedManagementTool/speedOverTime/components/SpeedOverTimeChartContainer'
import SpeedOverTimeOptions, {
  SpeedOverTimeOptionsValues,
} from '@/features/charts/speedManagementTool/speedOverTime/components/SpeedOverTimeOptions'
import SpeedVariabilityChartContainer from '@/features/charts/speedManagementTool/speedVariability/components/SpeedVariabilityChartContainer'
import SpeedVariabilityOptions, {
  SpeedVariabilityOptionsValues,
} from '@/features/charts/speedManagementTool/speedVariability/components/SpeedVariabilityOptions'
import SpeedViolationsChartOptions from '@/features/charts/speedManagementTool/speedViolations/components/SpeedViolationsChartOptions'
import {
  SM_ChartType,
  useSMCharts,
} from '@/features/speedManagementTool/api/getSMCharts'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import { Box, Divider, List, ListItemButton, Typography } from '@mui/material'
import { useEffect, useMemo, useState } from 'react'

type ChartOptions =
  | CongestionTrackingOptionsValues
  | SpeedOverTimeOptionsValues
  | SpeedOverDistanceChartOptionsValues
  | DataQualityChartOptionsValues
  | SpeedVariabilityOptionsValues
  | SpeedViolationsOptions
  | null

const SM_Charts = ({ routes }: { routes: SpeedManagementRoute[] }) => {
  const [selectedChart, setSelectedChart] = useState<SM_ChartType | null>(null)
  const [chartOptions, setChartOptions] = useState<ChartOptions>(null)

  const { multiselect, routeSpeedRequest } = useSpeedManagementStore()

  const segmentIds = useMemo(
    () => routes.map((route) => route.properties.route_id),
    [routes]
  )

  const updatedChartOptions = chartOptions
    ? {
        ...chartOptions,
        ...(multiselect ||
        selectedChart === SM_ChartType.DATA_QUALITY ||
        selectedChart === SM_ChartType.SPEED_VIOLATIONS
          ? { segmentIds: segmentIds }
          : { segmentId: segmentIds[0] }),
      }
    : null

  const chartTypes: SM_ChartType[] = useMemo(() => {
    return multiselect
      ? [
          SM_ChartType.SPEED_OVER_DISTANCE,
          SM_ChartType.SPEED_COMPLIANCE,
          SM_ChartType.DATA_QUALITY,
          SM_ChartType.SPEED_VIOLATIONS,
        ]
      : [
          SM_ChartType.CONGESTION_TRACKING,
          SM_ChartType.SPEED_OVER_TIME,
          SM_ChartType.DATA_QUALITY,
          SM_ChartType.SPEED_VARIABILITY,
          SM_ChartType.SPEED_VIOLATIONS,
        ]
  }, [multiselect])

  useEffect(() => {
    // If selectedChart is null or not valid for the current number of routes, set a default
    if (selectedChart === null || !chartTypes.includes(selectedChart)) {
      setSelectedChart(chartTypes[0])
      setChartOptions(null)
    }
  }, [routes, selectedChart, chartTypes])

  const { data, isLoading, refetch } = useSMCharts(
    selectedChart,
    updatedChartOptions
  )

  const handleChartChange = (chartType: SM_ChartType) => {
    setSelectedChart(chartType)
    setChartOptions(null)
  }

  const handleOptionsChange = (options: ChartOptions) => {
    setChartOptions(options)
  }

  const handleRunChart = () => {
    refetch()
  }

  const renderOptionsComponent = () => {
    switch (selectedChart) {
      case SM_ChartType.CONGESTION_TRACKING:
        return (
          <CongestionTrackingOptions
            onOptionsChange={
              handleOptionsChange as (
                options: CongestionTrackingOptionsValues
              ) => void
            }
            sourceId={routeSpeedRequest.sourceId}
          />
        )
      case SM_ChartType.SPEED_OVER_TIME:
        return (
          <SpeedOverTimeOptions
            onOptionsChange={
              handleOptionsChange as (
                options: SpeedOverTimeOptionsValues
              ) => void
            }
            sourceId={routeSpeedRequest.sourceId}
          />
        )
      case SM_ChartType.SPEED_OVER_DISTANCE:
        return (
          <SpeedOverDistanceOptions
            onOptionsChange={
              handleOptionsChange as (
                options: SpeedOverDistanceChartOptionsValues
              ) => void
            }
          />
        )
      case SM_ChartType.SPEED_COMPLIANCE:
        return (
          <SpeedComplianceChartOptions
            onOptionsChange={
              handleOptionsChange as (
                options: SpeedComplianceChartOptionsValues
              ) => void
            }
          />
        )
      case SM_ChartType.DATA_QUALITY:
        return (
          <DataQualityChartOptions
            onOptionsChange={
              handleOptionsChange as (
                options: DataQualityChartOptionsValues
              ) => void
            }
          />
        )
      case SM_ChartType.SPEED_VARIABILITY:
        return (
          <SpeedVariabilityOptions
            onOptionsChange={
              handleOptionsChange as (
                options: SpeedVariabilityOptionsValues
              ) => void
            }
            sourceId={routeSpeedRequest.sourceId}
          />
        )
      case SM_ChartType.SPEED_VIOLATIONS:
        return (
          <SpeedViolationsChartOptions
            onOptionsChange={
              handleOptionsChange as (options: SpeedViolationsOptions) => void
            }
            sourceId={routeSpeedRequest.sourceId}
          />
        )
      default:
        return null
    }
  }

  const renderChartContainer = () => {
    if (isLoading) {
      return <Typography sx={{ ml: 2 }}>Loading...</Typography>
    }

    if (!data) return null

    switch (selectedChart) {
      case SM_ChartType.SPEED_VARIABILITY:
        return <SpeedVariabilityChartContainer chartData={data} />
      case SM_ChartType.CONGESTION_TRACKING:
        return <CongestionTrackerChartsContainer chartData={data} />
      case SM_ChartType.SPEED_OVER_TIME:
        return <SpeedOverTimeChartContainer chartData={data} />
      case SM_ChartType.SPEED_OVER_DISTANCE:
        return <SpeedOverTimeChartContainer chartData={data} />
      case SM_ChartType.SPEED_COMPLIANCE:
        return <SpeedComplianceChartsContainer chartData={data} />
      case SM_ChartType.DATA_QUALITY:
        return <DataQualityChartContainer chartData={data} />
      case SM_ChartType.SPEED_VIOLATIONS:
        return <DataQualityChartContainer chartData={data} />
      default:
        return null
    }
  }

  return (
    <>
      <Box display="flex" sx={{ py: 2, pl: 3, height: '100%' }}>
        <Box
          sx={{
            flexShrink: 0,
            minWidth: '300px',
            overflowY: 'auto',
            maxHeight: '100%',
          }}
        >
          <List>
            {chartTypes.map((chartType) => (
              <ListItemButton
                key={chartType}
                selected={selectedChart === chartType}
                onClick={() => handleChartChange(chartType)}
              >
                {chartType}
              </ListItemButton>
            ))}
          </List>
        </Box>
        <Divider orientation="vertical" flexItem sx={{ mx: 3 }} />
        <Box
          flex={1}
          sx={{
            mt: 2,
            minHeight: '270px',
            display: 'flex',
            flexDirection: 'column',
          }}
        >
          <Box sx={{ flexGrow: 1 }}>{renderOptionsComponent()}</Box>
          <Box sx={{ mt: 2 }}>
            <LoadingButton
              variant="contained"
              onClick={handleRunChart}
              startIcon={<PlayArrowIcon />}
              loading={isLoading}
            >
              Run Chart
            </LoadingButton>
          </Box>
        </Box>
        <Divider sx={{ my: 2 }} />
      </Box>
      <Divider />
      <Box sx={{ width: '100%', mt: 2 }}>{renderChartContainer()}</Box>
    </>
  )
}

export default SM_Charts
