import { useChartsStore } from '@/stores/charts'
import {
  Alert,
  Box,
  Button,
  FormControl,
  InputLabel,
  Paper,
  TextField,
  Typography,
} from '@mui/material'
import { ChangeEvent, useEffect, useState } from 'react'

interface YAxisDefaultInputProps {
  value: string | null
  handleChange: (value: string | null) => void
  isMeasureDefaultView: boolean
}

export const YAxisDefaultInput = ({
  value,
  handleChange,
  isMeasureDefaultView,
}: YAxisDefaultInputProps) => {
  const [inputValue, setInputValue] = useState<string | null>(value)
  const [showError, setShowError] = useState<boolean>(false)
  const { yAxisMaxStore } = useChartsStore()

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

  const handleInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value
    setInputValue(newValue)
    if (isMeasureDefaultView) {
      handleChange(newValue)
    }
  }

  useEffect(() => {
    if (value === undefined) {
      setShowError(true)
    } else {
      setShowError(false)
      setInputValue(value)
    }
  }, [value])

  const applyChange = () => {
    if (inputValue !== yAxisMaxStore) {
      ;(handleChange as (value: string | null) => void)(inputValue)
    }
  }

  if (showError) {
    return (
      <Alert
        severity="error"
        sx={{
          mt: 2,
          '& .MuiAlert-message': {
            width: '100%',
          },
        }}
      >
        A Y-Axis Default value is not found for this Measure Default.
      </Alert>
    )
  }

  return isMeasureDefaultView ? (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
        marginRight: '29.3px',
      }}
    >
      <InputLabel sx={{ color: 'black' }}>Y-Axis Chart Default</InputLabel>
      <Box sx={{ display: 'flex' }}>
        <FormControl sx={{ width: '60px' }}>
          <label htmlFor="yAxis-default" style={visuallyHidden}>
            YAxis Chart Default
          </label>
          <TextField
            id="yAxis-default"
            type="number"
            value={value}
            onChange={handleInputChange}
            variant="standard"
          />
        </FormControl>
      </Box>
    </Box>
  ) : (
    <Paper
      sx={{
        px: 2,
        py: 1,
        my: 1,
        bgcolor: 'background.default',
        flexGrow: 1,
        display: 'flex',
        flexDirection: 'column',
      }}
      elevation={0}
    >
      <Typography variant="caption">Display</Typography>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          flexGrow: 1,
        }}
      >
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            width: '100%',
            alignItems: 'center',
          }}
        >
          <InputLabel sx={{ color: 'black' }}>Y-Axis Max</InputLabel>
          <Box sx={{ display: 'flex' }}>
            <FormControl sx={{ width: '60px' }}>
              <label htmlFor="yAxis-max" style={visuallyHidden}>
                YAxis Max
              </label>
              <TextField
                id="yAxis-max"
                type="number"
                value={inputValue}
                onChange={handleInputChange}
                variant="standard"
              />
            </FormControl>
          </Box>
        </Box>
        <Button
          variant="contained"
          size="small"
          sx={{
            alignSelf: 'flex-end',
            mt: 'auto',
          }}
          onClick={applyChange}
          disabled={inputValue === yAxisMaxStore}
        >
          Apply
        </Button>
      </Box>
    </Paper>
  )
}
