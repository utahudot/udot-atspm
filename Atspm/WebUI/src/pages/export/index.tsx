import { Location } from '@/api/config'
import {
  getAggregationDataFromLocationIdentifierAndDataType,
  getEventLogDataFromLocationIdentifierAndDataType,
} from '@/api/data'
import OptionsWrapper from '@/components/OptionsWrapper'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import SelectDateTime from '@/components/selectTimeSpan'
import { ResponseFormat } from '@/features/data/api/getEventLogs'
import {
  DataTypeOption,
  DataTypeSelector,
} from '@/features/data/components/dataTypeSelector'
import { downloadData, generateFilename } from '@/features/data/utils'
import SelectLocation from '@/features/locations/components/selectLocation'
import Authorization from '@/lib/Authorization'
import { dateToTimestamp } from '@/utils/dateTime'
import DownloadIcon from '@mui/icons-material/Download'
import { LoadingButton } from '@mui/lab'
import {
  Alert,
  Box,
  List,
  ListItemButton,
  ListItemText,
  Paper,
  useMediaQuery,
  useTheme,
} from '@mui/material'
import {
  endOfMonth,
  endOfWeek,
  isValid,
  startOfMonth,
  startOfToday,
  startOfTomorrow,
  startOfWeek,
} from 'date-fns'
import { useState } from 'react'

const DEFAULT_DATA_TYPE: DataTypeOption = {
  name: 'IndianaEvent',
  displayName: 'Indiana Event',
  type: 'raw',
}

const EXPORT_FORMATS: ResponseFormat[] = ['csv', 'json']

const REQUIRED_CLAIM = 'Data:View'

const buildRangeParams = (start: Date, end: Date) => ({
  start: isValid(start) ? dateToTimestamp(start) : '',
  end: isValid(end) ? dateToTimestamp(end) : '',
})

const mimeTypeForFormat = (format: ResponseFormat): string => {
  switch (format) {
    case 'json':
      return 'application/json'
    case 'xml':
      return 'application/xml'
    case 'csv':
    default:
      return 'text/csv'
  }
}

const ExportData = () => {
  const theme = useTheme()
  const isMobileView = useMediaQuery(theme.breakpoints.down('md'))

  const [location, setLocation] = useState<Location | null>(null)
  const [error, setError] = useState(false)

  const [startDateTime, setStartDateTime] = useState(startOfToday())
  const [endDateTime, setEndDateTime] = useState(startOfTomorrow())

  const [selectedDataType, setSelectedDataType] =
    useState<DataTypeOption>(DEFAULT_DATA_TYPE)

  const [isDownloading, setIsDownloading] = useState(false)
  const [downloadFormat, setDownloadFormat] = useState<ResponseFormat>('csv')

  const [calendarStartDate, setCalendarStartDate] = useState<Date>(
    startOfWeek(startOfMonth(startOfToday()))
  )
  const [calendarEndDate, setCalendarEndDate] = useState<Date>(
    endOfWeek(endOfMonth(startOfToday()))
  )

  // const missingDays = useMissingDays(
  //   location?.locationIdentifier ?? '',
  //   selectedDataType.type === 'raw' ? selectedDataType.name : '',
  //   calendarStartDate,
  //   calendarEndDate
  // )

  const handleStartDateTimeChange = (date: Date) => setStartDateTime(date)
  const handleEndDateTimeChange = (date: Date) => setEndDateTime(date)

  const handleCalendarRangeChange = (start: Date, end: Date) => {
    setCalendarStartDate(start)
    setCalendarEndDate(end)
  }

  const handleDateChange = (date: Date | null) => {
    if (!date) return
    const newStart = startOfWeek(startOfMonth(date))
    const newEnd = endOfWeek(endOfMonth(date))
    handleCalendarRangeChange(newStart, newEnd)
  }

  const downloadEventLogs = async () => {
    setIsDownloading(true)
    setError(false)

    try {
      const locationId = location?.locationIdentifier || ''
      const range = buildRangeParams(startDateTime, endDateTime)

      const fetchFn =
        selectedDataType.type === 'raw'
          ? getEventLogDataFromLocationIdentifierAndDataType
          : getAggregationDataFromLocationIdentifierAndDataType

      const data = await fetchFn(locationId, selectedDataType.name, range)

      const filename = generateFilename(
        location,
        selectedDataType,
        startDateTime,
        endDateTime,
        downloadFormat
      )

      const mimeType = mimeTypeForFormat(downloadFormat)
      downloadData(data, filename, mimeType)
    } catch (err) {
      console.error('Error fetching data:', err)
      setError(true)
    } finally {
      setIsDownloading(false)
    }
  }

  const canDownload = !!location

  return (
    <Authorization requiredClaim={REQUIRED_CLAIM}>
      <ResponsivePageLayout title="Export Data">
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
          <Paper
            sx={{
              flexGrow: 1,
              minWidth: '350px',
              p: 3,
            }}
          >
            <SelectLocation location={location} setLocation={setLocation} />
          </Paper>

          <Box display="flex" sx={{ gap: '15px', flexWrap: 'wrap' }}>
            <Paper
              sx={{
                p: 3,
                width: '336px',
                [theme.breakpoints.down('sm')]: {
                  width: '100%',
                },
              }}
            >
              <SelectDateTime
                dateFormat="MMM d, yyyy"
                views={['year', 'month', 'day']}
                startDateTime={startDateTime}
                endDateTime={endDateTime}
                changeStartDate={handleStartDateTimeChange}
                changeEndDate={handleEndDateTimeChange}
                noCalendar={isMobileView}
                // markDays={
                //   selectedDataType.type === 'raw' ? missingDays : undefined
                // }
                onChange={handleDateChange}
                onMonthChange={handleDateChange}
              />
            </Paper>

            <Box
              sx={{
                minWidth: '23.188rem',
                [theme.breakpoints.down('sm')]: { width: '100%' },
                display: 'flex',
                flexDirection: 'column',
                gap: 2,
              }}
            >
              <DataTypeSelector
                selectedDataType={selectedDataType}
                setSelectedDataType={setSelectedDataType}
              />

              <OptionsWrapper header="Export Options" noPadding>
                <Box sx={{ overflow: 'auto' }}>
                  <List sx={{ mt: '-8px' }}>
                    {EXPORT_FORMATS.map((opt) => (
                      <ListItemButton
                        key={opt}
                        onClick={() => setDownloadFormat(opt)}
                        selected={downloadFormat === opt}
                      >
                        <ListItemText primary={opt.toUpperCase()} />
                      </ListItemButton>
                    ))}
                  </List>
                </Box>
              </OptionsWrapper>
            </Box>
          </Box>
        </Box>

        <Box sx={{ display: 'flex', gap: '10px', mt: '10px', mb: '120px' }}>
          <LoadingButton
            loadingPosition="start"
            startIcon={<DownloadIcon />}
            loading={isDownloading}
            variant="contained"
            onClick={downloadEventLogs}
            disabled={!canDownload}
          >
            Download Data
          </LoadingButton>

          {error && (
            <Box sx={{ ml: '1rem', mt: '19px', height: '48px' }}>
              <Alert severity="error">No Data Found</Alert>
            </Box>
          )}
        </Box>
      </ResponsivePageLayout>
    </Authorization>
  )
}

export default ExportData
