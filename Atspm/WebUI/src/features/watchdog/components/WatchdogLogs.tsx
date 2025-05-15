import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'
import OptionalWatchDogFilters from '@/components/MapFilters/OptionalWatchDogFilters'
import { StyledPaper } from '@/components/StyledPaper'
import SelectDateTime from '@/components/selectTimeSpan/SelectTimeSpan'
import { Area } from '@/features/areas/types'
import {
  LogEventsData,
  useGetIssueTypes,
  useGetWatchdogLogs,
} from '@/features/watchdog/api/getWatchdogLogs'
import { useCreateWatchdogIgnoreEvents } from '@/features/watchdog/api/watchdogIgnoreEvents'
import { useNotificationStore } from '@/stores/notifications'
import { dateToTimestamp, toUTCDateStamp } from '@/utils/dateTime'
import { addSpaces } from '@/utils/string'
import { zodResolver } from '@hookform/resolvers/zod'
import NotificationsPausedIcon from '@mui/icons-material/NotificationsPaused'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Paper,
  Tooltip,
} from '@mui/material'
import {
  DataGrid,
  GridColDef,
  GridToolbarColumnsButton,
  GridToolbarContainer,
  GridToolbarDensitySelector,
  GridToolbarExport,
  GridToolbarFilterButton,
  gridClasses,
} from '@mui/x-data-grid'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { AxiosError } from 'axios'
import { startOfToday, startOfYesterday } from 'date-fns'
import { useCallback, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
interface transformWatchDogLog {
  id: number
  locationId: number
  locationIdentifier: string | null
  timestamp: string
  regionDescription: string
  jurisdictionName: string
  areas: string
  issueType: number
  phase: string | null
  details: string
  componentType: number
  componentId: number
  start: Date
  end: Date
}

// Schema for filtering events (table of events)
const watchdogLogsSchema = z.object({
  locationIdentifier: z.string().nullable().optional(),
  startDateTime: z.date(),
  endDateTime: z.date(),
  areaId: z.number().nullable().optional(),
  regionId: z.number().nullable().optional(),
  jurisdictionId: z.number().nullable().optional(),
  selectedIssueType: z.number().nullable().optional(),
})

// Schema for ignoring events
const ignoreEventSchema = z.object({
  start: z.date().nullable(),
  end: z.date().nullable(),
})

const WatchDogLogs = () => {
  const { addNotification } = useNotificationStore()

  const { data: issueTypesData } = useGetIssueTypes()
  const {
    data: watchdogLogsData,
    isLoading: isWatchdogLogsLoading,
    error: watchdogLogsError,
    mutateAsync: fetchWatchdogLogs,
  } = useGetWatchdogLogs()
  const { mutateAsync: addWatchdogIgnoreEvents } =
    useCreateWatchdogIgnoreEvents()

  const {
    handleSubmit: handleFilterSubmit,
    setValue: setFilterValue,
    watch: watchFilters,
  } = useForm<z.infer<typeof watchdogLogsSchema>>({
    resolver: zodResolver(watchdogLogsSchema),
    defaultValues: {
      startDateTime: startOfYesterday(),
      endDateTime: startOfToday(),
    },
  })

  const {
    handleSubmit: handleIgnoreSubmit,
    setValue: setIgnoreValue,
    watch: watchIgnore,
  } = useForm<z.infer<typeof ignoreEventSchema>>({
    resolver: zodResolver(ignoreEventSchema),
    defaultValues: {
      start: startOfYesterday(),
      end: null,
    },
  })

  // Watching form fields for filters (table)
  const locationIdentifier = watchFilters('locationIdentifier')
  const startDateTime = watchFilters('startDateTime')
  const endDateTime = watchFilters('endDateTime')
  const areaId = watchFilters('areaId')
  const regionId = watchFilters('regionId')
  const jurisdictionId = watchFilters('jurisdictionId')
  const selectedIssueType = watchFilters('selectedIssueType')

  // Watching form fields for ignoring events
  const startIgnoreTime = watchIgnore('start')
  const endIgnoreTime = watchIgnore('end')

  const [clickedRows, setClickedRows] = useState<
    Record<number, transformWatchDogLog>
  >({})
  const [selectedRows, setSelectedRows] = useState<number[]>([])
  const [isDialogOpen, setIsDialogOpen] = useState(false)
  const [isIconClicked, setIsIconClicked] = useState(false)
  const [processedRows, setProcessedRows] = useState<transformWatchDogLog[]>([])

  useMemo(() => {
    const logEvents = (watchdogLogsData as unknown as LogEventsData)?.logEvents
    if (logEvents) {
      const rows = logEvents.map((logEvent, index) => ({
        id: index,
        locationId: logEvent.locationId,
        locationIdentifier: logEvent.locationIdentifier,
        timestamp: logEvent.timestamp,
        regionDescription: logEvent.regionDescription,
        jurisdictionName: logEvent.jurisdictionName,
        areas: logEvent.areas.map((area: Area) => area.name).join(', '),
        issueType: logEvent.issueType,
        phase: logEvent.phase,
        details: logEvent.details,
        componentType: logEvent.componentType,
        componentId: logEvent.componentId,
      }))
      setProcessedRows(rows)
    }
  }, [watchdogLogsData])

  const issueTypes = useMemo(() => {
    if (!issueTypesData) return null
    return issueTypesData.reduce(
      (acc, issueType) => ({ ...acc, [issueType.id]: issueType.name }),
      {}
    )
  }, [issueTypesData])

  const handleIconClick = useCallback(() => {
    setIsIconClicked(!isIconClicked)
  }, [isIconClicked])

  const handleCancelClick = useCallback(() => {
    setIsIconClicked(false)
    setSelectedRows([])
    setClickedRows({})
  }, [])

  const handleFetchData = handleFilterSubmit(() => {
    fetchWatchdogLogs({
      start: dateToTimestamp(startDateTime),
      end: dateToTimestamp(endDateTime),
      areaId,
      regionId,
      jurisdictionId,
      issueType: selectedIssueType,
      locationIdentifier,
    })
  })

  const handleIgnoreEventsSubmit = handleIgnoreSubmit(async (data) => {
    setIsDialogOpen(false)

    const response = await Promise.all(
      selectedRows.map(async (rowId) => {
        const eventToIgnore = clickedRows?.[rowId]
        if (!eventToIgnore || !data.start || !data.end)
          return { rowId, success: false, error: 'Event not found' }

        try {
          await addWatchdogIgnoreEvents({
            locationId: eventToIgnore.locationId,
            locationIdentifier: eventToIgnore.locationIdentifier,
            issueType: eventToIgnore.issueType?.toString(),
            componentType: eventToIgnore.componentType?.toString(),
            componentId: eventToIgnore.componentId,
            phase: eventToIgnore.phase,
            start: toUTCDateStamp(data.start),
            end: toUTCDateStamp(data.end),
          })
          return { rowId, success: true }
        } catch (error) {
          return { rowId, success: false, error }
        }
      })
    )

    const succeededRows = response
      .filter((result) => result.success)
      .map((result) => result.rowId)
    const failedRows = response.filter((result) => !result.success)

    if (failedRows.length === 0) {
      addNotification({
        title: 'All events ignored successfully',
        type: 'success',
      })
    } else {
      addNotification({
        title: `${failedRows.length} events failed to be ignored`,
        type: 'error',
      })
    }

    setProcessedRows((prevRows) =>
      prevRows.filter((row) => !succeededRows.includes(row.id))
    )
    setSelectedRows([])
    setClickedRows({})
    setIsIconClicked(false)
  })

  const handleDialogClose = useCallback(() => {
    setIsDialogOpen(false)
  }, [])

  const handleRowSelection = useCallback(
    (id: number, checked: boolean) => {
      setSelectedRows((prev) =>
        checked ? [...prev, id] : prev.filter((rowId) => rowId !== id)
      )

      setClickedRows((prev) => {
        const foundRow = processedRows.find((row) => row.id === id)

        return checked && foundRow
          ? { ...prev, [id]: foundRow }
          : Object.fromEntries(
              Object.entries(prev).filter(([key]) => key !== id.toString())
            )
      })
    },
    [processedRows]
  )

  const columns: GridColDef[] = useMemo(() => {
    const baseColumns: GridColDef[] = [
      {
        field: 'locationIdentifier',
        headerName: 'Location',
        flex: 1,
        headerAlign: 'center',
      },
      {
        field: 'timestamp',
        headerName: 'Timestamp',
        flex: 1,
        headerAlign: 'center',
      },
      {
        field: 'regionDescription',
        headerName: 'Region',
        flex: 1,
        headerAlign: 'center',
      },
      {
        field: 'jurisdictionName',
        headerName: 'Jurisdiction',
        flex: 1,
        headerAlign: 'center',
      },
      { field: 'areas', headerName: 'Areas', flex: 1, headerAlign: 'center' },
      {
        field: 'issueType',
        headerName: 'Issue Type',
        flex: 1,
        headerAlign: 'center',
        valueGetter: (params) =>
          addSpaces(issueTypes?.[params as number]) ?? '',
      },
      { field: 'phase', headerName: 'Phase', flex: 1, headerAlign: 'center' },
      {
        field: 'details',
        headerName: 'Details',
        flex: 2,
        headerAlign: 'center',
      },
    ]

    if (isIconClicked) {
      const checkboxColumn: GridColDef = {
        field: 'checkbox',
        headerName: '',
        width: 50,
        renderCell: (params) => (
          <Checkbox
            checked={selectedRows.includes(params.id as number)}
            onChange={(e) =>
              handleRowSelection(params.id as number, e.target.checked)
            }
          />
        ),
      }
      return [checkboxColumn, ...baseColumns]
    }

    return baseColumns
  }, [selectedRows, isIconClicked, handleRowSelection, issueTypes])

  const CustomToolbar = useCallback(
    () => (
      <GridToolbarContainer>
        <Box sx={{ flexGrow: 1 }}>
          <GridToolbarColumnsButton />
          <GridToolbarFilterButton />
          <GridToolbarDensitySelector />
          <GridToolbarExport />
        </Box>
        <Box
          sx={{
            display: 'flex',
            alignItems: 'center',
            position: 'relative',
            transition: 'transform 0.3s ease',
          }}
        >
          <Button
            onClick={handleIconClick}
            sx={{
              color: 'red',
              minWidth: 'auto',
              transform: isIconClicked ? 'translateX(-300px)' : 'translateX(0)',
              mb: 1,
            }}
            aria-label="Ignore Events"
          >
            <Tooltip title="Ignore Events" placement="top">
              <NotificationsPausedIcon />
            </Tooltip>
          </Button>
          {isIconClicked && (
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                position: 'absolute',
                right: 0,
              }}
            >
              <Button
                onClick={() => setIsDialogOpen(true)}
                variant="contained"
                color="primary"
                sx={{ marginRight: 1, width: '200px' }}
                disabled={selectedRows.length === 0}
              >
                Ignore Events
              </Button>
              <Button
                onClick={handleCancelClick}
                variant="outlined"
                color="primary"
              >
                Cancel
              </Button>
            </Box>
          )}
        </Box>
      </GridToolbarContainer>
    ),
    [isIconClicked, selectedRows, handleIconClick, handleCancelClick]
  )

  const slots = useMemo(
    () => ({
      toolbar: CustomToolbar,
    }),
    [CustomToolbar]
  )

  const startTimeToLocaleString = startDateTime.toLocaleDateString('en-US', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  })

  const endTimeToLocaleString = endDateTime.toLocaleDateString('en-US', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  })

  const fileName = `WatchdogData-${startTimeToLocaleString}-${endTimeToLocaleString}`

  return (
    <>
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
        <StyledPaper sx={{ width: '23rem', padding: 3 }}>
          <SelectDateTime
            startDateTime={startDateTime}
            endDateTime={endDateTime}
            changeStartDate={(newValue) =>
              setFilterValue('startDateTime', newValue)
            }
            changeEndDate={(newValue) =>
              setFilterValue('endDateTime', newValue)
            }
          />
        </StyledPaper>
        <StyledPaper sx={{ width: '23rem' }}>
          <StyledComponentHeader header="Optional Filters" />
          <Box sx={{ padding: 3 }}>
            <OptionalWatchDogFilters
              issueType={issueTypes}
              setSelectedIssueType={(value) =>
                setFilterValue('selectedIssueType', value)
              }
              setAreaId={(value) => setFilterValue('areaId', value)}
              setRegionId={(value) => setFilterValue('regionId', value)}
              setJurisdictionId={(value) =>
                setFilterValue('jurisdictionId', value)
              }
              setLocationIdentifier={(value) =>
                setFilterValue('locationIdentifier', value)
              }
            />
          </Box>
        </StyledPaper>
      </Box>

      <LoadingButton
        loading={isWatchdogLogsLoading}
        startIcon={<PlayArrowIcon />}
        loadingPosition="start"
        variant="contained"
        onClick={handleFetchData}
        sx={{ marginY: 3, padding: '10px' }}
      >
        Generate Report
      </LoadingButton>

      {watchdogLogsError && (
        <Alert severity="error">
          {(watchdogLogsError as AxiosError).message}
        </Alert>
      )}

      <Box sx={{ width: '100%' }}>
        {processedRows.length > 0 && (
          <Paper>
            <DataGrid
              rows={processedRows}
              columns={columns}
              getRowId={(row) => row.id}
              disableDensitySelector
              slots={slots}
              slotProps={{
                toolbar: {
                  csvOptions: { fileName },
                  printOptions: { fileName },
                },
              }}
              sx={{
                '& .MuiDataGrid-row.selected-row': {
                  backgroundColor: 'lightgrey',
                },
                [`& .${gridClasses.columnHeaders}`]: {
                  position: 'sticky',
                  backgroundColor: 'white',
                  zIndex: 1,
                },
                [`& .${gridClasses.toolbarContainer}`]: {
                  position: 'sticky',
                  top: '0',
                  backgroundColor: 'white',
                  zIndex: '1',
                  pb: '5px',
                },
              }}
              getRowClassName={(params) =>
                selectedRows.includes(params.id as number) ? 'selected-row' : ''
              }
            />
          </Paper>
        )}
      </Box>

      <Dialog open={isDialogOpen} onClose={handleDialogClose}>
        <DialogTitle>Ignore Events</DialogTitle>
        <DialogContent>
          <Box display="flex" flexDirection="column" gap={2} mt={2}>
            <DatePicker
              label="Start Date"
              value={startIgnoreTime}
              onChange={(newValue) => setIgnoreValue('start', newValue)}
            />
            <DatePicker
              label="End Date"
              value={endIgnoreTime}
              onChange={(newValue) => setIgnoreValue('end', newValue)}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDialogClose} variant="outlined">
            Cancel
          </Button>
          <Button
            onClick={handleIgnoreEventsSubmit}
            variant="contained"
            color="primary"
          >
            Ignore Events
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}

export default WatchDogLogs
