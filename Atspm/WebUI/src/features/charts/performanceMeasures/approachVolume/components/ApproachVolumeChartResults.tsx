import { ApproachVolumeTable } from '@/features/charts/approachVolume/components/ApproachVolumeTable'
import ApacheEChart from '@/features/charts/components/apacheEChart'
import { TransformedApproachVolumeResponse } from '@/features/charts/types'
import { Box, Paper, useTheme } from '@mui/material'

export interface DefaultChartResultsProps {
  chartData: TransformedApproachVolumeResponse
  refs: React.RefObject<HTMLDivElement>[]
}

export default function ApproachVolumeChartResults({
  chartData,
  refs,
}: DefaultChartResultsProps) {
  const theme = useTheme()
  return (
    <>
      {chartData.data.charts.map((chartWrapper, index) => (
        <Box
          key={index}
          ref={refs[index]}
          sx={{
            overflow: 'hidden',
            maxHeight: '1500px',
            transition: 'max-height .5s',
          }}
        >
          <Paper sx={{ p: 4, my: 3, width: '99%', marginLeft: '2px' }}>
            <ApacheEChart
              id={`chart-${index}`}
              option={chartWrapper.chart}
              chartType={chartData.type}
              theme={theme.palette.mode}
              style={{ width: '100%', height: '700px' }}
            />
            <ApproachVolumeTable data={chartWrapper.table} />
          </Paper>
        </Box>
      ))}
    </>
  )
}
