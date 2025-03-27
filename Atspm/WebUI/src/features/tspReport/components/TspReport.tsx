import { TransitSignalPriorityResult } from '@/api/reports/aTSPMReportDataApi.schemas'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import {
  Paper,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import html2canvas from 'html2canvas'
import { jsPDF } from 'jspdf'
import { useRef, useState } from 'react'
import { useReactToPrint } from 'react-to-print'

interface TspReportProps {
  report: TransitSignalPriorityResult[]
}

const round = (value: number | undefined) =>
  value != null ? Number(value.toFixed(1)) : value

const TspReport = ({ report }: TspReportProps) => {
  const contentRef = useRef<HTMLDivElement>(null)
  const handlePrint = useReactToPrint({ contentRef })
  const [currentTab, setCurrentTab] = useState('0')

  const handleTabChange = (_: React.SyntheticEvent, newTab: string) => {
    setCurrentTab(newTab)
  }

  const handleDownloadPdf = async () => {
    if (!contentRef.current) return
    try {
      const canvas = await html2canvas(contentRef.current)
      const imgData = canvas.toDataURL('image/png')
      const pdf = new jsPDF({
        orientation: 'landscape',
        unit: 'px',
        format: 'a4',
      })
      const pdfWidth = pdf.internal.pageSize.getWidth()
      const pdfHeight = pdf.internal.pageSize.getHeight()

      pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight)
      pdf.save('TSP_Report.pdf')
    } catch (error) {
      console.error('PDF generation error:', error)
    }
  }

  return (
    <TabContext value={currentTab}>
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

      <Paper
        ref={contentRef}
        sx={{
          position: 'relative',
          p: 2,
          backgroundColor: 'white',
        }}
      >
        <Typography variant="h4" sx={{ textAlign: 'center', mb: 3 }}>
          Transit Signal Priority Report
        </Typography>

        {/* Tabs for Locations */}
        <TabList onChange={handleTabChange} aria-label="Location Tabs">
          {report.map((locationReport, index) => (
            <Tab
              key={index}
              label={locationReport?.locationPhases?.locationIdentifier}
              value={index.toString()}
            />
          ))}
        </TabList>

        {report.map((locationReport, index) => (
          <TabPanel
            key={locationReport?.locationPhases?.locationIdentifier}
            value={index.toString()}
            sx={{ p: 0 }}
          >
            <TableContainer component={Paper} sx={{ mb: 3 }}>
              <Table size="small">
                <TableHead>
                  <TableRow
                    sx={{
                      backgroundColor: '#f9f9fb',
                      '& .MuiTableCell-root': {
                        fontSize: '12px',
                        border: '1px solid rgb(224, 224, 224)',
                      },
                    }}
                  >
                    <TableCell>Plan</TableCell>
                    <TableCell>Phase #</TableCell>
                    <TableCell>Min Green</TableCell>
                    <TableCell>Yellow</TableCell>
                    <TableCell>Red Clearance</TableCell>
                    <TableCell>Min Time</TableCell>
                    <TableCell sx={{ minWidth: '100px' }}>
                      Programmed Split (sec)
                    </TableCell>
                    <TableCell
                      sx={{
                        color: 'purple',
                        minWidth: '100px',
                      }}
                    >
                      85th Percentile Split (sec)
                    </TableCell>
                    <TableCell
                      sx={{
                        color: 'purple',
                        minWidth: '100px',
                      }}
                    >
                      50th Percentile Split (sec)
                    </TableCell>
                    <TableCell sx={{ minWidth: '100px' }}>
                      Average Split (sec)
                    </TableCell>
                    <TableCell
                      sx={{
                        minWidth: '100px',
                      }}
                    >
                      <span style={{ color: 'blue' }}>Force Offs</span> or{' '}
                      <span style={{ color: 'red' }}>Max Outs</span> (%)
                    </TableCell>
                    <TableCell sx={{ color: 'green' }}>Gap Outs (%)</TableCell>
                    <TableCell>Skips (%)</TableCell>
                    <TableCell>TSP Max</TableCell>
                    <TableCell>Max Reduction</TableCell>
                    <TableCell>Max Extension</TableCell>
                    <TableCell>Priority Min</TableCell>
                    <TableCell>Priority Max</TableCell>
                    <TableCell sx={{ minWidth: '200px' }}>
                      Result Notes
                    </TableCell>
                    {/* TSP Ad sx={{fontSize: '12px'}}justments moved to the end */}
                    <TableCell sx={{ minWidth: '200px' }}>
                      {'Skips > 70% TSP Max'}
                    </TableCell>
                    <TableCell sx={{ minWidth: '200px' }}>
                      {'Force Offs < 40% TSP Max'}
                    </TableCell>
                    <TableCell sx={{ minWidth: '200px' }}>
                      {'Force Offs < 60% TSP Max'}
                    </TableCell>
                    <TableCell sx={{ minWidth: '200px' }}>
                      {'Force Offs < 80% TSP Max'}
                    </TableCell>
                  </TableRow>
                </TableHead>
                <TableBody
                  sx={{
                    '& .MuiTableCell-root': {
                      border: '1px solid rgb(224, 224, 224)',
                    },
                  }}
                >
                  {locationReport?.transitSignalPlans?.map((plan) => {
                    return plan?.phases?.map((phase, rowIndex) => (
                      <TableRow key={`${plan.planNumber}-${rowIndex}`}>
                        {rowIndex === 0 && (
                          <TableCell
                            rowSpan={plan?.phases?.length}
                            sx={{
                              borderRight: '1px solid lightgrey',
                              alignSelf: 'center',
                              textAlign: 'center',
                            }}
                          >
                            {plan.planNumber}
                          </TableCell>
                        )}
                        <TableCell>{phase.phaseNumber}</TableCell>
                        <TableCell>{round(phase?.minGreen)}</TableCell>
                        <TableCell>{round(phase.yellow)}</TableCell>
                        <TableCell>{round(phase.redClearance)}</TableCell>
                        <TableCell>{round(phase.minTime)}</TableCell>
                        <TableCell>{round(phase.programmedSplit)}</TableCell>
                        <TableCell sx={{ color: 'purple' }}>
                          {round(phase.percentileSplit85th)}
                        </TableCell>
                        <TableCell sx={{ color: 'purple' }}>
                          {round(phase.percentileSplit50th)}
                        </TableCell>
                        <TableCell>{round(phase.averageSplit)}</TableCell>
                        <TableCell
                          sx={{
                            color: plan.planNumber === 254 ? 'red' : 'blue',
                          }}
                        >
                          {round(phase.percentMaxOutsForceOffs)}
                        </TableCell>
                        <TableCell sx={{ color: 'green' }}>
                          {round(phase.percentGapOuts)}
                        </TableCell>
                        <TableCell>{round(phase.percentSkips)}</TableCell>
                        <TableCell>
                          {phase.recommendedTSPMax !== null
                            ? round(phase.recommendedTSPMax)
                            : ''}
                        </TableCell>
                        <TableCell>{round(phase.maxReduction)}</TableCell>
                        <TableCell>{round(phase.maxExtension)}</TableCell>
                        <TableCell>{round(phase.priorityMin)}</TableCell>
                        <TableCell>{round(phase.priorityMax)}</TableCell>
                        <TableCell>{phase.notes}</TableCell>
                        {/* TSP Adjustment Recommendations (Moved to End) */}
                        <TableCell>
                          {round(phase.skipsGreaterThan70TSPMax)}
                        </TableCell>
                        <TableCell>
                          {round(phase.forceOffsLessThan40TSPMax)}
                        </TableCell>
                        <TableCell>
                          {round(phase.forceOffsLessThan60TSPMax)}
                        </TableCell>
                        <TableCell>
                          {round(phase.forceOffsLessThan80TSPMax)}
                        </TableCell>
                      </TableRow>
                    ))
                  })}
                </TableBody>
              </Table>
            </TableContainer>
          </TabPanel>
        ))}
      </Paper>
    </TabContext>
  )
}

export default TspReport
