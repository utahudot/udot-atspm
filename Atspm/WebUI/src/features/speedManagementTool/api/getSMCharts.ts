// Import necessary modules and types
import { useQuery, UseQueryOptions, UseQueryResult } from 'react-query'

import { transformCongestionTrackerData } from '@/features/charts/speedManagementTool/congestionTracker/congestionTracker.transformer'
import transformSpeedOverDistanceData from '@/features/charts/speedManagementTool/speedOverDistance/components/speedOverDistance.transformer'
import transformSpeedOverTimeData from '@/features/charts/speedManagementTool/speedOverTime/speedOverTime.transformer'

import {
  postApiV1CongestionTrackingGetReportData,
  postApiV1SpeedOverDistanceGetReportData,
  postApiV1SpeedOverTimeGetReportData,
} from '@/api/speedManagement/aTSPMSpeedManagementApi'

import {
  CongestionTrackingOptions,
  SpeedOverDistanceOptions,
  SpeedOverTimeOptions,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'

export enum SM_ChartType {
  CONGESTION_TRACKING = 'Congestion Tracking',
  SPEED_OVER_TIME = 'Speed over Time',
  SPEED_OVER_DISTANCE = 'Speed over Distance',
}

type TransformedCongestionTrackerData = ReturnType<
  typeof transformCongestionTrackerData
>
type TransformedSpeedOverTimeData = ReturnType<
  typeof transformSpeedOverTimeData
>
type TransformedSpeedOverDistanceData = ReturnType<
  typeof transformSpeedOverDistanceData
>

// Map chart types to options and data types
type ChartOptionsMapping = {
  [SM_ChartType.CONGESTION_TRACKING]: CongestionTrackingOptions
  [SM_ChartType.SPEED_OVER_TIME]: SpeedOverTimeOptions
  [SM_ChartType.SPEED_OVER_DISTANCE]: SpeedOverDistanceOptions
}

type SMChartsDataMapping = {
  [SM_ChartType.CONGESTION_TRACKING]: TransformedCongestionTrackerData
  [SM_ChartType.SPEED_OVER_TIME]: TransformedSpeedOverTimeData
  [SM_ChartType.SPEED_OVER_DISTANCE]: TransformedSpeedOverDistanceData
}

type UseSMChartsOptions<TChartType extends SM_ChartType> = Omit<
  UseQueryOptions<SMChartsDataMapping[TChartType], Error>,
  'queryKey' | 'queryFn'
> & { enabled?: boolean }

// Implementation of useSMCharts
export function useSMCharts<TChartType extends SM_ChartType>(
  chartType: TChartType | null,
  chartOptions: ChartOptionsMapping[TChartType] | null,
  config?: UseSMChartsOptions<TChartType>
): UseQueryResult<SMChartsDataMapping[TChartType], Error> {
  const queryFn = async (): Promise<SMChartsDataMapping[TChartType]> => {
    console.log('chartType', chartType)
    switch (chartType) {
      case SM_ChartType.CONGESTION_TRACKING: {
        const response = await postApiV1CongestionTrackingGetReportData(
          chartOptions as CongestionTrackingOptions
        )
        return transformCongestionTrackerData(
          response
        ) as SMChartsDataMapping[TChartType]
      }
      case SM_ChartType.SPEED_OVER_TIME: {
        const response = await postApiV1SpeedOverTimeGetReportData(
          chartOptions as SpeedOverTimeOptions
        )
        return transformSpeedOverTimeData(
          response
        ) as SMChartsDataMapping[TChartType]
      }
      case SM_ChartType.SPEED_OVER_DISTANCE: {
        const response = await postApiV1SpeedOverDistanceGetReportData(
          chartOptions as SpeedOverDistanceOptions
        )
        return transformSpeedOverDistanceData(
          response
        ) as SMChartsDataMapping[TChartType]
      }
      default:
        throw new Error(`Unsupported chart type: ${chartType}`)
    }
  }

  return useQuery<SMChartsDataMapping[TChartType], Error>({
    queryKey: ['sm_charts', chartType, chartOptions],
    queryFn,
    enabled: false,
    ...config,
  })
}
