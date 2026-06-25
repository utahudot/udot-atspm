// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - historicTimeSpaceOptions.handler.test.ts
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
import type { Route } from '@/features/routes/types'
import { inferHistoricLocationIdentifier } from './historicTimeSpaceOptions.handler'

function buildRoute(
  id: number,
  routeLocations: Route['routeLocations']
): Route {
  return {
    id,
    name: `Route ${id}`,
    routeLocations,
  }
}

describe('inferHistoricLocationIdentifier', () => {
  const routes: Route[] = [
    buildRoute(17, [
      {
        approaches: [],
        locationIdentifier: '3003',
        order: 3,
        primaryPhase: '2',
        opposingPhase: '6',
        primaryDirectionId: '1',
        opposingDirectionId: '2',
        primaryDirectionDescription: 'Eastbound',
        opposingDirectionDescription: 'Westbound',
        isPrimaryOverlap: false,
        isOpposingOverlap: false,
        previousLocationDistanceId: null,
        nextLocationDistanceId: null,
        routeId: 17,
      },
      {
        approaches: [],
        locationIdentifier: '1001',
        order: 1,
        primaryPhase: '2',
        opposingPhase: '6',
        primaryDirectionId: '1',
        opposingDirectionId: '2',
        primaryDirectionDescription: 'Eastbound',
        opposingDirectionDescription: 'Westbound',
        isPrimaryOverlap: false,
        isOpposingOverlap: false,
        previousLocationDistanceId: null,
        nextLocationDistanceId: null,
        routeId: 17,
      },
      {
        approaches: [],
        locationIdentifier: '2002',
        order: 2,
        primaryPhase: '2',
        opposingPhase: '6',
        primaryDirectionId: '1',
        opposingDirectionId: '2',
        primaryDirectionDescription: 'Eastbound',
        opposingDirectionDescription: 'Westbound',
        isPrimaryOverlap: false,
        isOpposingOverlap: false,
        previousLocationDistanceId: null,
        nextLocationDistanceId: null,
        routeId: 17,
      },
    ]),
  ]

  it('falls back to the first route location by order', () => {
    expect(inferHistoricLocationIdentifier(routes, '17', '')).toBe('1001')
  })

  it('preserves an explicit location when it belongs to the route', () => {
    expect(inferHistoricLocationIdentifier(routes, '17', '2002')).toBe('2002')
  })

  it('replaces stale explicit locations with the selected route default', () => {
    expect(inferHistoricLocationIdentifier(routes, '17', '9999')).toBe('1001')
  })
})
