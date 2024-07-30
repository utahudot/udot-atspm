import { SplitMonitorChartOptionsDefaults } from '@/features/charts/splitMonitor/types'
import { Default } from '@/features/charts/types'
import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  SelectChangeEvent,
  Typography,
} from '@mui/material'
import { useState } from 'react'

interface SplitMonitorChartOptionsProps {
  chartDefaults: SplitMonitorChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
}

export const SplitMonitorChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: SplitMonitorChartOptionsProps) => {
  const [selectedPercentile, setSelectedPercentile] = useState(
    chartDefaults?.percentileSplit?.value || ''
  )

  const handleSelectedPercentileChange = (event: SelectChangeEvent<string>) => {
    const newPercentile = event.target.value
    setSelectedPercentile(newPercentile)

    handleChartOptionsUpdate({
      value: newPercentile,
      option: chartDefaults.percentileSplit.option,
      id: chartDefaults.percentileSplit.id,
    })
  }

  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
      <InputLabel htmlFor="percentile-split-input">
        <Typography>Percentile Split</Typography>
      </InputLabel>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <FormControl variant="outlined">
          <Select
            value={selectedPercentile}
            onChange={handleSelectedPercentileChange}
            variant="standard"
            size="small"
            sx={{ width: '60px' }}
            inputProps={{ id: 'percentile-split-input' }}
          >
            {['None', '50', '75', '85', '90', '95'].map((option) => (
              <MenuItem key={option} value={option}>
                {option}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        <Typography variant="caption" sx={{ ml: '0.5rem' }}>
          %
        </Typography>
      </Box>
    </Box>
  )
}
