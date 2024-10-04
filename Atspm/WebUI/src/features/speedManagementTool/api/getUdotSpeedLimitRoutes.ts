import axios from 'axios'
import { useQuery } from 'react-query'

const route =
  'https://maps.udot.utah.gov/central/rest/services/TrafficAndSafety/UDOT_Speed_Limits/MapServer/0/query?where=1%3D1&outFields=*&f=geojson'

export function useUdotSpeedLimitRoutes() {
  return useQuery([route], () =>
    axios.get(route).then((res) => res.data as UdotSpeedLimitRoute)
  )
}

interface UdotSpeedLimitRoute {
  features: {
    geometry: {
      coordinates: number[][]
    }
    properties: {
      route_id: string
      speedLimit: number
    }
  }[]
}
