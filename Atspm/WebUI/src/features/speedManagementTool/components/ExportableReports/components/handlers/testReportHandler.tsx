import {
  ExportableReportOptions,
  ExportableReportResult,
  ExportableReportType,
  ImpactDto,
  RouteSpeed,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import {
  RoutesResponse,
  SpeedManagementRoute,
} from '@/features/speedManagementTool/types/routes'
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { ReactNode, useCallback } from 'react'
import { formatFilters, generateBaseText, getTableData } from '../../common'
import { HotSpotForReportMap, ImpactHotspotForReportMap } from '../../types'

// KEEP FOR UTILIZING IN FUTURE

interface Props {
  options: ExportableReportOptions
}

interface MapProps {
  routes: SpeedManagementRoute[]
  hotspots: HotSpotForReportMap[]
  impacts: ImpactHotspotForReportMap[]
}

interface ReportPageProps {
  children: React.ReactNode
}

export interface SpeedPdfHandler {
  generatePDF(
    data: ExportableReportResult[],
    routeSpeeds: RoutesResponse
    // changeIsPdfDownloaded: (val: boolean | null) => void,
    // ReportMap: MemoExoticComponent<(props: MapProps) => JSX.Element>
  ): ReactNode
}

const getHotspotSegments = (
  segmentIds: string[],
  routes: SpeedManagementRoute[]
): HotSpotForReportMap[] => {
  if (segmentIds.length === 0) {
    return []
  }
  return segmentIds.map((id) => {
    const coordinates = routes.find((route) => route.properties.route_id === id)
      ?.geometry.coordinates as [number, number][]
    return {
      segmentId: id,
      coordinates: coordinates?.map((pair) => [pair[1], pair[0]]),
    }
  })
}

const getHotspotsBasedOnChartType = (
  speedSegments: RouteSpeed[],
  segmentSpeeds: SpeedManagementRoute[]
): HotSpotForReportMap[] => {
  let hotspotSegments: HotSpotForReportMap[] = []
  const segmentIds = speedSegments?.map((s) => s.segmentId as string)
  hotspotSegments = getHotspotSegments(segmentIds, segmentSpeeds)
  return hotspotSegments
}

const getAdjustedRoutes = (
  routeSpeeds: RoutesResponse
): SpeedManagementRoute[] => {
  const filteredRoutes = routeSpeeds?.features.filter(
    (route) => route?.geometry?.coordinates
  )

  const routes: SpeedManagementRoute[] =
    filteredRoutes?.map((feature) => ({
      ...feature,
      geometry: {
        ...feature.geometry,
        coordinates: feature.geometry.coordinates.map((coord) => [
          coord[1],
          coord[0],
        ]),
      },
      properties: feature.properties,
    })) || []

  return routes
}

const getImpactHostpot = (
  impacts: ImpactDto[],
  routeSpeeds: SpeedManagementRoute[]
): ImpactHotspotForReportMap[] => {
  const impactSegments: ImpactHotspotForReportMap[] = impacts.map((impact) => {
    const segmentIds = impact.segmentIds as string[]
    const coordinates = getHotspotSegments(
      segmentIds.filter((id) =>
        routeSpeeds.find((r) => r.properties.route_id === id)
      ),
      routeSpeeds
    )
    return {
      impactId: impact.id as string,
      impactedSegments: coordinates,
    }
  })
  return impactSegments
}

const createHotspotSegmentsFromImpacts = (
  impacts: ImpactHotspotForReportMap[]
): HotSpotForReportMap[] => {
  let hotspotSegments: HotSpotForReportMap[] = []
  impacts.forEach((impact) => {
    const middleImpactSegment = Math.floor(impact.impactedSegments.length / 2)
    const middleSegment = impact.impactedSegments[middleImpactSegment]
    hotspotSegments = [...hotspotSegments, middleSegment]
  })
  return hotspotSegments
}

const ReportHeader = (props: { title: string }) => {
  const { title } = props
  return (
    <Typography variant="h2" align="center" fontWeight="bold" gutterBottom>
      {title}
    </Typography>
  )
}

const ReportSubHeader = (props: { title: string }) => {
  const { title } = props
  return (
    <Typography variant="h5" align="left" fontWeight="bold" gutterBottom>
      {title}
    </Typography>
  )
}

const ReportFooter = () => {
  const footerText =
    'CONFIDENTIAL: RECORDS PRODUCED WITH THIS REQUEST MAY BE PROTECTED UNDER 23 USC 407.'
  return (
    <Box
      sx={{
        position: 'absolute',
        bottom: 10,
        width: '100%',
        textAlign: 'center',
      }}
    >
      <Typography variant="caption">{footerText}</Typography>
    </Box>
  )
}

const ReportPage = (props: ReportPageProps) => {
  const { children } = props
  return (
    <Box
      sx={{ width: '1100px', height: '850px', margin: '0 auto', padding: 2 }}
    >
      {children}
      <ReportFooter />
    </Box>
  )
}

const ReportSection = (props: { children: ReactNode }) => {
  const { children } = props
  return <Box sx={{ marginBottom: 2 }}>{children}</Box>
}

const ReportTable = (props: { headers: string[]; rows: string[][] }) => {
  const { headers, rows } = props
  return (
    <Box>
      <Table>
        <TableHead>
          <TableRow>
            {headers.map((header, index) => (
              <TableCell key={index}>{header}</TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row, rowIndex) => (
            <TableRow key={rowIndex}>
              {row.map((cell, cellIndex) => (
                <TableCell key={cellIndex}>{cell}</TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  )
}

export const useTestPDFHandler = (props: Props): SpeedPdfHandler => {
  const { options } = props

  const generatePDF = useCallback(
    async (data: ExportableReportResult[], routeSpeeds: RoutesResponse) => {
      try {
        const pages: React.ReactNode[] = []
        let currentPageContent: React.ReactNode[] = []
        let currentY = 0

        const generateReportMap = async (
          report: ExportableReportResult,
          routeSpeeds: RoutesResponse
        ) => {
          const { default: ReportMap } = await import('../reportMap/ReportMap')

          const speedSegments = getAdjustedRoutes(routeSpeeds)
          let impactSegments: ImpactHotspotForReportMap[] = []
          let hotspotSegments: HotSpotForReportMap[] = []
          if (report.impacts?.length) {
            impactSegments = getImpactHostpot(report.impacts, speedSegments)
            hotspotSegments = createHotspotSegmentsFromImpacts(impactSegments)
          } else {
            hotspotSegments = getHotspotsBasedOnChartType(
              report.reportData as RouteSpeed[],
              speedSegments
            )
          }

          return (
            <ReportMap
              routes={speedSegments || []}
              hotspots={hotspotSegments || []}
              impacts={impactSegments || []}
            />
          )
        }

        const addPage = () => {
          pages.push(
            <ReportPage key={pages.length}>{currentPageContent}</ReportPage>
          )
          currentPageContent = []
          currentY = 0
        }

        const addHeader = (title: string) => {
          currentPageContent.push(
            <ReportHeader key={`header-${pages.length}`} title={title} />
          )
          currentY += 50
        }

        const addSubheader = (title: string) => {
          currentPageContent.push(
            <ReportSubHeader key={`subheader-${pages.length}`} title={title} />
          )
          currentY += 30
        }

        const addSection = (
          content: React.ReactNode,
          yAmount: number,
          key?: string
        ) => {
          if (currentY + yAmount > 1100) {
            addPage()
          }
          currentPageContent.push(
            <ReportSection key={`${key || pages.length}-section`}>
              {content}
            </ReportSection>
          )
          currentY += yAmount
        }

        addHeader('Speed Management Reports')
        const filterText = formatFilters(options)
        addSubheader('Filters Summary')
        addSection(
          <Box component="pre" sx={{ whiteSpace: 'pre-wrap', fontSize: 14 }}>
            {filterText}
          </Box>,
          50
        )

        let index = 0
        for (const report of data) {
          if (index > 0) {
            addPage()
          }

          const baseText = generateBaseText(
            report.exportableReportType as ExportableReportType,
            options
          )

          addSection(
            <Typography>{baseText}</Typography>,
            20,
            `baseText-${index}`
          )

          const { headers, rows } = getTableData(report)
          if (rows.length > 0) {
            addSection(
              <ReportTable headers={headers} rows={rows} />,
              50,
              `table-${index}`
            )
          } else {
            addSection(
              <Typography>No data available for this report.</Typography>,
              10,
              `noData-${index}`
            )
          }

          addSection(
            await generateReportMap(report, routeSpeeds),
            500,
            `map-${index}`
          )

          index++
        }
        if (currentPageContent.length > 0) {
          addPage()
        }
        return <Box width="fit-content">{pages}</Box>
      } catch (error) {
        console.error('Error generating PDF:', error)
      }
    },
    [options]
  )

  return { generatePDF }
}
