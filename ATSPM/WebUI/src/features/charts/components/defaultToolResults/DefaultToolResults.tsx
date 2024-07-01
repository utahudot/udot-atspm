import ApacheEChart from '@/features/charts/components/apacheEChart'
import { TransformedToolResponse } from '@/features/charts/types'
import { Box, Paper, useTheme } from '@mui/material'

export interface DefaultToolResultsProps {
  chartData: TransformedToolResponse
  refs: React.RefObject<HTMLDivElement>[]
}

export default function DefaultToolResults({
  chartData,
  refs,
}: DefaultToolResultsProps) {
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
                height:
                  chartWrapper.chart.displayProps.numberOfLocations < 5
                    ? chartWrapper.chart.displayProps.numberOfLocations * 150 +
                      160 +
                      'px'
                    : chartWrapper.chart.displayProps.numberOfLocations * 70 +
                      160 +
                      'px',
                position: 'relative',
              }}
            />
          </Paper>
        </Box>
      ))}
    </>
  )
}
