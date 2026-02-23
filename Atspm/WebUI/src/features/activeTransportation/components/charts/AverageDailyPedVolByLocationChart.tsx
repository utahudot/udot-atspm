import { applyPrintMode } from '@/features/activeTransportation/components/charts/utils'
import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import ApacheEChart from '@/features/charts/components/apacheEChart'
import transformAvgDailyPedVolByLocation from '@/features/charts/pedat/avgDailyPedVolByLocation'
import { Paper } from '@mui/material'
import { useMemo } from 'react'

const AverageDailyPedVolByLocationChart = ({
  data,
  printMode,
}: PedatChartsContainerProps) => {
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
      />
    </Paper>
  )
}

export default AverageDailyPedVolByLocationChart
