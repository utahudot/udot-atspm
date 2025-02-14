import { SearchLocation as Location } from '@/api/config/aTSPMConfigurationApi.schemas'
import MultipleLocationsDisplay from '@/components/MultipleLocationsSelect/MultipleLocationsDisplay'
import MultipleLocationsSelect from '@/components/MultipleLocationsSelect/MultipleLocationsSelect'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import MultiDaySelect from '@/features/tspReport/components/DateCalendar'
import TspReport from '@/features/tspReport/components/TspReport'
import { DropResult } from '@hello-pangea/dnd'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import {
  Box,
  Button,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
} from '@mui/material'
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

interface TspReportOptions {
  selectedDays: Date[]
  locations: Location[]
}

export default function TspReportPage() {
  const [reportOptions, setReportOptions] = useState<TspReportOptions>({
    selectedDays: [],
    locations: [],
  })
  const [selectedReport, setSelectedReport] = useState(0)

  const setLocations = (locations: Location[]) => {
    setReportOptions((prev) => ({ ...prev, locations }))
  }

  const handleLocationDelete = (location: Location) => {
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
      setLocations(found.locations)
    } else {
      setSelectedReport(0)
      setLocations([])
    }
  }

  console.log(reportOptions)

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
        <LoadingButton
          startIcon={<PlayArrowIcon />}
          variant="contained"
          sx={{ padding: '10px', marginTop: '10px' }}
        >
          Generate Report
        </LoadingButton>
        <TspReport />
      </Box>
    </ResponsivePageLayout>
  )
}
