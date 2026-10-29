import { applyPrintMode } from '@/features/activeTransportation/components/charts/utils'
import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import ApacheEChart from '@/features/charts/components/apacheEChart'
import transformAvgDailyPedVolByLocation from '@/features/charts/pedat/avgDailyPedVolByLocation'
import { Paper, useTheme } from '@mui/material'
import { useMemo } from 'react'

const AverageDailyPedVolByLocationChart = ({
  data,
  printMode,
}: PedatChartsContainerProps) => {
  const theme = useTheme()
  // Report captures render on white, so keep them on the light chart theme
  const chartTheme = printMode ? 'light' : theme.palette.mode
  const base = transformAvgDailyPedVolByLocation(data || [])

  const option = useMemo(
    () => applyPrintMode(base, !!printMode),
    [base, printMode]
  )

  return (
    <Paper sx={{ padding: '25px', mb: 5 }}>
      <ApacheEChart
        id="avg-daily-ped-vol"
        option={option}
        style={{ width: '100%', height: '400px' }}
        hideInteractionMessage
        theme={chartTheme}
      />
    </Paper>
  )
}

export default AverageDailyPedVolByLocationChart
