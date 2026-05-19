import {
  AggClassification,
  CongestionTrackingDto,
  EffectivenessOfStrategiesDto,
  ExportableReportOptions,
  ExportableReportResult,
  ExportableReportType,
  ImpactDto,
  ImpactSpeedAggregationCategoryFilter,
  RouteSpeed,
  SpeedComplianceDto,
  SpeedDataType,
  SpeedVariabilityDto,
  SpeedViolationsDto,
  TimePeriodFilter,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { transformCongestionTrackerData } from '@/features/charts/speedManagementTool/congestionTracker/congestionTracker.transformer'
import transformEffectivenessOfStrategiesData from '@/features/charts/speedManagementTool/effectivenessOfStrategies/effectivenessOfStrategies.transformer'
import transformSpeedComplianceData from '@/features/charts/speedManagementTool/speedCompliance/speedCompliance.transformer'
import transformSpeedVariabilityData from '@/features/charts/speedManagementTool/speedVariability/speedVariability.transformer'
import transformSpeedViolationsData from '@/features/charts/speedManagementTool/speedViolations/speedViolations.transformer'
import {
  RoutesResponse,
  SpeedManagementRoute,
} from '@/features/speedManagementTool/types/routes'
import { EChartsOption, init } from 'echarts'
import jsPDF from 'jspdf'
import autoTable, { CellHookData } from 'jspdf-autotable'
import { MemoExoticComponent, useCallback } from 'react'
import { HotSpotForReportMap, ImpactHotspotForReportMap } from '../../types'
import { useReportMapHandler } from './ReportMapHandler'

interface Props {
  options: ExportableReportOptions
}

export const AggClassificationMapping = {
  [AggClassification.Total]: 'All Days',
  [AggClassification.Weekend]: 'Weekend',
  [AggClassification.Weekday]: 'Weekdays',
} as const

export const TimePeriodFilterMapping = {
  [TimePeriodFilter.AllDay]: 'All Hours',
  [TimePeriodFilter.OffPeak]: 'Off Peak',
  [TimePeriodFilter.AmPeak]: 'AM Peak',
  [TimePeriodFilter.PmPeak]: 'PM Peak',
  [TimePeriodFilter.MidDay]: 'Mid Day',
  [TimePeriodFilter.Evening]: 'Evening',
  [TimePeriodFilter.EarlyMorning]: 'Early Morning',
} as const

export const SourceIdToNamingMapping = {
  1: 'ATSPM',
  2: 'Pems',
  3: 'Clearguide',
}

export const SpeedDataTypeMapping = {
  [SpeedDataType.H]: 'Hourly',
  [SpeedDataType.M]: 'Monthly',
}

interface MapProps {
  routes: SpeedManagementRoute[]
  hotspots: HotSpotForReportMap[]
  impacts: ImpactHotspotForReportMap[]
}
export interface SpeedPdfHandler {
  generatePDF(
    data: ExportableReportResult[],
    routeSpeeds: RoutesResponse,
    changeIsPdfDownloaded: (val: boolean | null) => void,
    ReportMap: MemoExoticComponent<(props: MapProps) => JSX.Element>
  ): void
}

const roundToWholeNumber = (value: number | null | undefined): string => {
  return value !== null && value !== undefined ? value.toFixed(0) : 'N/A'
}

// Utility function to round numbers to two decimal places
const roundToTwoDecimals = (value: number | null | undefined): string =>
  value !== null && value !== undefined ? value.toFixed(2) : 'N/A'

const generateLocationFilter = (options: ExportableReportOptions): string => {
  const { city, county, region, functionalType, accessCategory } = options
  let regionNaming: string | undefined
  if (region && region !== undefined) {
    regionNaming = `Region ${region}`
  }
  const filters = [
    city,
    county,
    regionNaming,
    functionalType,
    accessCategory,
  ].filter(Boolean)
  return filters.join(', ')
}

const createLeaderShipFilterDescription = (
  options: ExportableReportOptions
): string => {
  const locationFilter = generateLocationFilter(options)
  const daysOfWeekFilter = options.aggClassification
    ? AggClassificationMapping[options.aggClassification]
    : 'All Days'
  const hoursFilter = options.timePeriod
    ? TimePeriodFilterMapping[options.timePeriod]
    : 'All Hours'
  const { startDate, endDate } = options

  return `The following report for ${
    SourceIdToNamingMapping[
      options.sourceId as keyof typeof SourceIdToNamingMapping
    ]
  } details the results of speed analyses on Utah state routes in ${locationFilter} between ${startDate} and ${endDate}. Only ${daysOfWeekFilter} and the hours between ${hoursFilter} were included in the analysis.`
}

const generateBaseText = (
  reportType: ExportableReportType,
  options: ExportableReportOptions
): string => {
  switch (reportType) {
    case ExportableReportType.Violations:
      return `The 5 segments in the Table are the segments that experienced the highest percentage of vehicles travelling at a violation speed (7 MPH over the speed limit for freeways, or 2 MPH over the speed limit otherwise).`
    case ExportableReportType.MostImproved:
      return `The 5 segments in the Table are the segments that experienced the greatest reduction in percentage of vehicles travelling at a violation speed (7 MPH over the speed limit for freeways, or 2 MPH over the speed limit otherwise).`
    case ExportableReportType.ComplianceToSpeedLimit:
      return `The 5 segments in the Table are the segments in that experienced the greatest difference between 85th percentile speeds and the posted speed limit (where the 85th percentile speed was higher than the posted speed limit).`
    case ExportableReportType.SpeedVariability:
      return `The 5 segments in the Table are the segments in that experienced the greatest range between daily minimum and maximum speed on average.`
    case ExportableReportType.EffectivenessOfStrategies:
      return `The segments in the Table are the segments in that experienced the largest speed reductions following the implementation of a speed management strategy.`
    default:
      return ''
  }
}

const generateReportSpecificText = (
  reportType: ExportableReportType,
  options: ExportableReportOptions,
  segment: RouteSpeed,
  impact?: ImpactDto
): string => {
  const daysOfWeekFilter = options.aggClassification
    ? AggClassificationMapping[options.aggClassification]
    : 'All Days'
  const hoursFilter = options.timePeriod
    ? TimePeriodFilterMapping[options.timePeriod]
    : 'All Hours'

  switch (reportType) {
    case ExportableReportType.Violations:
      return `${roundToTwoDecimals(
        segment.percentViolations
      )}% of the vehicles on ${
        segment.name
      } were traveling at a violation speed (7 MPH over the speed limit for freeways or 2 MPH over the speed limit otherwise) on ${daysOfWeekFilter} during ${hoursFilter} between ${
        options.startDate
      } and ${options.startDate}.`
    case ExportableReportType.MostImproved:
      return `The percentage of vehicles on ${
        segment.name
      } traveling at a violation speed (7 MPH over the speed limit for freeways or 2 MPH over the speed limit otherwise) on ${daysOfWeekFilter} during ${hoursFilter}, resulting in a ${roundToTwoDecimals(
        segment.percentViolations
      )} decrease in percent violations.`
    case ExportableReportType.ComplianceToSpeedLimit:
      return `The 85th percentile speed on ${
        segment.name
      } was ${roundToWholeNumber(
        segment.averageEightyFifthSpeed
      )} MPH on ${daysOfWeekFilter} during ${hoursFilter} between ${
        options.startDate
      } and ${options.endDate}. The posted speed limit was ${
        segment.speedLimit
      } MPH, which means the 85th percentile speed was ${roundToWholeNumber(
        (segment.averageEightyFifthSpeed as number) -
          (segment.speedLimit as number)
      )} MPH higher than the posted speed limit.`
    case ExportableReportType.SpeedVariability:
      return `On average, the daily speeds on ${
        segment.name
      } ranged from a minimum of ${roundToWholeNumber(
        segment.minSpeed
      )} MPH to a maximum of ${roundToWholeNumber(
        segment.maxSpeed
      )} MPH on ${daysOfWeekFilter} during ${hoursFilter} between ${
        options.startDate
      } and ${
        options.endDate
      }. This equates to a daily speed variability of ${roundToWholeNumber(
        segment.variability
      )} MPH.`
    case ExportableReportType.EffectivenessOfStrategies:
      return `The speed management strategy, Change in Average Speed, applied to ${impact?.description} was implemented on ${impact?.createdOn}. For 6 weeks before the implemention, the 85th percentile speed was ${roundToWholeNumber(
        impact?.beforeAverageEightyFifthSpeed
      )} MPH. 6 weeks after the implementation and a 6-week transition period, the 85th percentile speed decreased to ${roundToWholeNumber(
        impact?.afterAverageEightyFifthSpeed
      )} MPH, resulting in a ${roundToWholeNumber(
        (impact?.afterAverageEightyFifthSpeed as number) -
          (impact?.beforeAverageEightyFifthSpeed as number)
      )} MPH decrease in the 85th percentile speed.`
    default:
      return ''
  }
}

export const usePDFHandler = (props: Props): SpeedPdfHandler => {
  const { options } = props
  const mapHandler = useReportMapHandler()
  const generatePDF = useCallback(
    async (
      data: ExportableReportResult[],
      routeSpeeds: RoutesResponse,
      changeIsPdfDownloaded: (val: boolean | null) => void,
      ReportMap: MemoExoticComponent<(props: MapProps) => JSX.Element>
    ) => {
      try {
        const pdf = new jsPDF()
        const marginX = 10
        const marginY = 20
        const pageHeight = pdf.internal.pageSize.height // Page height in PDF units

        const getTableData = (
          report: ExportableReportResult
        ): {
          headers: string[]
          rows: string[][]
          didParseCell: (data: CellHookData) => void
        } => {
          let headers: string[] = []
          const rows: string[][] = []

          if (
            report.exportableReportType ===
              ExportableReportType.EffectivenessOfStrategies &&
            report.impacts
          ) {
            headers = [
              'Rank',
              'Name',
              '% Change in Eighty Fifth Speed',
              'Speed Limit',
            ]
            rows.push(
              ...(report.impacts?.map((item, idx) => [
                `${idx + 1}`,
                item?.description ?? '',
                `${roundToTwoDecimals(
                  item?.changeInEightyFifthPercentileSpeed
                )}`,
                `${item?.speedLimit} MPH`,
              ]) ?? [])
            )
            return {
              headers,
              rows,
              didParseCell(data) {
                if (data.column.index === 2) {
                  data.cell.styles.fontStyle = 'bold'
                }
              },
            }
          }

          if (!report.reportData || report.reportData.length === 0) {
            return {
              headers: ['Rank', 'Unknown Data', 'Name'],
              rows: [['No Data', '', '']],
              didParseCell: () => {
                return
              },
            }
          }

          switch (report.exportableReportType) {
            case ExportableReportType.Violations:
            case ExportableReportType.MostImproved:
              headers = ['Rank', 'Name', 'Percent Violations', 'Speed Limit']
              rows.push(
                ...report.reportData.map((item, idx) => [
                  `${idx + 1}`,
                  item?.name ?? '',
                  `${roundToTwoDecimals(item?.percentViolations)} %`,
                  `${item?.speedLimit} MPH`,
                ])
              )
              return {
                headers,
                rows,
                didParseCell: (data) => {
                  if (data.column.index === 2) {
                    data.cell.styles.fontStyle = 'bold'
                  }
                },
              }
            case ExportableReportType.ComplianceToSpeedLimit:
              headers = [
                'Rank',
                'Name',
                '85th Speed vs Limit',
                'Speed Limit',
                'Difference',
              ]
              const isChangePositive = report.reportData.map(
                (item) => (item.eightyFifthSpeedVsSpeedLimit as number) > 0
              )
              rows.push(
                ...report.reportData.map((item, idx) => [
                  `${idx + 1}`,
                  item?.name ?? '',
                  `${roundToWholeNumber(item?.averageEightyFifthSpeed)} MPH`,
                  `${item?.speedLimit} MPH`,
                  `${isChangePositive[idx] ? '+' : ''}${roundToWholeNumber(
                    item?.eightyFifthSpeedVsSpeedLimit as number
                  )} MPH`,
                ])
              )
              return {
                headers,
                rows,
                didParseCell: (data) => {
                  if (data.column.index === 4) {
                    data.cell.styles.fontStyle = 'bold'
                  }
                },
              }
            case ExportableReportType.SpeedVariability:
              headers = [
                'Rank',
                'Name',
                'Max Speed',
                'Min Speed',
                'Variability',
                'Speed Limit',
              ]
              rows.push(
                ...report.reportData.map((item, idx) => [
                  `${idx + 1}`,
                  item?.name ?? '',
                  `${roundToWholeNumber(item?.maxSpeed)} MPH`,
                  `${roundToWholeNumber(item?.minSpeed)} MPH`,
                  `${roundToWholeNumber(item?.variability)} MPH`,
                  `${item?.speedLimit} MPH`,
                ])
              )
              return {
                headers,
                rows,
                didParseCell: (data) => {
                  if (data.column.index === 4) {
                    data.cell.styles.fontStyle = 'bold'
                  }
                },
              }
            default:
              headers = ['Rank', 'Unknown Data', 'Name']
              rows.push(['No Data', '', ''])
          }

          return {
            headers,
            rows,
            didParseCell: () => {
              return
            },
          }
        }

        const checkPageHeight = (currentY: number, requiredHeight: number) => {
          if (currentY + requiredHeight > pageHeight) {
            pdf.addPage()
            return marginY // Reset to top margin on the new page
          }
          return currentY
        }

        const generateCongestionChartSnapshot = async (
          chartData: CongestionTrackingDto
        ): Promise<string | null> => {
          const container = document.createElement('div')
          container.style.width = '800px'
          container.style.height = '600px'
          container.style.position = 'absolute'
          container.style.top = '-10000px'
          container.style.left = '-10000px'
          document.body.appendChild(container)
          const chart = init(container, null, { renderer: 'canvas' })
          const options = transformCongestionTrackerData(chartData, 'month')
            .charts[0] as EChartsOption
          chart.setOption(options as EChartsOption)
          await new Promise<void>((resolve) => {
            chart.on('finished', () => resolve())
          })

          const base64 = chart.getDataURL({ type: 'png' })
          chart.dispose()
          document.body.removeChild(container)
          return base64
        }

        const generateViolationChartSnapshot = async (
          chartData: SpeedViolationsDto
        ): Promise<string | null> => {
          const container = document.createElement('div')
          container.style.width = '800px'
          container.style.height = '600px'
          container.style.position = 'absolute'
          container.style.top = '-10000px'
          container.style.left = '-10000px'
          document.body.appendChild(container)
          const chart = init(container, null, { renderer: 'canvas' })
          const options = transformSpeedViolationsData([chartData])
          chart.setOption(options.charts[0]?.chart as EChartsOption)
          await new Promise<void>((resolve) => {
            chart.on('finished', () => resolve())
          })

          const base64 = chart.getDataURL({ type: 'png' })
          chart.dispose()
          document.body.removeChild(container)
          return base64
        }

        const generateVariabilityChartSnapshot = async (
          chartData: SpeedVariabilityDto
        ): Promise<string | null> => {
          const container = document.createElement('div')
          container.style.width = '800px'
          container.style.height = '600px'
          container.style.position = 'absolute'
          container.style.top = '-10000px'
          container.style.left = '-10000px'
          document.body.appendChild(container)
          const chart = init(container, null, { renderer: 'canvas' })
          const options = transformSpeedVariabilityData(chartData)
            .charts[0] as EChartsOption
          chart.setOption(options)
          await new Promise<void>((resolve) => {
            chart.on('finished', () => resolve())
          })

          const base64 = chart.getDataURL({ type: 'png' })
          chart.dispose()
          document.body.removeChild(container)
          return base64
        }

        const generateComplianceChartSnapshot = async (
          chartData: SpeedComplianceDto
        ): Promise<string | null> => {
          const container = document.createElement('div')
          container.style.width = '800px'
          container.style.height = '600px'
          container.style.position = 'absolute'
          container.style.top = '-10000px'
          container.style.left = '-10000px'
          document.body.appendChild(container)
          const chart = init(container, null, { renderer: 'canvas' })
          const options = transformSpeedComplianceData(
            [chartData],
            null,
            false
          ) as EChartsOption
          chart.setOption(options)
          await new Promise<void>((resolve) => {
            chart.on('finished', () => resolve())
          })

          const base64 = chart.getDataURL({ type: 'png' })
          chart.dispose()
          document.body.removeChild(container)
          return base64
        }

        const generateEffectivenessOfStrategiesChartSnapshot = async (
          chartData: EffectivenessOfStrategiesDto[]
        ) => {
          const container = document.createElement('div')
          container.style.width = '800px'
          container.style.height = '600px'
          container.style.position = 'absolute'
          container.style.top = '-10000px'
          container.style.left = '-10000px'
          document.body.appendChild(container)
          const chart = init(container, null, { renderer: 'canvas' })
          const options = transformEffectivenessOfStrategiesData(
            chartData,
            undefined,
            ImpactSpeedAggregationCategoryFilter.ChangeInEightyFifthPercentileSpeed
          ) as EChartsOption
          chart.setOption(options)
          await new Promise<void>((resolve) => {
            chart.on('finished', () => resolve())
          })

          const base64 = chart.getDataURL({ type: 'png' })
          chart.dispose()
          document.body.removeChild(container)
          return base64
        }

        const addChartImage = (
          chartSnapshot: string | null,
          pdf: jsPDF,
          currentY: number,
          marginX: number
        ) => {
          if (chartSnapshot) {
            const chartHeight = 100 // Height of the chart in PDF units
            const chartWidth = 180 // Width of the chart in PDF units
            const adjustedY = checkPageHeight(currentY, chartHeight)
            pdf.addImage(
              chartSnapshot,
              'PNG',
              marginX,
              adjustedY,
              chartWidth,
              chartHeight
            )
            return adjustedY + chartHeight + 10 // Add spacing after chart
          }
          return currentY
        }

        const generateReportSpecificChart = async (
          pdf: jsPDF,
          report: ExportableReportResult,
          data: RouteSpeed,
          startY: number,
          startX: number
        ): Promise<number> => {
          let currentY = startY

          switch (report.exportableReportType) {
            case ExportableReportType.Violations:
            case ExportableReportType.MostImproved:
              if (report.speedViolationCharts) {
                const chartData = report.speedViolationCharts.find(
                  (chart) => chart.segmentId === data.segmentId
                )
                if (chartData) {
                  const chartSnapshot =
                    await generateViolationChartSnapshot(chartData)
                  currentY = addChartImage(chartSnapshot, pdf, currentY, startX)
                }
              }
              break
            case ExportableReportType.ComplianceToSpeedLimit:
              if (report.speedComplianceCharts) {
                const chartData = report.speedComplianceCharts.find(
                  (chart) => chart.segmentId === data.segmentId
                )
                if (chartData) {
                  const chartSnapshot =
                    await generateComplianceChartSnapshot(chartData)
                  currentY = addChartImage(chartSnapshot, pdf, currentY, startX)
                }
              }
              break
            case ExportableReportType.SpeedVariability:
              if (report.speedVariabilityCharts) {
                const chartData = report.speedVariabilityCharts.find(
                  (chart) => chart.segmentId === data.segmentId
                )
                if (chartData) {
                  const chartSnapshot =
                    await generateVariabilityChartSnapshot(chartData)
                  currentY = addChartImage(chartSnapshot, pdf, currentY, startX)
                }
              }
              break
            default:
              break
          }
          return currentY
        }

        const getDataForEoS = async (
          pdf: jsPDF,
          report: ExportableReportResult,
          startY: number,
          startX: number
        ) => {
          let currentY = startY

          if (report.impacts) {
            let i = 1
            for (const impact of report.impacts) {
              currentY = checkPageHeight(currentY, 50) // Check if enough space for the property table
              pdf.setFont('helvetica', 'bold')
              pdf.text(
                `Rank ${i}: Impact ${impact.description}`,
                startX,
                currentY
              )
              pdf.setFont('helvetica', 'normal')
              currentY += 5

              const specificText = generateReportSpecificText(
                report.exportableReportType as ExportableReportType,
                options,
                {} as RouteSpeed,
                impact
              )
              const pageWidth = pdf.internal.pageSize.width
              const textWidth = pageWidth - 2 * startX
              const wrappedText = pdf.splitTextToSize(specificText, textWidth)
              const baseTextHeight = wrappedText.length * 7 // Approximate height of the base text in PDF units

              pdf.text(wrappedText, startX, currentY + 8)
              currentY += baseTextHeight
              // createPropertyTable(pdf, data, currentY)

              if (report?.effectivenessOfStrategiesCharts) {
                const chartData = report.effectivenessOfStrategiesCharts.filter(
                  (chart) =>
                    (impact.segmentIds as string[])?.includes(
                      chart.segmentId as string
                    )
                )

                if (chartData) {
                  const chartSnapshot =
                    await generateEffectivenessOfStrategiesChartSnapshot(
                      chartData as EffectivenessOfStrategiesDto[]
                    )
                  currentY = addChartImage(chartSnapshot, pdf, currentY, startX)
                }
              }
              i++
            }
          }
        }

        const getDataForSegments = async (
          pdf: jsPDF,
          report: ExportableReportResult,
          startY: number,
          startX: number
        ) => {
          let currentY = startY

          if (report.reportData) {
            let i = 1
            for (const data of report.reportData) {
              currentY = checkPageHeight(currentY, 50) // Check if enough space for the property table
              pdf.setFont('helvetica', 'bold')
              pdf.text(`Rank ${i}: Segment ${data.name}`, startX, currentY)
              pdf.setFont('helvetica', 'normal')
              currentY += 5

              const specificText = generateReportSpecificText(
                report.exportableReportType as ExportableReportType,
                options,
                data,
                report.impacts ? report.impacts[i - 1] : undefined
              )
              const pageWidth = pdf.internal.pageSize.width
              const textWidth = pageWidth - 2 * startX
              const wrappedText = pdf.splitTextToSize(specificText, textWidth)
              const baseTextHeight = wrappedText.length * 7 // Approximate height of the base text in PDF units

              pdf.text(wrappedText, startX, currentY + 8)
              currentY += baseTextHeight
              // createPropertyTable(pdf, data, currentY)

              if (report?.congestionTrackingCharts) {
                const congestionChartData =
                  report.congestionTrackingCharts.find(
                    (chart) => chart.segmentId === data.segmentId
                  )

                if (congestionChartData) {
                  const chartSnapshot = await generateCongestionChartSnapshot(
                    congestionChartData as CongestionTrackingDto
                  )
                  currentY = addChartImage(chartSnapshot, pdf, currentY, startX)
                }
              }
              currentY = await generateReportSpecificChart(
                pdf,
                report,
                data,
                currentY,
                startX
              )
              i++
            }
          }
        }

        // Function to format filters into a string
        const formatFilters = (opts: ExportableReportOptions): string => {
          const filterDescriptions: string[] = []

          if (opts.reportManagementType === 'Leadership') {
            filterDescriptions.push(createLeaderShipFilterDescription(opts))
            return filterDescriptions.join('\n')
          }

          if (opts.accessCategory)
            filterDescriptions.push(`Access Category: ${opts.accessCategory}`)
          if (opts.aggClassification)
            filterDescriptions.push(
              `Aggregation Classification: ${opts.aggClassification}`
            )
          if (opts.city) filterDescriptions.push(`City: ${opts.city}`)
          if (opts.county) filterDescriptions.push(`County: ${opts.county}`)
          if (opts.startDate && opts.endDate) {
            filterDescriptions.push(
              `Date Range: ${opts.startDate} to ${opts.endDate}`
            )
          } else if (opts.startDate) {
            filterDescriptions.push(`Start Date: ${opts.startDate}`)
          } else if (opts.endDate) {
            filterDescriptions.push(`End Date: ${opts.endDate}`)
          }
          if (opts.functionalType)
            filterDescriptions.push(`Functional Type: ${opts.functionalType}`)
          if (opts.limit) filterDescriptions.push(`Limit: ${opts.limit}`)
          if (opts.order) filterDescriptions.push(`Order: ${opts.order}`)
          if (opts.region) filterDescriptions.push(`Region: ${opts.region}`)
          if (opts.sourceId !== null && opts.sourceId !== undefined)
            filterDescriptions.push(
              `Source ID: ${
                SourceIdToNamingMapping[
                  opts.sourceId as keyof typeof SourceIdToNamingMapping
                ]
              }`
            )
          if (opts.speedDataType)
            filterDescriptions.push(
              `Speed Data Type: ${SpeedDataTypeMapping[opts.speedDataType]}`
            )
          if (opts.timePeriod)
            filterDescriptions.push(`Time Period: ${opts.timePeriod}`)

          return filterDescriptions.join('\n')
        }

        const generateMapSnapshots = async () => {
          return Promise.all(
            data.map((report) =>
              mapHandler.generateMapSnapshot(
                routeSpeeds,
                report.reportData as RouteSpeed[],
                ReportMap,
                report.impacts ? report.impacts : undefined
              )
            )
          )
        }

        const mapSnapshots = await generateMapSnapshots()

        pdf.setFont('helvetica', 'bold')
        pdf.setFontSize(16) // Set font size for the main header
        const mainHeader = 'Speed Management Reports'
        const pageWidth = pdf.internal.pageSize.width
        const headerX = (pageWidth - pdf.getTextWidth(mainHeader)) / 2 // Center the header
        const marginYHeader = marginY // Use the same marginY for the header
        pdf.text(mainHeader, headerX, marginYHeader)

        // Add space below the main header
        const subHeaderY = marginYHeader + 10

        // Set up the sub-header for Filters Summary
        pdf.setFont('helvetica', 'bold')
        pdf.setFontSize(12) // Smaller font size for the sub-header
        pdf.text('Filters Summary', marginX, subHeaderY)

        // Reset to normal font for the filter text
        pdf.setFont('helvetica', 'normal')
        const filterText = formatFilters(options)
        const textLines = pdf.splitTextToSize(
          filterText,
          pageWidth - marginX * 2
        )
        let currentY = subHeaderY + 10
        pdf.text(textLines, marginX, currentY)

        currentY += textLines.length * 7 // Approximate height of the filter text

        let index = 0
        for (const report of data) {
          let reportY = currentY
          if (index > 0) {
            pdf.addPage()
            reportY = marginY
          }

          pdf.setFont('helvetica', 'bold')
          pdf.text(`${report.name}`, marginX, reportY)
          pdf.setFont('helvetica', 'normal')

          const baseText = generateBaseText(
            report.exportableReportType as ExportableReportType,
            options
          )
          const pageWidth = pdf.internal.pageSize.width
          const startX = 10
          const textWidth = pageWidth - 2 * startX
          const wrappedText = pdf.splitTextToSize(baseText, textWidth)
          const baseTextHeight = wrappedText.length * 7 // Approximate height of the base text in PDF units

          pdf.text(wrappedText, startX, reportY + 8)
          reportY += baseTextHeight

          const { headers, rows, didParseCell } = getTableData(report)

          if (rows.length > 0) {
            autoTable(pdf, {
              startY: reportY,
              head: [headers],
              body: rows,
              margin: { left: marginX, right: marginX },
              styles: { halign: 'center', valign: 'middle', fontSize: 8 },
              didParseCell: didParseCell,
            })
            reportY += rows.length * 10 + 7 // Approximate height of the table
          } else {
            pdf.text(
              'No data available for this report.',
              marginX,
              reportY + 30
            )
            reportY += 40
          }
          const mapBase64 = mapSnapshots[index]
          pdf.addImage(mapBase64, 'PNG', marginX, reportY, 180, 120)

          reportY += 130 // Approximate height of the map in PDF units
          if (
            report.exportableReportType ===
            ExportableReportType.EffectivenessOfStrategies
          ) {
            await getDataForEoS(pdf, report, reportY, marginX)
          } else {
            await getDataForSegments(pdf, report, reportY, marginX)
          }

          currentY = reportY
          index++
        }

        const pageCount = pdf.getNumberOfPages()
        const footerText =
          'CONFIDENTIAL: RECORDS PRODUCED WITH THIS REQUEST MAY BE PROTECTED UNDER 23 USC 407.'
        pdf.setFontSize(8)
        for (let i = 1; i <= pageCount; i++) {
          pdf.setPage(i)
          const pageWidth = pdf.internal.pageSize.width
          const textWidth = pdf.getTextWidth(footerText)
          const textX = (pageWidth - textWidth) / 2
          pdf.text(footerText, textX, pdf.internal.pageSize.height - 10)
        }

        const pdfBlob = pdf.output('blob')
        const url = URL.createObjectURL(pdfBlob)
        const link = document.createElement('a')
        link.href = url
        link.download = 'report.pdf'
        document.body.appendChild(link)
        link.click()
        link.remove()
        URL.revokeObjectURL(url)
        changeIsPdfDownloaded(false)
      } catch (error) {
        console.error('Error generating PDF:', error)
        changeIsPdfDownloaded(false)
      }
    },
    [mapHandler, options]
  )

  return { generatePDF }
}
