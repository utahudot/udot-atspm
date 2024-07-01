import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  SelectChangeEvent,
  Typography,
} from '@mui/material'

interface BinSizeProps {
  value: string
  handleChange: (event: SelectChangeEvent<string>) => void
  id?: string
}

export const BinSizeDropdown = ({
  value,
  handleChange,
  id = 'bin-size',
}: BinSizeProps) => {
  const labelId = `${id}-label`
  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'space-between',
      }}
    >
      <InputLabel htmlFor={labelId}>Bin Size</InputLabel>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <FormControl fullWidth>
          <Select
            label="Bin Size"
            value={value}
            onChange={handleChange}
            variant="standard"
            size="small"
            sx={{ width: '60px' }}
            inputProps={{ id: labelId }}
          >
            <MenuItem value={5}>5</MenuItem>
            <MenuItem value={15}>15</MenuItem>
            <MenuItem value={60}>60</MenuItem>
          </Select>
        </FormControl>
        <Typography variant="caption" sx={{ marginLeft: '0.5rem' }}>
          min
        </Typography>
      </Box>
    </Box>
  )
}
