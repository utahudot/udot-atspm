// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - fetchRouteDistance.ts
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion

import { RouteLocation } from '@/api/config'

import axios from 'axios'

export const fetchRouteDistance = async (
  routeLocations: RouteLocation[],
  profile = 'driving'
) => {
  const coordinates = routeLocations
    .map((rl) => `${rl.longitude},${rl.latitude}`)
    .join(';')

  let coords: number[][] = []
  let distanceInMeters = 0

  // 1) TRY MATCH FIRST
  try {
    const matchResponse = await axios.get(
      `https://router.project-osrm.org/match/v1/${profile}/${coordinates}?overview=full&geometries=geojson`
    )

    coords = matchResponse.data.matchings[0].geometry.coordinates
    distanceInMeters = matchResponse.data.matchings[0].distance
  } catch (matchError) {
    // 3) IF MATCH THROWS, FALL BACK TO ROUTE
    console.warn(
      'Match service threw an error. Attempting the Route service...'
    )
    try {
      const routeResponse = await axios.get(
        `https://router.project-osrm.org/route/v1/${profile}/${coordinates}?overview=full&geometries=geojson`
      )
      coords = routeResponse.data.routes[0].geometry.coordinates
      distanceInMeters = routeResponse.data.routes[0].distance
    } catch (routeError) {
      // If both Match and Route fail, throw the error
      console.error('Both Match and Route calls failed:', routeError)
      throw routeError
    }
  }

  // Convert meters to feet
  const distance = distanceInMeters * 3.28084

  // Flip the coordinates to match Leaflet's [lat, lng] format
  const shape = coords.map(([lng, lat]) => [lat, lng])

  return {
    distance,
    shape,
  }
}
