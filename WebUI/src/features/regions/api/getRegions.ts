import { Region } from '@/features/regions/types'
import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { ApiResponse } from '@/types'
import { useQuery } from 'react-query'

export const getRegions = async (): Promise<ApiResponse<Region>> => {
  return await configAxios.get('/Region')
}

type QueryFnType = typeof getRegions

type UseRegionsOptions = {
  config?: QueryConfig<QueryFnType>
}

export const useRegions = ({ config }: UseRegionsOptions = {}) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['regions'],
    queryFn: () => getRegions(),
  })
}
