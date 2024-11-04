import { DetectionTypeCount } from '@/features/watchdog/types'
import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType } from '@/lib/react-query'
import { useQuery } from 'react-query'

const getDetectionTypeCount = async (
  date: string
): Promise<DetectionTypeCount[]> => {
  const response = await configAxios.get(
    `/Location/GetDetectionTypeCount?date=${date}`
  )
  return response.value
}

type QueryFnType = typeof getDetectionTypeCount

export const useGetDetectionTypeCount = (date: string) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    queryKey: ['detectionTypeCount', date],
    queryFn: () => getDetectionTypeCount(date),
  })
}
