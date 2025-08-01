import { postMeasureOptionPreset, useGetMeasureType } from '@/api/config'
import { TransitSignalPriorityResult } from '@/api/reports/aTSPMReportDataApi.schemas'
import { TspReportOptions } from '@/pages/reports/transit-signal-priority'
import { useNotificationStore } from '@/stores/notifications'
import DownloadIcon from '@mui/icons-material/Download'
import SaveIcon from '@mui/icons-material/Save'
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
import { DataGrid, GridColDef } from '@mui/x-data-grid'
import Cookies from 'js-cookie'
import { useRef, useState } from 'react'
import * as XLSX from 'xlsx'

interface TspReportProps {
  report: TransitSignalPriorityResult[]
  reportOptions: TspReportOptions
}

export default function TspReport({ report, reportOptions }: TspReportProps) {
  const isLoggedIn = !!Cookies.get('loggedIn')
  const { addNotification } = useNotificationStore()
  const { data: measureTypesData } = useGetMeasureType()
  const measureTypes = measureTypesData?.value || []
  const contentRef = useRef<HTMLDivElement>(null)
  const [currentTab, setCurrentTab] = useState('0')
  const [isModalOpen, setModalOpen] = useState(false)
  const [paramName, setParamName] = useState('')

  const handleTabChange = (_: React.SyntheticEvent, newTab: string) => {
    setCurrentTab(newTab)
  }

  const handleSaveParameters = () => setModalOpen(true)
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
      '@odata.type': '#AtspmOptionsBase.TransitSignalPriorityOptions',
    }
    postMeasureOptionPreset({
      name: paramName,
      option: reportParams,
      measureTypeId: measureTypes.find((m) => m.abbreviation === 'TSP')?.id,
    })
      .then(() => {
        addNotification({
          title: 'Parameters saved successfully',
          type: 'success',
        })
      })
      .catch(() => {
        addNotification({
          title: 'Failed to save parameters',
          type: 'error',
        })
      })
    handleCloseModal()
  }

  const handleExportClick = () => {
    exportToExcel(report, reportOptions.locations)
  }

  function buildColumns(
    designatedPhasesLookup: Record<string, boolean>
  ): GridColDef[] {
    const getColumn = ({
      field,
      headerName,
      width,
      baseClassName,
      applyHighlight = true,
      combineRows = false,
    }: {
      field: string
      headerName: string
      width: number
      baseClassName?: string | ((params: any) => string)
      applyHighlight?: boolean
      combineRows?: boolean
    }): GridColDef => {
      return combineRows
        ? {
            field,
            headerName,
            width,
            cellClassName: (params) => {
              const base =
                typeof baseClassName === 'function'
                  ? baseClassName(params)
                  : (baseClassName ?? '')
              const shouldHighlight =
                applyHighlight && designatedPhasesLookup[params.id]
              return `${base} ${shouldHighlight ? 'highlightCell' : ''}`.trim()
            },
          }
        : {
            field,
            headerName,
            width,
            rowSpanValueGetter: () => (combineRows ? 'inherit' : undefined),
            cellClassName: (params) => {
              const base =
                typeof baseClassName === 'function'
                  ? baseClassName(params)
                  : (baseClassName ?? '')
              const shouldHighlight =
                applyHighlight && designatedPhasesLookup[params.id]
              return `${base} ${shouldHighlight ? 'highlightCell' : ''}`.trim()
            },
          }
    }

    return [
      getColumn({
        field: 'plan',
        headerName: 'Plan',
        width: 100,
        applyHighlight: false,
        combineRows: true,
      }),
      getColumn({
        field: 'numberOfCycles',
        headerName: '# of Cycles',
        width: 100,
        applyHighlight: false,
        combineRows: true,
      }),
      getColumn({ field: 'phaseNumber', headerName: 'Phase #', width: 100 }),
      getColumn({
        field: 'programmedSplit',
        headerName: 'Programmed Split (sec)',
        width: 160,
      }),
      getColumn({
        field: 'recommendedTSPMax',
        headerName: 'TSP Max',
        width: 100,
      }),
      getColumn({
        field: 'maxReduction',
        headerName: 'Max Reduction',
        width: 130,
      }),
      getColumn({
        field: 'maxExtension',
        headerName: 'Max Extension',
        width: 130,
      }),
      getColumn({
        field: 'priorityMin',
        headerName: 'Priority Min',
        width: 120,
      }),
      getColumn({
        field: 'priorityMax',
        headerName: 'Priority Max',
        width: 120,
      }),
      getColumn({ field: 'minGreen', headerName: 'Min Green', width: 100 }),
      getColumn({ field: 'yellow', headerName: 'Yellow', width: 100 }),
      getColumn({
        field: 'redClearance',
        headerName: 'Red Clearance',
        width: 120,
      }),
      getColumn({ field: 'minTime', headerName: 'Min Time', width: 100 }),
      getColumn({
        field: 'percentileSplit85th',
        headerName: '85th Percentile Split (sec)',
        width: 200,
        baseClassName: 'purple-text',
      }),
      getColumn({
        field: 'percentileSplit50th',
        headerName: '50th Percentile Split (sec)',
        width: 200,
        baseClassName: 'purple-text',
      }),
      getColumn({
        field: 'averageSplit',
        headerName: 'Average Split (sec)',
        width: 150,
      }),
      getColumn({
        field: 'percentMaxOutsForceOffs',
        headerName: 'Force Offs / Max Outs (%)',
        width: 200,
        baseClassName: (params) =>
          params.row.plan === 254 ? 'red-text' : 'blue-text',
      }),
      getColumn({
        field: 'percentGapOuts',
        headerName: 'Gap Outs (%)',
        width: 130,
        baseClassName: 'green-text',
      }),
      getColumn({ field: 'percentSkips', headerName: 'Skips (%)', width: 120 }),
      getColumn({ field: 'notes', headerName: 'Result Notes', width: 200 }),
      getColumn({
        field: 'skipsGreaterThan70TSPMax',
        headerName: 'Skips > 70% TSP Max',
        width: 180,
      }),
      getColumn({
        field: 'forceOffsLessThan40TSPMax',
        headerName: 'Force Offs < 40% TSP Max',
        width: 200,
      }),
      getColumn({
        field: 'forceOffsLessThan60TSPMax',
        headerName: 'Force Offs < 60% TSP Max',
        width: 200,
      }),
      getColumn({
        field: 'forceOffsLessThan80TSPMax',
        headerName: 'Force Offs < 80% TSP Max',
        width: 200,
      }),
    ]
  }

  return (
    <>
      <TabContext value={currentTab}>
        <Box sx={{ display: 'flex', justifyContent: 'right', my: 1 }}>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
            <Button
              size="small"
              variant="outlined"
              startIcon={<DownloadIcon />}
              color="primary"
              onClick={handleExportClick}
            >
              Download
            </Button>
            {isLoggedIn && (
              <Button
                size="small"
                variant="outlined"
                startIcon={<SaveIcon />}
                color="primary"
                onClick={handleSaveParameters}
              >
                Save Parameters
              </Button>
            )}
          </Box>
        </Box>
        <Paper
          ref={contentRef}
          sx={{ position: 'relative', backgroundColor: 'white' }}
        >
          <TabList
            onChange={handleTabChange}
            aria-label="Location Tabs"
            variant="scrollable"
          >
            {report.map((locationReport, index) => {
              const location = reportOptions.locations.find(
                (loc) =>
                  loc.locationIdentifier ===
                  locationReport.locationPhases?.locationIdentifier
              )
              return (
                <Tab
                  key={index}
                  label={
                    <Typography variant="subtitle2">
                      {location?.locationIdentifier} - {location?.primaryName} &{' '}
                      {location?.secondaryName}
                    </Typography>
                  }
                  value={index.toString()}
                />
              )
            })}
          </TabList>
          {report.map((locationReport, index) => {
            const location = reportOptions.locations.find(
              (loc) =>
                loc.locationIdentifier ===
                locationReport.locationPhases?.locationIdentifier
            )

            const rows =
              locationReport.transitSignalPlans?.flatMap((plan) =>
                plan.phases?.map((phase) => ({
                  plan: plan.planNumber,
                  numberOfCycles: plan.numberOfCycles,
                  phaseNumber: phase.phaseNumber,
                  programmedSplit: round(phase.programmedSplit),
                  recommendedTSPMax:
                    phase.recommendedTSPMax != null
                      ? round(phase.recommendedTSPMax)
                      : '',
                  maxReduction: round(phase.maxReduction),
                  maxExtension: round(phase.maxExtension),
                  priorityMin: round(phase.priorityMin),
                  priorityMax: round(phase.priorityMax),
                  minGreen: round(Math.round(phase.minGreen as number), 0),
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
                }))
              ) || []

            const designatedPhasesLookup = Object.fromEntries(
              rows
                .filter((r) =>
                  location?.designatedPhases?.includes(r.phaseNumber)
                )
                .map((r) => [`${r.plan}-${r.phaseNumber}`, true])
            )

            const columns = buildColumns(designatedPhasesLookup)

            return (
              <TabPanel
                key={locationReport?.locationPhases?.locationIdentifier}
                value={index.toString()}
                sx={{ p: 0 }}
              >
                <Box>
                  <Typography variant="h4" sx={{ m: 1 }}>
                    Manufacturer:
                    <span style={{ fontWeight: 'bold', marginLeft: 4 }}>
                      {locationReport?.locationPhases?.controllerManufacturer}
                    </span>
                  </Typography>
                </Box>
                <div style={{ height: 700, width: '100%' }}>
                  <DataGrid
                    getRowId={(row) => `${row.plan}-${row.phaseNumber}`}
                    rows={rows}
                    columns={columns}
                    unstable_rowSpanning
                    density="compact"
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
                      '& .highlightCell': {
                        backgroundColor: '#8cd0f230',
                      },
                    }}
                  />
                </div>
              </TabPanel>
            )
          })}
        </Paper>
      </TabContext>
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

function round(value: number | undefined, roundedNumber?: number) {
  return value != null ? Number(value.toFixed(roundedNumber || 1)) : value
}

export function exportToExcel(
  report: TransitSignalPriorityResult[],
  locations: {
    locationIdentifier: string
    primaryName?: string
    secondaryName?: string
  }[]
) {
  const workbook = XLSX.utils.book_new()
  report.forEach((locationReport) => {
    const matchedLoc = locations.find(
      (loc) =>
        loc.locationIdentifier ===
        locationReport.locationPhases?.locationIdentifier
    )
    const sheetName = matchedLoc ? `${matchedLoc.locationIdentifier}` : 'Sheet'
    const rows =
      locationReport.transitSignalPlans?.flatMap((plan) =>
        plan.phases?.map((phase) => ({
          plan: plan.planNumber,
          numberOfCycles: plan.numberOfCycles,
          phaseNumber: phase.phaseNumber,
          programmedSplit: round(phase.programmedSplit),
          recommendedTSPMax:
            phase.recommendedTSPMax != null
              ? round(phase.recommendedTSPMax)
              : '',
          maxReduction: round(phase.maxReduction),
          maxExtension: round(phase.maxExtension),
          priorityMin: round(phase.priorityMin),
          priorityMax: round(phase.priorityMax),
          minGreen: round(Math.round(phase.minGreen as number)),
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
          skipsGreaterThan70TSPMax: round(phase.skipsGreaterThan70TSPMax),
          forceOffsLessThan40TSPMax: round(phase.forceOffsLessThan40TSPMax),
          forceOffsLessThan60TSPMax: round(phase.forceOffsLessThan60TSPMax),
          forceOffsLessThan80TSPMax: round(phase.forceOffsLessThan80TSPMax),
        }))
      ) || []
    const headerRow = [
      'Plan',
      'Phase Number',
      'Programmed Split',
      'Recommended TSP Max',
      'Max Reduction',
      'Max Extension',
      'Priority Min',
      'Priority Max',
      'Min Green',
      'Yellow',
      'Red Clearance',
      'Min Time',
      '85th Percentile Split',
      '50th Percentile Split',
      'Average Split',
      'Force Offs / Max Outs (%)',
      'Gap Outs (%)',
      'Skips (%)',
      'Notes',
      'Skips > 70% TSP Max',
      'Force Offs < 40% TSP Max',
      'Force Offs < 60% TSP Max',
      'Force Offs < 80% TSP Max',
    ]
    const sheetData: (string | number)[][] = []
    sheetData.push(headerRow)
    rows.forEach((r) => {
      sheetData.push([
        r.plan ?? '',
        r.phaseNumber ?? '',
        r.programmedSplit ?? '',
        r.recommendedTSPMax ?? '',
        r.maxReduction ?? '',
        r.maxExtension ?? '',
        r.priorityMin ?? '',
        r.priorityMax ?? '',
        r.minGreen ?? '',
        r.yellow ?? '',
        r.redClearance ?? '',
        r.minTime ?? '',
        r.percentileSplit85th ?? '',
        r.percentileSplit50th ?? '',
        r.averageSplit ?? '',
        r.percentMaxOutsForceOffs ?? '',
        r.percentGapOuts ?? '',
        r.percentSkips ?? '',
        r.notes ?? '',
        r.skipsGreaterThan70TSPMax ?? '',
        r.forceOffsLessThan40TSPMax ?? '',
        r.forceOffsLessThan60TSPMax ?? '',
        r.forceOffsLessThan80TSPMax ?? '',
      ])
    })
    const worksheet = XLSX.utils.aoa_to_sheet(sheetData)
    const truncatedSheetName = sheetName.substring(0, 31)
    XLSX.utils.book_append_sheet(workbook, worksheet, truncatedSheetName)
  })
  const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' })
  const blob = new Blob([excelBuffer], { type: 'application/octet-stream' })
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.setAttribute('download', `TSP_Report.xlsx`)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}
