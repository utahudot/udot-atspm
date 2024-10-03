import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import { Box, TextField } from '@mui/material'
import { useEffect, useState } from 'react'
import transformEffectivenessOfStrategiesData from './effectivenessOfStrategies.transformer'

const EffectivenessOfStrategiesChartsContainer = ({
  chartData,
}: {
  chartData: any
}) => {
  const [customSpeedLimit, setCustomSpeedLimit] = useState<number | null>(null)
  const [transformedChartData, setTransformedChartData] =
    useState<any>(chartData)

  const handleSpeedLimitChange = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const value = event.target.value ? parseInt(event.target.value) : null
    setCustomSpeedLimit(value)
  }

  // Recalculate the chart data whenever customSpeedLimit or chartData changes
  useEffect(() => {
    if (customSpeedLimit !== null) {
      setTransformedChartData(
        transformEffectivenessOfStrategiesData(
          chartData.response,
          customSpeedLimit
        )
      )
    } else {
      setTransformedChartData(chartData) // Reset to the original data if no custom speed limit
    }
  }, [customSpeedLimit, chartData])

  const tableData = [0, 0, 0, 0]

  return (
    <>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mr: 8 }}>
        <TextField
          label="Custom Speed Limit"
          type="number"
          value={customSpeedLimit !== null ? customSpeedLimit : ''}
          onChange={handleSpeedLimitChange}
          InputProps={{ inputProps: { min: 0 } }} // Optional: Prevent negative numbers
          variant="outlined"
          sx={{ width: 200 }}
        />
      </Box>
      <Box sx={{ display: 'flex', justifyContent: 'center', pt: 2 }}>
        <ApacheEChart
          id="speed-over-time-chart"
          option={transformedChartData}
          style={{ width: '1100px', height: '500px' }}
          hideInteractionMessage
        />
      </Box>
    </>
  )
}

export default EffectivenessOfStrategiesChartsContainer
