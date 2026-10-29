import { applyPrintMode } from '@/features/activeTransportation/components/charts/utils'
import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import ApacheEChart from '@/features/charts/components/apacheEChart'
import timeSeriesByHourByLocationTransformer from '@/features/charts/pedat/timeSeriesByHourByLocationTransformer'
import { Paper, useTheme } from '@mui/material'
import { useMemo } from 'react'

const TimeSeriesByHourByLocationChart = ({
  data,
  printMode,
  timeUnit,
}: PedatChartsContainerProps) => {
  const theme = useTheme()
  // Report captures render on white, so keep them on the light chart theme
  const chartTheme = printMode ? 'light' : theme.palette.mode
  const base = timeSeriesByHourByLocationTransformer(data, timeUnit)

  const option = useMemo(
    () => applyPrintMode(base, !!printMode),
    [base, printMode]
  )

  return (
    <Paper sx={{ padding: '25px', mb: 5 }}>
      <ApacheEChart
        id="ped-vol-time-series-hour"
        option={option}
        style={{ width: '100%', height: '400px' }}
        hideInteractionMessage
        theme={chartTheme}
      />
    </Paper>
  )
}

export default TimeSeriesByHourByLocationChart
