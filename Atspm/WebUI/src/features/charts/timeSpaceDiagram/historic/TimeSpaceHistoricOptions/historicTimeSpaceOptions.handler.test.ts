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
