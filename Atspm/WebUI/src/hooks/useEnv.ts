import Axios from 'axios'
import { useQuery } from 'react-query'

export type EnvVariables = {
  CONFIG_URL: string | null
  REPORTS_URL: string | null
  IDENTITY_URL: string | null
  DATA_URL: string | null
  SPEED_URL: string | null
  MAP_DEFAULT_LATITUDE: string | null
  MAP_DEFAULT_LONGITUDE: string | null
  MAP_DEFAULT_ZOOM: string | null
  MAP_TILE_LAYER: string | null
  MAP_TILE_ATTRIBUTION: string | null
  POWERED_BY_IMAGE_URL: string | null
  SPEED_LIMIT_MAP_LAYER: string | null
}

const fetchEnv = async (): Promise<EnvVariables> => {
  const axios = Axios.create()
  const { data } = await axios.get<EnvVariables>('/api/get-env', {
    timeout: 5000,
  })
  return data
}

export const useEnv = () =>
  useQuery(['env'], fetchEnv, {
    staleTime: Infinity,
    cacheTime: Infinity,
    refetchOnWindowFocus: false,
    retry: 1,
  })
