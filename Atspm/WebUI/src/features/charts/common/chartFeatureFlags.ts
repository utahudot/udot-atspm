import { ChartType } from '@/features/charts/common/types'

const binStepChartTypes = new Set<ChartType>([
  ChartType.ApproachDelay,
  ChartType.ApproachSpeed,
  ChartType.ApproachVolume,
  ChartType.ArrivalsOnRed,
  ChartType.LeftTurnGapAnalysis,
  ChartType.PurdueCoordinationDiagram,
  ChartType.TurningMovementCounts,
  ChartType.WaitTime,
])

export function supportsBinStepLineToggle(
  chartType?: ChartType | string | null
) {
  return chartType != null && binStepChartTypes.has(chartType as ChartType)
}
