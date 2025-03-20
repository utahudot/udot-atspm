// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - fetchRouteDistance.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion
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
