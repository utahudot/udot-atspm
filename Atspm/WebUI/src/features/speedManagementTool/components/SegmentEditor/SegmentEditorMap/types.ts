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

// SegmentEditorMap/types.ts
export interface UdotRoute {
  geometry: {
    paths: number[][][]
  }
  attributes: {
    ROUTE_ID: string
    ROUTE_DIRECTION: string
    BEG_MILEAGE: number
    END_MILEAGE: number
    ROUTE_DESC: string
  }
}

export interface SegmentProperties {
  entities?: string[]
  // Add other properties as needed
}
