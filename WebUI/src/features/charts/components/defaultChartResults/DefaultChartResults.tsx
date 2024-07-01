import ApacheEChart from '@/features/charts/components/apacheEChart'
import { TransformedDefaultResponse } from '@/features/charts/types'
import { Box, Paper, useTheme } from '@mui/material'

export interface DefaultChartResultsProps {
  chartData: TransformedDefaultResponse
  refs: React.RefObject<HTMLDivElement>[]
}

export default function DefaultChartResults({
  chartData,
  refs,
}: DefaultChartResultsProps) {
  const theme = useTheme()

  console.log('chartData', chartData.data)

  return (
    <>
      {chartData.data.charts.map((chartWrapper, index) => (
        <Box
          key={index}
          ref={refs[index]}
          sx={{
            overflow: 'hidden',
            maxHeight: '1000px',
            transition: 'max-height .5s',
            minWidth: '600px',
          }}
        >
          <Paper
            sx={{
              p: 4,
              my: 3,
              height: chartWrapper.chart.displayProps?.height || '700px',
              width: '99%',
              marginLeft: '2px',
              backgroundColor: chartWrapper.chart.displayProps
                ?.isPermissivePhase
                ? theme.palette.background.highlight
                : 'white',
            }}
          >
            <ApacheEChart
              id={`chart-${index}`}
              option={chartWrapper.chart}
              chartType={chartData.type}
              theme={theme.palette.mode}
              style={{ width: '100%', height: '100%', position: 'relative' }}
            />
          </Paper>
        </Box>
      ))}
    </>
  )
}
