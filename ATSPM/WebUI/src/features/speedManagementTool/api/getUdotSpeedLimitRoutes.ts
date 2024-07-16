import { useGetRequest } from '@/hooks/useGetRequest'

const route =
  'https://maps.udot.utah.gov/central/rest/services/TrafficAndSafety/UDOT_Speed_Limits/MapServer/0/query?where=1%3D1&outFields=*&f=geojson'

export function useUdotSpeedLimitRoutes() {
  return useGetRequest({ route })
}
