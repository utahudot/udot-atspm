import CongestionTrackerChartsContainer from '@/features/charts/speedManagementTool/congestionTracker/components/CongestionTrackerChartsContainer'
import CongestionTrackingOptions, {
  CongestionTrackingOptionsValues,
} from '@/features/charts/speedManagementTool/congestionTracker/components/CongestionTrackingOptions'
import SpeedOverDistanceOptions, {
  SpeedOverDistanceOptionsValues,
} from '@/features/charts/speedManagementTool/speedOverDistance/components/SpeedOverDistanceChartOptions'
import SpeedOverTimeChartContainer from '@/features/charts/speedManagementTool/speedOverTime/components/SpeedOverTimeChartContainer'
import SpeedOverTimeOptions, {
  SpeedOverTimeOptionsValues,
} from '@/features/charts/speedManagementTool/speedOverTime/components/SpeedOverTimeOptions'
import {
  SM_ChartType,
  useSMCharts,
} from '@/features/speedManagementTool/api/getSMCharts'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import {
  Box,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  SelectChangeEvent,
  Typography,
} from '@mui/material'
import { useEffect, useMemo, useState } from 'react'

type ChartOptions =
  | CongestionTrackingOptionsValues
  | SpeedOverTimeOptionsValues
  | SpeedOverDistanceOptionsValues
  | null

const SM_Charts = ({ routes }: { routes: SpeedManagementRoute[] }) => {
  const [selectedChart, setSelectedChart] = useState<SM_ChartType | null>(null)
  const [chartOptions, setChartOptions] = useState<ChartOptions>(null)

  const { multiselect } = useSpeedManagementStore()

  const segmentIds = useMemo(
    () => routes.map((route) => route.properties.route_id),
    [routes]
  )

  const updatedChartOptions = chartOptions
    ? {
        ...chartOptions,
        ...(multiselect
          ? { segmentIds: segmentIds }
          : { segmentId: segmentIds[0] }),
      }
    : null

  const chartTypes: SM_ChartType[] = useMemo(() => {
    return routes.length === 1
      ? [SM_ChartType.CONGESTION_TRACKING, SM_ChartType.SPEED_OVER_TIME]
      : [SM_ChartType.SPEED_OVER_DISTANCE]
  }, [routes.length])

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

  const handleChartChange = (event: SelectChangeEvent<SM_ChartType>) => {
    setSelectedChart(event.target.value as SM_ChartType)
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
          />
        )
      case SM_ChartType.SPEED_OVER_DISTANCE:
        return (
          <SpeedOverDistanceOptions
            onOptionsChange={
              handleOptionsChange as (
                options: SpeedOverDistanceOptionsValues
              ) => void
            }
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
      case SM_ChartType.CONGESTION_TRACKING:
        return <CongestionTrackerChartsContainer chartData={data} />
      case SM_ChartType.SPEED_OVER_TIME:
        return <SpeedOverTimeChartContainer chartData={data} />
      case SM_ChartType.SPEED_OVER_DISTANCE:
        return <SpeedOverTimeChartContainer chartData={data} />
      default:
        return null
    }
  }

  return (
    <>
      <Box
        display="flex"
        sx={{ py: 2, pl: 3, backgroundColor: 'background.default' }}
      >
        <Box
          sx={{
            flexShrink: 0,
            minWidth: '300px',
            mt: 2,
          }}
        >
          <FormControl fullWidth>
            <InputLabel id="chart-select-label">Chart Select</InputLabel>
            <Select
              labelId="chart-select-label"
              id="chart-select"
              value={selectedChart || ''}
              label="Chart Select"
              onChange={handleChartChange}
            >
              {chartTypes.map((chartType) => (
                <MenuItem key={chartType} value={chartType}>
                  {chartType}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Box>
        <Divider orientation="vertical" flexItem sx={{ mx: 3 }} />
        <Box flex={1} sx={{ mt: 2 }}>
          {renderOptionsComponent()}
          <Box sx={{ flexShrink: 0 }}>
            <LoadingButton
              variant="contained"
              onClick={handleRunChart}
              sx={{ mt: 2 }}
              startIcon={<PlayArrowIcon />}
              disabled={
                !selectedChart || (routes.length === 1 && !chartOptions)
              }
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
