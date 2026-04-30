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
    .filter((item) => item.isSuccess && item.result)
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
