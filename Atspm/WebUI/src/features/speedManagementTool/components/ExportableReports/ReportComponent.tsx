import {
  EffectivenessOfStrategiesDto,
  ExportableReportOptions,
  ExportableReportResult,
  ExportableReportType,
  ImpactSpeedAggregationCategoryFilter,
  RouteSpeed,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import CongestionTrackingChartsContainer from '@/features/charts/speedManagementTool/congestionTracker/components/CongestionTrackerChartsContainer'
import { transformCongestionTrackerData } from '@/features/charts/speedManagementTool/congestionTracker/congestionTracker.transformer'
import EffectivenessOfStrategiesChartsContainer from '@/features/charts/speedManagementTool/effectivenessOfStrategies/EffectivenessOfStrategiesChartContainer'
import transformEffectivenessOfStrategiesData from '@/features/charts/speedManagementTool/effectivenessOfStrategies/effectivenessOfStrategies.transformer'
import SpeedComplianceChartsContainer from '@/features/charts/speedManagementTool/speedCompliance/SpeedComplianceChartsContainer'
import transformSpeedComplianceData from '@/features/charts/speedManagementTool/speedCompliance/speedCompliance.transformer'
import SpeedVariabilityChartContainer from '@/features/charts/speedManagementTool/speedVariability/components/SpeedVariabilityChartContainer'
import transformSpeedVariabilityData from '@/features/charts/speedManagementTool/speedVariability/speedVariability.transformer'
import SpeedViolationsChartContainer from '@/features/charts/speedManagementTool/speedViolations/components/SpeedViolationsChartContainer'
import transformSpeedViolationsData from '@/features/charts/speedManagementTool/speedViolations/speedViolations.transformer'
import PrintIcon from '@mui/icons-material/Print'
import {
  Box,
  Button,
  Divider,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { ReactNode, useCallback, useEffect, useRef, useState } from 'react'
import { useReactToPrint } from 'react-to-print'
import { RoutesResponse } from '../../types/routes'
import {
  createHotspotSegmentsFromImpacts,
  formatFilters,
  generateBaseText,
  generateReportSpecificText,
  getAdjustedRoutes,
  getHotspotsBasedOnChartType,
  getImpactHostpot,
  getTableData,
} from './common'
import {
  ERHourlyHandler,
  ERMonthlyHandler,
} from './components/handlers/handlers'
import { HotSpotForReportMap, ImpactHotspotForReportMap } from './types'

interface Props {
  data: ExportableReportResult[]
  routeSpeeds: RoutesResponse
  options: ExportableReportOptions
  handler: ERHourlyHandler | ERMonthlyHandler
}

export const ReportDisplayComponent = (props: Props) => {
  const { data, options, routeSpeeds, handler } = props
  const [reportContent, setReportContent] = useState<ReactNode>(null)
  const contentRef = useRef<HTMLDivElement>(null)
  const reactToPrintFn = useReactToPrint({
    contentRef,
    pageStyle: `
      @page {
        size: A4;
        @top-center {
          content: "";
        }
        @bottom-left {
          content: counter(page);
          font-size: 10pt;
        }
        @bottom-center {
          content: "CONFIDENTIAL: RECORDS PRODUCED WITH THIS REQUEST MAY BE PROTECTED UNDER 23 USC 407.";
          font-size: 8pt;
          font-weight: bold;
        }
      }
      @media print {
        body {
          -webkit-print-color-adjust: exact;
        }
        .MuiBox-root {
          width: 100%;
        }
        .MuiTypography-root {
          font-size: 12pt;
        }
        .MuiTable-root {
          width: 100%;
          border-collapse: collapse;
        }
        .MuiTableCell-root {
        }
        .page-break {
          page-break-before: always;
        }
        .avoid-page-break {
          break-inside: avoid;
        }
      }
    `,
  })

  const generateCongestionChart = (
    report: ExportableReportResult,
    segment: RouteSpeed
  ) => {
    if (report?.congestionTrackingCharts) {
      const congestionChartData = report.congestionTrackingCharts.find(
        (chart) => chart.segmentId === segment.segmentId
      )

      if (congestionChartData) {
        const options = transformCongestionTrackerData(
          congestionChartData,
          'month'
        )
        return (
          <Box className="avoid-page-break">
            <CongestionTrackingChartsContainer chartData={options as any} />
          </Box>
        )
      }
    }
    return ''
  }

  const generateReportSpecificChart = (
    report: ExportableReportResult,
    segment: RouteSpeed
  ) => {
    switch (report.exportableReportType) {
      case ExportableReportType.Violations:
      case ExportableReportType.MostImproved:
        if (report.speedViolationCharts) {
          const chartData = report.speedViolationCharts.find(
            (chart) => chart.segmentId === segment.segmentId
          )
          if (chartData) {
            const options = transformSpeedViolationsData([chartData])
            return (
              <Box className="avoid-page-break">
                <SpeedViolationsChartContainer chartData={options} />
              </Box>
            )
          }
        }
      case ExportableReportType.ComplianceToSpeedLimit:
        if (report.speedComplianceCharts) {
          const chartData = report.speedComplianceCharts.find(
            (chart) => chart.segmentId === segment.segmentId
          )
          if (chartData) {
            const options = transformSpeedComplianceData(
              [chartData],
              null,
              false
            )
            return (
              <Box className="avoid-page-break">
                <SpeedComplianceChartsContainer chartData={options} />
              </Box>
            )
          }
        }
      case ExportableReportType.SpeedVariability:
        if (report.speedVariabilityCharts) {
          const chartData = report.speedVariabilityCharts.find(
            (chart) => chart.segmentId === segment.segmentId
          )
          if (chartData) {
            const options = transformSpeedVariabilityData(chartData)
            return (
              <Box className="avoid-page-break">
                <SpeedVariabilityChartContainer chartData={options} />
              </Box>
            )
          }
        }
        return ''
      default:
        return ''
    }
  }

  const generateEoSChart = (eosCharts: EffectivenessOfStrategiesDto[]) => {
    const options = transformEffectivenessOfStrategiesData(
      eosCharts,
      undefined,
      ImpactSpeedAggregationCategoryFilter.ChangeInEightyFifthPercentileSpeed
    )

    return <EffectivenessOfStrategiesChartsContainer chartData={options} />
  }

  const getDataForSegments = useCallback(
    (report: ExportableReportResult) => {
      if (report.reportData) {
        return (
          <Box className="test">
            {report.reportData.map((segment, index) => {
              const text = generateReportSpecificText(
                report.exportableReportType,
                options,
                segment
              )
              return (
                <Box key={index} sx={{ marginTop: 3 }}>
                  <Typography variant="h6" fontWeight="bold" gutterBottom>
                    {`Rank ${index + 1}: Segment ${segment.name}`}
                  </Typography>
                  <Typography variant="h6" gutterBottom>
                    {text}
                  </Typography>
                  <Box
                    component="pre"
                    sx={{ whiteSpace: 'pre-wrap', fontSize: 14 }}
                  >
                    {generateCongestionChart(report, segment)}
                    {generateReportSpecificChart(report, segment)}
                  </Box>
                  <Divider
                    variant="fullWidth"
                    sx={{
                      marginTop: 2,
                      borderColor: 'black',
                      borderBottomWidth: 5,
                    }}
                  />
                </Box>
              )
            })}
          </Box>
        )
      }
    },
    [options]
  )

  const getDataForEoS = useCallback(
    (report: ExportableReportResult) => {
      if (report?.impacts) {
        return (
          <Box>
            {report.impacts.map((impact, index) => {
              const text = generateReportSpecificText(
                report.exportableReportType,
                options,
                {} as RouteSpeed,
                impact
              )
              let eoSCharts: EffectivenessOfStrategiesDto[] = []
              if (report.effectivenessOfStrategiesCharts) {
                eoSCharts = report?.effectivenessOfStrategiesCharts.filter(
                  (chart) =>
                    (impact.segmentIds as string[])?.includes(
                      chart.segmentId as string
                    )
                )
              }
              return (
                <Box key={index} sx={{ marginTop: 3 }}>
                  <Typography variant="h6" fontWeight="bold" gutterBottom>
                    {`Rank ${index + 1}: Segment ${impact.description}`}
                  </Typography>
                  <Typography variant="h6" gutterBottom>
                    {text}
                  </Typography>
                  <Box
                    component="pre"
                    sx={{ whiteSpace: 'pre-wrap', fontSize: 14 }}
                  >
                    {eoSCharts.length && generateEoSChart(eoSCharts)}
                  </Box>
                  <Divider
                    variant="fullWidth"
                    sx={{
                      marginTop: 2,
                      borderColor: 'black',
                      borderBottomWidth: 5,
                    }}
                  />
                </Box>
              )
            })}
          </Box>
        )
      }
    },
    [options]
  )

  const generateDataForReport = useCallback(
    (report: ExportableReportResult) => {
      if (
        report.exportableReportType ===
        ExportableReportType.EffectivenessOfStrategies
      ) {
        return getDataForEoS(report)
      } else {
        return getDataForSegments(report)
      }
    },
    [getDataForEoS, getDataForSegments]
  )

  const generateReportMap = useCallback(
    async (report: ExportableReportResult) => {
      const { default: ReportMap } = await import(
        './components/reportMap/ReportMap'
      )
      const reportData = report.reportData
      const speedSegments = getAdjustedRoutes(routeSpeeds)
      let impactSegments: ImpactHotspotForReportMap[] = []
      let hotspotSegments: HotSpotForReportMap[] = []
      if (report?.impacts?.length) {
        impactSegments = getImpactHostpot(report.impacts, speedSegments)
        hotspotSegments = createHotspotSegmentsFromImpacts(impactSegments)
      } else if (reportData && reportData?.length) {
        hotspotSegments = getHotspotsBasedOnChartType(reportData, speedSegments)
      }

      return (
        <Box sx={{ marginTop: 2, padding: 2, width: '100%', height: '500px' }}>
          <ReportMap
            routes={speedSegments || []}
            hotspots={hotspotSegments || []}
            impacts={impactSegments || []}
          />
        </Box>
      )
    },
    [routeSpeeds]
  )

  const generateReportContent = useCallback(async () => {
    const reports: ReactNode[] = []
    let index = 0
    for (const report of data) {
      if (!report.reportData) {
        continue
      }
      const reportMap = await generateReportMap(report)
      const table = generateTable(report, index)
      const data = generateDataForReport(report)
      reports.push(
        <Box key={index} sx={{ marginTop: 2 }} className="avoid-page-break">
          <Typography variant="h6" fontWeight="bold" gutterBottom>
            {`${report.name} Report`}
          </Typography>
          <Typography variant="h6" gutterBottom>
            {generateBaseText(report.exportableReportType, options)}
          </Typography>
          <Box component="pre" sx={{ whiteSpace: 'pre-wrap', fontSize: 10 }}>
            {table}
          </Box>
          <Box className="page-break" sx={{ marginTop: 2 }}>
            {reportMap}
          </Box>
          <Box sx={{ marginTop: 2 }}>{data}</Box>
        </Box>
      )
      index++
    }
    return reports
  }, [data, generateDataForReport, generateReportMap, options])

  useEffect(() => {
    const fetchReportContent = async () => {
      const content = await generateReportContent()
      setReportContent(content)
    }

    fetchReportContent()
  }, [data, generateReportContent, options, routeSpeeds])

  const handleDownload = useReactToPrint({
    onPrintError: (error) => console.log(error),
    contentRef: contentRef,
    print: async (printIframe) => {
      const document = printIframe.contentDocument
      if (document) {
        const { default: html2pdf } = await import('html2pdf.js')
        const html = document.getElementsByClassName('test')[0]
        const exporter = new html2pdf(html)
        await exporter.getPdf(true)
      }
    },
    pageStyle: `
    @page {
      size: A4;
      @top-center {
        content: "";
      }
      @bottom-left {
        content: counter(page);
        font-size: 10pt;
      }
      @bottom-center {
        content: "CONFIDENTIAL: RECORDS PRODUCED WITH THIS REQUEST MAY BE PROTECTED UNDER 23 USC 407.";
        font-size: 8pt;
        font-weight: bold;
      }
    }
    @media print {
      body {
        -webkit-print-color-adjust: exact;
      }
      .MuiBox-root {
        width: 100%;
      }
      .MuiTypography-root {
        font-size: 12pt;
      }
      .MuiTable-root {
        width: 100%;
        border-collapse: collapse;
      }
      .MuiTableCell-root {
      }
      .page-break {
        page-break-before: always;
      }
      // .avoid-page-break {
      //   break-inside: avoid;
      // }
    }
  `,
  })

  const generateTable = (report: ExportableReportResult, index: number) => {
    const { headers, rows, boldedColumn } = getTableData(report)
    return (
      <Table key={index} size="small">
        <TableHead>
          <TableRow>
            {headers.map((header, index) => (
              <TableCell key={index}>{header}</TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row, index) => (
            <TableRow key={index}>
              {row.map((cell, index) => {
                let fontWeight = 'none'
                if (boldedColumn && index == boldedColumn) {
                  fontWeight = 'bold'
                }
                return (
                  <TableCell key={index} sx={{ fontWeight }}>
                    {cell}
                  </TableCell>
                )
              })}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    )
  }

  return (
    <Box
      sx={{ display: 'flex', alignItems: 'center', flexDirection: 'column' }}
    >
      <Box sx={{ display: 'flex', flexDirection: 'column' }}>
        {!handler.error &&
          handler.reportData &&
          handler.reportData.length > 0 && (
            <>
              <Box
                sx={{
                  display: 'flex',
                  justifyContent: 'flex-end',
                  width: '100%',
                  gap: 2,
                }}
              >
                <Button
                  size="small"
                  variant="outlined"
                  startIcon={<PrintIcon />}
                  color="primary"
                  onClick={() => reactToPrintFn()}
                >
                  Print
                </Button>
              </Box>
              <Paper
                ref={contentRef}
                sx={{
                  marginTop: 2,
                  width: '850px',
                  padding: 2,
                }}
              >
                <Typography
                  sx={{ marginTop: 2 }}
                  variant="h3"
                  id="test"
                  align="center"
                  fontWeight="bold"
                  gutterBottom
                >
                  Speed Management Reports
                </Typography>
                <>
                  <Typography variant="h5" fontWeight="bold" gutterBottom>
                    Filters Summary
                  </Typography>
                  <Box sx={{ whiteSpace: 'pre-wrap', fontSize: 14 }}>
                    {formatFilters(options)}
                  </Box>
                  {reportContent}
                </>
              </Paper>
            </>
          )}
      </Box>
    </Box>
  )
}
