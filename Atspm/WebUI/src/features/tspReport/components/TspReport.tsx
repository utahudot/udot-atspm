import {
  postMeasureOptionPreset,
  useGetMeasureType,
} from '@/api/config/aTSPMConfigurationApi'
import { MeasureType } from '@/api/config/aTSPMConfigurationApi.schemas'
import { TransitSignalPriorityResult } from '@/api/reports/aTSPMReportDataApi.schemas'
import { TspReportOptions } from '@/pages/reports/transit-signal-priority'
import { useNotificationStore } from '@/stores/notifications'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import {
  Box,
  Button,
  Modal,
  Paper,
  Tab,
  TextField,
  Typography,
} from '@mui/material'
import {
  DataGrid,
  GridColDef,
  GridToolbarContainer,
  GridToolbarExport,
} from '@mui/x-data-grid'
import { useRef, useState } from 'react'

interface TspReportProps {
  report: TransitSignalPriorityResult[]
  reportOptions: TspReportOptions
}

const TspReport = ({ report, reportOptions }: TspReportProps) => {
  const { addNotification } = useNotificationStore()

  const { data: measureTypesData } = useGetMeasureType()
  const measureTypes = measureTypesData?.value || ([] as MeasureType[])

  const contentRef = useRef<HTMLDivElement>(null)
  const [currentTab, setCurrentTab] = useState('0')
  const [isModalOpen, setModalOpen] = useState(false)
  const [paramName, setParamName] = useState('')

  const handleTabChange = (_: React.SyntheticEvent, newTab: string) => {
    setCurrentTab(newTab)
  }

  const handleSaveParameters = () => {
    setModalOpen(true)
  }

  const handleCloseModal = () => {
    setModalOpen(false)
    setParamName('')
  }

  const handleConfirmSave = () => {
    const reportParams = {
      locationsAndPhases: reportOptions.locations.map((loc) => ({
        locationIdentifier: loc.locationIdentifier,
        designatedPhases: loc.designatedPhases,
      })),
      dates: reportOptions.selectedDays.map((date) => date.toISOString()),
      start: new Date().toISOString(),
      end: new Date().toISOString(),
      locationIdentifier: '0',
    }
    postMeasureOptionPreset({
      name: paramName,
      option: reportParams,
      measureTypeId: measureTypes.find((m) => m.name === 'TSP')?.id,
    })
    addNotification({
      title: 'Parameters saved successfully',
      type: 'success',
    })
    handleCloseModal()
  }

  return (
    <>
      <TabContext value={currentTab}>
        <Box sx={{ display: 'flex', justifyContent: 'right', my: 1 }}>
          <Button variant="outlined" onClick={handleSaveParameters}>
            {/* <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2, gap: 2 }}>
        <Button
          size="small"
          variant="outlined"
          startIcon={<PrintIcon />}
          color="primary"
          onClick={handlePrint}
        >
          Print
        </Button>

        <Button
          size="small"
          variant="outlined"
          startIcon={<DownloadIcon />}
          color="primary"
          onClick={handleDownloadPdf}
        >
          Download
        </Button>

        <Button
          size="small"
          variant="outlined"
          startIcon={<SaveIcon />}
          color="primary"
          onClick={handlePrint}
        >
          Save Parameters
        </Button>
      </Box> */}
            Save Parameters
          </Button>
        </Box>
        <Paper
          ref={contentRef}
          sx={{ position: 'relative', p: 2, backgroundColor: 'white' }}
        >
          <Typography variant="h4" sx={{ textAlign: 'center', mb: 3 }}>
            Transit Signal Priority Report
          </Typography>
          <TabList onChange={handleTabChange} aria-label="Location Tabs">
            {report.map((locationReport, index) => (
              <Tab
                key={index}
                label={locationReport?.locationPhases?.locationIdentifier}
                value={index.toString()}
              />
            ))}
          </TabList>
          {report.map((locationReport, index) => {
            const rows = locationReport.transitSignalPlans
              ?.map((plan) => {
                return plan.phases?.map((phase) => {
                  return {
                    plan: plan.planNumber,
                    phaseNumber: phase.phaseNumber,
                    programmedSplit: round(phase.programmedSplit),
                    recommendedTSPMax:
                      phase.recommendedTSPMax !== null
                        ? round(phase.recommendedTSPMax)
                        : '',
                    maxReduction: round(phase.maxReduction),
                    maxExtension: round(phase.maxExtension),
                    priorityMin: round(phase.priorityMin),
                    priorityMax: round(phase.priorityMax),
                    minGreen: round(phase.minGreen),
                    yellow: round(phase.yellow),
                    redClearance: round(phase.redClearance),
                    minTime: round(phase.minTime),
                    percentileSplit85th: round(phase.percentileSplit85th),
                    percentileSplit50th: round(phase.percentileSplit50th),
                    averageSplit: round(phase.averageSplit),
                    percentMaxOutsForceOffs: round(
                      phase.percentMaxOutsForceOffs
                    ),
                    percentGapOuts: round(phase.percentGapOuts),
                    percentSkips: round(phase.percentSkips),
                    notes: phase.notes,
                    skipsGreaterThan70TSPMax: round(
                      phase.skipsGreaterThan70TSPMax
                    ),
                    forceOffsLessThan40TSPMax: round(
                      phase.forceOffsLessThan40TSPMax
                    ),
                    forceOffsLessThan60TSPMax: round(
                      phase.forceOffsLessThan60TSPMax
                    ),
                    forceOffsLessThan80TSPMax: round(
                      phase.forceOffsLessThan80TSPMax
                    ),
                  }
                })
              })
              .flat()

            return (
              <TabPanel
                key={locationReport?.locationPhases?.locationIdentifier}
                value={index.toString()}
                sx={{ p: 0 }}
              >
                <div style={{ height: 700, width: '100%' }}>
                  <DataGrid
                    getRowId={(row) => `${row.plan}-${row.phaseNumber}`}
                    rows={rows}
                    columns={columns}
                    unstable_rowSpanning={true}
                    density="compact"
                    slots={{
                      toolbar: () =>
                        CustomToolbar(
                          locationReport?.locationPhases?.locationIdentifier ??
                            ''
                        ),
                    }}
                    disableColumnFilter
                    disableColumnMenu
                    disableColumnSelector
                    disableDensitySelector
                    disableRowSelectionOnClick
                    disableColumnSorting
                    hideFooter
                    sx={{
                      '& .MuiDataGrid-cell': {
                        borderRight: '1px solid lightgray',
                      },
                      '& .purple-text': {
                        color: 'purple',
                      },
                      '& .blue-text': {
                        color: 'blue',
                      },
                      '& .red-text': {
                        color: 'red',
                      },
                      '& .green-text': {
                        color: 'green',
                      },
                    }}
                  />
                </div>
              </TabPanel>
            )
          })}
        </Paper>
      </TabContext>

      {/* Save Parameters Modal */}
      <Modal open={isModalOpen} onClose={handleCloseModal}>
        <Paper
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: 400,
            p: 4,
            display: 'flex',
            flexDirection: 'column',
            gap: 2,
          }}
        >
          <Typography variant="h6">Save Parameters</Typography>
          <TextField
            label="Name"
            fullWidth
            value={paramName}
            onChange={(e) => setParamName(e.target.value)}
          />
          <Box>
            <Typography variant="subtitle2" sx={{ mb: 1 }}>
              Options:
            </Typography>
            <Box sx={{ maxHeight: 200, overflowY: 'auto', pl: 1 }}>
              Selected Locations: {reportOptions?.locations?.join(', ')}
            </Box>
          </Box>
          <Box
            sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, mt: 2 }}
          >
            <Button onClick={handleCloseModal}>Cancel</Button>
            <Button variant="contained" onClick={handleConfirmSave}>
              Save
            </Button>
          </Box>
        </Paper>
      </Modal>
    </>
  )
}

export default TspReport

const round = (value: number | undefined) =>
  value != null ? Number(value.toFixed(1)) : value

const columns: GridColDef[] = [
  { field: 'plan', headerName: 'Plan', width: 100 },
  {
    field: 'phaseNumber',
    headerName: 'Phase #',
    width: 100,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'programmedSplit',
    headerName: 'Programmed Split (sec)',
    width: 160,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'recommendedTSPMax',
    headerName: 'TSP Max',
    width: 100,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'maxReduction',
    headerName: 'Max Reduction',
    width: 130,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'maxExtension',
    headerName: 'Max Extension',
    width: 130,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'priorityMin',
    headerName: 'Priority Min',
    width: 120,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'priorityMax',
    headerName: 'Priority Max',
    width: 120,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'minGreen',
    headerName: 'Min Green',
    width: 100,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'yellow',
    headerName: 'Yellow',
    width: 100,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'redClearance',
    headerName: 'Red Clearance',
    width: 120,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'minTime',
    headerName: 'Min Time',
    width: 100,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'percentileSplit85th',
    headerName: '85th Percentile Split (sec)',
    width: 200,
    cellClassName: 'purple-text',
    rowSpanValueGetter: () => null,
  },
  {
    field: 'percentileSplit50th',
    headerName: '50th Percentile Split (sec)',
    width: 200,
    cellClassName: 'purple-text',
    rowSpanValueGetter: () => null,
  },
  {
    field: 'averageSplit',
    headerName: 'Average Split (sec)',
    width: 150,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'percentMaxOutsForceOffs',
    headerName: 'Force Offs / Max Outs (%)',
    width: 200,
    rowSpanValueGetter: () => null,
    cellClassName: (params) => {
      return params.row.plan === 254 ? 'red-text' : 'blue-text'
    },
  },
  {
    field: 'percentGapOuts',
    headerName: 'Gap Outs (%)',
    width: 130,
    cellClassName: 'green-text',
    rowSpanValueGetter: () => null,
  },
  { field: 'percentSkips', headerName: 'Skips (%)', width: 120 },
  { field: 'notes', headerName: 'Result Notes', width: 200 },
  {
    field: 'skipsGreaterThan70TSPMax',
    headerName: 'Skips > 70% TSP Max',
    width: 180,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'forceOffsLessThan40TSPMax',
    headerName: 'Force Offs < 40% TSP Max',
    width: 200,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'forceOffsLessThan60TSPMax',
    headerName: 'Force Offs < 60% TSP Max',
    width: 200,
    rowSpanValueGetter: () => null,
  },
  {
    field: 'forceOffsLessThan80TSPMax',
    headerName: 'Force Offs < 80% TSP Max',
    width: 200,
    rowSpanValueGetter: () => null,
  },
]

function CustomToolbar(locationIdentifier: string) {
  return (
    <GridToolbarContainer sx={{ p: 2 }}>
      <Box sx={{ flexGrow: 1 }} />
      <GridToolbarExport
        slotProps={{
          tooltip: { title: 'Export data' },
          button: { variant: 'outlined' },
        }}
        printOptions={{
          disableToolbarButton: true,
        }}
        csvOptions={{
          fileName: `TSP_Report-${locationIdentifier}.csv`,
          escapeFormulas: false,
        }}
      />
    </GridToolbarContainer>
  )
}
