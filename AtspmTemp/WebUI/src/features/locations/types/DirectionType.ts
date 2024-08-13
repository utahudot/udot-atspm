// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - DirectionType.ts
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
export const southbound = 'Southbound'
export const northbound = 'Northbound'
export const westbound = 'Westbound'
export const eastbound = 'Eastbound'
export const northEast = 'NorthEast'
export const northWest = 'NorthWest'
export const southEast = 'SouthEast'
export const southWest = 'SouthWest'

export const directionList = [
  southbound,
  northbound,
  westbound,
  eastbound,
  northEast,
  northWest,
  southEast,
  southWest,
]

export type directionType =
  | 'Southbound'
  | 'Northbound'
  | 'Westbound'
  | 'Eastbound'

export interface DirectionType {
  description: string
  abbreviation: string
  displayOrder: number
  id: string
}
