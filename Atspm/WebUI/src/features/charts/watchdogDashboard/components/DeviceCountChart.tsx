import ApacheEChart from '@/features/charts/components/apacheEChart/ApacheEChart'
import transformDeviceCountData from '@/features/charts/watchdogDashboard/deviceCount.transformer'
import { DeviceCount } from '@/features/watchdog/types'
import { Paper } from '@mui/material'
import React, { useMemo, useEffect, useRef } from 'react'

interface DeviceCountChartProps {
  data: DeviceCount[]
  isLoading: boolean
}

const DeviceCountChart: React.FC<DeviceCountChartProps> = ({
  data,
  isLoading,
}) => {
  const chartRef = useRef<any>(null)

  const deviceCountChartOption = useMemo(() => {
    if (data) {
      return transformDeviceCountData(data)
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
      {!isLoading && deviceCountChartOption && (
        <ApacheEChart
          ref={chartRef}
          id="watchdog-device-count-chart"
          option={deviceCountChartOption}
          loading={isLoading}
          style={{ width: '100%', height: 'calc(100% - 40px)' }}
          hideInteractionMessage
        />
      )}
    </Paper>
  )
}

export default DeviceCountChart