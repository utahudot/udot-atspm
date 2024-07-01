import { PurduePhaseTerminationChartOptionsDefaults } from '@/features/charts/purduePhaseTermination/types'
import { Default } from '@/features/charts/types'
import { Box, FormControl, TextField, Typography } from '@mui/material'
import { ChangeEvent, useState } from 'react'

interface PurduePhaseTerminationChartOptionsProps {
  chartDefaults: PurduePhaseTerminationChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
}

export const PurduePhaseTerminationChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: PurduePhaseTerminationChartOptionsProps) => {
  const [selectedConsecutiveCount, setSelectedConsecutiveCount] = useState(
    chartDefaults.selectedConsecutiveCount.value
  )

  const handleSelectedConsecutiveCountChange = (
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const newSelectedConsecutiveCount = event.target.value
    setSelectedConsecutiveCount(newSelectedConsecutiveCount)

    handleChartOptionsUpdate({
      id: chartDefaults.selectedConsecutiveCount.id,
      option: chartDefaults.selectedConsecutiveCount.option,
      value: newSelectedConsecutiveCount,
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
  };

  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
        padding: '0.25rem',
      }}
    >
      <Typography>Consecutive Count</Typography>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
      <FormControl sx={{ width: '60px' }}>
      <label htmlFor="selected-consecutive-count" style={visuallyHidden}>
        Selected Consecutive Count
      </label>
      <TextField
        id="selected-consecutive-count"
        type="number"
        value={selectedConsecutiveCount}
        onChange={handleSelectedConsecutiveCountChange}
        variant="standard"
      />
    </FormControl>
      </Box>
    </Box>
  )
}
