// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - segement.ts
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
export interface Segment {
  id: string;
  udotRouteNumber: string;
  startMilePoint: number;
  endMilePoint: number;
  functionalType: string;
  name: string;
  direction?: string;
  speedLimit: number;
  region?: string;
  city?: string;
  county?: string;
  shape?: GeoJSON.Geometry | any;
  shapeWKT?: string;
  alternateIdentifier?: string | null;
  accessCategory?: string;
  offset?: number;
  routeEntities?: SegmentEntity[] | null;
}  
  export interface SegmentEntity {
    entityId: number;
    sourceId: number;
    segmentId: string;
    entityType: string;
    length: number;
    segment?: Segment;
  }