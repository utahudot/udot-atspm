import transformWatchdogIssueTypeData from '@/features/charts/watchdogDashboard/watchdogIssueType.transformer'
import { IssueTypeGrouping } from '@/features/watchdog/types'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { ApiResponse } from '@/types'
import axios from 'axios'
import { useQuery } from 'react-query'

type QueryFnType = typeof getIssueTypeGroup

type UseGetIssueTypeGroupOptions = {
  start: string
  end: string
  config?: QueryConfig<QueryFnType>
}

export const getIssueTypeGroup = async (
  start: string,
  end: string
): Promise<IssueTypeGrouping[]> => {
  const response = await axios.post('https://localhost:44366/getIssueTypeGroup', {
    start,
    end,
  })
  return response.data

}

export const useGetIssueTypeGroup = ({
  start,
  end,
  config,
}: UseGetIssueTypeGroupOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['issueTypeGroup', start, end],
    queryFn: () => getIssueTypeGroup(start, end),
  })
}
