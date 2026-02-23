import { applyPrintMode } from '@/features/activeTransportation/components/charts/utils'
import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import ApacheEChart from '@/features/charts/components/apacheEChart'
import transformBoxPlotByLocationTransformer from '@/features/charts/pedat/boxPlotByLocationTransformer'
import { Paper } from '@mui/material'
import { useMemo } from 'react'

const BoxPlotByLocationChart = ({
  data,
  printMode,
  timeUnit,
}: PedatChartsContainerProps) => {
  const base = transformBoxPlotByLocationTransformer(data, timeUnit)

  const option = useMemo(
    () => applyPrintMode(base, !!printMode),
    [base, printMode]
  )

  return (
    <Paper sx={{ padding: '25px', mb: 5 }}>
      <ApacheEChart
        id="ped-vol-boxplot-location"
        option={option}
        style={{ width: '100%', height: '400px' }}
      />
    </Paper>
  )
}

export default BoxPlotByLocationChart
