import { DataPoint } from '@/features/charts/common/types'
import transformSpeedOverTimeData from '@/features/charts/speedOverTime/speedOverTime.transformer'
// import { transformSpeedOverTimeData } from '@/features/charts/SpeedOverTime/SpeedOverTime.transformer'
import { speedAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

export interface SpeedOverTimeParams {
  timeOptions: number
  startDate: string
  endDate: string
  startTime: string
  endTime: string
  daysOfWeek: number[]
  segmentId: string
  sourceId: number
}

export interface SpeedOverTimeResponse {
  segmentId: string
  segmentName: string
  startingMilePoint: number
  endingMilePoint: number
  speedLimit: number
  data: SpeedOverTimeData[]
}

interface SpeedOverTimeData {
  date: string
  series: {
    average: DataPoint[] | null
    eightyFifth: DataPoint[] | null
  }
}

export const getSpeedOverTimeData = async (
  options: SpeedOverTimeParams
): Promise<SpeedOverTimeResponse> => {
  options.startDate = '2024-05-01'
  options.endDate = '2024-05-31'
  const response = await speedAxios.post('SpeedOverTime/GetReportData', options)
  return transformSpeedOverTimeData(response)
}

type QueryFnType = typeof getSpeedOverTimeData

type BaseOptions = {
  options: SpeedOverTimeParams
  config?: QueryConfig<QueryFnType>
}

export const useSpeedOverTimeChart = ({ options, config }: BaseOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: true,
    queryKey: ['SpeedOverTime', options],
    queryFn: () => getSpeedOverTimeData(options),
  })
}
