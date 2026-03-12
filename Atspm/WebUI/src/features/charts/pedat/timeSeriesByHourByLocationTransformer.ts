import {
  createDataZoom,
  createGrid,
  createLegend,
  createTooltip,
} from '@/features/charts/common/transformers'
import type { EChartsOption, ToolboxComponentOption } from 'echarts'

type RawPoint = {
  timestamp?: string
  timeStamp?: string
  pedestrianCount: number
}
type Loc = {
  locationIdentifier?: string
  names?: string
  rawData?: RawPoint[]
}

function toHourBucket(ts: string): string {
  const d = new Date(ts)
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  const hh = String(d.getHours()).padStart(2, '0')
  return `${y}-${m}-${day} ${hh}:00`
}

function gatherBuckets(rows: Loc[]): string[] {
  const s = new Set<string>()
  for (const r of rows) {
    for (const p of r.rawData ?? []) {
      const ts = p.timestamp ?? p.timeStamp
      if (ts) s.add(toHourBucket(ts))
    }
  }
  return Array.from(s).sort()
}

function bucketize(row: Loc): Map<string, number> {
  const m = new Map<string, number>()
  for (const p of row.rawData ?? []) {
    const ts = p.timestamp ?? p.timeStamp
    if (!ts) continue
    const k = toHourBucket(ts)
    const v = Number(p.pedestrianCount ?? 0)
    m.set(k, (m.get(k) ?? 0) + (Number.isFinite(v) ? v : 0))
  }
  return m
}

export default function timeSeriesByHourByLocationTransformer(
  data: Loc[] = [],
  timeUnit: string
): EChartsOption {
  const xBuckets = gatherBuckets(data)
  const series = data.map((loc) => {
    const name = (loc.locationIdentifier || 'Unknown').trim()
    const m = bucketize(loc)
    const values = xBuckets.map((b) => m.get(b) ?? 0)
    return {
      name,
      type: 'bar',
      stack: 'total',
      barWidth: '100%',
      emphasis: { focus: 'series' },
      data: values,
    }
  })

  const title = {
    text: `Time Series of Pedestrian Volume, by ${timeUnit}, by Location`,
    left: 'center',
  }

  const xAxis = {
    type: 'category',
    name: 'Time',
    data: xBuckets,
    axisLabel: { formatter: (v: string) => v.split(' ')[0] },
  }

  const yAxis = { type: 'value', name: 'Pedestrian Volume' }
  const grid = createGrid({ top: 80, left: 60, right: 150, bottom: 80 })
  const legend = createLegend({
    bottom: 0,

    data: data.map((l) => (l.locationIdentifier || 'Unknown').trim()),
  })
  const dataZoom = createDataZoom()
  const toolbox: ToolboxComponentOption = {
    feature: {
      saveAsImage: { name: title },
      magicType: {
        type: ['stack', 'line', 'bar'],
      },
      dataView: {
        readOnly: true,
      },
    },
  }
  const tooltip = createTooltip({ trigger: 'axis' })

  return {
    title,
    xAxis,
    yAxis,
    grid,
    legend,
    dataZoom,
    toolbox,
    tooltip,
    series,
  }
}
