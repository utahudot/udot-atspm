import { WatchdogDashboardData } from '@/features/watchdog/types'
import { reportsAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
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
  const response = await reportsAxios.post(
    'WatchDogDashboard/getDashboardGroup',
    {
      start,
      end,
    }
  )
  return response
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
