import SelectTimeSpan from '@/components/selectTimeSpan'
import { TSHistoricHandler } from '@/features/charts/timeSpaceDiagram/historic/TimeSpaceHistoricOptions/historicTimeSpaceOptions.handler'
import TimeSpaceRouteSelect from '@/features/charts/timeSpaceDiagram/shared/components/TimeSpaceRouteSelect/TimeSpaceRouteSelect'
import {
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  Paper,
  TextField,
} from '@mui/material'
import { differenceInMinutes } from 'date-fns'
import { useMemo, useRef } from 'react'

interface Props {
  handler: TSHistoricHandler
}

export const HistoricOptionsComponent = ({ handler }: Props) => {
  const options = handler.toOptions()
  const fileInputRef = useRef<HTMLInputElement | null>(null)
  const timeSpaceHistoricWarning = useMemo(() => {
    const diffMinutes = differenceInMinutes(
      handler.endDateTime,
      handler.startDateTime
    )
    return diffMinutes > 20
      ? 'A time span of 20 minutes or less is recommend for this diagram.'
      : null
  }, [handler.startDateTime, handler.endDateTime])

  return (
    <Box display="flex" gap={2}>
      <TimeSpaceRouteSelect handler={handler} />
      <Paper sx={{ p: 3, maxWidth: '320px' }}>
        <SelectTimeSpan
          startDateTime={handler.startDateTime}
          endDateTime={handler.endDateTime}
          changeStartDate={handler.changeStartDate}
          changeEndDate={handler.changeEndDate}
          warning={timeSpaceHistoricWarning}
        />
        <FormControlLabel
          control={
            <Checkbox
              checked={Boolean(options.includeSrmSearch)}
              onChange={(e) =>
                handler.applyFromOptions({
                  includeSrmSearch: e.target.checked,
                })
              }
            />
          }
          label="Include SRM Search"
        />
        <Box mt={1}>
          <input
            ref={fileInputRef}
            type="file"
            accept=".csv,text/csv"
            style={{ display: 'none' }}
            onChange={(e) => {
              const selectedFile = e.target.files?.[0] ?? null
              handler.applyFromOptions({
                srmCsvFile: selectedFile,
              })
            }}
          />
          <Button
            variant="outlined"
            size="small"
            fullWidth
            disabled={!options.includeSrmSearch}
            onClick={() => fileInputRef.current?.click()}
          >
            Select SRM CSV File
          </Button>
        </Box>
        <TextField
          fullWidth
          size="small"
          margin="dense"
          label="Selected File"
          value={options.srmCsvFile?.name ?? ''}
          disabled
          helperText="Chosen file is sent as compressed content."
        />
      </Paper>
    </Box>
  )
}

export default HistoricOptionsComponent
