import {
  TspErrorState,
  TspLocation,
} from '@/pages/reports/transit-signal-priority'
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
import { useMemo, useState } from 'react'

interface Props {
  locations: TspLocation[]
  onLocationDelete: (location: TspLocation) => void
  onDeleteAllLocations: (locations: TspLocation[]) => void
  onUpdateLocation: (updatedLocation: TspLocation) => void
  errorState: TspErrorState
}

export default function MultipleLocationsDisplay({
  locations,
  onLocationDelete,
  onDeleteAllLocations,
  onUpdateLocation,
  errorState,
}: Props) {
  const [globalSelectedPhases, setGlobalSelectedPhases] = useState<number[]>([
    2, 6,
  ])

  const allGlobalPhases = useMemo(() => {
    const phaseSet = new Set<number>()
    locations.forEach((loc) => {
      loc.approaches?.forEach((a) => {
        if (typeof a.protectedPhaseNumber === 'number') {
          phaseSet.add(a.protectedPhaseNumber)
        }
      })
    })
    return Array.from(phaseSet).sort((a, b) => a - b)
  }, [locations])

  function handleGlobalPhaseChange(newPhases: number[]) {
    const phasesAdded = newPhases.filter(
      (phase) => !globalSelectedPhases.includes(phase)
    )
    const phasesRemoved = globalSelectedPhases.filter(
      (phase) => !newPhases.includes(phase)
    )
    locations.forEach((location) => {
      let updatedPhases = [...(location.designatedPhases || [])]
      phasesAdded.forEach((phase) => {
        const hasPhase = location.approaches?.some(
          (a) => a.protectedPhaseNumber === phase
        )
        if (hasPhase && !updatedPhases.includes(phase)) {
          updatedPhases.push(phase)
        }
      })
      phasesRemoved.forEach((phase) => {
        updatedPhases = updatedPhases.filter((p) => p !== phase)
      })
      onUpdateLocation({ ...location, designatedPhases: updatedPhases })
    })
    setGlobalSelectedPhases(newPhases)
  }

  function handlePhaseChange(location: TspLocation, selectedPhases: number[]) {
    onUpdateLocation({ ...location, designatedPhases: selectedPhases })
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
          <InputLabel id="global-phases-label">Global Phases</InputLabel>
          <Select
            labelId="global-phases-label"
            multiple
            label="Global Phases"
            value={globalSelectedPhases}
            onChange={(e) =>
              handleGlobalPhaseChange(e.target.value as number[])
            }
            renderValue={(selected) => (selected as number[]).join(', ')}
          >
            {allGlobalPhases.length > 0 ? (
              allGlobalPhases.map((phase) => (
                <MenuItem key={phase} value={phase}>
                  {phase}
                </MenuItem>
              ))
            ) : (
              <MenuItem disabled>No Global Phases Available</MenuItem>
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
              <TableCell align="center">Designated Phases</TableCell>
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {locations.map((location) => {
              const locationPhaseOptions = Array.from(
                new Set(
                  location.approaches?.map((a) => a.protectedPhaseNumber) || []
                )
              ).sort((a, b) => a - b)
              const isInError =
                errorState.type === 'MISSING_PHASES' &&
                errorState.locationIDs.has(String(location.id))
              return (
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
                  <TableCell align="center">
                    <FormControl
                      variant="outlined"
                      size="small"
                      sx={{ minWidth: 150 }}
                      error={isInError}
                      fullWidth
                    >
                      <InputLabel id={`phase-label-${location.id}`}>
                        Phases
                      </InputLabel>
                      <Select
                        labelId={`phase-label-${location.id}`}
                        multiple
                        label="Phases"
                        value={location.designatedPhases || []}
                        onChange={(e) =>
                          handlePhaseChange(
                            location,
                            e.target.value as number[]
                          )
                        }
                        renderValue={(selected) =>
                          (selected as number[]).join(', ')
                        }
                      >
                        {locationPhaseOptions.length > 0 ? (
                          locationPhaseOptions.map((phase) => (
                            <MenuItem key={phase} value={phase}>
                              {phase}
                            </MenuItem>
                          ))
                        ) : (
                          <MenuItem disabled>No Phases Available</MenuItem>
                        )}
                      </Select>
                    </FormControl>
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
              )
            })}
          </TableBody>
        </Table>
      </Box>
    </>
  )
}
