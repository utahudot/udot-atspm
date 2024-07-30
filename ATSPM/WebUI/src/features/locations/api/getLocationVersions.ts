import { Location } from '@/features/locations/types'
import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

export const fetchVersionData = async (
  locationId: string | null
): Promise<Location[]> => {
  const response = await configAxios.get(
    `Location/GetAllVersionsOfLocation(identifier='${locationId}')`
  )

  return response.value
}

type QueryFnType = typeof fetchVersionData

type useConfigOptions = {
  locationId: string | null
  config?: QueryConfig<QueryFnType>
}

export const useLocationVersions = ({
  locationId,
  config,
}: useConfigOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: !!locationId,
    queryKey: ['versionData', locationId],
    queryFn: () => fetchVersionData(locationId),
  })
}
