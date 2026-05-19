import {
  ExportableReportOptions,
  ExportableReportResult,
  ReportManagementType,
  SpeedDataType,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { DataSource } from '@/features/speedManagementTool/enums'
import { RoutesResponse } from '@/features/speedManagementTool/types/routes'
import { useState } from 'react'
import {
  AnalysisPeriodHandler,
  useAnalysisPeriodHandler,
} from '../../../common/handlers/AnalysisPeriodHandler'
import {
  LocationFiltersHandler,
  useLocationFiltersHandler,
} from '../../../common/handlers/LocationFiltersHandler'

export interface ERBaseHandler
  extends AnalysisPeriodHandler,
    LocationFiltersHandler {
  handleSubmit(
    data: ExportableReportResult[],
    routeSpeeds: RoutesResponse,
    options: ExportableReportOptions
  ): void
  reportData: ExportableReportResult[]
  routeSpeeds: RoutesResponse | null
  options: ExportableReportOptions | null
  sourceId: DataSource
  updateSourceId(sourceId: DataSource): void
  speedDataType: SpeedDataType
  updateSpeedDataType(dataType: SpeedDataType): void
  managementType: ReportManagementType
  updateManagementType(managementType: ReportManagementType): void
  error: string | null
  setError: (error: string | null) => void
}

export const useERBaseHandler = (): ERBaseHandler => {
  const [sourceId, setSourceId] = useState<DataSource[]>([DataSource.ATSPM])
  const [speedDataType, setSpeedDataType] = useState<SpeedDataType>(
    SpeedDataType.M
  )
  const [managementType, setManagementType] = useState<ReportManagementType>(
    ReportManagementType.Engineer
  )
  const [reportData, setReportData] = useState<ExportableReportResult[]>([])
  const [routeSpeeds, setRouteSpeeds] = useState<RoutesResponse | null>(null)
  const [options, setOptions] = useState<ExportableReportOptions | null>(null)
  const [error, setError] = useState<string | null>(null)
  const analysisHandler = useAnalysisPeriodHandler()
  const locationFiltersHandler = useLocationFiltersHandler()

  const component: ERBaseHandler = {
    ...analysisHandler,
    ...locationFiltersHandler,
    reportData,
    routeSpeeds,
    options,
    sourceId,
    speedDataType,
    managementType,
    error,
    setError,
    updateSourceId(sourceId) {
      setSourceId(sourceId)
    },
    updateSpeedDataType(dataType) {
      setSpeedDataType(dataType)
    },
    updateManagementType(managementType) {
      setManagementType(managementType)
    },
    handleSubmit(apiResultData, routeSpeedResultData, options) {
      setReportData(apiResultData)
      setRouteSpeeds(routeSpeedResultData as unknown as RoutesResponse)
      setOptions(options)
      setError(null)
    },
  }

  return component
}
