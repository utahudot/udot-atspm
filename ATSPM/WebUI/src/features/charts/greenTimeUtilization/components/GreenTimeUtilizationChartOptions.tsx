import { GreenTimeUtilizationChartOptionsDefaults } from '@/features/charts/greenTimeUtilization/types'
import { Default } from '@/features/charts/types'
import { Box, FormControl, TextField, Typography } from '@mui/material'
import { ChangeEvent, useState } from 'react'

interface GreenTimeUtilizationChartOptionsProps {
  chartDefaults: GreenTimeUtilizationChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
}

export const GreenTimeUtilizationChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: GreenTimeUtilizationChartOptionsProps) => {
  const [xAxisBinSize, setxAxisBinSize] = useState(
    chartDefaults.xAxisBinSize?.value
  )
  const [yAxisBinSize, setyAxisBinSize] = useState(
    chartDefaults.yAxisBinSize?.value
  )

  const handleXAxisBinSizeChange = (
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const newBinSize = event.target.value
    setxAxisBinSize(newBinSize)

    handleChartOptionsUpdate({
      id: chartDefaults.xAxisBinSize.id,
      option: chartDefaults.xAxisBinSize.option,
      value: newBinSize,
    })
  }

  const handleYAxisBinSizeChange = (
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const newBinSize = event.target.value
    setyAxisBinSize(newBinSize)

    handleChartOptionsUpdate({
      id: chartDefaults.yAxisBinSize.id,
      option: chartDefaults.yAxisBinSize.option,
      value: newBinSize,
    })
  }

  const visuallyHidden = {
    position: 'absolute',
    width: '1px',
    height: '1px',
    padding: 0,
    margin: '-1px',
    overflow: 'hidden',
    clip: 'rect(0, 0, 0, 0)',
    border: 0,
  }

  return (
    <>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          padding: '0.25rem',
        }}
      >
        <Typography>Y-Axis Bin Size</Typography>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <label htmlFor="y-axis-bin-size" style={visuallyHidden}>
            Y-Axis Bin Size
          </label>
          <FormControl sx={{ width: '60px' }}>
            <TextField
              id="y-axis-bin-size"
              type="number"
              value={yAxisBinSize}
              onChange={handleYAxisBinSizeChange}
              variant="standard"
            />
          </FormControl>
          <Typography variant="caption" sx={{ marginLeft: '0.5rem' }}>
            sec
          </Typography>
        </Box>
      </Box>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          padding: '0.25rem',
        }}
      >
        <Typography>X-Axis Bin Size</Typography>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <FormControl sx={{ width: '60px' }}>
            <label htmlFor="x-axis-bin-size" style={visuallyHidden}>
              X-Axis Bin Size
            </label>
            <TextField
              id="x-axis-bin-size"
              type="number"
              value={xAxisBinSize}
              onChange={handleXAxisBinSizeChange}
              variant="standard"
            />
          </FormControl>
          <Typography variant="caption" sx={{ marginLeft: '0.5rem' }}>
            min
          </Typography>
        </Box>
      </Box>
    </>
  )
}
