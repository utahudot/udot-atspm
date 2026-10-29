import { applyPrintMode } from '@/features/activeTransportation/components/charts/utils'
import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import ApacheEChart from '@/features/charts/components/apacheEChart'
import transformDailyPedVolByMonthTransformer from '@/features/charts/pedat/dailyPedVolByMonthTransformer'
import { Paper, useTheme } from '@mui/material'
import { useMemo } from 'react'

const months = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
]

const DailyPedVolByMonthChart = ({
  data,
  printMode,
}: PedatChartsContainerProps) => {
  const theme = useTheme()
  // Report captures render on white, so keep them on the light chart theme
  const chartTheme = printMode ? 'light' : theme.palette.mode
  const combinedData = months?.map((month, monthIndex) => {
    const monthVolume =
      data
        ?.map(
          (loc) =>
            loc.averageVolumeByMonthOfYear?.find(
              (d) => d.index === monthIndex + 1
            )?.volume || 0
        )
        .reduce((a, b) => a + b, 0) || 0
    return { month, averageVolume: monthVolume }
  })

  const base = transformDailyPedVolByMonthTransformer(combinedData || [])

  const option = useMemo(
    () => applyPrintMode(base, !!printMode),
    [base, printMode]
  )

  return (
    <Paper sx={{ padding: '25px' }}>
      <ApacheEChart
        id="daily-ped-vol-month"
        option={option}
        style={{ width: '100%', height: '400px' }}
        hideInteractionMessage
        theme={chartTheme}
      />
    </Paper>
  )
}

export default DailyPedVolByMonthChart
