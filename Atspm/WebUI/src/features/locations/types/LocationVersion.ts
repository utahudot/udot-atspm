// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - LocationVersion.ts
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
type Jurisdiction = {
  countyParish: string | null
  name: string
  mpo: string
  otherPartners: string | null
  id: number
}

type Region = {
  description: string
  id: number
}

export type LocationVersion = {
  latitude: number
  longitude: number
  note: string
  primaryName: string
  locationIdentifier: string
  secondaryName: string
  jurisdictionId: number
  chartEnabled: boolean
  versionAction: string
  start: string
  pedsAre1to1: boolean
  locationTypeId: number
  regionId: number
  id: number
  jurisdiction: Jurisdiction
  region: Region
}
