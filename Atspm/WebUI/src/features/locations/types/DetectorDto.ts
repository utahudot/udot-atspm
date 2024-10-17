// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - DetectorDto.ts
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
import { directionType } from './DirectionType'

interface DetectorsDto {
  id: number
  detectionChannel: number
  direction: directionType
  phase: number
  permPhase: number
  overlap: boolean
  enabled: boolean
  detectionTypes: string
  detectionHardware: string
  latencyCorrection: string
  movementType: string
  laneNumber: number
  laneType: string
  distanceFromStopBar: number
  decisionPoint: string
  movementDelay: string
  minSpeedFilter: number
  comment: string
  detectorComments: Comment[]
}
interface Comment {
  comment: string
  timeStamp: string
  detectorId: number
  id: number
}

export default DetectorsDto
