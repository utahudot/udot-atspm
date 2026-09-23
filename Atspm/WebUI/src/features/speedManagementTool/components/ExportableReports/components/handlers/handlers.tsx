import {
  useGetRouteSpeeds,
  usePostApiV1ExportableReportsGetReportData,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'
import {
  AggClassification,
  ExportableReportOptions,
  ExportableReportResult,
  RouteSpeedOptions,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { RoutesResponse } from '@/features/speedManagementTool/types/routes'
import { toUTCDateStamp } from '@/utils/dateTime'
import { useCallback, useRef, useState } from 'react'
import { ERBaseHandler } from './ExportableReportsHandler'
import {
  ReportDateTimeHandler,
  useReportDateTimeHandler,
} from './ReportDateTimeHandler'

interface Props {
  baseHandler: ERBaseHandler
}

export interface ERHourlyHandler extends ERBaseHandler, ReportDateTimeHandler {
  isPdfDownloading: boolean | null
  handleSubmit(): void
  handleCancel(): void
  hasError: boolean
  clearError(): void
}

export interface ERMonthlyHandler extends ERBaseHandler, ReportDateTimeHandler {
  aggClassification: AggClassification
  isPdfDownloading: boolean | null
  changeAggClassification(classification: AggClassification): void
  handleSubmit(): void
  handleCancel(): void
  hasError: boolean
  clearError(): void
}

function getRouteSpeedOptions(options: ExportableReportOptions) {
  const routeSpeedOptions: RouteSpeedOptions = {
    startDate: options.startDate,
    endDate: options.endDate,
    sourceId: options.sourceId as number,
    accessCategory: options.accessCategory,
    aggClassification: options.aggClassification,
    city: options.city,
    county: options.county,
    region: options.region,
    violationThreshold: 5,
    functionalType: options.functionalType,
  } as RouteSpeedOptions
  return routeSpeedOptions
}

export const useERMonthlyReportHandler = (props: Props) => {
  const { baseHandler } = props
  const [isPdfDownloading, setIsPdfDownloading] = useState<boolean | null>(null)
  const [aggClassification, setAggClassification] = useState<AggClassification>(
    AggClassification.Total
  )

  const [hasError, setHasError] = useState<boolean>(false)
  const isCancelledRef = useRef(false)

  const reportDateHandler = useReportDateTimeHandler()

  const apiResult = usePostApiV1ExportableReportsGetReportData()
  const routeSpeedResult = useGetRouteSpeeds()

  const getExportableReportOptions = useCallback(
    (): ExportableReportOptions =>
      ({
        startDate: toUTCDateStamp(reportDateHandler.parsedStartMonth as Date),
        endDate: toUTCDateStamp(reportDateHandler.parsedEndMonth as Date),
        sourceId: baseHandler.sourceId,
        speedDataType: baseHandler.speedDataType,
        timePeriod: baseHandler.analysisPeriod,
        city: baseHandler.city,
        county: baseHandler.county,
        region: baseHandler.region,
        accessCategory: baseHandler.accessCategory,
        functionalType: baseHandler.functionalType,
        aggClassification: aggClassification,
        reportManagementType: baseHandler.managementType,
      }) as ExportableReportOptions,
    [
      reportDateHandler.parsedStartMonth,
      reportDateHandler.parsedEndMonth,
      baseHandler.sourceId,
      baseHandler.speedDataType,
      baseHandler.analysisPeriod,
      baseHandler.city,
      baseHandler.county,
      baseHandler.region,
      baseHandler.functionalType,
      baseHandler.accessCategory,
      baseHandler.managementType,
      aggClassification,
    ]
  )

  const submitReportRequest = async () => {
    isCancelledRef.current = false
    setHasError(false)
    const options = getExportableReportOptions()
    try {
      const routeSpeedResultData = await routeSpeedResult.mutateAsync({
        data: getRouteSpeedOptions(options),
      })
      const apiResultData = await apiResult.mutateAsync({ data: options })
      if (isCancelledRef.current) {
        return
      }

      if (
        (apiResultData as unknown as ExportableReportResult[]) &&
        (routeSpeedResultData as unknown as RoutesResponse)
      ) {
        const reportData = apiResultData as ExportableReportResult[]
        if (reportData.length === 0) {
          setHasError(true)
          baseHandler.setError('No data found')
        } else {
          baseHandler.handleSubmit(
            reportData,
            routeSpeedResultData as unknown as RoutesResponse,
            options
          )
        }
      } else {
        setHasError(true)
        baseHandler.setError('No data found')
      }
    } catch (e) {
      setHasError(true)
      baseHandler.setError(
        'Failed to generate report: ' + (e.message || 'Unknown error')
      )
    } finally {
      setIsPdfDownloading(false)
    }
  }

  const handleCancel = () => {
    isCancelledRef.current = true
    setIsPdfDownloading(false)
  }

  const clearError = () => {
    setHasError(false)
    baseHandler.setError(null)
  }

  const component: ERMonthlyHandler = {
    ...reportDateHandler,
    ...baseHandler,
    aggClassification,
    isPdfDownloading,
    hasError,
    clearError,
    changeAggClassification(classification) {
      setAggClassification(classification)
    },
    handleSubmit() {
      setIsPdfDownloading(true)
      clearError()
      submitReportRequest()
    },
    handleCancel,
  }
  return component
}

export const useExportableReportHandler = (props: Props) => {
  const [isPdfDownloading, setIsPdfDownloading] = useState<boolean | null>(null)
  const [hasError, setHasError] = useState<boolean>(false)
  const { baseHandler } = props
  const reportDateHandler = useReportDateTimeHandler()

  const apiResult = usePostApiV1ExportableReportsGetReportData()
  const routeSpeedResult = useGetRouteSpeeds()
  const isCancelledRef = useRef(false)

  // Helper function to generate the options object
  const getExportableReportOptions = useCallback(
    (): ExportableReportOptions =>
      ({
        startDate: toUTCDateStamp(reportDateHandler.startDateTime as Date),
        endDate: toUTCDateStamp(reportDateHandler.endDateTime as Date),
        sourceId: baseHandler.sourceId,
        speedDataType: baseHandler.speedDataType,
        timePeriod: baseHandler.analysisPeriod,
        city: baseHandler.city,
        county: baseHandler.county,
        region: baseHandler.region,
        accessCategory: baseHandler.accessCategory,
        functionalType: baseHandler.functionalType,
        reportManagementType: baseHandler.managementType,
      }) as ExportableReportOptions,
    [
      reportDateHandler.startDateTime,
      reportDateHandler.endDateTime,
      baseHandler.sourceId,
      baseHandler.speedDataType,
      baseHandler.analysisPeriod,
      baseHandler.city,
      baseHandler.county,
      baseHandler.region,
      baseHandler.functionalType,
      baseHandler.accessCategory,
      baseHandler.managementType,
    ]
  )

  const submitHourlyReportRequest = async () => {
    isCancelledRef.current = false
    setHasError(false)
    const options = getExportableReportOptions()
    try {
      const routeSpeedResultData = await routeSpeedResult.mutateAsync({
        data: getRouteSpeedOptions(options),
      })
      const apiResultData = await apiResult.mutateAsync({ data: options })
      if (isCancelledRef.current) return
      if (
        (apiResultData as unknown as ExportableReportResult[]) &&
        (routeSpeedResultData as unknown as RoutesResponse)
      ) {
        const reportData = apiResultData as ExportableReportResult[]
        if (reportData.length === 0) {
          setHasError(true)
          baseHandler.setError('No data found')
        } else {
          baseHandler.handleSubmit(
            reportData,
            routeSpeedResultData as unknown as RoutesResponse,
            options as ExportableReportOptions
          )
        }
      } else {
        setHasError(true)
        baseHandler.setError('No data found')
      }
    } catch (e) {
      setHasError(true)
      baseHandler.setError(
        'Failed to generate report: ' + (e.message || 'Unknown error')
      )
    } finally {
      setIsPdfDownloading(false)
    }
  }

  const handleCancel = () => {
    isCancelledRef.current = true
    setIsPdfDownloading(false)
  }

  const clearError = () => {
    setHasError(false)
    baseHandler.setError(null)
  }

  const component: ERHourlyHandler = {
    ...reportDateHandler,
    ...baseHandler,
    isPdfDownloading,
    hasError,
    clearError,
    handleSubmit() {
      setIsPdfDownloading(true)
      clearError()
      submitHourlyReportRequest()
    },
    handleCancel,
  }

  return component
}
