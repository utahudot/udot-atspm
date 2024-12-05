import { WatchdogDashboardData } from '@/features/watchdog/types'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import axios from 'axios'
import { useQuery } from 'react-query'

type QueryFnType = typeof getWatchdogDashboardData


type UseGetWatchdogDashboardDataOptions = {
  start: string
  end: string
  enabled?: boolean
  config?: QueryConfig<QueryFnType>
}

export const getWatchdogDashboardData = async (
  start: string,
  end: string
): Promise<WatchdogDashboardData> => {
  const response = await axios.post('https://localhost:44366/getIssueTypeGroup', {
    start,
    end,
  })
  return response.data

}

export const useGetWatchdogDashboardData = ({
  start,
  end,
  enabled = false,
  config,
}: UseGetWatchdogDashboardDataOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['issueTypeGroup', start, end],
    queryFn: () => getWatchdogDashboardData(start, end),
    enabled: enabled,
  })
}