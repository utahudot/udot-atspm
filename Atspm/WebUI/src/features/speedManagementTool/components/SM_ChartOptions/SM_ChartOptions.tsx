import { ChartType } from '@/features/charts/common/types'
import { getDisplayNameFromChartType } from '@/features/charts/utils'
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
import CongestionTrackerChartOptions from './CongestionTrackingOptions'
import SpeedOverTimeOptions from './SpeedOverTimeOptions'

const availableCharts = {
  CongestionTracker: CongestionTrackerChartOptions,
  SpeedOverTime: SpeedOverTimeOptions,
} as const

const SM_ChartOptions = () => {
  const [selectedChartType, setSelectedChartType] = useState<ChartType | null>(
    null
  )

  const handleChartTypeChange = (event: SelectChangeEvent<string>) => {
    setSelectedChartType(event.target.value as ChartType)
  }

  const renderChartOptionsComponent = () => {
    const ChartComponent =
      selectedChartType &&
      availableCharts[selectedChartType as keyof typeof availableCharts]

    if (!ChartComponent) return null

    return <ChartComponent />
  }

  return (
    <>
      <FormControl fullWidth sx={{ mb: 1 }}>
        <InputLabel htmlFor="chart-type-label">Chart</InputLabel>
        <Select
          inputProps={{ id: 'chart-type-label' }}
          value={selectedChartType ?? ''}
          label="Chart"
          onChange={handleChartTypeChange}
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

export default SM_ChartOptions
