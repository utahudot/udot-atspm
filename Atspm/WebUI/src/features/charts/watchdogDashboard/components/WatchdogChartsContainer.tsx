import { WatchdogDashboardData } from '@/features/watchdog/types'
import { Box } from '@mui/material'
import React from 'react'
import ControllerTypeChart from './ControllerTypeChart'
import DetectionTypeChart from './DetectionTypeChart'
import DetectionTypeCountChart from './DetectionTypeCountChart'
import DeviceCountChart from './DeviceCountChart'
import IssueTypeChart from './IssueTypeChart'

interface WatchdogChartsContainerProps {
  data: WatchdogDashboardData
  isLoading: boolean
}

const WatchdogChartsContainer: React.FC<WatchdogChartsContainerProps> = ({
  data,
  isLoading,
}) => {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <Box sx={{ display: 'flex', gap: 3 }}>
        <Box sx={{ flex: 1 }}>
          <DeviceCountChart data={data?.deviceCount} isLoading={isLoading} />
        </Box>
        <Box sx={{ flex: 1 }}>
          <DetectionTypeCountChart
            data={data?.detectionTypeCount}
            isLoading={isLoading}
          />
        </Box>
      </Box>
      <Box>
        <ControllerTypeChart
          data={data?.controllerTypeGroup}
          isLoading={isLoading}
        />
      </Box>
      <Box>
        <IssueTypeChart data={data?.issueTypeGroup} isLoading={isLoading} />
      </Box>
      <Box>
        <DetectionTypeChart
          data={data?.detectionTypeGroup}
          isLoading={isLoading}
        />
      </Box>
    </Box>
  )
}

export default WatchdogChartsContainer
