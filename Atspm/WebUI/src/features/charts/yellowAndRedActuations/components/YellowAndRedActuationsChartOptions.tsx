import { Default } from '@/features/charts/types'
import { YellowAndRedActuationsChartOptionsDefaults } from '@/features/charts/yellowAndRedActuations/types'
import { YAxisDefaultInput } from '@/features/charts/components/selectChart/YAxisDefaultInput'
import { Box, FormControl, TextField, Typography,SelectChangeEvent } from '@mui/material'
import { ChangeEvent, useState } from 'react'


const visuallyHidden: React.CSSProperties = {
  position: 'absolute',
  width: '1px',
  height: '1px',
  padding: '0px',
  margin: '-1px',
  overflow: 'hidden',
  clip: 'rect(0, 0, 0, 0)',
  border: '0px',
}

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
  const [yAxisDefault, setYAxisDefault] = useState(
    chartDefaults.yAxisDefault?.value
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

  const handleYAxisDefaultChange = (event: SelectChangeEvent<string>) => {
    const newYAxisDefault = event.target.value
    setYAxisDefault(newYAxisDefault)

    handleChartOptionsUpdate({
      id: chartDefaults.yAxisDefault.id,
      option: chartDefaults.yAxisDefault.option,
      value: newYAxisDefault,
    })
  }



  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
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
            <YAxisDefaultInput
              value={yAxisDefault}
              handleChange={handleYAxisDefaultChange}
            />
    </Box>
  )
}
