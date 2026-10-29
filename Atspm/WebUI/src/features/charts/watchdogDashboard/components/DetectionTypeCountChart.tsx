import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import transformDetectionTypeCountData from '@/features/charts/watchdogDashboard/watchdogDetectionTypeCount.transformer'
import { DetectionTypeCount } from '@/features/watchdog/types'
import { Paper, useTheme } from '@mui/material'
import React, { useEffect, useMemo, useRef } from 'react'

interface DetectionTypeCountChartProps {
  data: DetectionTypeCount[]
  isLoading: boolean
}

const DetectionTypeCountChart: React.FC<DetectionTypeCountChartProps> = ({
  data,
  isLoading,
}) => {
  const theme = useTheme()
  const chartRef = useRef<any>(null)

  const detectionTypeCountChartOption = useMemo(() => {
    if (data) {
      return transformDetectionTypeCountData(data)
    }
    return null
  }, [data])

  useEffect(() => {
    const handleResize = () => {
      if (chartRef.current) {
        chartRef.current.getEchartsInstance().resize()
      }
    }

    window.addEventListener('resize', handleResize)

    return () => {
      window.removeEventListener('resize', handleResize)
    }
  }, [])

  return (
    <Paper elevation={3} sx={{ height: 375, padding: 1 }}>
      {!isLoading && detectionTypeCountChartOption && (
        <ApacheEChart
          theme={theme.palette.mode}
          ref={chartRef}
          id="watchdog-detection-type-count-chart"
          option={detectionTypeCountChartOption}
          loading={isLoading}
          style={{ width: '100%', height: 'calc(100% - 40px)' }}
          hideInteractionMessage
        />
      )}
    </Paper>
  )
}

export default DetectionTypeCountChart
