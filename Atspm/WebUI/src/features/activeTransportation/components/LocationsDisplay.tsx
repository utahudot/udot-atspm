import { ATErrorState, Location } from '@/pages/reports/active-transportation'
import DeleteIcon from '@mui/icons-material/Delete'
import {
  Box,
  Button,
  Divider,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import { useMemo } from 'react'

interface LocationsDisplayProps {
  locations: Location[]
  phase?: number | ''
  setPhase: (phase: number | '') => void
  onLocationDelete: (location: Location) => void
  onDeleteAllLocations: (locations: Location[]) => void
  onUpdateLocation: (updatedLocation: Location) => void
  errorState: ATErrorState
}

const LocationsDisplay = ({
  locations,
  phase,
  setPhase,
  onLocationDelete,
  onDeleteAllLocations,
  onUpdateLocation,
}: LocationsDisplayProps) => {
  const pedestrianPhases = useMemo(() => {
    const set = new Set<number>()

    locations.forEach((loc) => {
      if (loc.pedsAre1to1) {
        // phases are 1:1 → use protectedPhaseNumber if it exists and is not 0
        loc.approaches?.forEach((a) => {
          const phase = a.protectedPhaseNumber
          if (typeof phase === 'number' && phase !== 0) {
            set.add(phase)
          }
        })
      } else {
        // phases are not 1:1 → use pedestrianPhaseNumber if it’s not null
        loc.approaches?.forEach((a) => {
          const pedPhase = a.pedestrianPhaseNumber
          if (pedPhase != null) {
            set.add(pedPhase)
          }
        })
      }
    })

    return Array.from(set).sort((a, b) => a - b)
  }, [locations])

  function handlePhaseChange(newPhase: number | '') {
    setPhase(newPhase)
  }

  if (!locations.length) {
    return (
      <Typography variant="body2" color="textSecondary" sx={{ width: '480px' }}>
        No locations selected
      </Typography>
    )
  }

  return (
    <>
      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        sx={{ mb: 1 }}
      >
        <FormControl size="small" sx={{ minWidth: 220 }}>
          <InputLabel id="phase-label">Phase</InputLabel>
          <Select
            labelId="phase-label"
            label="Phase"
            value={phase || 'All'}
            onChange={(e) => handlePhaseChange(e.target.value as number | '')}
          >
            <MenuItem value={'All'}>All</MenuItem>
            {pedestrianPhases.length === 0 ? (
              <MenuItem disabled>No phases available</MenuItem>
            ) : (
              pedestrianPhases.map((phase) => (
                <MenuItem key={phase} value={phase}>
                  {phase}
                </MenuItem>
              ))
            )}
          </Select>
        </FormControl>

        <Button
          startIcon={<DeleteIcon />}
          variant="outlined"
          color="error"
          size="small"
          onClick={() => onDeleteAllLocations(locations)}
        >
          Remove All
        </Button>
      </Box>

      <Box
        sx={{
          maxHeight: '505px',
          minWidth: '450px',
          maxWidth: '600px',
          overflowY: 'auto',
        }}
      >
        <Table size="small" aria-label="locations table" stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell>
                <Typography variant="subtitle2">Selected Locations</Typography>
              </TableCell>
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {locations.map((location) => (
              <TableRow hover key={location.id}>
                <TableCell sx={{ pl: 0.5 }}>
                  <Box display="flex" alignItems="center">
                    <Divider
                      orientation="vertical"
                      variant="fullWidth"
                      flexItem
                    />
                    <Box ml={1}>
                      {location.locationIdentifier} - {location.primaryName}{' '}
                      {location.secondaryName}
                    </Box>
                  </Box>
                </TableCell>
                <TableCell align="right">
                  <IconButton
                    color="error"
                    onClick={() => onLocationDelete(location)}
                  >
                    <DeleteIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Box>
    </>
  )
}

export default LocationsDisplay
