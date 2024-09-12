import transformSpeedOverDistanceData from '@/features/charts/speedManagementTool/speedOverDistance/components/speedOverDistance.transformer'
import { speedAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

export interface SpeedOverDistanceParams {
  startDate: string
  endDate: string
  segmentIds: string[]
}

export interface SpeedOverDistanceResponse {
  segmentId: string
  segmentName: string
  startingMilePoint: number
  endingMilePoint: number
  speedLimit: number
  average: number
  eightyFifth: number
  startDate: string
  endDate: string
}

export const getSpeedOverDistances = async (
  options: SpeedOverDistanceParams
): Promise<SpeedOverDistanceResponse> => {
  const response = await speedAxios.post('api/SpeedOverDistance', options)
  return transformSpeedOverDistanceData(response)
}

type QueryFnType = typeof getSpeedOverDistances

type BaseOptions = {
  options: SpeedOverDistanceParams
  config?: QueryConfig<QueryFnType>
}

export const useSpeedOverDistanceChart = ({ options, config }: BaseOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: true,
    queryKey: ['SpeedOverDistance', options],
    queryFn: () => getSpeedOverDistances(options),
  })
}
