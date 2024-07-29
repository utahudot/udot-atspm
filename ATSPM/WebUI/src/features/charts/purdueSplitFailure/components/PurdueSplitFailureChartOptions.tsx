import { PurdueSplitFailureChartOptionsDefaults } from '@/features/charts/purdueSplitFailure/types'
import { Default } from '@/features/charts/types'
import { Box, FormControl, TextField, Typography } from '@mui/material'
import { ChangeEvent, useState } from 'react'

interface PurdueSplitFailureChartOptionsProps {
  chartDefaults: PurdueSplitFailureChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
}

export const PurdueSplitFailureChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: PurdueSplitFailureChartOptionsProps) => {
  const [firstSecondsOfRed, setFirstSecondsOfRed] = useState(
    chartDefaults.firstSecondsOfRed.value
  )

  const handleFirstSecondsOfRedChange = (
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const newFirstSecondsOfRed = event.target.value
    setFirstSecondsOfRed(newFirstSecondsOfRed)

    handleChartOptionsUpdate({
      id: chartDefaults.firstSecondsOfRed.id,
      option: chartDefaults.firstSecondsOfRed.option,
      value: newFirstSecondsOfRed,
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
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
        padding: '0.25rem',
      }}
    >
      <Typography>First Seconds of Red</Typography>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <FormControl sx={{ width: '60px' }}>
          <label htmlFor="first-seconds-of-red" style={visuallyHidden}>
            First Seconds of Red
          </label>
          <TextField
            id="first-seconds-of-red"
            type="number"
            value={firstSecondsOfRed}
            onChange={handleFirstSecondsOfRedChange}
            variant="standard"
          />
        </FormControl>
      </Box>
    </Box>
  )
}
