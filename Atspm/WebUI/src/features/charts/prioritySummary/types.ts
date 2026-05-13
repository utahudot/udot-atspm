import { PrioritySummaryCycleDto, PrioritySummaryResult } from '@/api/reports'
import { ChartType } from '@/features/charts/common/types'
import { EChartsOption } from 'echarts'

type IconCode = 113 | 114 | 116 | 117

export function toIconPoints(
  cycles: PrioritySummaryCycleDto[],
  code: IconCode
) {
  const out: Array<[string, number]> = []

  for (const c of cycles) {
    const inMs = Date.parse(c.checkIn)
    const outMs = Date.parse(c.checkOut)
    if (!Number.isFinite(inMs) || !Number.isFinite(outMs)) continue

    const timestamps =
      code === 113
        ? c.earlyGreen
        : code === 114
          ? c.extendGreen
          : code === 116
            ? c.preemptForceOff
            : c.tspEarlyForceOff

    for (const ts of timestamps ?? []) {
      const tMs = Date.parse(ts)
      if (!Number.isFinite(tMs)) continue
      if (tMs < inMs || tMs > outMs) continue
      out.push([c.checkIn, (tMs - inMs) / 1000])
    }
  }

  return out
}

export interface RawPrioritySummaryResponse {
  data: PrioritySummaryResult
}

export interface TransformedPrioritySummaryResponse {
  type: ChartType
  data: {
    charts: Array<{ chart: EChartsOption }>
  }
}
