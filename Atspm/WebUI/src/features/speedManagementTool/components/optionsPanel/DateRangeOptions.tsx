import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { Box, TextField } from '@mui/material'
import { ChangeEvent } from 'react'

export default function DateRangeOptions() {
  const { setRouteSpeedRequest, routeSpeedRequest } = useStore()

  const handleStartDateChange = (event: ChangeEvent<HTMLInputElement>) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      startDate: event.target.value,
    })
  }

  const handleEndDateChange = (event: ChangeEvent<HTMLInputElement>) => {
    setRouteSpeedRequest({ ...routeSpeedRequest, endDate: event.target.value })
  }

  return (
    <>
      <StyledComponentHeader header={'Date Range'} />
      <Box padding={'10px'}>
        <Box display="flex" marginTop={'10px'}>
          <Box padding={'10px'} paddingLeft={'0px'}>
            <TextField
              type="date"
              label="Start"
              value={routeSpeedRequest.startDate}
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
              value={routeSpeedRequest.endDate}
              onChange={handleEndDateChange}
            />
          </Box>
        </Box>
      </Box>
    </>
  )
}
