// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceCore.types.ts
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
export interface TimeSpaceCoreRow {
  start: string
  end: string
  locationIdentifier: string
  locationDescription: string
  approachDescription: string
  phaseType: 'Primary' | 'Opposing'
  distanceToNextLocation: number
  calculatedDistanceToNext: number
  calculatedDistanceToPrevious: number
  isIgnoredLocation: boolean
  speed: number
  cycleLength: number | null
  order?: number
}

export type TimeSpaceDistanceSpacingMode = 'distance' | 'sequence' | 'hybrid'

export type TimeSpaceDisplayDistanceOffset = (
  index: number,
  rawDistanceOffset: number
) => number

export interface TimeSpacePhaseLayout<T extends TimeSpaceCoreRow> {
  primaryDirection: string
  opposingDirection: string
  primaryPhaseData: T[]
  opposingPhaseData: T[]
  rawDistanceData: number[]
  distanceScale: number
  locationCenterDistanceData: number[]
  primaryDistanceData: number[]
  opposingDistanceData: number[]
  getDisplayDistanceOffset: TimeSpaceDisplayDistanceOffset
  minDisplayDistance: number
  maxDisplayDistance: number
  chartHeight: number
}
