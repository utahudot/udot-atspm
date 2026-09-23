// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - types.ts
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
  AggClassification,
  SpeedDataType,
  TimePeriodFilter,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'

export interface HotSpotForReportMap {
  segmentId: string
  coordinates: [number, number][]
}

export interface ImpactHotspotForReportMap {
  impactId: string
  impactedSegments: HotSpotForReportMap[]
}

export const AggClassificationMapping = {
  [AggClassification.Total]: 'All Days',
  [AggClassification.Weekend]: 'Weekend',
  [AggClassification.Weekday]: 'Weekdays',
} as const

export const TimePeriodFilterMapping = {
  [TimePeriodFilter.AllDay]: 'All Hours',
  [TimePeriodFilter.OffPeak]: 'Off Peak',
  [TimePeriodFilter.AmPeak]: 'AM Peak',
  [TimePeriodFilter.PmPeak]: 'PM Peak',
  [TimePeriodFilter.MidDay]: 'Mid Day',
  [TimePeriodFilter.Evening]: 'Evening',
  [TimePeriodFilter.EarlyMorning]: 'Early Morning',
} as const

export const SourceIdToNamingMapping = {
  1: 'ATSPM',
  2: 'Pems',
  3: 'Clearguide',
}

export const SpeedDataTypeMapping = {
  [SpeedDataType.H]: 'Hourly',
  [SpeedDataType.M]: 'Monthly',
}
