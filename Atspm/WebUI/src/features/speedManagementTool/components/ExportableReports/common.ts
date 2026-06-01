import {
  ExportableReportOptions,
  ExportableReportResult,
  ExportableReportType,
  ImpactDto,
  RouteSpeed,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { RoutesResponse, SpeedManagementRoute } from '../../types/routes'
import {
  AggClassificationMapping,
  HotSpotForReportMap,
  ImpactHotspotForReportMap,
  SourceIdToNamingMapping,
  SpeedDataTypeMapping,
  TimePeriodFilterMapping,
} from './types'

export const roundToWholeNumber = (
  value: number | null | undefined
): string => {
  return value !== null && value !== undefined ? value.toFixed(0) : 'N/A'
}

// Utility function to round numbers to two decimal places
export const roundToTwoDecimals = (value: number | null | undefined): string =>
  value !== null && value !== undefined ? value.toFixed(2) : 'N/A'

export const generateLocationFilter = (
  options: ExportableReportOptions
): string => {
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

export const createLeaderShipFilterDescription = (
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

export const generateBaseText = (
  reportType: ExportableReportType,
  options: ExportableReportOptions
): string => {
  const { startDate, endDate } = options

  switch (reportType) {
    case ExportableReportType.Violations:
      return `The 5 segments in the table are the segments that experienced the highest percentage of vehicles travelling at a violation speed (2 MPH over the speed limit).`
    case ExportableReportType.MostImproved:
      return `The 5 segments in the table are the segments that experienced the greatest reduction in percentage of vehicles travelling at a violation speed (2 MPH over the speed limit).`
    case ExportableReportType.ComplianceToSpeedLimit:
      return `The 5 segments in the table are the segments that experienced the greatest difference between 85th percentile speeds and the posted speed limit (where the 85th percentile speed was higher than the posted speed limit).`
    case ExportableReportType.SpeedVariability:
      return `The 5 segments in the table are the segments that experienced the greatest range between daily minimum and maximum speed on average.`
    case ExportableReportType.EffectivenessOfStrategies:
      return `The segments in the table are the segments that experienced the largest speed reductions following the implementation of a speed management strategy sometime between ${startDate} and ${endDate}.`
    default:
      return ''
  }
}

export const generateReportSpecificText = (
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
      } were traveling at a violation speed (2 MPH over the speed limit) on ${daysOfWeekFilter} during ${hoursFilter} between ${
        options.startDate
      } and ${options.startDate}.`
    case ExportableReportType.MostImproved:
      return `The percentage of vehicles on ${
        segment.name
      } traveling at a violation speed (2 MPH over the speed limit) on ${daysOfWeekFilter} during ${hoursFilter}, resulting in a ${roundToTwoDecimals(
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

// Function to format filters into a string
export const formatFilters = (opts: ExportableReportOptions): string => {
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
    filterDescriptions.push(`Date Range: ${opts.startDate} to ${opts.endDate}`)
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
    filterDescriptions.push(
      `Time Period: ${TimePeriodFilterMapping[opts.timePeriod]}`
    )

  return filterDescriptions.join('\n')
}

export const getTableData = (
  report: ExportableReportResult
): {
  headers: string[]
  rows: string[][]
  boldedColumn?: number
} => {
  let headers: string[] = []
  const rows: string[][] = []

  if (
    report.exportableReportType ===
      ExportableReportType.EffectivenessOfStrategies &&
    report.impacts
  ) {
    headers = ['Rank', 'Name', '% Change in Eighty Fifth Speed', 'Speed Limit']
    rows.push(
      ...(report.impacts?.map((item, idx) => [
        `${idx + 1}`,
        item?.description ?? '',
        `${roundToTwoDecimals(item?.changeInEightyFifthPercentileSpeed)}`,
        `${item?.speedLimit} MPH`,
      ]) ?? [])
    )
    return {
      headers,
      rows,
      boldedColumn: 2,
    }
  }

  if (!report.reportData || report.reportData.length === 0) {
    return {
      headers: ['Rank', 'Unknown Data', 'Name'],
      rows: [['No Data', '', '']],
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
        boldedColumn: 2,
      }
    case ExportableReportType.ComplianceToSpeedLimit:
      headers = ['Rank', 'Name', '85th Speed', 'Speed Limit', 'Difference']
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
        boldedColumn: 4,
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
        boldedColumn: 4,
      }
    default:
      headers = ['Rank', 'Unknown Data', 'Name']
      rows.push(['No Data', '', ''])
  }

  return {
    headers,
    rows,
  }
}

export const getHotspotSegments = (
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

export const getHotspotsBasedOnChartType = (
  speedSegments: RouteSpeed[],
  segmentSpeeds: SpeedManagementRoute[]
): HotSpotForReportMap[] => {
  let hotspotSegments: HotSpotForReportMap[] = []
  const segmentIds = speedSegments?.map((s) => s.segmentId as string)
  hotspotSegments = getHotspotSegments(segmentIds, segmentSpeeds)
  return hotspotSegments
}

export const getAdjustedRoutes = (
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

export const getImpactHostpot = (
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

export const createHotspotSegmentsFromImpacts = (
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
