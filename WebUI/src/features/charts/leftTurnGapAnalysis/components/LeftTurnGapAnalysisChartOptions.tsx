import { BinSizeDropdown } from '@/features/charts/components/selectChart/BinSizeDropdown'
import { LeftTurnGapAnalysisChartOptionsDefaults } from '@/features/charts/leftTurnGapAnalysis/types'
import { Default } from '@/features/charts/types'
import {
  Box,
  FormControl,
  SelectChangeEvent,
  TextField,
  Typography,
} from '@mui/material'
import { ChangeEvent, useState } from 'react'

interface LeftTurnGapAnalysisChartOptionsProps {
  chartDefaults: LeftTurnGapAnalysisChartOptionsDefaults
  handleChartOptionsUpdate: (update: Default) => void
}

export const LeftTurnGapAnalysisChartOptions = ({
  chartDefaults,
  handleChartOptionsUpdate,
}: LeftTurnGapAnalysisChartOptionsProps) => {
  const [binSize, setBinSize] = useState(chartDefaults.binSize.value)
  const [gap1Min, setgap1Min] = useState(chartDefaults.gap1Min.value)
  const [gap1Max, setgap1Max] = useState(chartDefaults.gap1Max.value)
  const [gap2Min, setgap2Min] = useState(chartDefaults.gap2Min.value)
  const [gap2Max, setgap2Max] = useState(chartDefaults.gap2Max.value)
  const [gap3Min, setgap3Min] = useState(chartDefaults.gap3Min.value)
  const [gap3Max, setgap3Max] = useState(chartDefaults.gap3Max.value)
  const [gap4Min, setgap4Min] = useState(chartDefaults.gap4Min.value)
  const [trendLineGapThreshold, setTrendLineGapThreshold] = useState(
    chartDefaults.trendLineGapThreshold.value
  )

  const handleBinSizeChange = (event: SelectChangeEvent<string>) => {
    const newBinSize = event.target.value
    setBinSize(newBinSize)

    handleChartOptionsUpdate({
      id: chartDefaults.binSize.id,
      option: chartDefaults.binSize.option,
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

  const handleTrendLineGapThresholdChange = (
    event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    const newTrendLineGapThreshold = event.target.value
    setTrendLineGapThreshold(newTrendLineGapThreshold)

    handleChartOptionsUpdate({
      id: chartDefaults.trendLineGapThreshold.id,
      option: chartDefaults.trendLineGapThreshold.option,
      value: newTrendLineGapThreshold,
    })
  }

  const handleInputChange =
    (option: string, setter: (value: string) => void, id: number) =>
    (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      const { value } = event.target
      setter(value)
      handleChartOptionsUpdate({
        option,
        value,
        id,
      })
    }

  const formatGapLabel = (gapNumber: number) => `Gap ${gapNumber} Range`

  const renderGapInput = (
    gapNumber: number,
    gapStart: string,
    setGapStart: (value: string) => void,
    gapEnd: string | null,
    setGapEnd: ((value: string) => void) | null
  ) => (
    <Box
      sx={{ display: 'flex', justifyContent: 'space-between', marginY: '1rem' }}
    >
      <Typography>{formatGapLabel(gapNumber)}</Typography>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <FormControl
          variant="standard"
          size="small"
          sx={{ width: '60px', marginRight: '0.5rem' }}
        >
          <label htmlFor={`gap${gapNumber}Min`} style={visuallyHidden}>
            Gap Start
          </label>
          <TextField
            id={`gap${gapNumber}Min`}
            value={gapStart}
            onChange={handleInputChange(
              `gap${gapNumber}Min`,
              setGapStart,
              chartDefaults[`gap${gapNumber}Min`]?.id
            )}
            variant="standard"
            size="small"
            type="number"
            inputProps={{ step: 0.1 }}
            sx={{ width: '100%' }}
          />
        </FormControl>
        {setGapEnd && <Typography sx={{ marginX: '0.5rem' }}>–</Typography>}
        {setGapEnd && (
          <FormControl
            variant="standard"
            size="small"
            sx={{ width: '60px', marginRight: '0.5rem' }}
          >
            <label htmlFor={`gap${gapNumber}Max`} style={visuallyHidden}>
              Gap End
            </label>
            <TextField
              id={`gap${gapNumber}Max`}
              value={gapEnd}
              onChange={handleInputChange(
                `gap${gapNumber}Max`,
                setGapEnd,
                chartDefaults[`gap${gapNumber}Max`]?.id
              )}
              variant="standard"
              size="small"
              type="number"
              inputProps={{ step: 0.1 }}
              sx={{ width: '100%' }}
            />
          </FormControl>
        )}
        <Typography variant="caption">sec</Typography>
      </Box>
    </Box>
  )

  return (
    <>
      <BinSizeDropdown value={binSize} handleChange={handleBinSizeChange} />
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          marginY: '1rem',
        }}
      >
        <Typography>Trend Line Gap Threshold</Typography>
        <Box sx={{ display: 'flex', alignItems: 'center' }}>
          <FormControl sx={{ width: '60px' }}>
            <label htmlFor="trendLineGapThreshold" style={visuallyHidden}>
              Trend Line Gap Threshold
            </label>
            <TextField
              id="trendLineGapThreshold"
              type="number"
              inputProps={{ step: 0.1 }}
              value={trendLineGapThreshold}
              onChange={handleTrendLineGapThresholdChange}
              variant="standard"
              placeholder="Enter threshold"
              size="small"
            />
          </FormControl>
          <Typography variant="caption" sx={{ marginLeft: '0.5rem' }}>
            sec
          </Typography>
        </Box>
      </Box>
      {renderGapInput(1, gap1Min, setgap1Min, gap1Max, setgap1Max)}
      {renderGapInput(2, gap2Min, setgap2Min, gap2Max, setgap2Max)}
      {renderGapInput(3, gap3Min, setgap3Min, gap3Max, setgap3Max)}
      {renderGapInput(4, gap4Min, setgap4Min, null, null)}
    </>
  )
}
