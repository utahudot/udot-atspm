import SelectDateTime from '@/components/selectTimeSpan'
import { Box, FormHelperText } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers'
import { ReportDateTimeHandler } from '../ExportableReports/components/handlers/ReportDateTimeHandler'

interface Props {
  handler: ReportDateTimeHandler
  isMonthly: boolean
}

export default function DateTimeOptions(props: Props) {
  const { handler, isMonthly } = props
  // return (
  //   <SelectDateTime
  //     dateFormat={'MMM dd, yyyy'}
  //     startDateTime={handler.startDateTime}
  //     endDateTime={handler.endDateTime}
  //     views={['year', 'month', 'day']}
  //     changeStartDate={handler.changeStartDate}
  //     changeEndDate={handler.changeEndDate}
  //     timePeriod={false}
  //   />
  // )
  if (!isMonthly) {
    return (
      <SelectDateTime
        dateFormat={'MMM dd, yyyy'}
        startDateTime={handler.startDateTime}
        endDateTime={handler.endDateTime}
        views={['year', 'month', 'day']}
        changeStartDate={handler.changeStartDate}
        changeEndDate={handler.changeEndDate}
        timePeriod={false}
      />
    )
  }

  return (
    <>
      <Box
        sx={{ display: 'flex', flexDirection: 'column', gap: 3, maxWidth: 300 }}
      >
        <DatePicker
          label="Start month"
          views={['month', 'year']}
          format="MMM yyyy"
          value={handler.parsedStartMonth}
          onChange={handler.changeStartMonth}
        />

        <DatePicker
          label="End month"
          views={['month', 'year']}
          format="MMM yyyy"
          value={handler.parsedEndMonth}
          onChange={handler.changeEndMonth}
        />
        <FormHelperText sx={{ mt: -2 }}>
          Includes every month from the start through the end month (inclusive).
          The current month will be excluded if it is not complete.
        </FormHelperText>
      </Box>
    </>
  )
}
