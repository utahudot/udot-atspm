import Subtitle from '@/features/speedManagementTool/components/subtitle'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { Box, TextField } from '@mui/material'
import { ChangeEvent } from 'react'

export default function DateRangeOptions() {
  const { setRouteSpeedRequest, routeSpeedRequest } = useStore()

  const handleStartDateChange = (event: ChangeEvent<HTMLInputElement>) => {
    setRouteSpeedRequest({ ...routeSpeedRequest, start: event.target.value })
  }

  const handleEndDateChange = (event: ChangeEvent<HTMLInputElement>) => {
    setRouteSpeedRequest({ ...routeSpeedRequest, end: event.target.value })
  }

  return (
    <Box padding={'10px'}>
      <Box>
        <Subtitle>Date Range</Subtitle>
      </Box>
      <Box display="flex" marginTop={'10px'}>
        <Box padding={'10px'} paddingLeft={'0px'}>
          <TextField
            type="date"
            label="Start"
            value={routeSpeedRequest.start}
            onChange={handleStartDateChange}
          />
        </Box>
        <Box display={'flex'} alignItems={'center'}>
          –
        </Box>
        <Box padding={'10px'}>
          <TextField
            type="date"
            label="End"
            value={routeSpeedRequest.end}
            onChange={handleEndDateChange}
          />
        </Box>
      </Box>
    </Box>
  )
}
