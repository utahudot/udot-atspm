import { Location } from '@/api/config/aTSPMConfigurationApi.schemas'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import SelectDateTime from '@/components/selectTimeSpan'
import { ResponseFormat, useEventLogs } from '@/features/data/api/getEventLogs'
import SelectDataType from '@/features/data/components/selectDataType'
import { useGetAggData } from '@/features/data/exportData/api/getAggData'
import SelectLocation from '@/features/locations/components/selectLocation'
import useMissingDays from '@/hooks/useMissingDays'
import Authorization from '@/lib/Authorization'
import { dateToTimestamp } from '@/utils/dateTime'
import ArrowDropDownIcon from '@mui/icons-material/ArrowDropDown'
import { LoadingButton } from '@mui/lab'
import {
  Alert,
  Box,
  Button,
  ButtonGroup,
  ClickAwayListener,
  Grow,
  MenuItem,
  MenuList,
  Paper,
  Popper,
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
import { useEffect, useRef, useState } from 'react'

const options = ['csv', 'json', 'xml']

const ExportData = () => {
  const theme = useTheme()
  const [location, setLocation] = useState<Location | null>(null)
  const [error, setError] = useState(false)
  const [startDateTime, setStartDateTime] = useState(startOfToday())
  const [endDateTime, setEndDateTime] = useState(startOfTomorrow())
  const [selectedDataType, setSelectedDataType] = useState<string | null>(
    'All Raw Data'
  )
  const [isAllRawDataSelected, setIsAllRawDataSelected] =
    useState<boolean>(true)
  const [isDownloading, setIsDownloading] = useState<boolean>(false)
  const [downloadFormat, setDownloadFormat] = useState<ResponseFormat>('csv')
  const [selectedIndex, setSelectedIndex] = useState(0)
  const [calendarStartDate, setCalendarStartDate] = useState<Date>(
    startOfWeek(startOfMonth(startOfToday()))
  )
  const [calendarEndDate, setCalendarEndDate] = useState<Date>(
    endOfWeek(endOfMonth(startOfToday()))
  )

  const [open, setOpen] = useState(false)
  const anchorRef = useRef<HTMLDivElement>(null)
  const isMobileView = useMediaQuery(theme.breakpoints.down('md'))

  useEffect(() => {
    setIsAllRawDataSelected(selectedDataType === 'All Raw Data')
  }, [selectedDataType])

  const { refetch: refetchEventLogs } = useEventLogs({
    locationIdentifier: location?.locationIdentifier || '',
    start: isValid(startDateTime) ? dateToTimestamp(startDateTime) : '',
    end: isValid(endDateTime) ? dateToTimestamp(endDateTime) : '',
    ResponseFormat: downloadFormat,
  })

  const { refetch: refetchAggData } = useGetAggData(
    location?.locationIdentifier,
    selectedDataType,
    isValid(startDateTime) ? dateToTimestamp(startDateTime) : '',
    isValid(endDateTime) ? dateToTimestamp(endDateTime) : ''
  )

  const missingDays = useMissingDays(
    location?.locationIdentifier ?? '',
    'IndianaEvent',
    calendarStartDate,
    calendarEndDate
  )

  const handleStartDateTimeChange = (date: Date) => setStartDateTime(date)
  const handleEndDateTimeChange = (date: Date) => setEndDateTime(date)

  const handleMenuItemClick = (
    event: React.MouseEvent<HTMLLIElement, MouseEvent>,
    index: number
  ) => {
    setSelectedIndex(index)
    setDownloadFormat(options[index] as ResponseFormat)
    setOpen(false)
  }

  const handleToggle = () => {
    setOpen((prevOpen) => !prevOpen)
  }

  const handleClose = (event: Event) => {
    if (
      anchorRef.current &&
      anchorRef.current.contains(event.target as HTMLElement)
    ) {
      return
    }
    setOpen(false)
  }

  const downloadEventLogs = async () => {
    setIsDownloading(true)
    setError(false)
    try {
      let response, data
      if (selectedDataType === 'All Raw Data') {
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
        selectedDataType,
        startDateTime,
        endDateTime,
        isAllRawDataSelected,
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

  const requiredClaim = 'Data:View'
  return (
    <Authorization requiredClaim={requiredClaim}>
      <ResponsivePageLayout title={'Export Data'}>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
          <Paper
            sx={{
              flexGrow: 1,
              minWidth: '350px',
              padding: theme.spacing(3),
            }}
          >
            <SelectLocation location={location} setLocation={setLocation} />
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
                startDateOnly={isAllRawDataSelected}
                changeStartDate={handleStartDateTimeChange}
                changeEndDate={handleEndDateTimeChange}
                noCalendar={isMobileView}
                markDays={location ? missingDays : undefined}
                onMonthChange={(date) => {
                  const newStart = startOfWeek(startOfMonth(date))
                  const newEnd = endOfWeek(endOfMonth(date))
                  handleCalendarRangeChange(newStart, newEnd)
                }}
              />
            </Paper>
            <Box
              sx={{
                minWidth: '23.188rem',
                [theme.breakpoints.down('sm')]: { width: '100%' },
              }}
            >
              <SelectDataType
                selectedDataType={selectedDataType}
                setSelectedDataType={setSelectedDataType}
              />
            </Box>
          </Box>
        </Box>

        <Box sx={{ display: 'flex', gap: '10px', mt: '10px', mb: '120px' }}>
          <ButtonGroup
            variant="contained"
            ref={anchorRef}
            aria-label="split button"
            disabled={!location}
            sx={{ height: '45px', mt: '20px' }}
          >
            <LoadingButton
              loadingPosition="start"
              loading={isDownloading}
              variant="contained"
              onClick={downloadEventLogs}
              disabled={!location}
            >
              Download {downloadFormat}
            </LoadingButton>
            <Button
              size="small"
              aria-controls={open ? 'split-button-menu' : undefined}
              aria-expanded={open ? 'true' : undefined}
              aria-label="select download format"
              aria-haspopup="menu"
              onClick={handleToggle}
            >
              <ArrowDropDownIcon />
            </Button>
          </ButtonGroup>
          <Popper
            sx={{ zIndex: 1300, pl: '140px' }}
            open={open}
            anchorEl={anchorRef.current}
            transition
            disablePortal
            placement="bottom"
            modifiers={[
              {
                name: 'preventOverflow',
                options: {
                  altAxis: true,
                  altBoundary: true,
                  tether: true,
                  rootBoundary: 'document',
                  padding: 8,
                },
              },
            ]}
          >
            {({ TransitionProps }) => (
              <Grow
                {...TransitionProps}
                style={{ transformOrigin: 'center top' }}
              >
                <Paper>
                  <ClickAwayListener onClickAway={handleClose}>
                    <MenuList id="split-button-menu" autoFocusItem>
                      {options.map((option, index) => (
                        <MenuItem
                          key={option}
                          selected={index === selectedIndex}
                          onClick={(event) => handleMenuItemClick(event, index)}
                        >
                          {option}
                        </MenuItem>
                      ))}
                    </MenuList>
                  </ClickAwayListener>
                </Paper>
              </Grow>
            )}
          </Popper>
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
  selectedDataType: string | null,
  start: Date,
  end: Date,
  isAllRawData: boolean,
  downloadFormat: ResponseFormat
): string => {
  const startStr = dateToTimestamp(start)
  const endStr = dateToTimestamp(end)
  const baseName = `${location?.locationIdentifier}_${selectedDataType?.replace(/\s+/g, '')}_${startStr}`

  return isAllRawData
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
