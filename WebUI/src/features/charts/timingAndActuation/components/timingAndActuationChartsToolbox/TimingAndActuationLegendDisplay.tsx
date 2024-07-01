import ApacheEChart from '@/features/charts/components/apacheEChart'
import { TransformedTimingAndActuationResponse } from '@/features/charts/types'
import ArrowDropDownIcon from '@mui/icons-material/ArrowDropDown'
import ArrowDropUpIcon from '@mui/icons-material/ArrowDropUp'
import { Box, Button, Paper, Typography, useTheme } from '@mui/material'
import { useState } from 'react'

interface TimingAndActuationLegendDisplayProps {
  chartData: TransformedTimingAndActuationResponse
}

function TimingAndActuationLegendDisplay({
  chartData,
}: TimingAndActuationLegendDisplayProps) {
  const theme = useTheme()
  const [showLegend, setShowLegend] = useState(false)

  const toggleLegend = () => {
    setShowLegend(!showLegend)
  }

  return (
    <>
      <Button
        onClick={toggleLegend}
        sx={{
          marginRight: 1,
          mx: '2px',
          color: theme.palette.text.primary,
          textTransform: 'none',
        }}
        endIcon={showLegend ? <ArrowDropUpIcon /> : <ArrowDropDownIcon />}
      >
        <Typography
          fontWeight={400}
          fontSize={'.8rem'}
          sx={{ textTransform: 'none' }}
        >
          Legend
        </Typography>
      </Button>
      {showLegend ? (
        <Paper
          sx={{
            position: 'absolute',
            left: '50%',
            transform: 'translateX(-50%)',
            top: '35px',
            p: 2,
            my: 1,
            width: '1050px',
            border: '1px solid #d3d3d3',
          }}
        >
          <Typography variant="h4" sx={{ marginBottom: 2 }}>
            Timing and Actuation Legend
          </Typography>
          <Box display={'flex'}>
            {chartData.data.legends.map((legend, index) => (
              <ApacheEChart
                key={index}
                id="legend"
                option={legend}
                chartType={chartData.type}
                theme={theme.palette.mode}
                style={{
                  width: '100%',
                  minWidth: '350px',
                  minHeight: '200px',
                }}
              />
            ))}
          </Box>
        </Paper>
      ) : null}
    </>
  )
}

export default TimingAndActuationLegendDisplay
