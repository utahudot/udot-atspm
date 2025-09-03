import { Location } from '@/api/config'
import OptionsWrapper from '@/components/OptionsWrapper'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import SelectDateTime from '@/components/selectTimeSpan'
import { ResponseFormat, useEventLogs } from '@/features/data/api/getEventLogs'
import SelectDataType, {
  DataTypeOption,
} from '@/features/data/components/selectDataType'
import { useGetAggData } from '@/features/data/exportData/api/getAggData'
import SelectLocation from '@/features/locations/components/selectLocation'
import useMissingDays from '@/hooks/useMissingDays'
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
  addDays,
  endOfMonth,
  endOfWeek,
  isValid,
  startOfMonth,
  startOfToday,
  startOfTomorrow,
  startOfWeek,
} from 'date-fns'
import { useState } from 'react'

const options = ['csv', 'json']

const ExportData = () => {
  const theme = useTheme()

  const [location, setLocation] = useState<Location | null>(null)
  const [error, setError] = useState(false)
  const [startDateTime, setStartDateTime] = useState(startOfToday())
  const [endDateTime, setEndDateTime] = useState(startOfTomorrow())
  const [selectedOption, setSelectedOption] = useState<DataTypeOption>({
    name: 'IndianaEvent',
    type: 'raw',
  })
  const [isDownloading, setIsDownloading] = useState<boolean>(false)
  const [downloadFormat, setDownloadFormat] = useState<ResponseFormat>('csv')
  const [selectedIndex, setSelectedIndex] = useState(0)
  const [calendarStartDate, setCalendarStartDate] = useState<Date>(
    startOfWeek(startOfMonth(startOfToday()))
  )
  const [calendarEndDate, setCalendarEndDate] = useState<Date>(
    endOfWeek(endOfMonth(startOfToday()))
  )

  const isMobileView = useMediaQuery(theme.breakpoints.down('md'))

  // Todo: for now, the end date is the start date + 1 day for raw data

  const { refetch: refetchEventLogs } = useEventLogs({
    locationIdentifier: location?.locationIdentifier || '',
    start: isValid(startDateTime) ? dateToTimestamp(startDateTime) : '',
    end: isValid(startDateTime)
      ? dateToTimestamp(addDays(startDateTime, 1))
      : '',
    dataType: selectedOption.name,
    ResponseFormat: downloadFormat,
  })

  const { refetch: refetchAggData } = useGetAggData(
    location?.locationIdentifier,
    selectedOption.name,
    isValid(startDateTime) ? dateToTimestamp(startDateTime) : '',
    isValid(startDateTime) ? dateToTimestamp(addDays(startDateTime, 1)) : ''
  )

  const missingDays = useMissingDays(
    location?.locationIdentifier ?? '',
    selectedOption.name ?? '',
    calendarStartDate,
    calendarEndDate
    // selectedOption.type === 'aggregation'
  )

  const handleStartDateTimeChange = (date: Date) => setStartDateTime(date)
  const handleEndDateTimeChange = (date: Date) => setEndDateTime(date)

  const downloadEventLogs = async () => {
    setIsDownloading(true)
    setError(false)
    try {
      let response, data
      if (selectedOption.type === 'raw') {
        response = await refetchEventLogs()
        if (response.status === 'error') {
          setError(true)
          return
        }
        data = response.data
      } else {
        response = await refetchAggData()
        data = response.data
      }

      const filename = generateFilename(
        location,
        selectedOption,
        startDateTime,
        endDateTime,
        downloadFormat
      )

      let mimeType = ''
      switch (downloadFormat) {
        case 'json':
          mimeType = 'application/json'
          break
        case 'xml':
          mimeType = 'application/xml'
          break
        case 'csv':
        default:
          mimeType = 'text/csv'
          break
      }
      downloadData(data, filename, mimeType)
    } catch (err) {
      console.error('Error fetching data:', err)
      setError(true)
    } finally {
      setIsDownloading(false)
    }
  }

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

  const requiredClaim = 'Data:View'
  return (
    <Authorization requiredClaim={requiredClaim}>
      <ResponsivePageLayout title={'Export Data'}>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
          <Paper
            sx={{
              flexGrow: 1,
              minWidth: '350px',
              padding: 3,
            }}
          >
            <SelectLocation
              location={location}
              setLocation={setLocation}
              mapHeight={'350px'}
            />
          </Paper>

          <Box display="flex" sx={{ gap: '15px', flexWrap: 'wrap' }}>
            <Paper
              sx={{
                padding: 3,
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
                startDateOnly={selectedOption.type === 'raw'}
                changeStartDate={handleStartDateTimeChange}
                changeEndDate={handleEndDateTimeChange}
                noCalendar={isMobileView}
                markDays={location ? missingDays : undefined}
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
              <SelectDataType
                selectedDataType={selectedOption}
                setSelectedDataType={setSelectedOption}
              />
              <Box display="flex" flexDirection="column">
                <OptionsWrapper header="Export Options" noPadding>
                  <Box sx={{ overflow: 'auto' }}>
                    <List sx={{ marginTop: '-8px' }}>
                      {options.map((opt, index) => (
                        <ListItemButton
                          key={opt}
                          onClick={() => {
                            setSelectedIndex(index)
                            setDownloadFormat(opt as ResponseFormat)
                          }}
                          selected={selectedIndex === index}
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
        </Box>

        <Box sx={{ display: 'flex', gap: '10px', mt: '10px', mb: '120px' }}>
          <LoadingButton
            loadingPosition="start"
            startIcon={<DownloadIcon />}
            loading={isDownloading}
            variant="contained"
            onClick={downloadEventLogs}
            disabled={!location}
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

const formatData = (data: any, type: string): string => {
  if (type === 'application/json') {
    return JSON.stringify(data)
  }
  if (type === 'text/csv' || type === 'application/xml') {
    const eventLogData = data.flatMap((d) => d.data)
    if (eventLogData.length === 0) return ''

    const headers = Object.keys(eventLogData[0])
    if (type === 'text/csv') {
      // Build CSV string
      let csv = headers.join(',') + '\n'
      eventLogData.forEach((eventLog: any) => {
        const row = headers
          .map((header) => {
            if (header === 'timestamp') {
              return eventLog[header].replace('T', ' ')
            }
            // Wrap string with commas in quotes
            if (
              typeof eventLog[header] === 'string' &&
              eventLog[header].includes(',')
            ) {
              return `"${eventLog[header]}"`
            }
            return eventLog[header]
          })
          .join(',')
        csv += row + '\n'
      })
      return csv
    } else if (type === 'application/xml') {
      // Build XML string
      let xml = '<?xml version="1.0" encoding="UTF-8"?>\n<eventLogs>\n'
      eventLogData.forEach((eventLog: any) => {
        xml += '  <eventLog>\n'
        headers.forEach((header) => {
          xml += `    <${header}>${eventLog[header]}</${header}>\n`
        })
        xml += '  </eventLog>\n'
      })
      xml += '</eventLogs>'
      return xml
    }
  }
  return data // Fallback for other formats
}

const generateFilename = (
  location: Location | null,
  selectedDataType: DataTypeOption | null,
  start: Date,
  end: Date,
  downloadFormat: ResponseFormat
): string => {
  const startStr = dateToTimestamp(start)
  const endStr = dateToTimestamp(end)
  const baseName = `${location?.locationIdentifier}_${selectedDataType?.name.replace(/\s+/g, '')}_${startStr}`

  return selectedDataType?.type === 'raw'
    ? `${baseName}.${downloadFormat}`
    : `${baseName}_${endStr}.${downloadFormat}`
}

const downloadData = (data: any, filename: string, mimeType: string) => {
  const formattedData = formatData(data, mimeType)
  const blob = new Blob([formattedData], { type: mimeType })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}
