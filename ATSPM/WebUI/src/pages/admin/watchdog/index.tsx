import { OptionalWatchDogFilters } from '@/components/MapFilters/OptionalWatchDogFilters'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { StyledPaper } from '@/components/StyledPaper'
import SelectDateTime from '@/components/selectTimeSpan/SelectTimeSpan'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import { Location } from '@/features/locations/types/Location'
import {
  getIssueTypes,
  getWatchdogLogs,
} from '@/features/watchdog/api/getWatchdogLogs'
import Authorization from '@/lib/Authorization'
import { useSidebarStore } from '@/stores/sidebar'
import DescriptionOutlinedIcon from '@mui/icons-material/DescriptionOutlined'
import { LoadingButton } from '@mui/lab'
import {
  Alert,
  Box,
  Paper,
  Theme,
  useMediaQuery,
  useTheme,
} from '@mui/material'

import {
  DataGrid,
  GridCellParams,
  GridColDef,
  GridToolbar,
  GridToolbarExport,
  gridClasses,
} from '@mui/x-data-grid'
import { startOfToday, startOfTomorrow } from 'date-fns'
import { useEffect, useState } from 'react'

const tableCellStyle = {
  '& table': {
    width: '100%',
    borderCollapse: 'collapse',
    border: '1px solid black',
  },
  '& th, & td': {
    border: '1px solid black',
    padding: '8px',
    textAlign: 'left',
  },
}

const WatchDog = () => {
  const today = new Date()
  const [location, setLocation] = useState<Location | null>(null)
  const [locations, setLocations] = useState<Location[]>([])
  const [locationIdentifier, setLocationidentifier] = useState<string | null>(
    null
  )
  const [startDateTime, setStartDateTime] = useState(startOfToday())
  const [endDateTime, setEndDateTime] = useState(startOfTomorrow())
  const { isSidebarOpen } = useSidebarStore();

  const [areaId, setAreaId] = useState<number | null>(null)
  const [regionId, setRegionId] = useState<number | null>(null)
  const [jurisdictionId, setJurisdictionId] = useState<number | null>(null)
  const { data } = useLatestVersionOfAllLocations()
  const [isCheckingRecords, setIsCheckingRecords] = useState<boolean>(false)
  const [wdData, setWData] = useState<any>(null)
  const [selectedIssueType, setSelectedIssueType] = useState<any>(null)
  const [issueTypes, setIssueTypes] = useState<any>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState(null)

  const theme = useTheme()
  const mode = theme.palette.mode
  const isSmallScreen = useMediaQuery((theme: Theme) =>
    theme.breakpoints.down('sm')
  )
  const isMobileView = useMediaQuery(theme.breakpoints.down('md'))

  const start = startDateTime
  const end = endDateTime

  const handleStartDateTimeChange = (date: Date) => {
    setStartDateTime(date)
  }

  const handleEndDateTimeChange = (date: Date) => {
    setEndDateTime(date)
  }

  const userFiltersInput = {
    start,
    end,
    areaId,
    regionId,
    jurisdictionId,
    issueType: selectedIssueType,
    locationIdentifier,
  }

  const wrappedCellStyle = (params: GridCellParams) => {
    return (
      <div
        style={{
          width: '100%',
          maxHeight: '100px', // Limit the height of the cells
          overflow: 'auto', // Add scroll bars when the content exceeds the cell's height
          whiteSpace: 'normal',
          overflowWrap: 'break-word',
        }}
      >
        {params.value}
      </div>
    )
  }

  const centeredCellStyle = (params: GridCellParams) => {
    return (
      <div style={{ textAlign: 'center', width: '100%' }}>{params.value}</div>
    )
  }

  const columns: GridColDef[] = [
    {
      field: 'LocationIdentifier',
      headerName: 'Location',
      flex: 1,
      minWidth: isSmallScreen ? 190 : 70,
      headerAlign: 'center',
      renderCell: centeredCellStyle,
    },
    {
      field: 'Timestamp',
      headerName: 'Timestamp',
      flex: 1,
      minWidth: isSmallScreen ? 150 : 70,
      headerAlign: 'center',
      renderCell: wrappedCellStyle,
    },
    {
      field: 'RegionDescription',
      headerName: 'Region',
      flex: 1,
      minWidth: isSmallScreen ? 150 : 65,
      headerAlign: 'center',
      renderCell: centeredCellStyle,
    },
    {
      field: 'JurisdictionName',
      headerName: 'Jurisdiction',
      flex: 1,
      minWidth: isSmallScreen ? 150 : 80,
      headerAlign: 'center',
      renderCell: wrappedCellStyle,
    },
    {
      field: 'Areas',
      headerName: 'Areas',
      flex: 1,
      minWidth: isSmallScreen ? 150 : 70,
      headerAlign: 'center',
      renderCell: centeredCellStyle,
    },
    {
      field: 'IssueType',
      headerName: 'Issue Type',
      flex: 1,
      minWidth: isSmallScreen ? 150 : 90,
      headerAlign: 'center',
      renderCell: centeredCellStyle,
    },
    {
      field: 'Phase',
      headerName: 'Phase',
      flex: 1,
      minWidth: isSmallScreen ? 150 : 30,
      headerAlign: 'center',
      renderCell: centeredCellStyle,
    },
    {
      field: 'Details',
      headerName: 'Details',
      flex: 2,
      minWidth: isSmallScreen ? 150 : undefined,
      headerAlign: 'center',
      renderCell: wrappedCellStyle,
    },
  ]

  let filteredData = []
  if (wdData && wdData.logEvents) {
    filteredData = wdData.logEvents.map((obj: any, key: number) => {
      let areaName = obj.areas.map((a: Area) => a.name)
      return {
        id: key,
        LocationIdentifier: obj.locationIdentifier,
        Timestamp: obj.timestamp,
        RegionDescription: obj.regionDescription,
        JurisdictionName: obj.jurisdictionName,
        Areas: areaName,
        IssueType: `${obj.issueType} - ${issueTypes[obj.issueType]}`,
        Details: obj.details,
        Phase: obj.phase,
      }
    })
  }

  const fetchData = async () => {
    setError(null)
    setIsCheckingRecords(true)
    try {
      const response = await getWatchdogLogs(userFiltersInput)
      if (response) {
        setWData(response)
      }
    } catch (error) {
      console.error('Error fetching data:', error)
      setWData(null)
      setError(error.response.data)
    } finally {
      setIsCheckingRecords(false)
    }
  }

  useEffect(() => {
    const fetchIssueTypes = async () => {
      try {
        setIsLoading(true)
        const response = await getIssueTypes()

        if (Array.isArray(response)) {
          let issueTypesObj: { [key: string]: string } = {}
          response.forEach((issueType) => {
            let formattedName = issueType.name.replace(/([A-Z])/g, ' $1').trim()
            issueTypesObj[issueType.id] = formattedName
          })
          setIssueTypes(issueTypesObj)
        }
      } catch (err) {
        setError(err as Error)
      } finally {
        setIsLoading(false)
      }
    }

    fetchIssueTypes()
  }, [])

  useEffect(() => {
    if (data) {
      setLocations(data.value)
    }
  }, [data])

  useEffect(() => {
    if (location) {
      setLocationidentifier(location.locationIdentifier)
    } else {
      setLocationidentifier(null)
    }
  }, [location?.locationIdentifier])

  const requiredClaim = 'Watchdog:View'

  return (
    <>
      <Authorization requiredClaim={requiredClaim}>
        <ResponsivePageLayout title={'Watchdog'}>
          <Box
            sx={{
              display: 'flex',
              gap: 2,
              flexDirection: 'row',
              flexWrap: 'wrap',
            }}
          >
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
              <StyledPaper
                sx={{
                  flexGrow: 1,
                  width: '23.188rem',
                  padding: theme.spacing(3),
                }}
              >
                <SelectDateTime
                  startDateTime={startDateTime}
                  endDateTime={endDateTime}
                  changeStartDate={handleStartDateTimeChange}
                  changeEndDate={handleEndDateTimeChange}
                  noCalendar={isMobileView}
                />
              </StyledPaper>
            </Box>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
              <StyledPaper
                sx={{
                  flexGrow: 1,
                  width: '23.188rem',
                  padding: theme.spacing(3),
                }}
              >
                {/* <Typography
            sx={{
              color: 'gray',
              fontSize: '.9rem',
              padding: '0px 0px 15px 0px',
            }}
          >
            Optional Filters
          </Typography> */}
                <OptionalWatchDogFilters
                  issueType={issueTypes}
                  setSelectedIssueType={setSelectedIssueType}
                  setAreaId={setAreaId}
                  setRegionId={setRegionId}
                  setJurisdictionId={setJurisdictionId}
                />
              </StyledPaper>
            </Box>
          </Box>
          {
            <Box
              sx={{
                display: 'flex',
                width: '100%',
              }}
            >
              <LoadingButton
                loading={isLoading}
                loadingPosition="start"
                startIcon={<DescriptionOutlinedIcon />}
                variant="contained"
                sx={{ margin: '20px 0', padding: '10px' }}
                onClick={fetchData}
              >
                Generate Report
              </LoadingButton>
              {wdData?.logEvents.length === 0 && (
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    marginLeft: '1rem',
                  }}
                >
                  <Alert severity="warning">
                    No data available in date range
                  </Alert>
                </Box>
              )}
              {error && (
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    marginLeft: '1rem',
                  }}
                >
                  <Alert severity="error">{error}</Alert>
                </Box>
              )}
            </Box>
          }
    <Box
    sx={{
      display: 'flex',
      flexDirection: 'column',
      minWidth: '375px',
      padding:'1px',
      [theme.breakpoints.up('md')]: {
        maxWidth: isSidebarOpen ? 'calc(100vw - 340px)' : '100%',
        

      }
    }}
  >
          {filteredData.length > 0 && (
            <Paper>
              <DataGrid
                getRowHeight={() => 'auto'}
                rows={filteredData}
                columns={columns}
                disableDensitySelector
                slots={{ toolbar: GridToolbar }}
                components={{ Toolbar: GridToolbarExport }}
                componentsProps={{
                  toolbar: {
                    csvOptions: {
                      fileName: `WatchDogData-${start.toLocaleDateString(
                        'en-US',
                        {
                          year: 'numeric',
                          month: '2-digit',
                          day: '2-digit',
                        }
                      )}-${end.toLocaleDateString('en-US', {
                        year: 'numeric',
                        month: '2-digit',
                        day: '2-digit',
                      })}`,
                    },
                    printOptions: {
                      fileName: `WatchDogData-${start.toLocaleDateString(
                        'en-US',
                        {
                          year: 'numeric',
                          month: '2-digit',
                          day: '2-digit',
                        }
                      )}-${end.toLocaleDateString('en-US', {
                        year: 'numeric',
                        month: '2-digit',
                        day: '2-digit',
                      })}`,
                    },
                  },
                }}
                pageSizeOptions={[{ value: 100, label: '100' }]}
                sx={{
                  [`& .${gridClasses.cell}`]: {
                    paddingTop: '20px',
                    paddingBottom: '20px',
                    ...tableCellStyle, 
                  },
                  [`& .${gridClasses.columnHeaders}`]: {
                    position: 'sticky',
                    top: '30px',
                    backgroundColor: mode === 'light' ? 'white' : '#1F2A40',
                    zIndex: '1',
                  },
                  [`& .${gridClasses.toolbarContainer}`]: {
                    position: 'sticky',
                    top: '0',
                    backgroundColor: mode === 'light' ? 'white' : '#1F2A40',
                    zIndex: '1',
                  },
                  [`& .${gridClasses.main}`]: {
                    overflow: 'inherit',
                  },
                }}
              />
            </Paper>
          )}
          </Box>
        </ResponsivePageLayout>
      </Authorization>
    </>
  )
}

export default WatchDog
