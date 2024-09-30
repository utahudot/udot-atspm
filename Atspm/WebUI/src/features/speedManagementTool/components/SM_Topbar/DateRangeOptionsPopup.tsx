import OptionsPopupWrapper from '@/features/speedManagementTool/components/SM_Topbar/OptionsPopupWrapper'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { Box } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { format, isValid, parse } from 'date-fns'

export default function DateRangeOptionsPopup() {
  const { setRouteSpeedRequest, routeSpeedRequest } = useStore()

  const { startDate, endDate } = routeSpeedRequest

  // Parse the start and end dates
  const parsedStartDate = startDate
    ? parse(startDate, 'yyyy-MM-dd', new Date())
    : null
  const isStartDateValid = parsedStartDate ? isValid(parsedStartDate) : false

  const parsedEndDate = endDate
    ? parse(endDate, 'yyyy-MM-dd', new Date())
    : null
  const isEndDateValid = parsedEndDate ? isValid(parsedEndDate) : false

  const handleStartDateChange = (date: Date | null) => {
    if (date && isValid(date)) {
      const formattedDate = format(date, 'yyyy-MM-dd')
      setRouteSpeedRequest({
        ...routeSpeedRequest,
        startDate: formattedDate,
      })
    } else {
      setRouteSpeedRequest({
        ...routeSpeedRequest,
        startDate: '',
      })
    }
  }

  const handleEndDateChange = (date: Date | null) => {
    if (date && isValid(date)) {
      const formattedDate = format(date, 'yyyy-MM-dd')
      setRouteSpeedRequest({
        ...routeSpeedRequest,
        endDate: formattedDate,
      })
    } else {
      setRouteSpeedRequest({
        ...routeSpeedRequest,
        endDate: '',
      })
    }
  }

  // Generate the date range label
  const getDateRangeLabel = () => {
    let formattedStartDate = ''
    let formattedEndDate = ''

    if (isStartDateValid && parsedStartDate) {
      formattedStartDate = format(parsedStartDate, 'MMM d, yyyy')
    }

    if (isEndDateValid && parsedEndDate) {
      formattedEndDate = format(parsedEndDate, 'MMM d, yyyy')
    }

    if (formattedStartDate && formattedEndDate) {
      return `${formattedStartDate} - ${formattedEndDate}`
    } else if (formattedStartDate) {
      return `${formattedStartDate} -`
    } else if (formattedEndDate) {
      return `- ${formattedEndDate}`
    } else {
      return ''
    }
  }

  return (
    <OptionsPopupWrapper
      label="date-range"
      getLabel={getDateRangeLabel}
      width="255px"
    >
      <Box padding="10px">
        <Box display="flex" marginTop="10px">
          <Box padding="10px" paddingLeft="0px">
            <DatePicker
              label="Start"
              sx={{ width: '200px' }}
              value={isStartDateValid ? parsedStartDate : null}
              onChange={handleStartDateChange}
            />
          </Box>
          <Box display="flex" alignItems="center">
            -
          </Box>
          <Box padding="10px">
            <DatePicker
              label="End"
              sx={{ width: '200px' }}
              value={isEndDateValid ? parsedEndDate : null}
              onChange={handleEndDateChange}
            />
          </Box>
        </Box>
      </Box>
    </OptionsPopupWrapper>
  )
}
