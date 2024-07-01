import { Area } from '@/features/areas/types'
import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { ApiResponse } from '@/types'
import { useQuery } from 'react-query'

export const getAreas = async (): Promise<ApiResponse<Area>> => {
  return await configAxios.get('/Area')
}

type QueryFnType = typeof getAreas

type UseAreasOptions = {
  config?: QueryConfig<QueryFnType>
}

export const useAreas = ({ config }: UseAreasOptions = {}) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['areas'],
    queryFn: () => getAreas(),
  })
}
