import { Jurisdiction } from '@/features/jurisdictions/types'
import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { ApiResponse } from '@/types'
import { useQuery } from 'react-query'

export const getJurisdictions = async (): Promise<
  ApiResponse<Jurisdiction>
> => {
  return await configAxios.get('/Jurisdiction')
}

type QueryFnType = typeof getJurisdictions

type UseJurisdictionsOptions = {
  config?: QueryConfig<QueryFnType>
}

export const useJurisdictions = ({ config }: UseJurisdictionsOptions = {}) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    queryKey: ['jurisdictions'],
    queryFn: () => getJurisdictions(),
  })
}
