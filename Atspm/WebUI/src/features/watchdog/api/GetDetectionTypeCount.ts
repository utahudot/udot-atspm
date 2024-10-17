import { DetectionTypeCount } from '@/features/watchdog/types'
import { ExtractFnReturnType } from '@/lib/react-query'
import axios from 'axios'
import { useQuery } from 'react-query'

const getDetectionTypeCount = async (
  date: string
): Promise<DetectionTypeCount[]> => {
  const response = await axios.get(
    `https://localhost:44315/config/api/v1/Location/GetDetectionTypeCount?date=${date}`
  )
  return response.data.value
}

type QueryFnType = typeof getDetectionTypeCount

export const useGetDetectionTypeCount = (date: string) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    queryKey: ['detectionTypeCount', date],
    queryFn: () => getDetectionTypeCount(date),
  })
}
