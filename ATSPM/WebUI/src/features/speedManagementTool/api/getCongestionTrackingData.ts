import { DataPoint } from '@/features/charts/common/types'
import { transformCongestionTrackerData } from '@/features/charts/congestionTracker/congestionTracker.transformer'
import { speedAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

export interface CongestionTrackingParams {
  segmentId: string
  startDate: string
  endDate: string
}

export interface CongestionTrackerResponse {
  segmentId: string
  segmentName: string
  startingMilePoint: number
  endingMilePoint: number
  speedLimit: number
  data: CongestionTrackerData[]
}

interface CongestionTrackerData {
  date: string
  series: {
    average: DataPoint[] | null
    eightyFifth: DataPoint[] | null
  }
}

export const getCongestionTrackings = async (
  options: CongestionTrackingParams
): Promise<CongestionTrackerResponse> => {
  options.startDate = '2021-04-01'
  options.endDate = '2024-06-30'
  options.sourceId = 3
  const response = await speedAxios.post(
    'CongestionTracking/GetReportData',
    options
  )
  return transformCongestionTrackerData(response, 'month')
}

type QueryFnType = typeof getCongestionTrackings

type BaseOptions = {
  options: CongestionTrackingParams
  config?: QueryConfig<QueryFnType>
}

export const useCongestionTrackingChart = ({
  options,
  config,
}: BaseOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: true,
    queryKey: ['CongestionTracking', options],
    queryFn: () => getCongestionTrackings(options),
  })
}
