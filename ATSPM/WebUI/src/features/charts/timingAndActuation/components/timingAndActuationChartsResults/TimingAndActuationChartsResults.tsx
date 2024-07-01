import ApacheEChart from '@/features/charts/components/apacheEChart'
import { TransformedTimingAndActuationResponse } from '@/features/charts/types'
import { Color } from '@/features/charts/utils'
import { Box, Paper, useTheme } from '@mui/material'

export interface TimingAndActuationResultsProps {
  chartData: TransformedTimingAndActuationResponse
  refs: React.RefObject<HTMLDivElement>[]
}

export default function TimingAndActuationChartsResults({
  chartData,
  refs,
}: TimingAndActuationResultsProps) {
  const theme = useTheme()
  return (
    <>
      {chartData.data.charts.map((chartWrapper, index) => (
        <Box
          key={index}
          ref={refs[index]}
          sx={{
            overflow: 'hidden',
            maxHeight: '1000px',
            transition: 'max-height .3s',
          }}
        >
          <Paper
            sx={{
              p: 2,
              pb: 1,
              my: 1,
              mt: index === 0 ? 3 : 1,
              width: '99%',
              marginLeft: '2px',
              backgroundColor:
                chartWrapper.chart.displayProps?.phaseType == 'Permissive'
                  ? theme.palette.background.highlight
                  : 'white',
              borderLeft: (() => {
                switch (
                  chartWrapper.chart.displayProps.approachDescription.charAt(0)
                ) {
                  case 'N':
                    return `7px solid ${Color.Blue}` // Blue border for 'N'
                  case 'S':
                    return `7px solid ${Color.BrightRed}` // Red border for 'S'
                  case 'E':
                    return `7px solid ${Color.Yellow}` // Yellow border for 'E'
                  case 'W':
                    return `7px solid ${Color.Orange}` // Green border for 'W'
                  default:
                    return 'none' // No border otherwise
                }
              })(),
            }}
          >
            <ApacheEChart
              id={`chart-${index}`}
              option={chartWrapper.chart}
              chartType={chartData.type}
              theme={theme.palette.mode}
              style={{
                width: '100%',
                height:
                  chartWrapper.chart.displayProps.amountOfChannels * 22 +
                  (index === 0 ? 200 : 150) +
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
