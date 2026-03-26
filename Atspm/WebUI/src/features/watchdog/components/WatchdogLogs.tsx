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
import {
  useCreateWatchdogIgnoreEvents,
  useGetWatchdogIgnoreEvents,
} from '@/features/watchdog/api/watchdogIgnoreEvents'
import { useNotificationStore } from '@/stores/notifications'
import { dateToTimestamp, toUTCDateStamp } from '@/utils/dateTime'
import { zodResolver } from '@hookform/resolvers/zod'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
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
  GridRenderCellParams,
  GridToolbarColumnsButton,
  GridToolbarContainer,
  GridToolbarDensitySelector,
  GridToolbarExport,
  GridToolbarFilterButton,
  gridClasses,
} from '@mui/x-data-grid'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { AxiosError } from 'axios'
import {
  endOfDay,
  isAfter,
  isBefore,
  startOfDay,
  startOfToday,
  startOfYesterday,
} from 'date-fns'
import { useCallback, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
interface transformWatchDogLog {
  id: number
  key?: string | number | null
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
  ignored: boolean
}

interface WatchdogIgnoreEvent {
  id?: number
  locationId?: number
  locationIdentifier?: string | null
  start?: string
  end?: string | null
  componentType?: number | string | null
  componentId?: number | null
  issueType?: number | string
  phase?: number | string | null
}

const normalizeWatchdogValue = (value: number | string | null | undefined) =>
  String(value ?? '')
    .replace(/[^a-zA-Z0-9]/g, '')
    .toLowerCase()

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
  end: z
    .date()
    .nullable()
    .refine((value) => value !== null, {
      message: 'End date is required',
    }),
})

const WatchDogLogs = () => {
  const { addNotification } = useNotificationStore()

  const { data: issueTypesData } = useGetIssueTypes()
  const {
    data: watchdogIgnoreEventsData,
    refetch: refetchWatchdogIgnoreEvents,
  } = useGetWatchdogIgnoreEvents()
  const {
    data: watchdogLogsData,
    isLoading: isWatchdogLogsLoading,
    error: watchdogLogsError,
    mutateAsync: fetchWatchdogLogs,
    isSuccess: isWatchdogLogsSuccess,
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
    formState: { errors: ignoreErrors },
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
  const [showIgnoreValidation, setShowIgnoreValidation] = useState(false)
  const [ignoreStartHasInputError, setIgnoreStartHasInputError] =
    useState(false)
  const [ignoreEndHasInputError, setIgnoreEndHasInputError] = useState(false)

  const issueTypes = useMemo(() => {
    if (!issueTypesData) return null
    return issueTypesData.reduce(
      (acc, issueType) => ({ ...acc, [issueType.id]: issueType.name }),
      {}
    )
  }, [issueTypesData])

  const ignoreEvents = useMemo(
    () =>
      ((watchdogIgnoreEventsData as { value?: WatchdogIgnoreEvent[] })?.value ??
        []) as WatchdogIgnoreEvent[],
    [watchdogIgnoreEventsData]
  )

  const isEventIgnored = useCallback(
    (logEvent: {
      locationId: number
      locationIdentifier: string | null
      componentType: number
      componentId: number
      issueType: number
      phase: string | null
    }) => {
      const now = new Date()
      const issueTypeName = issueTypes?.[logEvent.issueType]
      const componentTypeName =
        logEvent.componentType === 0
          ? 'Location'
          : logEvent.componentType === 1
            ? 'Approach'
            : logEvent.componentType === 2
              ? 'Detector'
              : String(logEvent.componentType)

      return ignoreEvents.some((ignoreEvent) => {
        if (!ignoreEvent.start) return false

        const ignoreStart = startOfDay(new Date(ignoreEvent.start))
        const ignoreEnd = ignoreEvent.end
          ? endOfDay(new Date(ignoreEvent.end))
          : null
        const ignoreIssueType = normalizeWatchdogValue(ignoreEvent.issueType)
        const ignoreComponentType = normalizeWatchdogValue(
          ignoreEvent.componentType
        )
        const ignorePhase =
          ignoreEvent.phase == null
            ? null
            : normalizeWatchdogValue(ignoreEvent.phase)
        const ignoreComponentId = ignoreEvent.componentId ?? null
        const logIssueType = normalizeWatchdogValue(
          issueTypeName ?? logEvent.issueType
        )
        const logComponentType = normalizeWatchdogValue(componentTypeName)
        const logPhase =
          logEvent.phase == null ? null : normalizeWatchdogValue(logEvent.phase)

        const matchesLocation =
          (ignoreEvent.locationId != null &&
            ignoreEvent.locationId === logEvent.locationId) ||
          (ignoreEvent.locationIdentifier != null &&
            ignoreEvent.locationIdentifier === logEvent.locationIdentifier)

        const matchesIssueType =
          ignoreIssueType === '' || ignoreIssueType === logIssueType

        const matchesComponentType =
          ignoreComponentType === '' ||
          ignoreComponentType === logComponentType ||
          ignoreComponentType === normalizeWatchdogValue(logEvent.componentType)

        const matchesComponentId =
          ignoreComponentId == null ||
          ignoreComponentId === -1 ||
          ignoreComponentId === logEvent.componentId

        const matchesPhase = ignorePhase == null || ignorePhase === logPhase

        const matchesEvent =
          matchesLocation &&
          matchesIssueType &&
          matchesComponentType &&
          matchesComponentId &&
          matchesPhase

        if (!matchesEvent) return false
        if (isBefore(now, ignoreStart)) return false
        if (ignoreEnd && isAfter(now, ignoreEnd)) return false

        return true
      })
    },
    [ignoreEvents, issueTypes]
  )

  useMemo(() => {
    const logEvents = (watchdogLogsData as unknown as LogEventsData)?.logEvents
    if (logEvents) {
      const rows = logEvents.map((logEvent) => ({
        ...logEvent,
        areas: logEvent.areas.map((area: Area) => area.name).join(', '),
        issueType: issueTypes?.[logEvent.issueType] ?? logEvent.issueType,
        ignored: isEventIgnored(logEvent),
      }))
      setProcessedRows(rows)
    }
  }, [watchdogLogsData, issueTypes, isEventIgnored])

  const returnedLogEvents =
    (watchdogLogsData as unknown as LogEventsData)?.logEvents ?? []
  const showNoLogsMessage =
    isWatchdogLogsSuccess &&
    !isWatchdogLogsLoading &&
    returnedLogEvents.length === 0

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
        if (!eventToIgnore || !data.start)
          return { rowId, success: false, error: 'Event not found' }

        console.log('Ignoring event:', eventToIgnore)

        try {
          await addWatchdogIgnoreEvents({
            key: eventToIgnore.key,
            locationId: eventToIgnore.locationId,
            locationIdentifier: eventToIgnore.locationIdentifier,
            issueType: eventToIgnore.issueType?.toString(),
            componentType: eventToIgnore.componentType?.toString(),
            componentId: eventToIgnore.componentId,
            phase: eventToIgnore.phase,
            start: toUTCDateStamp(data.start),
            end: data?.end ? toUTCDateStamp(data.end) : null,
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

    await refetchWatchdogIgnoreEvents()
    setProcessedRows((prevRows) =>
      prevRows.map((row) =>
        succeededRows.includes(row.id) ? { ...row, ignored: true } : row
      )
    )
    setSelectedRows([])
    setClickedRows({})
    setIsIconClicked(false)
    setShowIgnoreValidation(false)
  })

  const handleDialogClose = useCallback(() => {
    setIsDialogOpen(false)
    setShowIgnoreValidation(false)
    setIgnoreStartHasInputError(false)
    setIgnoreEndHasInputError(false)
  }, [])

  const handleOpenIgnoreDialog = useCallback(() => {
    setShowIgnoreValidation(false)
    setIgnoreStartHasInputError(false)
    setIgnoreEndHasInputError(false)
    setIsDialogOpen(true)
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
        valueGetter: (params) => addSpaces(params) ?? '',
      },
      { field: 'key', headerName: 'Key', flex: 1, headerAlign: 'center' },
      { field: 'phase', headerName: 'Phase', flex: 1, headerAlign: 'center' },
      {
        field: 'details',
        headerName: 'Details',
        flex: 2,
        headerAlign: 'center',
      },
      {
        field: 'ignored',
        headerName: 'Ignored',
        width: 90,
        align: 'center',
        headerAlign: 'center',
        renderCell: (params: GridRenderCellParams<transformWatchDogLog>) =>
          params.row.ignored ? (
            <CheckCircleIcon fontSize="small" sx={{ color: 'error.main' }} />
          ) : (
            ''
          ),
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
  }, [selectedRows, isIconClicked, handleRowSelection])

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
                onClick={handleOpenIgnoreDialog}
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
    [
      isIconClicked,
      selectedRows,
      handleIconClick,
      handleCancelClick,
      handleOpenIgnoreDialog,
    ]
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

      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 2,
          flexWrap: 'wrap',
          my: 3,
        }}
      >
        <LoadingButton
          loading={isWatchdogLogsLoading}
          startIcon={<PlayArrowIcon />}
          loadingPosition="start"
          variant="contained"
          onClick={handleFetchData}
          sx={{ padding: '10px' }}
        >
          Generate Report
        </LoadingButton>

        {showNoLogsMessage && (
          <Alert severity="error">
            No watchdog logs were found for the selected filters.
          </Alert>
        )}
      </Box>

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
              onError={(error) => setIgnoreStartHasInputError(error != null)}
              onChange={(newValue) =>
                setIgnoreValue('start', newValue, {
                  shouldValidate: showIgnoreValidation,
                })
              }
              slotProps={{
                textField: {
                  error: showIgnoreValidation && ignoreStartHasInputError,
                  helperText:
                    showIgnoreValidation && ignoreStartHasInputError
                      ? 'Invalid date'
                      : undefined,
                },
              }}
            />
            <DatePicker
              label="End Date"
              value={endIgnoreTime}
              onError={(error) => setIgnoreEndHasInputError(error != null)}
              onChange={(newValue) =>
                setIgnoreValue('end', newValue, {
                  shouldValidate: showIgnoreValidation,
                })
              }
              slotProps={{
                textField: {
                  required: true,
                  error:
                    showIgnoreValidation &&
                    (ignoreEndHasInputError || !!ignoreErrors.end),
                  helperText: showIgnoreValidation
                    ? ignoreEndHasInputError
                      ? 'Invalid date'
                      : ignoreErrors.end?.message
                    : undefined,
                },
              }}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDialogClose} variant="outlined">
            Cancel
          </Button>
          <Button
            onClick={() => {
              setShowIgnoreValidation(true)
              void handleIgnoreEventsSubmit()
            }}
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
