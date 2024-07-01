import { ResponseFormat, useEventLogs } from '@/features/data/api/getEventLogs'
import SelectDataType from '@/features/data/components/selectDataType'
import SelectLocation from '@/features/locations/components/selectLocation'
import { Location } from '@/features/locations/types'
import { LoadingButton } from '@mui/lab'
import {
  Alert,
  Box,
  CircularProgress,
  Paper,
  useMediaQuery,
  useTheme,
} from '@mui/material'

import { StyledPaper } from '@/components/StyledPaper'
import ArrowDropDownIcon from '@mui/icons-material/ArrowDropDown'
import Button from '@mui/material/Button'
import ButtonGroup from '@mui/material/ButtonGroup'
import ClickAwayListener from '@mui/material/ClickAwayListener'
import Grow from '@mui/material/Grow'
import MenuItem from '@mui/material/MenuItem'
import MenuList from '@mui/material/MenuList'
import Popper from '@mui/material/Popper'

import { ResponsivePageLayout } from '@/components/ResponsivePage'
import Authorization from '@/lib/Authorization'
import { dateToTimestamp } from '@/utils/dateTime'
import { isValid, startOfToday, startOfTomorrow } from 'date-fns'

import SelectDateTime from '@/components/selectTimeSpan'
import { useGetAggData } from '@/features/data/exportData/api/getAggData'
import { useEffect, useRef, useState } from 'react'

const ExportData = () => {
  const theme = useTheme()
  const [location, setLocation] = useState<Location | null>(null)
  const [error, setError] = useState(false)
  //date options
  const [startDateTime, setStartDateTime] = useState(startOfToday())
  const [endDateTime, setEndDateTime] = useState(startOfTomorrow())

  // data Types and codes
  const [selectedDataType, setSelectedDataType] = useState<string | null>(
    'All Raw Data'
  )
  // const [eventCodes, setEventCodes] = useState<string | null>('')
  // const [eventParams, setEventParams] = useState<string>('')

  //data queries states
  const [isDownloading, setIsDownloading] = useState<boolean>(false)
  const [downloadFormat, setDownloadFormat] = useState<ResponseFormat>('csv')

  //drop down button states
  const [open, setOpen] = useState(false)
  const anchorRef = useRef<HTMLDivElement>(null)
  const [selectedIndex, setSelectedIndex] = useState(2)

  const isMobileView = useMediaQuery(theme.breakpoints.down('md'))

  const handleStartDateTimeChange = (date: Date) => {
    setStartDateTime(date)
  }

  const handleEndDateTimeChange = (date: Date) => {
    setEndDateTime(date)
  }

  const downloadData = (data: any, filename: string, type: string) => {
    setError(false)
    console.log(data)
    let formattedData = ''

    if (type === 'application/json') {
      formattedData = JSON.stringify(data)
    } else if (type === 'text/csv' || type === 'application/xml') {
      // Extract the event log data from the data object
      const eventLogData = data[0].data

      if (eventLogData.length > 0) {
        const headers = Object.keys(eventLogData[0])

        if (type === 'text/csv') {
          // Generate the CSV headers
          formattedData = headers.join(',') + '\n'

          // Generate the CSV rows
          eventLogData.forEach((eventLog: any) => {
            const row = headers.map((header) => eventLog[header]).join(',')
            formattedData += row + '\n'
          })
        } else if (type === 'application/xml') {
          // Generate the XML content
          formattedData = '<?xml version="1.0" encoding="UTF-8"?>\n'
          formattedData += '<eventLogs>\n'

          eventLogData.forEach((eventLog: any) => {
            formattedData += '  <eventLog>\n'
            headers.forEach((header) => {
              formattedData += `    <${header}>${eventLog[header]}</${header}>\n`
            })
            formattedData += '  </eventLog>\n'
          })

          formattedData += '</eventLogs>'
        }
      }
    } else {
      formattedData = data
    }

    const blob = new Blob([formattedData], { type })
    const url = URL.createObjectURL(blob)

    const a = document.createElement('a')
    a.href = url
    a.download = filename
    document.body.appendChild(a)
    a.click()

    document.body.removeChild(a)
    URL.revokeObjectURL(url)
  }
  const downloadEventLogs = async () => {
    setIsDownloading(true)
    setError(false)
    try {
      let response
      let data
      if (selectedDataType == 'All Raw Data') {
        response = await refetchEventLogs()
        console.log('dans response ', response)
        if (response.status === 'error'){
          setError(true)
          setIsDownloading(false)

          return
        }
        data = response.data
  
        setIsDownloading(false)
      } else {
        response = await refetchAggData()
        data = response.data
        setIsDownloading(false)
      }

      const filename = `${location?.locationIdentifier}_${selectedDataType
        ?.split(' ')
        .join('')}_${dateToTimestamp(startDateTime)}_${dateToTimestamp(
        endDateTime
      )}.${downloadFormat}`

      let mimeType = ''
      switch (downloadFormat) {
        case 'json':
          mimeType = 'application/json'
          break
        case 'xml':
          mimeType = 'application/xml'
          break
        case 'csv':
          mimeType = 'text/csv'
          break
      }

      downloadData(data, filename, mimeType)
    } catch (error) {
      setIsDownloading(false)
      console.error('Error fetching data:', error)
      setError(true)
    }
  }

  const options = ['csv', 'json', 'xml']

  const handleMenuItemClick = (
    event: React.MouseEvent<HTMLLIElement, MouseEvent>,
    index: number
  ) => {
    setSelectedIndex(index)
    setDownloadFormat(options[index])
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

  //React Query Fetch
  const {
    refetch: refetchEventLogs,
    data: eventLogsData,
    isLoading: isLoadingEventLogs,
    isError: eventLogError,
  } = useEventLogs({
    locationIdentifier: location?.locationIdentifier
      ? location.locationIdentifier
      : '',
    start: isValid(startDateTime) ? dateToTimestamp(startDateTime) : '',
    end: isValid(endDateTime) ? dateToTimestamp(endDateTime) : '',
    ResponseFormat: downloadFormat,
  })

  const {
    data: aggData,
    isLoading: aggDataIsLoading,
    isError: aggDataError,
    refetch: refetchAggData,
  } = useGetAggData(
    location?.locationIdentifier,
    selectedDataType,
    isValid(startDateTime) ? dateToTimestamp(startDateTime) : '',
    isValid(endDateTime) ? dateToTimestamp(endDateTime) : ''
  )

  useEffect(() => {
    console.log('LocationIdentifer: ', location?.locationIdentifier)
    console.log('Data Type: ', selectedDataType)
    console.log('Start Time:', startDateTime)
    console.log('end Time:', dateToTimestamp(endDateTime))

  }, [selectedDataType])

  const requiredClaim = 'Data:View'
  return (
    <Authorization requiredClaim={requiredClaim}>
      <ResponsivePageLayout title={'Export Data'}>
        <Box
          sx={{
            display: 'flex',
            flexWrap: 'wrap',
            gap: 2,
          }}
        >
          <StyledPaper
            sx={{
              flexGrow: 1,
              minWidth: '23.188rem',
              padding: theme.spacing(3),
            }}
          >
            <SelectLocation location={location} setLocation={setLocation} />
          </StyledPaper>

          <Box display="flex" sx={{ gap: '15px', flexWrap: 'wrap' }}>
            <StyledPaper
              sx={{
                padding: 3,
                width: '23.188rem',
                [theme.breakpoints.down('sm')]: {
                  width: '100%',
                },
              }}
            >
              {/* <SelectTimeSpan
                startDateTime={startDateTime}
                endDateTime={endDateTime}
                changeStartDate={handleStartDateTimeChange}
                changeEndDate={handleEndDateTimeChange}
                noCalendar={isMobileView}
              /> */}
              <SelectDateTime
                // chartType={chartType}
                startDateTime={startDateTime}
                endDateTime={endDateTime}
                changeStartDate={handleStartDateTimeChange}
                changeEndDate={handleEndDateTimeChange}
                noCalendar={isMobileView}
              />
            </StyledPaper>
            <Box
              sx={{
                minWidth: '23.188rem',
                [theme.breakpoints.down('sm')]: {
                  width: '100%',
                },
              }}
            >
              <SelectDataType
                selectedDataType={selectedDataType}
                setSelectedDataType={setSelectedDataType}
                // eventCodes={eventCodes}
                // setEventCodes={setEventCodes}
                // eventParams={eventParams}
                // setEventParams={setEventParams}
              />
            </Box>
          </Box>
        </Box>

        <Box sx={{ display: 'flex', gap: '10px', marginTop: '10px', marginBottom: "120px" }}>
          <ButtonGroup
            variant="contained"
            ref={anchorRef}
            aria-label="split button"
            disabled={!location}
            sx={{ height: '45px', marginTop: '20px' }}
          >
            <LoadingButton
              loadingPosition="start"
              startIcon={
                isDownloading ? (
                  <CircularProgress size={14} style={{ color: 'lightgray' }} />
                ) : null
              }
              variant="contained"
              onClick={downloadEventLogs}
              disabled={!location}
            >
              Download {downloadFormat}
            </LoadingButton>
            <Button
              size="small"
              // variant="outlined"
              aria-controls={open ? 'split-button-menu' : undefined}
              aria-expanded={open ? 'true' : undefined}
              aria-label="select merge strategy"
              aria-haspopup="menu"
              onClick={handleToggle}
            >
              <ArrowDropDownIcon />
            </Button>
          </ButtonGroup>
          <Popper
            sx={{
              zIndex: 1300,
              paddingLeft:'140px',
            }}
            open={open}
            anchorEl={anchorRef.current}
            role={undefined}
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
                style={{
                  transformOrigin: 'center top',
                }}
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
            <Box
              sx={{
                display: 'flex',
                marginLeft: '1rem',
                marginTop: '19px',
                height: "48px"
              }}
            >
              <Alert  severity="error">No Data Found</Alert>
            </Box>
          )}
        </Box>
      </ResponsivePageLayout>
    </Authorization>
  )
}

export default ExportData
