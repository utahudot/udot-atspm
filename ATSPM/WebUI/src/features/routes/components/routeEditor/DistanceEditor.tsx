import RefreshIcon from '@mui/icons-material/Refresh'
import {
  Box,
  IconButton,
  InputAdornment,
  Paper,
  TextField,
} from '@mui/material'
import React from 'react'

interface DistanceEditorProps {
  routeLocation: any // Replace 'any' with the correct type for your routeLocation
}

const DistanceEditor: React.FC<DistanceEditorProps> = ({ routeLocation }) => {
  // State to hold the distance value
  const [distance, setDistance] = React.useState('')

  // Function to handle distance change
  const handleDistanceChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setDistance(event.target.value)
  }

  // Function to handle submit action
  const handleSubmit = () => {
    console.log(`Distance for location ${routeLocation.id}: ${distance}`)
    // Implement the submit logic
  }

  return (
    <Box
      display="flex"
      justifyContent="flex-start"
      alignItems="center"
      sx={{ width: '150px', marginLeft: 1 }}
    >
      <Paper>
        <TextField
          value={distance}
          onChange={handleDistanceChange}
          variant="outlined"
          style={{ width: '100px' }}
          InputProps={{
            endAdornment: <InputAdornment position="end">ft</InputAdornment>,
          }}
        />
      </Paper>
      <IconButton onClick={handleSubmit} color="primary" size="large">
        <RefreshIcon />
      </IconButton>
    </Box>
  )
}

export default DistanceEditor
