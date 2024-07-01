import { LocationExpanded } from '@/features/locations/types'

export interface RouteApproaches {
  id: number
  direction: string
  phase: string
  overlap: boolean
  primary: boolean
  opposing: boolean
  locationIdentifier: string
}

export interface RouteLocation {
  locationIdentifier: string
  order: number
  primaryPhase: string | null
  opposingPhase: string | null
  primaryDirectionId: string | null
  opposingDirectionId: string | null
  primaryDirection?: {
    abbreviation: string
    description: string
    displayOrder: number
    id: string
  }
  opposingDirection?: {
    abbreviation: string
    description: string
    displayOrder: number
    id: string | null
  }
  isPrimaryOverlap: boolean | null
  isOpposingOverlap: boolean | null
  previousLocationDistanceId: number | null
  nextLocationDistanceId: number | null
  nextLocationDistance?: RouteDistance | null
  previousLocationDistance?: RouteDistance | null
  routeId: number | null
  id?: string
  primaryName?: string
  secondaryName?: string
  latitude?: number
  longitude?: number
  locationId?: string
}

export interface Route {
  id: number
  name: string
  routeLocations: RouteLocation[]
}

export interface RouteDistance {
  distance: number
  id: number
  locationIdentifierA: string
  locationIdentifierB: string
}

export interface RouteWithExpandedLocations {
  id: number
  name: string
  routeLocations: LocationExpanded[]
}
