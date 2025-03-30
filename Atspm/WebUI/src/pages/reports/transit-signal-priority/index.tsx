import {
  useGetLocationLatestVersionOfAllLocations,
  useGetMeasureType,
  useGetMeasureTypeMeasureOptionPresetsFromKey,
} from '@/api/config/aTSPMConfigurationApi'
import {
  Approach,
  SearchLocation as Location,
  MeasureType,
} from '@/api/config/aTSPMConfigurationApi.schemas'
import { useGetTransitSignalPriorityReportData } from '@/api/reports/aTSPMReportDataApi'
import MultipleLocationsDisplay from '@/components/MultipleLocationsSelect/MultipleLocationsDisplay'
import MultipleLocationsSelect, {
  getLocationWithApproaches,
} from '@/components/MultipleLocationsSelect/MultipleLocationsSelect'
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
import { AxiosError } from 'axios'
import { startOfYesterday, subDays } from 'date-fns'
import { useState } from 'react'

export interface TspLocation extends Location {
  approaches: Approach[]
  designatedPhases: number[]
}

export const getPhases = (location: TspLocation) => {
  return Array.from(
    new Set(location.approaches?.map((a) => a.protectedPhaseNumber) || [])
  ).sort((a, b) => a - b)
}

export type TspErrorState =
  | { type: 'NONE' }
  | { type: 'NO_LOCATIONS' }
  | { type: 'MISSING_PHASES'; locationIDs: Set<string> }
  | { type: '400' }
  | { type: 'UNKNOWN'; message: string }

export interface TspReportOptions {
  selectedDays: Date[]
  locations: TspLocation[]
}

export default function TspReportPage() {
  const [reportOptions, setReportOptions] = useState<TspReportOptions>({
    selectedDays: [subDays(startOfYesterday(), 1), startOfYesterday()],
    locations: [],
  })
  const [selectedReport, setSelectedReport] = useState(0)
  const [errorState, setErrorState] = useState<TspErrorState>({ type: 'NONE' })

  const {
    data: reportResponse,
    mutateAsync: fetchTspReport,
    isLoading: loadingReport,
  } = useGetTransitSignalPriorityReportData()

  const { data: locationsData } = useGetLocationLatestVersionOfAllLocations()
  const locations = locationsData?.value || ([] as TspLocation[])

  const { data: measureTypesData } = useGetMeasureType()
  const measureTypes = measureTypesData?.value || ([] as MeasureType[])

  const { data: savedOptionsData } =
    useGetMeasureTypeMeasureOptionPresetsFromKey(
      measureTypes.find((m) => m.abbreviation === 'TSP')?.id
    )
  const savedOptions = savedOptionsData?.value || ([] as MeasureType[])

  function renderErrorAlert() {
    if (errorState.type === 'NO_LOCATIONS') {
      return (
        <Alert severity="error">Please select one or more locations.</Alert>
      )
    }
    if (errorState.type === 'MISSING_PHASES') {
      return (
        <Alert severity="error">
          Please select phases for the highlighted locations.
        </Alert>
      )
    }
    if (errorState.type === '400') {
      return (
        <Alert severity="error">
          400: The requested resource was not found.
        </Alert>
      )
    }
    if (errorState.type === 'UNKNOWN') {
      return (
        <Alert severity="error">
          Something went wrong: {errorState.message}
        </Alert>
      )
    }
    return null
  }

  function setLocations(locations: TspLocation[]) {
    const hadNoLocations = errorState.type === 'NO_LOCATIONS'
    // check each location, and set it's designated phases to 2 and 6 if available.
    locations.forEach((loc) => {
      const phases = getPhases(loc)
      if (phases.includes(2) && phases.includes(6)) {
        loc.designatedPhases = [2, 6]
      }
    })

    setReportOptions((prev) => ({ ...prev, locations }))

    if (hadNoLocations && locations.length > 0) {
      setErrorState({ type: 'NONE' })
    }
  }

  function handleLocationDelete(location: TspLocation) {
    setLocations(
      reportOptions.locations.filter((loc) => loc.id !== location.id)
    )
  }

  function handleReorderLocations(dropResult: DropResult) {
    if (!dropResult.destination) return
    const items = Array.from(reportOptions.locations)
    const [reorderedItem] = items.splice(dropResult.source.index, 1)
    items.splice(dropResult.destination.index, 0, reorderedItem)
    setLocations(items)
  }

  const handleSavedReportParametersChange = async (
    e: React.ChangeEvent<{ value: unknown }>
  ) => {
    const selectedParameters = savedOptions.find(
      (option) => option.id === Number(e.target.value)
    )
    if (selectedParameters) {
      setSelectedReport(selectedParameters.id)
      const locationsWithApproaches = await getLocationWithApproaches(
        selectedParameters.option.locationsAndPhases.map((loc) => {
          return locations.find(
            (l) => l.locationIdentifier === loc.locationIdentifier
          )
        })
      )
      const locationsWithApproachesWithPhases = locationsWithApproaches.map(
        (loc) => {
          return {
            ...loc,
            designatedPhases: selectedParameters.option.locationsAndPhases.find(
              (l) => l.locationIdentifier === loc.locationIdentifier
            )?.designatedPhases,
          }
        }
      )
      setReportOptions({
        selectedDays: selectedParameters.option.dates.map(
          (date) => new Date(date)
        ),
        locations: locationsWithApproachesWithPhases,
      })
    } else {
      setSelectedReport(0)
      setLocations([])
    }
  }

  function handleUpdateLocation(updatedLocation: TspLocation) {
    const updatedLocations = reportOptions.locations.map((loc) =>
      loc.id === updatedLocation.id ? updatedLocation : loc
    )
    setReportOptions((prev) => ({ ...prev, locations: updatedLocations }))

    if (errorState.type === 'MISSING_PHASES') {
      const newIDs = new Set(errorState.locationIDs)
      if (
        updatedLocation.designatedPhases &&
        updatedLocation.designatedPhases.length > 0
      ) {
        newIDs.delete(String(updatedLocation.id))
      }
      if (newIDs.size === 0) {
        setErrorState({ type: 'NONE' })
      } else {
        setErrorState({ type: 'MISSING_PHASES', locationIDs: newIDs })
      }
    }
  }

  async function generateReport() {
    if (reportOptions.locations.length === 0) {
      setErrorState({ type: 'NO_LOCATIONS' })
      return
    }
    const missingPhases = reportOptions.locations.filter(
      (loc) => !loc.designatedPhases || loc.designatedPhases.length === 0
    )
    if (missingPhases.length > 0) {
      setErrorState({
        type: 'MISSING_PHASES',
        locationIDs: new Set(missingPhases.map((loc) => String(loc.id))),
      })
      return
    }
    setErrorState({ type: 'NONE' })
    try {
      await fetchTspReport({
        data: {
          locationsAndPhases: reportOptions.locations.map((loc) => ({
            locationIdentifier: loc.locationIdentifier,
            designatedPhases: loc.designatedPhases,
          })),
          dates: reportOptions.selectedDays.map((date) => date.toISOString()),
          locationIdentifier: '0',
          start: new Date().toISOString(),
          end: new Date().toISOString(),
        },
      })
    } catch (err: unknown) {
      if (err instanceof AxiosError && err.response?.status === 404) {
        setErrorState({ type: '400' })
      } else if (err instanceof Error) {
        setErrorState({ type: 'UNKNOWN', message: err.message || 'Error' })
      } else {
        setErrorState({ type: 'UNKNOWN', message: 'Error' })
      }
    }
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
            onChange={handleSavedReportParametersChange}
            inputProps={{ id: 'route-parameters-select' }}
          >
            <MenuItem key={0} value={0}>
              None
            </MenuItem>
            {savedOptions.map((options) => (
              <MenuItem key={options.id} value={options.id}>
                {options.name}
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
                errorState={errorState}
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
            sx={{ padding: '10px', mb: 2 }}
            onClick={generateReport}
          >
            Generate Report
          </LoadingButton>
          {renderErrorAlert()}
        </Box>
        {reportResponse && (
          <TspReport report={reportResponse} reportOptions={reportOptions} />
        )}
      </Box>
    </ResponsivePageLayout>
  )
}
