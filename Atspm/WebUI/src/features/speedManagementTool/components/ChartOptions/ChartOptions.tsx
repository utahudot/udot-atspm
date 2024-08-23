import { ChartType } from '@/features/charts/common/types'
import { CongestionTrackerChartOptions } from '@/features/charts/congestionTracker/components/CongestionTrackerChartOptions/CongestionTrackerChartOptions'
import { getDisplayNameFromChartType } from '@/features/charts/utils'
import {
  Box,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Typography,
} from '@mui/material'

const availableCharts = {
  CongestionTracker: CongestionTrackerChartOptions,
} as const

const ChartOptions = () => {
  const renderChartOptionsComponent = () => {
    //     const ChartComponent =
    //       availableCharts[chartType as keyof typeof availableCharts]

    //     if (!ChartComponent || !chartDefaults) return null

    //     if (isLoading) return <div>Loading...</div>

    return <CongestionTrackerChartOptions />
  }

  return (
    <>
      <FormControl fullWidth sx={{ mb: 1 }}>
        <InputLabel htmlFor="chart-type-label">Chart</InputLabel>
        <Select
          inputProps={{ id: 'chart-type-label' }}
          value={'Congestion Tracker'}
          label="Chart"
          // displayEmpty
          // disabled={!location}
        >
          {Object.keys(availableCharts).map((type) => (
            <MenuItem key={type} value={type}>
              {getDisplayNameFromChartType(type as ChartType)}
            </MenuItem>
          ))}
        </Select>
      </FormControl>
      <Box marginTop={1} minHeight={'32px'}>
        <Divider sx={{ mb: 2 }}>
          <Typography variant="caption">Options</Typography>
        </Divider>
        {renderChartOptionsComponent()}
      </Box>
    </>
  )
}

export default ChartOptions
