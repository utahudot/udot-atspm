import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import transformDetectionTypeData from '@/features/charts/watchdogDashboard/watchDogDetectionType.transformer'
import { Paper } from '@mui/material'
import React, { useMemo } from 'react'

interface DetectionTypeChartProps {
  data: any // Replace 'any' with the correct type for your data
  isLoading: boolean
}

const DetectionTypeChart: React.FC<DetectionTypeChartProps> = ({
  data,
  isLoading,
}) => {
  const detectionTypeChartOption = useMemo(() => {
    if (data) {
      return transformDetectionTypeData(data)
    }
    return null
  }, [data])

  return (
    <Paper elevation={3} sx={{ height: 600, padding: 1 }}>
      <div style={{ display: 'flex', justifyContent: 'center', width: '100%' }}>
        <h2>Detection Type Breakdown</h2>
      </div>
 
      {!isLoading && detectionTypeChartOption && (
        <ApacheEChart
          id="watchdog-detection-type-chart"
          option={detectionTypeChartOption}
          loading={isLoading}
          style={{ width: '100%', height: 'calc(100% - 80px)' }}
          hideInteractionMessage
        />
      )}
    
    </Paper>
  )
}

export default DetectionTypeChart
