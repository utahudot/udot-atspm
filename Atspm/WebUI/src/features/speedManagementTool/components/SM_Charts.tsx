import CongestionTrackerChartsContainer from '@/features/charts/speedManagementTool/congestionTracker/components/CongestionTrackerChartsContainer'
import SpeedOverTimeChartContainer from '@/features/charts/speedManagementTool/speedOverTime/components/SpeedOverTimeChartContainer'
import {
  SM_ChartType,
  useSMCharts,
} from '@/features/speedManagementTool/api/getSMCharts'
import SpeedOverTimeOptions from '@/features/speedManagementTool/components/SM_ChartOptions/SpeedOverTimeOptions'
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
import { useState } from 'react'
import CongestionTrackingOptions, {
  CongestionTrackingOptionsValues,
} from './SM_ChartOptions/CongestionTrackingOptions'

const SM_Charts = ({ route }: { route: SpeedManagementRoute }) => {
  const [selectedChart, setSelectedChart] = useState<SM_ChartType | null>(
    SM_ChartType.CONGESTION_TRACKING
  )
  const [chartOptions, setChartOptions] =
    useState<CongestionTrackingOptionsValues | null>(null)

  const handleChartChange = (event: SelectChangeEvent<SM_ChartType>) => {
    setSelectedChart(event.target.value as SM_ChartType)
    setChartOptions(null)
  }

  const handleOptionsChange = (options: CongestionTrackingOptionsValues) => {
    setChartOptions(options)
  }

  const { submittedRouteSpeedRequest } = useSpeedManagementStore()

  const { data, isLoading, refetch } = useSMCharts({
    chartType: selectedChart,
    chartOptions: {
      ...chartOptions,
      segmentId: route.properties.route_id,
      sourceId: submittedRouteSpeedRequest?.sourceId,
    },
  })

  const renderOptionsComponent = () => {
    switch (selectedChart) {
      case SM_ChartType.CONGESTION_TRACKING:
        return (
          <CongestionTrackingOptions onOptionsChange={handleOptionsChange} />
        )
      case SM_ChartType.SPEED_OVER_TIME:
        return <SpeedOverTimeOptions onOptionsChange={handleOptionsChange} />
      default:
        return null
    }
  }

  const handleRunChart = () => {
    refetch()
  }

  const renderChartContainer = () => {
    if (isLoading) {
      return <Typography sx={{ ml: 2 }}>Loading...</Typography>
    }

    if (!data) return null

    console.log('stuff', data)

    switch (selectedChart) {
      case SM_ChartType.CONGESTION_TRACKING:
        return <CongestionTrackerChartsContainer chartData={data} />
      case SM_ChartType.SPEED_OVER_TIME:
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
              value={selectedChart ?? ''}
              label="Chart Select"
              onChange={handleChartChange}
            >
              <MenuItem value={SM_ChartType.CONGESTION_TRACKING}>
                {SM_ChartType.CONGESTION_TRACKING}
              </MenuItem>
              <MenuItem value={SM_ChartType.SPEED_OVER_TIME}>
                {SM_ChartType.SPEED_OVER_TIME}
              </MenuItem>
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
              disabled={!selectedChart || !chartOptions}
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
