import { ChartType } from '@/features/charts/common/types'
import CongestionTrackerChartOptions from '@/features/charts/speedManagementTool/congestionTracker/components/CongestionTrackingOptions'
import DataQualityChartOptions from '@/features/charts/speedManagementTool/dataQuality/components/DataQualityChartOptions'
import SpeedOverDistanceOptions from '@/features/charts/speedManagementTool/speedOverDistance/components/SpeedOverDistanceChartOptions'
import SpeedOverTimeOptions from '@/features/charts/speedManagementTool/speedOverTime/components/SpeedOverTimeOptions'
import { getDisplayNameFromChartType } from '@/features/charts/utils'
import { SM_ChartType } from '@/features/speedManagementTool/api/getSMCharts'
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

const availableCharts = {
  CongestionTracker: CongestionTrackerChartOptions,
  SpeedOverTime: SpeedOverTimeOptions,
  SpeedOverDistance: SpeedOverDistanceOptions,
  DataQuality: DataQualityChartOptions,
} as const

const SM_ChartOptions = () => {
  const [selectedChartType, setSelectedChartType] =
    useState<SM_ChartType | null>(null)

  const handleChartTypeChange = (event: SelectChangeEvent<string>) => {
    setSelectedChartType(event.target.value as SM_ChartType)
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
