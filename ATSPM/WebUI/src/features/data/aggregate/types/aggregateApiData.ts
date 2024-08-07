// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - aggregateApiData.ts
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
import { ExpandLocationForAggregation } from '../handlers/expandLocationHandler'

export interface AggregateApiData {
  locationIdentifiers: string[]
  start: string
  end: string
  aggregationType: number
  dataType: number
  timeOptions: AggregateTimeOptions
  selectedAggregationType: number
  selectedXAxisType: number
  selectedSeries: number
  locations: ExpandLocationForAggregation[]
  filterDirections: AggregateFilterDirection[]
  filterMovements: AggregateFilterMovement[]
}

export interface AggregateTimeOptions {
  start: string
  end: string
  timeOfDayStartHour: number
  timeOfDayStartMinute: number
  timeOfDayEndHour: number
  timeOfDayEndMinute: number
  daysOfWeek: number[]
  timeOption: number
  selectedBinSize: number
}

export interface AggregateFilterDirection {
  directionTypeId: number
  description: string
  include: boolean
}

export interface AggregateFilterMovement {
  movementTypeId: number
  description: string
  include: boolean
}
