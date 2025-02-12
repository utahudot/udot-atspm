import { Location } from '@/api/config/aTSPMConfigurationApi.schemas'
import MultipleLocationsDisplay from '@/components/MultipleLocationsSelect/MultipleLocationsDisplay'
import MultipleLocationsSelect from '@/components/MultipleLocationsSelect/MultipleLocationsSelect'
import SelectDateTime from '@/components/selectTimeSpan'
import { MultiSelectCheckbox } from '@/features/aggregateData/components/chartOptions/MultiSelectCheckbox'
import TspReport from '@/features/tspReport/components/TspReport'
import { DropResult } from '@hello-pangea/dnd'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import {
  Box,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
} from '@mui/material'
import { startOfToday, startOfYesterday } from 'date-fns'
import { useState } from 'react'

const daysOfWeek = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
]

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
  start: Date
  end: Date
  locations: Location[]
  daysOfWeek: number[]
}

const TspReportPage = () => {
  const [reportOptions, setReportOptions] = useState<TspReportOptions>({
    start: startOfYesterday(),
    end: startOfToday(),
    locations: [],
    daysOfWeek: [2, 3, 4],
  })
  const [selectedReport, setSelectedReport] = useState(0)

  // const { isLoading, generateReport } = useGenerateReport()

  const setLocations = (locations: Location[]) => {
    setReportOptions((prev) => ({ ...prev, locations }))
  }

  const setDaysOfWeek = (daysOfWeek: number[]) => {
    setReportOptions((prev) => ({ ...prev, daysOfWeek }))
  }

  const setStart = (start: Date) => {
    setReportOptions((prev) => ({ ...prev, start }))
  }

  const setEnd = (end: Date) => {
    setReportOptions((prev) => ({ ...prev, end }))
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
    const selectedReport = savedReportsMock.find(
      (report) => report.id === e.target.value
    )
    if (selectedReport) {
      setSelectedReport(selectedReport.id)
      setLocations(selectedReport.locations)
      setDaysOfWeek(selectedReport.daysOfWeek)
      setStart(selectedReport.start)
      setEnd(selectedReport.end)
    } else {
      setSelectedReport(0)
      setLocations([])
      setDaysOfWeek([0, 1, 2, 3, 4, 5, 6])
      setStart(startOfYesterday())
      setEnd(startOfToday())
    }
  }

  return (
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
          {savedReportsMock?.map((report) => (
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
        <Paper sx={{ p: 3, maxWidth: '320px' }}>
          <SelectDateTime
            changeStartDate={setStart}
            changeEndDate={setEnd}
            startDateTime={reportOptions.start}
            endDateTime={reportOptions.end}
            warning={'Holiday warning'}
          />
        </Paper>
        <Box>
          <MultiSelectCheckbox
            header="Days of Week"
            itemList={daysOfWeek}
            selectedItems={reportOptions.daysOfWeek}
            setSelectedItems={setDaysOfWeek}
          />
        </Box>
      </Box>
      <LoadingButton
        // loading={isLoading}
        loadingPosition="start"
        startIcon={<PlayArrowIcon />}
        variant="contained"
        sx={{ padding: '10px', marginTop: '10px' }}
        // onClick={handleGenerateReport}
      >
        Generate Report
      </LoadingButton>
      <TspReport />
    </Box>
  )
}
export default TspReportPage
