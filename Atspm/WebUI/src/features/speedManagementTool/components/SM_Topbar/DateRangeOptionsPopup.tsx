import OptionsPopupWrapper from '@/features/speedManagementTool/components/SM_Topbar/OptionsPopupWrapper'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { Box, FormHelperText } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers'
import { endOfMonth, format, isValid, parse, subMonths } from 'date-fns'

export default function DateRangeOptionsPopup() {
  const { setRouteSpeedRequest, routeSpeedRequest } = useStore()
  const { startDate, endDate } = routeSpeedRequest

  // Parse start and end dates into valid month-year format
  const parsedStartDate = startDate
    ? parse(startDate, 'yyyy-MM-dd', new Date())
    : null
  const parsedEndDate = endDate
    ? parse(endDate, 'yyyy-MM-dd', new Date())
    : null

  const isStartDateValid = parsedStartDate ? isValid(parsedStartDate) : false
  const isEndDateValid = parsedEndDate ? isValid(parsedEndDate) : false

  const handleStartDateChange = (date: Date | null) => {
    if (date && isValid(date)) {
      const formattedDate = format(date, 'yyyy-MM-01') // Set to the first day of the month
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

  const handleEndDateChange = (date: Date) => {
    const endOfMonthDate = endOfMonth(date)
    if (date && isValid(date)) {
      const formattedDate = format(endOfMonthDate, 'yyyy-MM-dd') // Set to the first day of the month
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

  const getDateRangeLabel = () => {
    const formattedStartDate = isStartDateValid
      ? format(parsedStartDate!, 'MMM yyyy')
      : ''
    const formattedEndDate = isEndDateValid
      ? format(parsedEndDate!, 'MMM yyyy')
      : ''

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
        <Box display="flex" marginTop="10px" alignItems="center">
          <Box padding="10px" paddingLeft="0px">
            <DatePicker
              label="Start month"
              views={['month', 'year']}
              format="MMM yyyy"
              value={isStartDateValid ? parsedStartDate : null}
              onChange={handleStartDateChange}
              sx={{ width: '200px' }}
            />
          </Box>

          <Box display="flex" alignItems="center">
            -
          </Box>
          <Box padding="10px">
            <DatePicker
              label="End month"
              views={['month', 'year']}
              format="MMM yyyy"
              value={isEndDateValid ? parsedEndDate : null}
              onChange={handleEndDateChange}
              sx={{ width: '200px' }}
              // disable current month
              maxDate={subMonths(new Date(), 1)}
            />
          </Box>
        </Box>
        <FormHelperText sx={{ mt: -0.5 }}>
          Includes every month from the start through the end month (inclusive).
          <br />
          Only complete months can be selected.
        </FormHelperText>
      </Box>
    </OptionsPopupWrapper>
  )
}
