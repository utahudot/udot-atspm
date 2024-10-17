import { Box } from '@mui/material'
import { DateTimePicker } from '@mui/x-date-pickers'

interface SelectDateTimeProps {
  startDateTime: Date
  endDateTime: Date
  dateFormat?: string
  changeStartDate(date: Date): void
  changeEndDate(date: Date): void
}
const HorizontalDateInput = ({
  startDateTime,
  endDateTime,
  changeStartDate,
  changeEndDate,
  dateFormat = 'MMM dd, yyyy',
}: SelectDateTimeProps) => {
  const handleStartDateTimeChange = (date: Date | null) => {
    if (!date) return
    changeStartDate(date)
  }

  const handleEndDateTimeChange = (date: Date | null) => {
    if (!date) return
    changeEndDate(date)
  }

  return (
    <>
      <Box sx={{ display: 'flex', gap: 2 }}>
        <DateTimePicker
          sx={{ width: '100%' }}
          value={startDateTime}
          onChange={handleStartDateTimeChange}
          label="Start"
          format={dateFormat}
          disableFuture={true}
          minutesStep={1}
        />
        <DateTimePicker
          sx={{ width: '100%' }}
          value={endDateTime}
          onChange={handleEndDateTimeChange}
          label="End"
          format={dateFormat}
          disableFuture={true}
          minutesStep={1}
        />
      </Box>
    </>
  )
}

export default HorizontalDateInput
