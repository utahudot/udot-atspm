// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - directions.ts
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
export const turningMovementDirectionOrder = [
  'Northbound',
  'Southbound',
  'Eastbound',
  'Westbound',
  'Northeast',
  'Northwest',
  'Southeast',
  'Southwest',
] as const

const directionAliases: Record<string, string> = {
  northbound: 'Northbound',
  north: 'Northbound',
  nb: 'Northbound',
  n: 'Northbound',
  southbound: 'Southbound',
  south: 'Southbound',
  sb: 'Southbound',
  s: 'Southbound',
  eastbound: 'Eastbound',
  east: 'Eastbound',
  eb: 'Eastbound',
  e: 'Eastbound',
  westbound: 'Westbound',
  west: 'Westbound',
  wb: 'Westbound',
  w: 'Westbound',
  northeast: 'Northeast',
  'north-east': 'Northeast',
  ne: 'Northeast',
  northwest: 'Northwest',
  'north-west': 'Northwest',
  nw: 'Northwest',
  southeast: 'Southeast',
  'south-east': 'Southeast',
  se: 'Southeast',
  southwest: 'Southwest',
  'south-west': 'Southwest',
  sw: 'Southwest',
}

const directionOrderIndex = new Map<string, number>(
  turningMovementDirectionOrder.map((direction, index) => [direction, index])
)

export function normalizeTurningMovementDirection(raw: string) {
  const normalized = raw.trim().toLowerCase()
  return directionAliases[normalized] ?? raw
}

export function compareTurningMovementDirections(a: string, b: string) {
  const normalizedA = normalizeTurningMovementDirection(a)
  const normalizedB = normalizeTurningMovementDirection(b)
  const orderA = directionOrderIndex.get(normalizedA) ?? Number.MAX_SAFE_INTEGER
  const orderB = directionOrderIndex.get(normalizedB) ?? Number.MAX_SAFE_INTEGER

  if (orderA !== orderB) return orderA - orderB

  return normalizedA.localeCompare(normalizedB)
}

export function getAvailableTurningMovementDirections(rawDirections: string[]) {
  return Array.from(
    new Set(
      rawDirections
        .map(normalizeTurningMovementDirection)
        .filter((direction) => direction.length > 0)
    )
  ).sort(compareTurningMovementDirections)
}
