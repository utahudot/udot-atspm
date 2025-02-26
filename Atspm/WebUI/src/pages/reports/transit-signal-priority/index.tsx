import {
  Approach,
  SearchLocation as Location,
} from '@/api/config/aTSPMConfigurationApi.schemas'
import { useGetTransitSignalPriorityReportData } from '@/api/reports/aTSPMReportDataApi'
import MultipleLocationsDisplay from '@/components/MultipleLocationsSelect/MultipleLocationsDisplay'
import MultipleLocationsSelect from '@/components/MultipleLocationsSelect/MultipleLocationsSelect'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import MultiDaySelect from '@/features/tspReport/components/DateCalendar'
import TspReport from '@/features/tspReport/components/TspReport'
import { DropResult } from '@hello-pangea/dnd'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import {
  Alert,
  Box,
  Button,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
} from '@mui/material'
import { startOfYesterday } from 'date-fns'
import { useState } from 'react'

const savedReportsMock = [
  {
    id: 1,
    name: 'Report 1',
    start: new Date(),
    end: new Date(),
    locations: [
      { id: 1, name: 'Location 1' },
      { id: 2, name: 'Location 2' },
    ],
    daysOfWeek: [1, 2, 3],
  },
  {
    id: 2,
    name: 'Report 2',
    start: new Date('2021-10-01'),
    end: new Date('2021-10-31'),
    locations: [
      { id: 3, name: 'Location 3' },
      { id: 4, name: 'Location 4' },
      { id: 5, name: 'Location 5' },
      { id: 6, name: 'Location 6' },
    ],
    daysOfWeek: [4, 5, 6],
  },
]

export interface TspLocation extends Location {
  approaches: Approach[]
  designatedPhases: number[]
}

interface TspReportOptions {
  selectedDays: Date[]
  locations: TspLocation[]
}

export default function TspReportPage() {
  const [reportOptions, setReportOptions] = useState<TspReportOptions>({
    selectedDays: [startOfYesterday()],
    locations: [],
  })
  const [selectedReport, setSelectedReport] = useState(0)

  // NEW: Used for error handling
  const [userHasTriedRun, setUserHasTriedRun] = useState(false) // Tracks if user pressed "Generate Report"
  const [errorLocations, setErrorLocations] = useState<Set<string>>(new Set()) // Which location IDs have no phases
  const [showErrorAlert, setShowErrorAlert] = useState(false) // Controls the MUI Alert

  const {
    data: reportResponse,
    mutateAsync: fetchTspReport,
    isLoading: loadingReport,
  } = useGetTransitSignalPriorityReportData()

  const setLocations = (locations: TspLocation[]) => {
    setReportOptions((prev) => ({ ...prev, locations }))
  }

  const handleLocationDelete = (location: TspLocation) => {
    setLocations(
      reportOptions.locations.filter((loc) => loc.id !== location.id)
    )
  }

  const handleReorderLocations = (dropResult: DropResult) => {
    if (!dropResult.destination) return
    const items = Array.from(reportOptions.locations)
    const [reorderedItem] = items.splice(dropResult.source.index, 1)
    items.splice(dropResult.destination.index, 0, reorderedItem)
    setLocations(items)
  }

  const handleSavedReportChange = (
    e: React.ChangeEvent<{ value: unknown }>
  ) => {
    const found = savedReportsMock.find((r) => r.id === e.target.value)
    if (found) {
      setSelectedReport(found.id)
      // This mock data has incomplete TspLocation shape. (Just ignoring in this example)
      setLocations(found.locations as TspLocation[])
    } else {
      setSelectedReport(0)
      setLocations([])
    }
  }

  // Update a single location (e.g. designatedPhases)
  const handleUpdateLocation = (updatedLocation: TspLocation) => {
    const updateLocations = reportOptions.locations.map((loc) =>
      loc.id === updatedLocation.id ? updatedLocation : loc
    )
    setLocations(updateLocations)

    // If user already tried to run, let's re-check if this location still needs error highlight
    if (userHasTriedRun) {
      const newErrorLocations = new Set(errorLocations)
      // If user selected phases for a previously error location, remove from error set
      if (updatedLocation.designatedPhases?.length > 0) {
        newErrorLocations.delete(String(updatedLocation.id))
      }
      // If no more errors remain, hide the alert
      if (newErrorLocations.size === 0) {
        setShowErrorAlert(false)
      }
      setErrorLocations(newErrorLocations)
    }
  }

  const generateReport = () => {
    // Let them press, but if any location is missing phases, show error & do NOT run
    const missingPhases = reportOptions.locations.filter(
      (loc) => !loc.designatedPhases || loc.designatedPhases.length === 0
    )

    if (missingPhases.length > 0) {
      // Prepare error sets
      const errorSet = new Set<string>(
        missingPhases.map((loc) => String(loc.id))
      )
      setErrorLocations(errorSet)
      setUserHasTriedRun(true)
      setShowErrorAlert(true)
      // Do not run the diagram
      return
    }

    // If no missing phases, clear error states and run the chart
    setUserHasTriedRun(false)
    setShowErrorAlert(false)
    setErrorLocations(new Set())

    fetchTspReport({
      data: {
        locationsAndPhases: reportOptions.locations.map((loc) => ({
          locationIdentifier: loc.locationIdentifier,
          designatedPhases: loc.designatedPhases,
        })),
        dates: reportOptions.selectedDays.map((date) => date.toISOString()),
      },
    })
  }

  return (
    <ResponsivePageLayout title="Transit Signal Priority">
      <Box>
        <FormControl
          sx={{ minWidth: 200, marginBottom: 2, bgcolor: 'paper.default' }}
        >
          <InputLabel htmlFor="route-parameters-select">
            Saved Report Parameters
          </InputLabel>
          <Select
            label="Saved Report Parameters"
            variant="outlined"
            value={selectedReport}
            onChange={handleSavedReportChange}
            inputProps={{ id: 'route-parameters-select' }}
          >
            <MenuItem key={0} value={0}>
              None
            </MenuItem>
            {savedReportsMock.map((report) => (
              <MenuItem key={report.id} value={report.id}>
                {report.name} - {report.start.toDateString()} -{' '}
                {report.end.toDateString()}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        <Box sx={{ display: 'flex', flexDirection: 'row', gap: 2 }}>
          <Paper sx={{ p: 3, display: 'flex', flexDirection: 'row' }}>
            <Box sx={{ width: '400px' }}>
              <MultipleLocationsSelect
                selectedLocations={reportOptions.locations}
                setLocations={setLocations}
              />
            </Box>
            <Divider orientation="vertical" sx={{ mx: 2 }} />
            <Box>
              <MultipleLocationsDisplay
                locations={reportOptions.locations}
                onLocationDelete={handleLocationDelete}
                onDeleteAllLocations={() => setLocations([])}
                onLocationsReorder={handleReorderLocations}
                onUpdateLocation={handleUpdateLocation}
                // Pass the relevant states so child can highlight missing phases & show error text only if user tried
                userHasTriedRun={userHasTriedRun}
                errorLocations={errorLocations}
              />
            </Box>
          </Paper>
          <Paper sx={{ maxWidth: '390px' }}>
            <Box display="flex" justifyContent="flex-end">
              <Button
                size="small"
                variant="outlined"
                sx={{ m: 2, mb: 0 }}
                onClick={() =>
                  setReportOptions((prev) => ({
                    ...prev,
                    selectedDays: [],
                  }))
                }
              >
                Clear All
              </Button>
            </Box>
            <MultiDaySelect
              selectedDays={reportOptions.selectedDays}
              onSelectedDaysChange={(newSelectedDays) =>
                setReportOptions((prev) => ({
                  ...prev,
                  selectedDays: newSelectedDays,
                }))
              }
            />
          </Paper>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mt: 2 }}>
          <LoadingButton
            startIcon={<PlayArrowIcon />}
            loading={loadingReport}
            variant="contained"
            sx={{ padding: '10px' }}
            onClick={generateReport}
          >
            Generate Report
          </LoadingButton>
          {showErrorAlert && (
            <Alert severity="error">
              Please select at least one phase for each location before running
              the report.
            </Alert>
          )}
        </Box>
        {reportResponse && <TspReport report={reportResponse} />}
      </Box>
    </ResponsivePageLayout>
  )
}
