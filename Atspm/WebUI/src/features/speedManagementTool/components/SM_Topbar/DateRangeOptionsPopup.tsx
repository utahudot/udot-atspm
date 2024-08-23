import OptionsPopupWrapper from '@/features/speedManagementTool/components/SM_Topbar/OptionsPopupWrapper'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { Box, TextField } from '@mui/material'
import { format } from 'date-fns'
import { ChangeEvent } from 'react'

export default function DateRangeOptionsPopup() {
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

  const getDateRangeLabel = () => {
    const { startDate, endDate } = routeSpeedRequest
    const formattedStartDate = format(new Date(startDate), 'MMM d, yyyy')
    const formattedEndDate = format(new Date(endDate), 'MMM d, yyyy')
    return `${formattedStartDate} - ${formattedEndDate}`
  }

  return (
    <OptionsPopupWrapper label="date-range" getLabel={getDateRangeLabel}>
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
    </OptionsPopupWrapper>
  )
}
