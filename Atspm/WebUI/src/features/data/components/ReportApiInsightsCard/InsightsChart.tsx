import ApacheEChart from '@/features/charts/components/apacheEChart'
import { Color } from '@/features/charts/utils'
import { formatBytes } from '@/utils/formatting'
import { Box, Skeleton, Typography } from '@mui/material'
import { memo } from 'react'
import type { Metric } from './InsightsHeader'

export type ChartItem = { name: string; value: number }

interface InsightChartProps {
  chartHeight: number
  horizontal: boolean
  items: ChartItem[]
  title?: string
  subtitle?: string
  metric: Metric
  loading?: boolean
}

function InsightsChart({
  chartHeight,
  horizontal,
  items,
  title,
  subtitle,
  metric,
  loading,
}: InsightChartProps) {
  const isBytes = metric === 'DataDownloaded'

  const valueLabel = (n: number) =>
    isBytes ? formatBytes(n) : `${Math.round(n)}`

  if (loading)
    return (
      <Box>
        <Skeleton height={chartHeight ?? 400} width={'100%'} />
      </Box>
    )

  if (items.length === 0)
    return (
      <Box>
        <Typography variant="h6" color="text.secondary">
          No data available for the selected metric and date range.
        </Typography>
      </Box>
    )

  return loading ? (
    <Skeleton height={chartHeight ?? 400} width={'100%'} />
  ) : (
    <Box sx={{ height: '100%', width: '100%' }}>
      <ApacheEChart
        key={items.length}
        id="insights-chart"
        hideInteractionMessage
        option={
          horizontal
            ? {
                title: title
                  ? {
                      text: title,
                      subtext: subtitle,
                      left: 'left',
                      top: 0,
                      textStyle: { fontSize: 14, fontWeight: 700 },
                      subtextStyle: { fontSize: 11 },
                    }
                  : undefined,
                tooltip: {
                  trigger: 'axis',
                  axisPointer: { type: 'shadow' },
                  valueFormatter: (v: any) => valueLabel(Number(v)),
                },
                toolbox: { feature: { saveAsImage: { name: title } } },
                grid: {
                  left: 230,
                  right: 50,
                  top: 60,
                  bottom: 20,
                },
                xAxis: {
                  type: 'value',
                  axisLabel: {
                    formatter: (v: any) => valueLabel(Number(v)),
                  },
                },
                yAxis: {
                  type: 'category',
                  data: items.map((x) => x.name),
                  axisLabel: { width: 240, overflow: 'truncate' },
                },
                color: Color.LightBlue,
                series: [
                  {
                    type: 'bar',
                    data: items.map((x) => x.value),
                    label: {
                      show: true,
                      position: 'right',
                      formatter: (p: any) => valueLabel(Number(p.value)),
                    },
                  },
                ],
              }
            : {
                title: title
                  ? {
                      text: title,
                      subtext: subtitle,
                      left: 'left',
                      top: 0,
                      textStyle: { fontSize: 14, fontWeight: 700 },
                      subtextStyle: { fontSize: 11 },
                    }
                  : undefined,
                tooltip: {
                  trigger: 'axis',
                  axisPointer: { type: 'shadow' },
                  valueFormatter: (v: any) => valueLabel(Number(v)),
                },
                grid: { left: 50, right: 20, top: 70, bottom: 60 },
                xAxis: {
                  type: 'category',
                  data: items.map((x) => x.name),
                  axisLabel: { rotate: items.length > 10 ? 30 : 0 },
                },
                yAxis: {
                  type: 'value',
                  axisLabel: { formatter: (v: any) => valueLabel(Number(v)) },
                },
                color: Color.LightBlue,
                series: [
                  {
                    type: 'line',
                    areaStyle: {},
                    data: items.map((x) => x.value),
                  },
                ],
              }
        }
        style={{ height: '100%', width: '100%' }}
      />
    </Box>
  )
}

export default memo(InsightsChart)
