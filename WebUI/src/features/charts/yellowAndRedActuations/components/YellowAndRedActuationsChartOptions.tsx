import { Default } from '@/features/charts/types'
import { YellowAndRedActuationsChartOptionsDefaults } from '@/features/charts/yellowAndRedActuations/types'
import { Box, FormControl, TextField, Typography } from '@mui/material'
import { ChangeEvent, useState } from 'react'

interface YellowAndRedActuationsChartOptionsProps {
  chartDefaults: YellowAndRedActuationsChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
}

export const YellowAndRedActuationsChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: YellowAndRedActuationsChartOptionsProps) => {
  const [severeLevel, setSevereLevel] = useState(
    chartDefaults.severeLevelSeconds.value
  )

  const handleSevereLevelChange = (
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const severeLevelSeconds = event.target.value
    setSevereLevel(severeLevelSeconds)

    handleChartOptionsUpdate({
      id: chartDefaults.severeLevelSeconds.id,
      option: chartDefaults.severeLevelSeconds.option,
      value: severeLevelSeconds,
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
      }}
    >
      <Typography>Severe Level</Typography>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <FormControl sx={{ width: '60px' }}>
          <label htmlFor="severe-level" style={visuallyHidden}>
            Severe Level
          </label>
          <TextField
            id="severe-level"
            type="number"
            value={severeLevel}
            onChange={handleSevereLevelChange}
            variant="standard"
          />
        </FormControl>
        <Typography variant="caption" sx={{ marginLeft: '0.5rem' }}>
          sec
        </Typography>
      </Box>
    </Box>
  )
}
