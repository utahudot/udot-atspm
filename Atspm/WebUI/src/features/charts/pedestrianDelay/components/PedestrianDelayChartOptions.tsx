import { YAxisDefaultInput } from '@/features/charts/components/selectChart/YAxisDefaultInput'
import { PedestrianDelayChartOptionsDefaults } from '@/features/charts/pedestrianDelay/types'
import { Default } from '@/features/charts/types'
import {
  Box,
  FormControl,
  SelectChangeEvent,
  TextField,
  Typography,
} from '@mui/material'
import { ChangeEvent, useState } from 'react'

interface PedestrianDelayChartOptionsProps {
  chartDefaults: PedestrianDelayChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
}

export const PedestrianDelayChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: PedestrianDelayChartOptionsProps) => {
  const [timeBuffer, setTimeBuffer] = useState(chartDefaults.timeBuffer.value)
  const [pedRecallThreshold, setPedRecallThreshold] = useState(
    chartDefaults.pedRecallThreshold.value
  )
  const [yAxisDefault, setYAxisDefault] = useState(
    chartDefaults.yAxisDefault?.value
  )

  const handleTimeBufferChange = (
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const newTimeBuffer = event.target.value
    setTimeBuffer(newTimeBuffer)

    handleChartOptionsUpdate({
      id: chartDefaults.timeBuffer.id,
      option: chartDefaults.timeBuffer.option,
      value: newTimeBuffer,
    })
  }

  const handlePedRecallThresholdChange = (
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const newPedRecallThreshold = event.target.value
    setPedRecallThreshold(newPedRecallThreshold)

    handleChartOptionsUpdate({
      id: chartDefaults.pedRecallThreshold.id,
      option: chartDefaults.pedRecallThreshold.option,
      value: newPedRecallThreshold,
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

  return (
    <>
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            padding: '0.25rem',
          }}
        >
          <Typography>Time Buffer</Typography>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <FormControl sx={{ width: '60px' }}>
              <label htmlFor="time-buffer" style={visuallyHidden}>
                Time Buffer
              </label>
              <TextField
                id="time-buffer"
                type="number"
                value={timeBuffer}
                onChange={handleTimeBufferChange}
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
          <Typography>Ped Recall Threshold</Typography>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            <FormControl sx={{ width: '60px' }}>
              <label htmlFor="ped-recall-threshold" style={visuallyHidden}>
                Ped Recall Threshold
              </label>
              <TextField
                id="ped-recall-threshold"
                type="number"
                value={pedRecallThreshold}
                onChange={handlePedRecallThresholdChange}
                variant="standard"
              />
            </FormControl>
            <Typography variant="caption" sx={{ mx: '.6rem' }}>
              %
            </Typography>
          </Box>
        </Box>
        <YAxisDefaultInput
          value={yAxisDefault}
          handleChange={handleYAxisDefaultChange}
        />
      </Box>
    </>
  )
}
