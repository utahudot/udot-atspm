import { TransitSignalPriorityResult } from '@/api/reports/aTSPMReportDataApi.schemas'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Paper, Tab, Typography } from '@mui/material'
import {
  DataGrid,
  GridColDef,
  GridToolbarContainer,
  GridToolbarExport,
} from '@mui/x-data-grid'
import { useRef, useState } from 'react'

interface TspReportProps {
  report: TransitSignalPriorityResult[]
}

const TspReport = ({ report }: TspReportProps) => {
  const contentRef = useRef<HTMLDivElement>(null)
  const [currentTab, setCurrentTab] = useState('0')
  const handleTabChange = (_: React.SyntheticEvent, newTab: string) => {
    setCurrentTab(newTab)
  }
  return (
    <TabContext value={currentTab}>
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
                console.log('60', plan.planNumber)
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
                  percentMaxOutsForceOffs: round(phase.percentMaxOutsForceOffs),
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
          console.log('rows', rows)
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
                        locationReport?.locationPhases?.locationIdentifier ?? ''
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
  )
}

export default TspReport

const round = (value: number | undefined) =>
  value != null ? Number(value.toFixed(1)) : value

const columns: GridColDef[] = [
  { field: 'plan', headerName: 'Plan', width: 100 },
  { field: 'phaseNumber', headerName: 'Phase #', width: 100 },
  {
    field: 'programmedSplit',
    headerName: 'Programmed Split (sec)',
    width: 160,
  },
  { field: 'recommendedTSPMax', headerName: 'TSP Max', width: 100 },
  { field: 'maxReduction', headerName: 'Max Reduction', width: 130 },
  { field: 'maxExtension', headerName: 'Max Extension', width: 130 },
  { field: 'priorityMin', headerName: 'Priority Min', width: 120 },
  { field: 'priorityMax', headerName: 'Priority Max', width: 120 },
  { field: 'minGreen', headerName: 'Min Green', width: 100 },
  { field: 'yellow', headerName: 'Yellow', width: 100 },
  { field: 'redClearance', headerName: 'Red Clearance', width: 120 },
  { field: 'minTime', headerName: 'Min Time', width: 100 },
  {
    field: 'percentileSplit85th',
    headerName: '85th Percentile Split (sec)',
    width: 200,
    cellClassName: 'purple-text',
  },
  {
    field: 'percentileSplit50th',
    headerName: '50th Percentile Split (sec)',
    width: 200,
    cellClassName: 'purple-text',
  },
  { field: 'averageSplit', headerName: 'Average Split (sec)', width: 150 },
  {
    field: 'percentMaxOutsForceOffs',
    headerName: 'Force Offs / Max Outs (%)',
    width: 200,
    cellClassName: (params) => {
      return params.row.plan === 254 ? 'red-text' : 'blue-text'
    },
  },
  {
    field: 'percentGapOuts',
    headerName: 'Gap Outs (%)',
    width: 130,
    cellClassName: 'green-text',
  },
  { field: 'percentSkips', headerName: 'Skips (%)', width: 120 },
  { field: 'notes', headerName: 'Result Notes', width: 200 },
  {
    field: 'skipsGreaterThan70TSPMax',
    headerName: 'Skips > 70% TSP Max',
    width: 180,
  },
  {
    field: 'forceOffsLessThan40TSPMax',
    headerName: 'Force Offs < 40% TSP Max',
    width: 200,
  },
  {
    field: 'forceOffsLessThan60TSPMax',
    headerName: 'Force Offs < 60% TSP Max',
    width: 200,
  },
  {
    field: 'forceOffsLessThan80TSPMax',
    headerName: 'Force Offs < 80% TSP Max',
    width: 200,
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
