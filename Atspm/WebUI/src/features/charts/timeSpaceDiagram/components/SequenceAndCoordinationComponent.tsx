import OptionsWrapper from '@/components/OptionsWrapper'
import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material'
import React, { useEffect, useState } from 'react'
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

  useEffect(() => {
    if (locationWithSequence.length > 0) {
      setSelectedLocation(locationWithSequence[0].locationIdentifier)
    }
  }, [locationWithSequence])

  useEffect(() => {
    if (!selectedLocation) return

    const selectedSeqLocation = locationWithSequence.find(
      (loc) => loc.locationIdentifier === selectedLocation
    )
    const selectedCoordLocation = locationWithCoordPhases.find(
      (loc) => loc.locationIdentifier === selectedLocation
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
  }, [selectedLocation, locationWithSequence, locationWithCoordPhases])

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
    <OptionsWrapper header="Sequence and Coordination">
      <Box display="flex" alignItems="center" marginBottom={2}>
        <InputLabel htmlFor="location-select">
          <Typography color="black" marginRight="8px" variant="subtitle1">
            Select Location:
          </Typography>
        </InputLabel>
        <FormControl variant="outlined" size="small">
          <Select
            style={{ minWidth: '150px' }}
            value={selectedLocation || ''}
            onChange={handleLocationChange}
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
      <Box sx={{ minHeight: '235px' }}>
        {selectedLocation && (
          <>
            <Typography variant="subtitle1" marginBottom={2} marginTop={2}>
              Sequence:
            </Typography>
            {sequenceValues.map((value, index) => (
              <Box
                key={index}
                display="flex"
                alignItems="center"
                marginBottom={2}
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
                  sx={{ flexGrow: 1 }}
                />
              </Box>
            ))}
            <Box marginTop={2}>
              <InputLabel htmlFor="coordinated-phases">
                <Typography color="black" variant="subtitle1">
                  Coordinated Phases:
                </Typography>
              </InputLabel>
              <TextField
                variant="outlined"
                value={coordPhaseValue}
                onChange={handleCoordPhaseChange}
                size="small"
                inputProps={{ id: 'coordinated-phases' }}
                fullWidth
                sx={{ marginTop: 1 }}
              />
            </Box>
          </>
        )}
      </Box>
    </OptionsWrapper>
  )
}

export default SequenceAndCoordinationComponent
