// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - getUdotSpeedLimitRoutes.test.ts
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
import {
  buildSpeedLimitRouteUrl,
  normalizeSpeedLimitRoute,
} from './getUdotSpeedLimitRoutes'

const LAYER =
  'https://maps.udot.utah.gov/central/rest/services/TrafficAndSafety/UDOT_Speed_Limits/MapServer/0/query'

const DEFAULT_QUERY =
  'where=1%3D1&outFields=*&returnGeometry=true&outSR=4326&f=geojson'

describe('normalizeSpeedLimitRoute', () => {
  it('leaves a clean value untouched', () => {
    expect(normalizeSpeedLimitRoute(LAYER)).toBe(LAYER)
  })

  it('strips a leading quote left without its closing quote', () => {
    expect(normalizeSpeedLimitRoute(`'${LAYER}?where`)).toBe(`${LAYER}?where`)
  })

  it('strips matched quotes and surrounding whitespace', () => {
    expect(normalizeSpeedLimitRoute(` "${LAYER}" `)).toBe(LAYER)
    expect(normalizeSpeedLimitRoute(`'${LAYER}'`)).toBe(LAYER)
  })

  it.each(['', "'", '"'])(
    'preserves SQL filter quotes inside a URL with %p wrapping',
    (wrapper) => {
      const route = `${LAYER}?where=route_id='0006'`
      const normalized = normalizeSpeedLimitRoute(
        `${wrapper}${route}${wrapper}`
      )

      expect(normalized).toBe(route)
      const requestUrl = buildSpeedLimitRouteUrl(normalized as string)
      expect(new URL(requestUrl as string).searchParams.get('where')).toBe(
        "route_id='0006'"
      )
    }
  )

  it('treats missing, blank, and quote-only values as unset', () => {
    expect(normalizeSpeedLimitRoute(undefined)).toBeUndefined()
    expect(normalizeSpeedLimitRoute('   ')).toBeUndefined()
    expect(normalizeSpeedLimitRoute(`''`)).toBeUndefined()
  })
})

describe('buildSpeedLimitRouteUrl', () => {
  it('adds the default query params to a bare layer query url', () => {
    expect(buildSpeedLimitRouteUrl(LAYER)).toBe(`${LAYER}?${DEFAULT_QUERY}`)
  })

  it('fills an empty where param left by a truncated value', () => {
    expect(buildSpeedLimitRouteUrl(`${LAYER}?where`)).toBe(
      `${LAYER}?${DEFAULT_QUERY}`
    )
  })

  it('keeps params that are already set', () => {
    const url = new URL(
      buildSpeedLimitRouteUrl(`${LAYER}?where=speedLimit>=55&f=json`) as string
    )

    expect(url.searchParams.get('where')).toBe('speedLimit>=55')
    expect(url.searchParams.get('f')).toBe('json')
    expect(url.searchParams.get('outSR')).toBe('4326')
  })

  it('returns null for a value that is not a url', () => {
    expect(buildSpeedLimitRouteUrl(`'${LAYER}`)).toBeNull()
    expect(buildSpeedLimitRouteUrl('not a url')).toBeNull()
  })
})
