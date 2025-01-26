import { YAxisDefaultInput } from '@/features/charts/components/selectChart/YAxisDefaultInput'
import { SplitMonitorChartOptionsDefaults } from '@/features/charts/splitMonitor/types'
import { Default } from '@/features/charts/types'
import { useChartsStore } from '@/stores/charts'
import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  SelectChangeEvent,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'

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
  const { yAxisDefault, setYAxisDefault } = useChartsStore()

  useEffect(() => {
    if (chartDefaults.yAxisDefault?.value) {
      setYAxisDefault(chartDefaults.yAxisDefault.value)
    }
  }, [chartDefaults.yAxisDefault?.value, setYAxisDefault])

  const handleSelectedPercentileChange = (event: SelectChangeEvent<string>) => {
    const newPercentile = event.target.value
    setSelectedPercentile(newPercentile)

    handleChartOptionsUpdate({
      value: newPercentile,
      option: chartDefaults.percentileSplit.option,
      id: chartDefaults.percentileSplit.id,
    })
  }

  const handleYAxisDefaultChange = (val: number) => {
    setYAxisDefault(val)
  }

  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
        flexDirection: 'column',
        gap: 1,
      }}
    >
      <Box display="flex" justifyContent={'space-between'}>
        <InputLabel
          sx={{ color: 'black' }}
          htmlFor="percentile-split-input"
          id="percentile-split-input-label"
        >
          Percentile Split
        </InputLabel>
        <Box
          sx={{ display: 'flex', alignItems: 'center', marginRight: '12px' }}
        >
          <FormControl variant="outlined">
            <Select
              labelId="percentile-split-input-label"
              id="percentile-split-input"
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
      <Paper
        sx={{ px: 2, py: 1, my: 1, bgcolor: 'background.default' }}
        elevation={0}
      >
        <Typography variant="caption">Display</Typography>
        <YAxisDefaultInput
          value={yAxisDefault}
          handleChange={handleYAxisDefaultChange}
        />
      </Paper>
    </Box>
  )
}
