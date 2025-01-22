import { Box, FormControl, InputLabel, TextField } from '@mui/material'
import { ChangeEvent } from 'react'

interface YAxisDefaultInputProps {
  value: string
  handleChange: (event: ChangeEvent<HTMLInputElement>) => void
  id?: string
}

export const YAxisDefaultInput = ({
  value,
  handleChange,
  id = 'bin-size',
}: YAxisDefaultInputProps) => {
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
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
        marginRight: '29.3px',
      }}
    >
      <InputLabel sx={{ color: 'black' }}>Y-Axis Chart Default </InputLabel>
      <Box sx={{ display: 'flex' }}>
        <FormControl sx={{ width: '60px' }}>
          <label htmlFor="yAxis-defualt" style={visuallyHidden}>
            YAxis Chart Default
          </label>
          <TextField
            id="yAxis-defualt"
            type="number"
            value={value}
            onChange={handleChange}
            variant="standard"
          />
        </FormControl>
      </Box>
    </Box>
  )
}
