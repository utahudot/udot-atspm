import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import { Alert, Box, Button, Paper, useTheme } from '@mui/material'
import AnalysisPeriodOptions from '../../common/AnalysisPeriodOptions'
import DateTimeOptions from '../../common/DateTimeOptions'
import { LocationFiltersOptions } from '../../common/LocationFiltersOptionsComponent'
import SourceSelectOptions from '../../common/SourceSelectOptions'
import { ERHourlyHandler } from './handlers/handlers'

interface Props {
  handler: ERHourlyHandler
}

export const HourlyReportOptionsComponent = (props: Props) => {
  const theme = useTheme()
  const { handler } = props

  return (
    <Box>
      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
        <Paper
          sx={{
            display: 'flex',
            flexDirection: 'column',
            p: 2,
            minWidth: '350px',
          }}
        >
          <SourceSelectOptions handler={handler} />
          <AnalysisPeriodOptions handler={handler} />
        </Paper>
        <Paper
          sx={{
            display: 'flex',
            flexDirection: 'column',
            p: 3,
            minWidth: '350px',
          }}
        >
          <DateTimeOptions handler={handler} isMonthly={false} />
        </Paper>
        <Paper
          sx={{
            display: 'flex',
            flexDirection: 'column',
            p: 2,
            minWidth: '400px',
            maxWidth: '400px',
          }}
        >
          <LocationFiltersOptions handler={handler} />
        </Paper>
      </Box>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'flex-start',
          marginTop: 2,
          gap: 2,
        }}
      >
        <LoadingButton
          loading={
            handler.isPdfDownloading ? handler.isPdfDownloading : undefined
          }
          loadingPosition="start"
          startIcon={<PlayArrowIcon />}
          variant="contained"
          color="primary"
          onClick={handler.handleSubmit}
        >
          Generate Report
        </LoadingButton>
        {handler.isPdfDownloading && (
          <Button variant="text" color="primary" onClick={handler.handleCancel}>
            Cancel
          </Button>
        )}
        {handler.hasError && (
          <Alert severity="error">
            {handler.error
              ? handler.error
              : 'No data available for the selected parameters'}
          </Alert>
        )}
      </Box>
    </Box>
  )
}
