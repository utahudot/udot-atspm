import CongestionTrackerChartsContainer from '@/features/charts/congestionTracker/components/CongestionTrackerChartsContainer'
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
} from '@mui/material'
import { useState } from 'react'
import CongestionTrackingOptions, {
  CongestionTrackingOptionsValues,
} from './CongestionTrackingOptions'
// import SpeedOverTimeOptions from './SpeedOverTimeOptions';

enum SM_ChartType {
  CONGESTION_TRACKING = 'Congestion Tracking',
  SPEED_OVER_TIME = 'Speed over Time',
}

const SM_Charts = ({ route }: { route: SpeedManagementRoute }) => {
  const [selectedChart, setSelectedChart] = useState<SM_ChartType | null>(
    SM_ChartType.CONGESTION_TRACKING
  )
  const [chartOptions, setChartOptions] =
    useState<CongestionTrackingOptionsValues | null>(null)
  const [showChart, setShowChart] = useState(false)

  const handleChartChange = (event: SelectChangeEvent<SM_ChartType>) => {
    setSelectedChart(event.target.value as SM_ChartType)
    setChartOptions(null)
    setShowChart(false) // Reset the chart display when the chart type changes
  }

  const handleOptionsChange = (options: CongestionTrackingOptionsValues) => {
    setChartOptions(options)
  }

  const renderOptionsComponent = () => {
    switch (selectedChart) {
      case SM_ChartType.CONGESTION_TRACKING:
        return (
          <CongestionTrackingOptions onOptionsChange={handleOptionsChange} />
        )
      case SM_ChartType.SPEED_OVER_TIME:
        // return <SpeedOverTimeOptions onOptionsChange={handleOptionsChange} />;
        return null
      default:
        return null
    }
  }

  const renderChartContainer = () => {
    if (!showChart) return null

    switch (selectedChart) {
      case SM_ChartType.CONGESTION_TRACKING:
        return (
          <CongestionTrackerChartsContainer
            selectedRouteId={route.properties.route_id}
            options={chartOptions}
          />
        )
      case SM_ChartType.SPEED_OVER_TIME:
        // return <SpeedOverTimeChartsContainer options={chartOptions} />;
        return null
      default:
        return null
    }
  }

  const handleRunChart = () => {
    setShowChart(true)
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
