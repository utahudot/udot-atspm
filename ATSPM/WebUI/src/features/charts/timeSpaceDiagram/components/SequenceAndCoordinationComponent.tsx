import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import React, { useState } from 'react'
import { LocationWithCoordPhases, LocationWithSequence } from '../types'

interface CoordinatedTableProps {
  locationWithSequence: LocationWithSequence[]
  updateLocationWithSequence: (
    locationWithSequence: LocationWithSequence
  ) => void
  locationWithCoordPhases: LocationWithCoordPhases[]
  updateLocationWithCoordPhases: (
    locationWithCoordPhases: LocationWithCoordPhases
  ) => void
}

const SequenceAndCoordinationComponent = ({
  locationWithSequence,
  updateLocationWithSequence,
  locationWithCoordPhases,
  updateLocationWithCoordPhases,
}: CoordinatedTableProps) => {
  const [selectedLocation, setSelectedLocation] = useState<string | null>(null)
  const [sequenceValues, setSequenceValues] = useState<string[]>([])
  const [coordPhaseValue, setCoordPhaseValue] = useState<string>('')

  const handleLocationChange = (
    event: React.ChangeEvent<{ value: unknown }>
  ) => {
    const newLocation = event.target.value as string
    setSelectedLocation(newLocation)

    // Set initial values for sequence and coordination phase if available
    const selectedSeqLocation = locationWithSequence.find(
      (loc) => loc.locationIdentifier === newLocation
    )
    const selectedCoordLocation = locationWithCoordPhases.find(
      (loc) => loc.locationIdentifier === newLocation
    )

    if (selectedSeqLocation) {
      setSequenceValues(
        selectedSeqLocation.sequence.map((seq) => seq.join(','))
      )
    } else {
      setSequenceValues([])
    }

    if (selectedCoordLocation) {
      setCoordPhaseValue(selectedCoordLocation.coordinatedPhases.join(','))
    } else {
      setCoordPhaseValue('')
    }
  }

  const handleSequenceChange = (
    index: number,
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const newValue = event.target.value
    const newSequenceValues = [...sequenceValues]
    newSequenceValues[index] = newValue
    setSequenceValues(newSequenceValues)

    // Transform the string values into number arrays
    const newSequence = newSequenceValues.map((str) =>
      str === '' ? [] : str.split(',').map(Number)
    )
    const updatedLocation = locationWithSequence.find(
      (loc) => loc.locationIdentifier === selectedLocation
    )
    if (updatedLocation) {
      const updatedLocationCopy = { ...updatedLocation }
      updatedLocationCopy.sequence = newSequence
      updateLocationWithSequence(updatedLocationCopy)
    }
  }

  const handleCoordPhaseChange = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const newValue = event.target.value
    setCoordPhaseValue(newValue)

    // Transform the string value into number array
    const newCoordPhase = newValue === '' ? [] : newValue.split(',').map(Number)
    const updatedLocation = locationWithCoordPhases.find(
      (loc) => loc.locationIdentifier === selectedLocation
    )
    if (updatedLocation) {
      const updatedLocationCopy = { ...updatedLocation }
      updatedLocationCopy.coordinatedPhases = newCoordPhase
      updateLocationWithCoordPhases(updatedLocationCopy)
    }
  }

  return (
    <Box>
      <Box display="flex" alignItems="center">
        <InputLabel htmlFor={'location-select'}>
          <Typography color="black" marginRight="8px" variant="subtitle1">
            Select Location:
          </Typography>
        </InputLabel>
        <FormControl>
          <Select
            value={selectedLocation || ''}
            onChange={handleLocationChange}
            size="small"
            inputProps={{ id: 'location-select' }}
          >
            {locationWithSequence.map((location) => (
              <MenuItem
                key={location.locationIdentifier}
                value={location.locationIdentifier}
              >
                {location.locationIdentifier}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>
      {selectedLocation && (
        <Box>
          <Box>
            <Typography variant="subtitle1" marginBottom={1} mt={4}>
              Sequence:
            </Typography>
            {sequenceValues.map((value, index) => (
              <Box
                key={index}
                display="flex"
                alignItems="center"
                marginBottom={1}
              >
                <InputLabel htmlFor={`location-${index + 1}`}>
                  <Typography color="black" variant="body1" marginRight={1}>
                    Band {index + 1}:
                  </Typography>
                </InputLabel>

                <TextField
                  variant="outlined"
                  value={value}
                  size="small"
                  onChange={(event) => handleSequenceChange(index, event)}
                  inputProps={{ id: `location-${index + 1}` }}
                />
              </Box>
            ))}
          </Box>
          <Box display="flex" alignItems="center" marginTop={4}>
            <InputLabel htmlFor={`coordinated-phases`}>
              <Typography color="black" variant="subtitle1" flex={1}>
                Coordinated Phases:
              </Typography>
            </InputLabel>
            <TextField
              variant="outlined"
              value={coordPhaseValue}
              onChange={handleCoordPhaseChange}
              size="small"
              sx={{ flex: '1' }}
              inputProps={{ id: `coordinated-phases` }}
            />
          </Box>
        </Box>
      )}
    </Box>
  )
}

export default SequenceAndCoordinationComponent
