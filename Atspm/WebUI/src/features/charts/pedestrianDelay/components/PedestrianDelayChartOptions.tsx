import { YAxisDefaultInput } from '@/features/charts/components/selectChart/YAxisDefaultInput'
import { PedestrianDelayChartOptionsDefaults } from '@/features/charts/pedestrianDelay/types'
import { Default } from '@/features/charts/types'
import { useChartsStore } from '@/stores/charts'
import {
  Box,
  FormControl,
  Paper,
  SelectChangeEvent,
  TextField,
  Typography,
} from '@mui/material'
import { ChangeEvent, useEffect, useState } from 'react'

interface PedestrianDelayChartOptionsProps {
  chartDefaults: PedestrianDelayChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
  isMeasureDefaultView?: boolean
}

export const PedestrianDelayChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
  isMeasureDefaultView = false,
}: PedestrianDelayChartOptionsProps) => {
  const [timeBuffer, setTimeBuffer] = useState(chartDefaults.timeBuffer.value)
  const [pedRecallThreshold, setPedRecallThreshold] = useState(
    chartDefaults.pedRecallThreshold.value
  )
  const [yAxisMax, setYAxisMax] = useState<string | null>(
    chartDefaults.yAxisDefault?.value ?? null
  )
  const { setYAxisMaxStore } = useChartsStore()

  useEffect(() => {
    setYAxisMaxStore(chartDefaults.yAxisDefault?.value ?? 180)
  }, [chartDefaults.yAxisDefault?.value, setYAxisMaxStore])

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

  const handleYAxisChartStoreUpdate = (val: string | null) => {
    setYAxisMaxStore(val)
  }

  const updateYAxisDefault = (event: SelectChangeEvent<string>) => {
    const newYAxis = event.target.value
    setYAxisMax(newYAxis)

    handleChartOptionsUpdate({
      value: newYAxis,
      option: chartDefaults.yAxisDefault.option,
      id: chartDefaults.yAxisDefault.id,
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
        {isMeasureDefaultView ? (
          <YAxisDefaultInput
            value={yAxisMax}
            handleChange={updateYAxisDefault}
            isMeasureDefaultView={isMeasureDefaultView}
          />
        ) : (
          <Paper
            sx={{ px: 2, py: 1, my: 1, bgcolor: 'background.default' }}
            elevation={0}
          >
            <Typography variant="caption">Display</Typography>
            <YAxisDefaultInput
              value={yAxisMax}
              handleChange={handleYAxisChartStoreUpdate}
              isMeasureDefaultView={isMeasureDefaultView}
            />
          </Paper>
        )}
      </Box>
    </>
  )
}
