// import transformWatchdogIssueTypeData from '@/features/charts/watchdogDashboard/watchdogIssueType.transformer'
import { IssueTypeGrouping } from '@/features/watchdog/types'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { ApiResponse } from '@/types'
import axios from 'axios'
import { useQuery } from 'react-query'

type QueryFnType = typeof getDetectionTypeGroup

type UseGetDetectionTypeGroupOptions = {
  start: string
  end: string
  config?: QueryConfig<QueryFnType>
}

export const getDetectionTypeGroup = async (
  start: string,
  end: string
): Promise<IssueTypeGrouping[]> => {
  const response = await axios.post('https://localhost:44366/getDetectionTypeGroup', {
    start,
    end,
  })
//   return transformWatchdogIssueTypeData(response.data)
return response.data

}

export const useGetDetectionTypeGroup = ({
  start,
  end,
  config,
}: UseGetDetectionTypeGroupOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['issueTypeGroup', start, end],
    queryFn: () => getDetectionTypeGroup(start, end),
  })
}
