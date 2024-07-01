import { RouteLocation } from '@/features/routes/types'
import axios from 'axios'

export const fetchRouteDistance = async (
  routeLocations: RouteLocation[],
  profile = 'driving'
) => {
  try {
    // Construct the coordinates string for OSRM
    const coordinates = routeLocations
      .map((rl) => `${rl.longitude},${rl.latitude}`)
      .join(';')

    // Make the request to the OSRM API using the Match service
    const response = await axios.get(
      `https://router.project-osrm.org/match/v1/${profile}/${coordinates}?overview=full&geometries=geojson`
    )

    const coords = response.data.matchings[0].geometry.coordinates
    const distanceInMeters = response.data.matchings[0].distance
    const distance = distanceInMeters * 3.28084 // convert meters to feet

    // flip the coordinates to match the Leaflet format
    const shape = coords.map(([lng, lat]: [number, number]) => [lat, lng])

    return {
      distance,
      shape,
    }
  } catch (error) {
    console.error(
      'Failed to fetch matched route from OSRM. Message:',
      (error as Error).message
    )
  }
}
