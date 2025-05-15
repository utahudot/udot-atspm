import { YAxisDefaultInput } from '@/features/charts/components/selectChart/YAxisDefaultInput'
import { SplitMonitorChartOptionsDefaults } from '@/features/charts/splitMonitor/types'
import { Default } from '@/features/charts/types'
import { useChartsStore } from '@/stores/charts'
import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  SelectChangeEvent,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'

interface SplitMonitorChartOptionsProps {
  chartDefaults: SplitMonitorChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
  isMeasureDefaultView?: boolean
}

export const SplitMonitorChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
  isMeasureDefaultView = false,
}: SplitMonitorChartOptionsProps) => {
  const [selectedPercentile, setSelectedPercentile] = useState(
    chartDefaults?.percentileSplit?.value
  )
  const { setYAxisMaxStore } = useChartsStore()

  const [yAxisMax, setYAxisMax] = useState<string | null>(
    chartDefaults.yAxisDefault?.value
  )

  useEffect(() => {
    setYAxisMaxStore(chartDefaults.yAxisDefault?.value)
  }, [chartDefaults.yAxisDefault?.value, setYAxisMaxStore])

  const handleSelectedPercentileChange = (event: SelectChangeEvent<string>) => {
    const newPercentile = event.target.value
    setSelectedPercentile(newPercentile)

    handleChartOptionsUpdate({
      value: newPercentile,
      option: chartDefaults.percentileSplit.option,
      id: chartDefaults.percentileSplit.id,
    })
  }

  const updateYAxisDefault = (newYAxis: string) => {
    setYAxisMax(newYAxis)

    if (isMeasureDefaultView) {
      handleChartOptionsUpdate({
        value: newYAxis,
        option: chartDefaults.yAxisDefault.option,
        id: chartDefaults.yAxisDefault.id,
      })
    } else {
      setYAxisMaxStore(newYAxis)
    }
  }

  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
        flexDirection: 'column',
        gap: 1,
        flex: '1 1 0%',
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

      <YAxisDefaultInput
        value={yAxisMax}
        handleChange={updateYAxisDefault}
        isMeasureDefaultView={isMeasureDefaultView}
      />
    </Box>
  )
}
