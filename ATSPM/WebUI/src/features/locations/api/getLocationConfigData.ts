import { Location } from '@/features/locations/types'
import { configAxios } from '@/lib/axios'
import { ExtractFnReturnType, QueryConfig } from '@/lib/react-query'
import { useQuery } from 'react-query'

export const fetchLocationConfiguration = async (
  locationId: string | null
): Promise<Location[]> => {
  const response = await configAxios.get(
    `Location/${locationId}?expand=approaches($expand=directionType, detectors($expand=detectionTypes, detectorComments))`
  )

  return response.value
}

type QueryFnType = typeof fetchLocationConfiguration

type useConfigOptions = {
  locationId: string | null
  config?: QueryConfig<QueryFnType>
}

export const useLocationConfigData = ({
  locationId,
  config,
}: useConfigOptions) => {
  return useQuery<ExtractFnReturnType<QueryFnType>>({
    ...config,
    enabled: !!locationId,
    queryKey: ['configData', locationId],
    queryFn: () => fetchLocationConfiguration(locationId),
  })
}
