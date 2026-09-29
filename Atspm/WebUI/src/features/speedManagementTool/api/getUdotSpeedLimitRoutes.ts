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
import { useEffect } from 'react'
import { useQuery } from 'react-query'

const DEFAULT_QUERY_PARAMS = {
  where: '1=1',
  outFields: '*',
  returnGeometry: 'true',
  outSR: '4326',
  f: 'geojson',
} as const

// Deployment tooling may retain a wrapper quote or omit its closing quote.
// Only unwrap quoted values: a clean URL can end in a SQL filter's quote.
export const normalizeSpeedLimitRoute = (route: string | undefined) => {
  let value = route?.trim()
  if (!value) return undefined

  const quote = value[0]
  if (quote === "'" || quote === '"') {
    value = value.slice(1)
    if (value.endsWith(quote)) value = value.slice(0, -1)
  }

  return value.trim() || undefined
}

export const buildSpeedLimitRouteUrl = (route: string) => {
  let url: URL
  try {
    url = new URL(route)
  } catch {
    return null
  }

  Object.entries(DEFAULT_QUERY_PARAMS).forEach(([key, value]) => {
    // A bare `?where` reads back as '', so empty params get defaults too
    if (!url.searchParams.get(key)) {
      url.searchParams.set(key, value)
    }
  })

  return url.toString()
}

export function useUdotSpeedLimitRoutes() {
  const env = useEnv()
  const route = normalizeSpeedLimitRoute(env.SPEED_LIMIT_MAP_LAYER)
  const requestUrl = route ? buildSpeedLimitRouteUrl(route) : null

  useEffect(() => {
    if (route && !requestUrl) {
      console.warn(`SPEED_LIMIT_MAP_LAYER is not a valid URL: ${route}`)
    }
  }, [route, requestUrl])

  return useQuery(
    ['udot-speed-limit', requestUrl],
    () =>
      axios
        .get(requestUrl as string)
        .then((res) => res.data as UdotSpeedLimitRoute),
    { enabled: !!requestUrl }
  )
}

interface UdotSpeedLimitRoute {
  features: {
    geometry: { coordinates: number[][] }
    properties: { route_id: string; speedLimit: number }
  }[]
}
