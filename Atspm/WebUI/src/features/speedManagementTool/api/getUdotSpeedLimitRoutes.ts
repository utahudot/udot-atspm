import { useEnv } from '@/hooks/useEnv'
import axios from 'axios'
import { useQuery } from 'react-query'

export function useUdotSpeedLimitRoutes() {
  const { data: env } = useEnv()
  const route = env?.SPEED_LIMIT_MAP_LAYER

  return useQuery(
    ['udot-speed-limit', route],
    () => axios.get(route).then((res) => res.data as UdotSpeedLimitRoute),
    { enabled: !!route }
  )
}

interface UdotSpeedLimitRoute {
  features: {
    geometry: { coordinates: number[][] }
    properties: { route_id: string; speedLimit: number }
  }[]
}
