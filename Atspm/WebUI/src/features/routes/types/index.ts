// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - index.ts
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
