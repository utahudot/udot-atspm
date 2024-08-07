import { ChartType } from '@/features/charts/common/types'
import { useEventLogs } from '@/features/data/api/getEventLogs'
import { LoadingButton } from '@mui/lab'
import { Box, CircularProgress, Typography } from '@mui/material'
import { format, startOfToday, startOfTomorrow } from 'date-fns'

import { useEffect, useState } from 'react'

//Types

export type LocationOptionsType = {
  location: string
  setLocation: (location: string) => void
  chart: ChartType
  setChart: (chartType: ChartType) => void
  startDateTime: Date
  setStartDateTime: (startDateTime: Date) => void
  endDateTime: Date
  setEndDateTime: (endDateTime: Date) => void
  areaId: number
  regionId: number
  jurisdictionId: number
}

interface EventLog {
  eventCode: number
  eventParam: number
  locationIdentifier: string
  timestamp: string
}

//Data Type selection
const dataAggregationLabels = [
  'All Raw Data',
  'ApproachPcdAgg',
  'ApproachSpeedAgg',
  'ApproachSplitFailAgg',
  'ApproachYellowRedActuationsAgg',
  'DetectorEventCountAgg',
  'PhaseCycleAgg',
  'PhaseLeftTurnGapAgg',
  'PhasePedAgg',
  'PhaseSplitMonitorAgg',
  'PhaseTerminationAgg',
  'PremptionAgg',
  'PriorityAgg',
  'LocationEventCountAgg',
  'LocationPlanAgg',
]

const QueryDataExportBtn = () => {
  //singals Map / context
  const [location, setLocation] = useState('')
  const [startDateTime, setStartDateTime] = useState(startOfToday())
  const [endDateTime, setEndDateTime] = useState(startOfTomorrow())
  const [selectedDataType, setSelectedDataType] = useState<string | null>(
    dataAggregationLabels[0]
  )
  const [eventCodes, setEventCodes] = useState<string | null>('')
  const [eventParams, setEventParams] = useState<string>('')

  //data queries states
  const [recordCount, setRecordCount] = useState<number | null>(null)
  const [eventLogs, setEventLogs] = useState<EventLog[]>([])
  const [isCheckingRecords, setIsCheckingRecords] = useState<boolean>(false)
  const [isDownloading, setIsDownloading] = useState<boolean>(false)
  const [cachedEventLogs, setCachedEventLogs] = useState<EventLog[]>([]) //used to avoid making 2 queries (1 for check record count and 1 for download button)
  const [lastQueriedParams, setLastQueriedParams] = useState({
    location: '',
    startDateTime: '',
    endDateTime: '',
  })

  //Data Type

  //REACT QUERY API
  const {
    refetch,
    data: eventLogsData,
    isLoading: isLoadingEventLogs,
  } = useEventLogs({
    params: {
      locationIdentifier: location,
      start: format(startDateTime, 'yyyy-MM-dd'), // Format to 'yyyy-MM-dd'
      end: format(endDateTime, 'yyyy-MM-dd'), // Format to 'yyyy-MM-dd'
    },
  })

  useEffect(() => {
    refetch().then((response) => {
      // Ensure the response data is of the expected type
      setEventLogs(response.data as EventLog[])
    })
  }, [refetch])

  //DOWNLOAD DATA FUNCTIONS
  // convert data to csv and download
  const createCSV = (latestEventLogs: EventLog) => {
    if (!latestEventLogs) {
      console.error('No data available to export')
      return
    }

    const headers = Object.keys(latestEventLogs[0]).join(',')

    const csvContent =
      'data:text/csv;charset=utf-8,' +
      headers +
      '\n' +
      latestEventLogs.map((e) => Object.values(e).join(',')).join('\n')

    //creates hidden download link to click
    const encodedUri = encodeURI(csvContent)
    const link = document.createElement('a')
    link.setAttribute('href', encodedUri)
    link.setAttribute(
      'download',
      `${location}_${selectedDataType?.split(' ').join('')}_${format(
        startDateTime,
        'MM-dd-yyyy'
      )}_${format(endDateTime, 'MM-dd-yyyy')}.csv`
    )
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  }

  const fetchEventLogs = async () => {
    setIsCheckingRecords(true)
    try {
      const response = await refetch()
      const eventLogsData = response.data as EventLog[]
      setEventLogs(eventLogsData)
      setCachedEventLogs(eventLogsData)
      // Update last queried parameters (checks to see if user input is the same as the last query)
      setLastQueriedParams({
        location,
        startDateTime: format(startDateTime, 'yyyy-MM-dd'),
        endDateTime: format(endDateTime, 'yyyy-MM-dd'),
      })
      setIsCheckingRecords(false)
      return eventLogsData
    } catch (error) {
      console.error('Error fetching data:', error)
      setIsCheckingRecords(false)
      return [] // Return empty array in case of error
    }
  }

  const countRowsAndUpdate = async () => {
    setIsCheckingRecords(true)
    const eventLogsData = await fetchEventLogs()
    const count = eventLogsData.length
    setRecordCount(count)
    setIsCheckingRecords(false)
  }

  const recordCountCheck = () => {
    let message
    if (recordCount !== null) {
      if (recordCount > 1048577) {
        message = (
          <Typography sx={{ color: 'red' }}>
            Current selection is {recordCount.toLocaleString()} records. Only
            1,048,577 will be downloaded. Please shorten date range.
          </Typography>
        )
      } else if (recordCount !== null) {
        message = (
          <Typography>
            Your current request will generate {recordCount.toLocaleString()}{' '}
            records.
          </Typography>
        )
      } else {
        message = (
          <Typography>Limit of allowed record count: 1,048,577</Typography>
        )
      }
    }
    return (
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        {message}
      </Box>
    )
  }

  const downloadData = async () => {
    setIsDownloading(true)
    // Check if inputs have changed since the last query
    const inputsChanged =
      location !== lastQueriedParams.location ||
      format(startDateTime, 'yyyy-MM-dd') !== lastQueriedParams.startDateTime ||
      format(endDateTime, 'yyyy-MM-dd') !== lastQueriedParams.endDateTime

    let latestEventLogs = cachedEventLogs
    if (inputsChanged || cachedEventLogs.length === 0) {
      // Fetch new data if inputs have changed or if the cache is empty
      try {
        const response = await refetch()
        latestEventLogs = response.data as EventLog[]
        setCachedEventLogs(latestEventLogs) // Cache the newly fetched data
        setLastQueriedParams({
          location,
          startDateTime: format(startDateTime, 'yyyy-MM-dd'),
          endDateTime: format(endDateTime, 'yyyy-MM-dd'),
        })
      } catch (error) {
        setIsDownloading(false)
        console.error('Error fetching data:', error)
        return
      }
    }

    if (!latestEventLogs || latestEventLogs.length === 0) {
      setIsDownloading(false)
      console.error('No data available to export')
      return
    }
    setIsDownloading(false)
    createCSV(latestEventLogs)
  }

  return (
    <Box display="flex" sx={{ gap: '10px', marginTop: '10px' }}>
      {
        <LoadingButton
          loadingPosition="start"
          startIcon={
            isDownloading ? (
              <CircularProgress size={14} style={{ color: 'lightgray' }} />
            ) : null
          }
          variant="contained"
          onClick={downloadData}
          disabled={!location}
          sx={{ margin: '20px 0', padding: '10px' }}
        >
          Download
        </LoadingButton>
      }
      {
        <LoadingButton
          loadingPosition="start"
          startIcon={
            isCheckingRecords ? (
              <CircularProgress size={14} style={{ color: 'lightgray' }} />
            ) : null
          }
          variant="contained"
          onClick={countRowsAndUpdate}
          disabled={!location}
          sx={{ margin: '20px 0', padding: '10px' }}
        >
          {isCheckingRecords ? 'Checking Records' : 'Check Record Count'}
        </LoadingButton>
      }
      {recordCountCheck()}
    </Box>
  )
}

export default QueryDataExportBtn
