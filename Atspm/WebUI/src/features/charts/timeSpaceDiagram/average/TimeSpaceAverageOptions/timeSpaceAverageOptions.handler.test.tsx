import { ToolType } from '@/features/charts/common/types'
import type { Route, RouteLocation } from '@/features/routes/types'
import { act, renderHook, waitFor } from '@testing-library/react'
import { useAverageOptionsHandler } from './timeSpaceAverageOptions.handler'

function buildRouteLocation(
  locationIdentifier: string,
  order: number
): RouteLocation {
  return {
    approaches: [],
    locationIdentifier,
    order,
    primaryPhase: '2',
    opposingPhase: '6',
    primaryDirectionId: '1',
    opposingDirectionId: '2',
    primaryDirectionDescription: 'Northbound',
    opposingDirectionDescription: 'Southbound',
    isPrimaryOverlap: false,
    isOpposingOverlap: false,
    previousLocationDistanceId: null,
    nextLocationDistanceId: null,
    routeId: 4122,
  }
}

const routes: Route[] = [
  {
    id: 4122,
    name: '4122',
    routeLocations: [
      buildRouteLocation('7192', 1),
      buildRouteLocation('7191', 2),
    ],
  },
]

describe('useAverageOptionsHandler', () => {
  it('serializes custom sequence and coordinated phases into the request shape', async () => {
    const { result } = renderHook(() => useAverageOptionsHandler({ routes }))

    act(() => {
      result.current.setRouteId('4122')
    })

    await waitFor(() => {
      expect(result.current.routeLocationWithSequence).toHaveLength(2)
      expect(result.current.routeLocationWithCoordPhases).toHaveLength(2)
    })

    act(() => {
      result.current.updateLocationWithSequence({
        locationIdentifier: '7192',
        sequence: [
          [2, 1, 3, 4],
          [6, 5, 8, 7],
        ],
      })
      result.current.updateLocationWithCoordPhases({
        locationIdentifier: '7192',
        coordinatedPhases: [1, 5],
      })
    })

    const options = result.current.toOptions()

    expect(options.sequence).toEqual([
      {
        locationIdentifier: '7192',
        sequence: [
          [2, 1, 3, 4],
          [6, 5, 8, 7],
        ],
      },
      {
        locationIdentifier: '7191',
        sequence: [
          [1, 2, 3, 4],
          [5, 6, 7, 8],
        ],
      },
    ])
    expect(options.coordinatedPhases).toEqual([
      {
        locationIdentifier: '7192',
        coordinatedPhases: [1, 5],
      },
      {
        locationIdentifier: '7191',
        coordinatedPhases: [2, 6],
      },
    ])

    const params = result.current.toSearchParams()

    expect(params.get('toolType')).toBe(String(ToolType.TimeSpaceAverage))
    expect(JSON.parse(params.get('sequence') ?? '[]')).toEqual(
      options.sequence
    )
    expect(JSON.parse(params.get('coordinatedPhases') ?? '[]')).toEqual(
      options.coordinatedPhases
    )
  })
})
