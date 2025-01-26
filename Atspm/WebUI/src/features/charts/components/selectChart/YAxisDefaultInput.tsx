import { Box, Button, FormControl, InputLabel, TextField } from '@mui/material'
import { ChangeEvent, useState } from 'react'

interface YAxisDefaultInputProps {
  value: string | null
  handleChange: (value: string | null) => void
  id?: string
}

export const YAxisDefaultInput = ({
  value,
  handleChange,
  id = 'bin-size',
}: YAxisDefaultInputProps) => {
  const [inputValue, setInputValue] = useState<string | null>(value)

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
    setInputValue(e.target.value)
  }

  const applyChange = () => {
    if (inputValue !== value) {
      handleChange(inputValue)
    }
  }

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'flex-start',
        position: 'relative',
        height: '300px',
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
            <label htmlFor="yAxis-defualt" style={visuallyHidden}>
              YAxis Chart Default
            </label>
            <TextField
              id="yAxis-defualt"
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
          marginTop: 'auto',
        }}
        onClick={applyChange}
        disabled={inputValue === value}
      >
        Apply
      </Button>
    </Box>
  )
}
