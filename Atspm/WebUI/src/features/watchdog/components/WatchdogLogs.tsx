import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'
import OptionalWatchDogFilters from '@/components/MapFilters/OptionalWatchDogFilters'
import { StyledPaper } from '@/components/StyledPaper'
import SelectDateTime from '@/components/selectTimeSpan/SelectTimeSpan'
import {
  LogEventsData,
  useGetIssueTypes,
  useGetWatchdogLogs,
} from '@/features/watchdog/api/getWatchdogLogs'
import NotificationsPausedIcon from '@mui/icons-material/NotificationsPaused'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import { Alert, Box, Button, Checkbox, Paper, Tooltip } from '@mui/material'
import {
  DataGrid,
  GridColDef,
  GridToolbarColumnsButton,
  GridToolbarContainer,
  GridToolbarDensitySelector,
  GridToolbarExport,
  GridToolbarFilterButton,
  GridToolbarProps,
  gridClasses,
} from '@mui/x-data-grid'
import { AxiosError } from 'axios'
import { startOfToday, startOfTomorrow } from 'date-fns'
import { useEffect, useState } from 'react'

const WatchDogLogs = () => {
  const [locationIdentifier, setLocationIdentifier] = useState<string | null>(
    null
  )
  const [startDateTime, setStartDateTime] = useState<Date>(startOfToday())
  const [endDateTime, setEndDateTime] = useState<Date>(startOfTomorrow())
  const [areaId, setAreaId] = useState<number | null>(null)
  const [regionId, setRegionId] = useState<number | null>(null)
  const [jurisdictionId, setJurisdictionId] = useState<number | null>(null)
  const [selectedIssueType, setSelectedIssueType] = useState<number | null>(
    null
  )
  const [issueTypes, setIssueTypes] = useState<Record<string, string> | null>(
    null
  )
  const [isIconClicked, setIsIconClicked] = useState(false)
  const [selectedRows, setSelectedRows] = useState<number[]>([])

  const { data: issueTypesData } = useGetIssueTypes()
  const {
    data: watchdogLogsData,
    isLoading: isWatchdogLogsLoading,
    error: watchdogLogsError,
    mutateAsync: fetchWatchdogLogs,
  } = useGetWatchdogLogs()

  useEffect(() => {
    if (issueTypesData) {
      const mappedIssueTypes: Record<string, string> = issueTypesData.reduce(
        (acc, issueType) => ({ ...acc, [issueType.id]: issueType.name }),
        {}
      )
      setIssueTypes(mappedIssueTypes)
    }
  }, [issueTypesData])

  const handleFetchData = () => {
    fetchWatchdogLogs({
      start: startDateTime,
      end: endDateTime,
      areaId,
      regionId,
      jurisdictionId,
      issueType: selectedIssueType,
      locationIdentifier,
    })
  }

  const handleIconClick = () => {
    setIsIconClicked(!isIconClicked)
  }

  const handleCancelClick = () => {
    setIsIconClicked(false)
    setSelectedRows([]) // Reset selected rows when cancelling
  }

  const handleIgnoreEventsClick = () => {
    console.log('Ignore Events button clicked')
    console.log('Selected Rows:', selectedRows)
    // Future implementation: Open a popup with a time range input
  }

  const checkboxColumn: GridColDef = {
    field: 'checkbox',
    headerName: '',
    width: 50,
    renderCell: (params) => {
      const isSelected = selectedRows.includes(params.id as number)
      const handleChange = (event) => {
        if (event.target.checked) {
          setSelectedRows((prev) => [...prev, params.id as number])
        } else {
          setSelectedRows((prev) => prev.filter((id) => id !== params.id))
        }
      }
      return (
        <Checkbox
          checked={isSelected}
          onChange={handleChange}
          color="primary"
        />
      )
    },
  }

  const columns: GridColDef[] = [
    {
      field: 'LocationIdentifier',
      headerName: 'Location',
      flex: 1,
      headerAlign: 'center',
      renderCell: (params) => (
        <div style={{ textAlign: 'center', width: '100%' }}>{params.value}</div>
      ),
    },
    {
      field: 'Timestamp',
      headerName: 'Timestamp',
      flex: 1,
      headerAlign: 'center',
      renderCell: (params) => (
        <div style={{ whiteSpace: 'normal', wordWrap: 'break-word' }}>
          {params.value}
        </div>
      ),
    },
    {
      field: 'RegionDescription',
      headerName: 'Region',
      flex: 1,
      headerAlign: 'center',
      renderCell: (params) => (
        <div style={{ textAlign: 'center', width: '100%' }}>{params.value}</div>
      ),
    },
    {
      field: 'JurisdictionName',
      headerName: 'Jurisdiction',
      flex: 1,
      headerAlign: 'center',
      renderCell: (params) => (
        <div style={{ whiteSpace: 'normal', wordWrap: 'break-word' }}>
          {params.value}
        </div>
      ),
    },
    {
      field: 'Areas',
      headerName: 'Areas',
      flex: 1,
      headerAlign: 'center',
      renderCell: (params) => (
        <div style={{ textAlign: 'center', width: '100%' }}>{params.value}</div>
      ),
    },
    {
      field: 'IssueType',
      headerName: 'Issue Type',
      flex: 1,
      headerAlign: 'center',
      renderCell: (params) => (
        <div style={{ textAlign: 'center', width: '100%' }}>
          {issueTypes
            ? `${params.value} - ${issueTypes[params.value]}`
            : params.value}
        </div>
      ),
    },
    {
      field: 'Phase',
      headerName: 'Phase',
      flex: 1,
      headerAlign: 'center',
      renderCell: (params) => (
        <div style={{ textAlign: 'center', width: '100%' }}>{params.value}</div>
      ),
    },
    {
      field: 'Details',
      headerName: 'Details',
      flex: 2,
      headerAlign: 'center',
      renderCell: (params) => (
        <div
          style={{
            whiteSpace: 'normal',
            wordWrap: 'break-word',
            overflow: 'auto',
            maxHeight: '100px',
          }}
        >
          {params.value}
        </div>
      ),
    },
  ]

  const columnsWithCheckbox = isIconClicked
    ? [checkboxColumn, ...columns]
    : columns

  const processedRows =
    (watchdogLogsData as unknown as LogEventsData)?.logEvents?.map(
      (logEvent, index: number) => ({
        id: index,
        LocationIdentifier: logEvent.locationIdentifier,
        Timestamp: logEvent.timestamp,
        RegionDescription: logEvent.regionDescription,
        JurisdictionName: logEvent.jurisdictionName,
        Areas: logEvent.areas.map((area) => area.name).join(', '),
        IssueType: logEvent.issueType,
        Phase: logEvent.phase,
        Details: logEvent.details,
      })
    ) || []

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

  const CustomToolbar = (props: GridToolbarProps) => {
    const { csvOptions, printOptions } = props

    return (
      <GridToolbarContainer>
        <Box sx={{ flexGrow: 1 }}>
          <GridToolbarColumnsButton />
          <GridToolbarFilterButton />
          <GridToolbarDensitySelector />
          <GridToolbarExport
            csvOptions={csvOptions}
            printOptions={printOptions}
          />
        </Box>
        <Box
          sx={{ display: 'flex', alignItems: 'center', position: 'relative' }}
        >
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              transform: isIconClicked ? 'translateX(-300px)' : 'translateX(0)',
            }}
          >
            <Button
              onClick={handleIconClick}
              sx={{ color: 'red', minWidth: 'auto' }}
              aria-label="Ignore Events"
            >
              <Tooltip title="Ignore Events" placement="top">
                <NotificationsPausedIcon />
              </Tooltip>
            </Button>
          </Box>
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
                onClick={handleIgnoreEventsClick}
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
    )
  }

  return (
    <>
      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
        <StyledPaper sx={{ width: '23rem', padding: 3 }}>
          <SelectDateTime
            startDateTime={startDateTime}
            endDateTime={endDateTime}
            changeStartDate={setStartDateTime}
            changeEndDate={setEndDateTime}
          />
        </StyledPaper>
        <StyledPaper sx={{ width: '23rem' }}>
          <StyledComponentHeader header="Optional Filters" />
          <Box sx={{ padding: 3 }}>
            <OptionalWatchDogFilters
              issueType={issueTypes}
              setSelectedIssueType={setSelectedIssueType}
              setAreaId={setAreaId}
              setRegionId={setRegionId}
              setJurisdictionId={setJurisdictionId}
              setLocationIdentifier={setLocationIdentifier}
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
              columns={columnsWithCheckbox}
              disableDensitySelector
              slots={{ toolbar: CustomToolbar }}
              slotProps={{
                toolbar: {
                  csvOptions: { fileName },
                  printOptions: { fileName },
                },
              }}
              pageSizeOptions={[{ value: 100, label: '100' }]}
              sx={{
                [`& .${gridClasses.cell}`]: {
                  paddingTop: '20px',
                  paddingBottom: '20px',
                },
                [`& .${gridClasses.columnHeaders}`]: {
                  position: 'sticky',
                  backgroundColor: 'white',
                  zIndex: '1',
                },
                [`& .${gridClasses.toolbarContainer}`]: {
                  position: 'sticky',
                  top: '0',
                  backgroundColor: 'white',
                  zIndex: '1',
                  pb: '5px',
                },
              }}
            />
          </Paper>
        )}
      </Box>
    </>
  )
}

export default WatchDogLogs
