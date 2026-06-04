// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - getUdotSpeedLimitRoutes.ts
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
import { useEnv } from '@/hooks/useEnv'
import axios from 'axios'
import { useQuery } from 'react-query'

const DEFAULT_QUERY_PARAMS = {
  where: '1=1',
  outFields: '*',
  returnGeometry: 'true',
  outSR: '4326',
  f: 'geojson',
} as const

const buildSpeedLimitRouteUrl = (route: string) => {
  const url = new URL(route)

  Object.entries(DEFAULT_QUERY_PARAMS).forEach(([key, value]) => {
    if (!url.searchParams.get(key)) {
      url.searchParams.set(key, value)
    }
  })

  return url.toString()
}

export function useUdotSpeedLimitRoutes() {
  const env = useEnv()
  const route = env.SPEED_LIMIT_MAP_LAYER

  return useQuery(
    ['udot-speed-limit', route],
    () => {
      if (!route) {
        throw new Error('SPEED_LIMIT_MAP_LAYER is not configured.')
      }

      return axios
        .get(buildSpeedLimitRouteUrl(route))
        .then((res) => res.data as UdotSpeedLimitRoute)
    },
    { enabled: !!route }
  )
}

interface UdotSpeedLimitRoute {
  features: {
    geometry: { coordinates: number[][] }
    properties: { route_id: string; speedLimit: number }
  }[]
}
