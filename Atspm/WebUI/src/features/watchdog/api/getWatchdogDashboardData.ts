// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - getWatchdogDashboardData.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
    `/WatchDogDashboard/getDashboardGroup`,
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
