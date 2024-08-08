import { Box } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers'

interface CongestionTrackerChartOptionsProps {
  han
}

export const CongestionTrackerChartOptions =
  ({}: CongestionTrackerChartOptionsProps) => {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
        {/* <InputLabel htmlFor="percentile-split-input">
          <Typography>Percentile Split</Typography>
        </InputLabel> */}
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <DatePicker label={'Month'} views={['month', 'year']} />
        </Box>
      </Box>
    )
  }
