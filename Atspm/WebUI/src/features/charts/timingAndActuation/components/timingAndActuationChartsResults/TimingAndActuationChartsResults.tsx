import ApacheEChart from '@/features/charts/components/apacheEChart'
import { TransformedTimingAndActuationResponse } from '@/features/charts/types'
import { getDirectionAccentBorder } from '@/features/locations/utils/directionAccent'
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
      <Box>
        <Paper
          sx={{
            p: 2,
            pb: 1,
            my: 1,
            mt: 3,
            width: '100%',
            marginLeft: '2px',
          }}
        >
          <ApacheEChart
            id="chart-title"
            option={chartData.data.title}
            chartType={chartData.type}
            theme={theme.palette.mode}
            hideInteractionMessage
            style={{
              width: '100%',
              height: '100px',
              position: 'relative',
            }}
          />
        </Paper>
      </Box>

      {chartData.data.charts.map((chartWrapper, index) => (
        <Box
          key={index}
          ref={refs[index]}
          sx={{
            maxHeight: '1000px',
            transition: 'max-height .3s',
          }}
        >
          <Paper
            sx={{
              p: 2,
              pb: 1,
              my: 1,
              mt: 1,
              width: '100%',
              marginLeft: '2px',
              backgroundColor:
                chartWrapper.chart.displayProps?.phaseType == 'Permissive'
                  ? theme.palette.background.highlight
                  : 'white',
              borderLeft: getDirectionAccentBorder(
                chartWrapper.chart.displayProps.approachDescription
              ),
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
                  chartWrapper.chart.displayProps.amountOfChannels * 30 +
                  150 +
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
