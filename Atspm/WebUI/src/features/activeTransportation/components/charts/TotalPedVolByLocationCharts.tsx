import { applyPrintMode } from '@/features/activeTransportation/components/charts/utils'
import { PedatChartsContainerProps } from '@/features/activeTransportation/components/PedatChartsContainer'
import ApacheEChart from '@/features/charts/components/apacheEChart'

import {
  transformBlockChartTransformer,
  transformPieChartTransformer,
} from '@/features/charts/pedat/totalPedVolByLocationTransformer'
import { Box, Paper, Typography } from '@mui/material'
import { useMemo } from 'react'

const TotalPedVolByLocationCharts = ({
  data,
  printMode,
}: PedatChartsContainerProps) => {
  const totalVolume = data
    ?.map((loc) => loc.totalVolume || 0)
    .reduce((a, b) => a + b, 0)
  const percentageData = data?.map((loc) => ({
    locationIdentifier: loc.locationIdentifier,
    percentage: totalVolume ? ((loc.totalVolume || 0) / totalVolume) * 100 : 0,
  }))
  const pieOption = transformPieChartTransformer(percentageData || [])
  const blockOption = transformBlockChartTransformer(percentageData || [])

  const pie = useMemo(
    () => applyPrintMode(pieOption, !!printMode),
    [pieOption, printMode]
  )

  const block = useMemo(
    () => applyPrintMode(blockOption, !!printMode),
    [blockOption, printMode]
  )

  return (
    <Paper sx={{ padding: '25px', mb: 5 }}>
      <Box sx={{ textAlign: 'center', mb: 2 }}>
        <Typography variant="h4" sx={{ fontWeight: 'bold' }} gutterBottom>
          Total Pedestrian Volume, by Location
        </Typography>
      </Box>

      <Box
        sx={{
          display: 'flex',
          flexDirection: 'row',
          gap: 4,
          justifyContent: 'space-between',
        }}
      >
        <Box sx={{ width: '50%', height: 400 }}>
          <ApacheEChart
            id="ped-vol-pie"
            option={pie}
            style={{ height: '100%' }}
          />
        </Box>
        <Box sx={{ width: '50%', height: 400 }}>
          <ApacheEChart
            id="ped-vol-block"
            option={block}
            style={{ height: '100%' }}
          />
        </Box>
      </Box>
    </Paper>
  )
}

export default TotalPedVolByLocationCharts
