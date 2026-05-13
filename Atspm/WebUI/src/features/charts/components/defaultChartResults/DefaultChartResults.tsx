import ApacheEChart from '@/features/charts/components/apacheEChart'
import { TransformedDefaultResponse } from '@/features/charts/types'
import { Box, Paper, useTheme } from '@mui/material'

export interface DefaultChartResultsProps {
  chartData: TransformedDefaultResponse
  refs: React.RefObject<HTMLDivElement>[]
  chartComponent?: React.ComponentType<any>
}

export default function DefaultChartResults({
  chartData,
  refs,
  chartComponent: ChartComponent = ApacheEChart,
}: DefaultChartResultsProps) {
  const theme = useTheme()

  return (
    <>
      {chartData.data.charts.map((chartWrapper, index) => (
        <Box
          key={index}
          ref={refs[index]}
          sx={{
            maxHeight: '1000px',
            transition: 'max-height .5s',
            minWidth: '600px',
          }}
        >
          <Paper
            sx={{
              p: 2,
              my: 2,
              height: chartWrapper.chart.displayProps?.height || '200px',
              width: '100%',
              marginLeft: '2px',
              backgroundColor: chartWrapper.chart.displayProps
                ?.isPermissivePhase
                ? theme.palette.background.highlight
                : 'white',
            }}
          >
            <ChartComponent
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
