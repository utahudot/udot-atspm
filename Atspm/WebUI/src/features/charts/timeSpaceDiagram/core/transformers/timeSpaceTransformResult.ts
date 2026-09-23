// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - timeSpaceTransformResult.ts
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
import { ToolType } from '@/features/charts/common/types'
import type { TransformedTimeSpaceResponse } from '@/features/charts/types'
import type { EChartsOption } from 'echarts'
import type {
  TimeSpaceBaseData,
  TimeSpaceDiagramPhaseResult,
} from '../../shared/types'

export function unwrapTimeSpaceTransformResults<T extends TimeSpaceBaseData>(
  wrappedData: TimeSpaceDiagramPhaseResult<T>[]
) {
  const errorMessages = wrappedData
    .filter((item) => !item.isSuccess && item.error)
    .map((item) => item.error as string)
  const successfulData = wrappedData
    .filter((item) => item.result)
    .map((item) => item.result as T)

  return {
    errorMessages,
    successfulData,
  }
}

export function buildEmptyTimeSpaceTransformResult(
  toolType: ToolType.TimeSpaceHistoric | ToolType.TimeSpaceAverage,
  errorMessages: string[]
): TransformedTimeSpaceResponse & { errors?: string[] } {
  return {
    type: toolType,
    data: { chart: {} },
    errors:
      errorMessages.length > 0
        ? errorMessages
        : [
            'No valid time space diagram data available. All phases returned errors.',
          ],
  }
}

export function buildTimeSpaceTransformResult(
  toolType: ToolType.TimeSpaceHistoric | ToolType.TimeSpaceAverage,
  chart: EChartsOption,
  errorMessages: string[]
): TransformedTimeSpaceResponse & { errors?: string[] } {
  const result: TransformedTimeSpaceResponse & { errors?: string[] } = {
    type: toolType,
    data: {
      chart,
    },
  }

  if (errorMessages.length > 0) {
    result.errors = errorMessages
  }

  return result
}
