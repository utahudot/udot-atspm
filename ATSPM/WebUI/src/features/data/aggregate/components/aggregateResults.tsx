import ApacheEChart from '@/features/charts/components/apacheEChart'
import { Box, Paper, useTheme } from '@mui/material'
import { TransformedAggregateData } from '../types/aggregateData'

export interface props {
  chartData: TransformedAggregateData
  refs: React.RefObject<HTMLDivElement>[]
}

export default function AggregateResults({ chartData, refs }: props) {
  const theme = useTheme()

  return (
    <>
      {chartData.data.charts.map((chartWrapper, index) => (
        <Box
          key={index}
          ref={refs[index]}
          sx={{
            overflow: 'hidden',
            minWidth: '600px',
          }}
        >
          <Paper
            sx={{
              p: 4,
              my: 3,
              width: '99%',
              marginLeft: '2px',
              backgroundColor: 'white',
            }}
          >
            <ApacheEChart
              id={`chart-${index}`}
              option={chartWrapper.chart}
              theme={theme.palette.mode}
              style={{
                width: '100%',
                height: '500px',
                position: 'relative',
              }}
            />
          </Paper>
        </Box>
      ))}
    </>
  )
}
