import React from 'react'
import { Box } from '@mui/material'
import ControllerTypeChart from './ControllerTypeChart'
import IssueTypeChart from './IssueTypeChart'
import DetectionTypeChart from './DetectionTypeChart'
import { WatchdogDashboardData } from '@/features/watchdog/types'

interface WatchdogChartsContainerProps {
  data: WatchdogDashboardData
  isLoading: boolean
}

const WatchdogChartsContainer: React.FC<WatchdogChartsContainerProps> = ({
  data,
  isLoading,
}) => {
console.log("TEST")
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
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
