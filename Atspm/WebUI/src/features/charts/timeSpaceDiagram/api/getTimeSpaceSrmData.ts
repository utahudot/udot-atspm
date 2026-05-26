// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - getTimeSpaceSrmData.ts
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
  TimeSpaceSrmOptions,
  TimeSpaceSrmPhaseOverlay,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { reportsRequest } from '@/lib/axios'
import { dateToTimestamp } from '@/utils/dateTime'
import { useMutation } from 'react-query'

export const getTimeSpaceSrmData = async (
  options: TimeSpaceSrmOptions
): Promise<TimeSpaceSrmPhaseOverlay[]> => {
  const payload = {
    ...options,
    routeId: Number(options.routeId),
    start: dateToTimestamp(options.start),
    end: dateToTimestamp(options.end),
  }

  return reportsRequest<TimeSpaceSrmPhaseOverlay[]>({
    url: 'api/v1/TimeSpaceDiagram/getSrmData',
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    data: payload,
  })
}

export const useTimeSpaceSrmData = () => {
  return useMutation(getTimeSpaceSrmData)
}
